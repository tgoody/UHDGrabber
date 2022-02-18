using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UHDGrabber.Data;

namespace UHDGrabber.Services;

public class RadarrGrabberService
{

    private HttpClient _httpClient = new();
    private string apiString = "api/v3";
    public string? RadarrApiKey { get; set; }


    public string? RadarrIp { get; set; }
    private string? RadarrUrl => $"http://{RadarrIp}";

    public async Task<bool> TestConfig()
    {
        try
        {
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var response = await _httpClient.GetAsync($"{RadarrUrl}/{apiString}/system/status?apiKey={RadarrApiKey}", cancellationToken.Token);
            var responseText = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<List<LocalMovieContainer>> PullAllMovies()
    {
        var resultMovies = new List<LocalMovieContainer>();
        
        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        var response = await _httpClient.GetAsync($"{RadarrUrl}/{apiString}/movie?apiKey={RadarrApiKey}", cancellationToken.Token);
        var responseText = await response.Content.ReadAsStringAsync();
        
        var radarrMovies = JArray.Parse(responseText);
        foreach (var movie in radarrMovies)
        {
            var newContainer = movie.ToObject<LocalMovieContainer>();
            var movieImagesJson = movie["images"] as JArray;
            newContainer.ImageUrl = (string?)movieImagesJson?.FirstOrDefault(mv => (string?) mv["coverType"] == "poster")?["remoteUrl"];
            resultMovies.Add(newContainer);
        }
        
        return resultMovies;
    }
}