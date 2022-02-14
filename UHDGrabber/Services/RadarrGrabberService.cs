namespace UHDGrabber.Services;

public class RadarrGrabberService
{
    public string? RadarrApiKey
    {
        get => _radarrApiKey;
        set => _radarrApiKey = value;
    }

    public string? RadarrIp
    {
        get => _radarrIp;
        set => _radarrIp = value;
    }

    private string? _radarrApiKey;
    private string? _radarrIp;
}