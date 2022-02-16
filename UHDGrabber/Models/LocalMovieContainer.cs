using Newtonsoft.Json;

namespace UHDGrabber.Data;

public class LocalMovieContainer
{
    [JsonProperty("title")]
    private string _movieName;
    
    [JsonProperty("imdbId")]
    private string _imdbId;
    
    [JsonProperty("magnet_links")]
    private List<string> _magnetLinks;



}