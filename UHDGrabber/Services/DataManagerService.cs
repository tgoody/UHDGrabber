using Newtonsoft.Json;
using UHDGrabber.Data;

namespace UHDGrabber.Services;

public class DataManagerService
{
    private class SaveDataContainer
    {
        public RadarrGrabberService RadarrGrabberService;
        public List<LocalMovieContainer> LocalMovieContainers;
    }
    
    private static RadarrGrabberService _radarrGrabberService;
    private static IndexerSearchService _indexerSearchService;
    public static List<LocalMovieContainer> LocalMovieContainers = new();

    public bool HasMovies => LocalMovieContainers.Count > 0;
    public DataManagerService(RadarrGrabberService radarrGrabberService, IndexerSearchService indexerSearchService)
    {
        _radarrGrabberService = radarrGrabberService;
        _indexerSearchService = indexerSearchService;
        LoadDataToServices();
    }

    public void SaveDataToFile()
    {
        File.Delete(@"uhdgrabberdata.ugd");
        var file = new StreamWriter("uhdgrabberdata.ugd");
        var dataToSave = new SaveDataContainer
        {
            RadarrGrabberService = _radarrGrabberService,
            LocalMovieContainers = LocalMovieContainers
        };
        var temp = JsonConvert.SerializeObject(dataToSave);
        file.WriteLine(temp);
        file.Close();
    }

    public void LoadDataToServices()
    {
        if (!File.Exists("uhdgrabberdata.ugd"))
            return;
        
        var file = new StreamReader("uhdgrabberdata.ugd");
        var json = file.ReadToEnd();
        var saveDataContainer = JsonConvert.DeserializeObject<SaveDataContainer>(json);
        
        //if this data is available, do something so the user doesn't have to worry about inputting
        _radarrGrabberService.RadarrIp = saveDataContainer?.RadarrGrabberService.RadarrIp;
        _radarrGrabberService.RadarrApiKey = saveDataContainer?.RadarrGrabberService.RadarrApiKey;
        if (saveDataContainer?.LocalMovieContainers != null)
        {
            LocalMovieContainers = saveDataContainer.LocalMovieContainers;
            PullRadarrMovies();
        }
        
        file.Close();
    }

    public async Task PullRadarrMovies()
    {
        
        var newMovieContainers = await _radarrGrabberService.PullAllMovies();
        MergeMovieContainers(newMovieContainers);
        SaveDataToFile();
    }

    private void MergeMovieContainers(List<LocalMovieContainer> newMovieContainers)
    {
        foreach (var newMovie in newMovieContainers)
        {
            var matchedMovie = LocalMovieContainers.SingleOrDefault(mv => mv.ImdbId == newMovie.ImdbId);
            if (matchedMovie == null)
                continue;
            newMovie.LastSearched = matchedMovie.LastSearched;
            newMovie.DownloadObjects = matchedMovie.DownloadObjects;
        }

        LocalMovieContainers = newMovieContainers;
    }

    public async Task StartIndexerSearch()
    {
        var moviesWithoutMagnetLinks = LocalMovieContainers.Where(
            mv => mv.DownloadObjects == null || mv.DownloadObjects.Count <= 0).
            OrderBy(mv => !mv.LastSearched.HasValue).ThenBy(mv => mv.LastSearched);
        foreach (var movie in moviesWithoutMagnetLinks)
        {
            movie.LastSearched = DateTime.Now;
            var success = await _indexerSearchService.SearchMovie(movie);
            if (!success)
            {
                Console.Error.WriteLine($"Could not parse indexer data for movie: {movie.MovieName}");
            }
            SaveDataToFile();
        }
    }
    
    

}