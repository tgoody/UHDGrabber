using Newtonsoft.Json;

namespace UHDGrabber.Data;

public class LocalMovieContainer
{
    public class DownloadObject
    {
        public string filename { get; set; }
        public string category { get; set; }
        [JsonProperty("download")]
        public string magnetLink { get; set; }
    }
    
    [JsonProperty("title")]
    public string MovieName { get; set; }

    [JsonProperty("imdbId")]
    public string ImdbId { get; set; }

    [JsonProperty("magnet_links")] 
    public List<DownloadObject>? DownloadObjects { get; set; } = new();
    
    public DateTime? LastSearched { get; set; }
}