using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UHDGrabber.Data;

namespace UHDGrabber.Services;

public class IndexerSearchService
{

    private string baseUrl = "http://torrentapi.org/pubapi_v2.php?";
    private string? rarbgToken = null;

    private HttpClient _httpClient = new()
    {
        DefaultRequestHeaders =
        {
            {
                "User-Agent",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.95 Safari/537.11"
            }
        }
    };
    private DateTime? _lastSearchRequest = null;
    private TimeSpan _timeBetweenRequests = TimeSpan.FromSeconds(5);

    public IndexerSearchService()
    {
        GetRarbgToken();
        
    }

    private async Task GetRarbgToken()
    {
        var tokenUrl = baseUrl + "get_token=get_token&app_id=UHDGrabber";
        var response = await _httpClient.GetAsync(tokenUrl);
        var responseText = await response.Content.ReadAsStringAsync();
        var tokenJson = JObject.Parse(responseText);
        rarbgToken = tokenJson.ContainsKey("token") ? (string?)tokenJson["token"] : "";
        _lastSearchRequest = DateTime.Now;
    }

    public async Task<bool> SearchMovie(LocalMovieContainer localMovieContainer)
    {
        if (string.IsNullOrEmpty(rarbgToken))
        {
            await Task.Delay(_timeBetweenRequests);
            await GetRarbgToken();
            return false;
        }

        while (DateTime.Now < _lastSearchRequest.Value.Add(_timeBetweenRequests))
        {
            await Task.Delay(_timeBetweenRequests);
        }

        _lastSearchRequest = DateTime.Now;
        var searchUrl = baseUrl + 
                        $"mode=search&search_imdb={localMovieContainer.ImdbId}&category=movies&category=50;51;52&limit=100" +
                        $"&token={rarbgToken}&app_id=UHDGrabber";

        var response = await _httpClient.GetAsync(searchUrl);
        var responseString = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseString);
        try
        {
            var responseJson =  JObject.Parse(responseString);
            if (!responseJson.HasValues || responseJson["error_code"] != null)
            {
                return false;
            }

            var jsonDownloadList = responseJson["torrent_results"] as JArray;
            if (jsonDownloadList == null || jsonDownloadList.Count < 1)
            {
                return false;
            }
            
            foreach (var downloadJson in jsonDownloadList)
            {
                localMovieContainer.DownloadObjects.Add(downloadJson.ToObject<LocalMovieContainer.DownloadObject>());
            }

            return true;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return false;
        }
    }
    
}