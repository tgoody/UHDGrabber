using Newtonsoft.Json;

namespace UHDGrabber.Services;

public class DataManagerService
{
    private class SaveDataContainer
    {
        public RadarrGrabberService RadarrGrabberService;
    }
    
    private static RadarrGrabberService _radarrGrabberService;
    public DataManagerService(RadarrGrabberService radarrGrabberService)
    {
        _radarrGrabberService = radarrGrabberService;
        LoadDataToServices();
    }
    
   

    public void SaveDataToFile()
    {
        var file = new StreamWriter("uhdgrabberdata.ugd");
        var dataToSave = new SaveDataContainer
        {
            RadarrGrabberService = _radarrGrabberService
        };
        var temp = JsonConvert.SerializeObject(dataToSave);
        file.WriteLine(temp);
        file.Close();
    }

    public void LoadDataToServices()
    {
        var file = new StreamReader("uhdgrabberdata.ugd");
        var json = file.ReadToEnd();
        var saveDataContainer = JsonConvert.DeserializeObject<SaveDataContainer>(json);
        _radarrGrabberService.RadarrIp = saveDataContainer?.RadarrGrabberService.RadarrIp;
        _radarrGrabberService.RadarrApiKey = saveDataContainer?.RadarrGrabberService.RadarrApiKey;
        file.Close();
    }

}