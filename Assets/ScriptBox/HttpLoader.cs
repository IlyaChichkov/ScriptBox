using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;

public class HttpLoader
{
    enum StatusCode
    {
        Moved = 301,
        OK = 200,
        Redirect = 302,
        NotFound = 404
    }
    public enum LoadFileType
    {
        OPTIONS,
        ICON,
        SCRIPT
    }
    private static bool isMapLoaded = false;
    private static HttpClient httpClient;
    private static string mapJson = "";
    private string getJsonContent = "";
    private bool isLoadingTemplates = false;

    string path = Application.dataPath;
    string fileName = "script-box-config.json";

    public HttpLoader()
    {
        if (httpClient == null)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            httpClient = new HttpClient(handler);
        }

        InitCacheFile();
        ClientSetup();

        LoadMap();
    }

    async public Task<JToken> GetRemoteMap()
    {
        if (!isMapLoaded)
        {
            await LoadMap();
            SBDebugger.Log(mapJson);
        }
        return JToken.Parse(mapJson);
    }
    public void DisposeClient()
    {
        httpClient.Dispose();
    }
    private void ClientSetup()
    {
        httpClient.DefaultRequestHeaders.Add("User-Agent", "ScriptBox");
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
    }
    private void InitCacheFile()
    {
        if (File.Exists(path + fileName))
        {
            File.Create(path + fileName);
        }
    }

    private void CacheRead(string key)
    {
        if (File.Exists(path + fileName))
        {
            File.Create(path + fileName);
        }
    }

    async Task LoadMap()
    {
        bool isMapLoaded = bool.Parse(PlayerPrefs.GetString("isMapLoaded", "false"));
        if (isMapLoaded)
        {
            SBDebugger.Log("Loaded map from prefs");
            mapJson = PlayerPrefs.GetString("map_json", "null");
            return;
        }
        try
        {
            SBDebugger.Log($"Loading map...");

            using HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/IlyaChichkov/scripts-box-templates/master/map.json");

            // получаем ответ
            mapJson = await response.Content.ReadAsStringAsync();
            PlayerPrefs.SetString("map_json", mapJson);
            PlayerPrefs.SetString("isMapLoaded", "true");
            HttpLoader.isMapLoaded = true;
        }
        catch (System.Exception ex)
        {
            SBDebugger.Log(ex);
        }
    }
    public async Task<byte[]> LoadImage(string group, string folder, string fileName = "icon.png")
    {
        string link = $"https://raw.githubusercontent.com/IlyaChichkov/scripts-box-templates/master/{group}/{folder}/{fileName}";
        if (string.IsNullOrEmpty(folder))
        {
            link = $"https://raw.githubusercontent.com/IlyaChichkov/scripts-box-templates/master/{group}/{fileName}";
        }
        SBDebugger.Log($"Loading {folder} folder in github. Link: {link}");

        using HttpResponseMessage response = await httpClient.GetAsync(link);
        if ((int)response.StatusCode != (int)StatusCode.OK)
        {
            SBDebugger.Warning("Image was not loaded! Status code: " + response.StatusCode);
            return null;
        }
        // получаем ответ
        return await response.Content.ReadAsByteArrayAsync();
    }
    public async Task<byte[]> LoadTemplateIcon(string group, string folder, string fileName = "icon.png")
    {
        byte[] iconBytes = await LoadImage(group, folder);
        if (iconBytes == null)
        {
            iconBytes = await LoadImage("default-template", "");
        }
        return iconBytes;
    }
    public async Task<string> LoadFile(string group, string folder, LoadFileType fileType, string fileName = "")
    {
        try
        {
            switch (fileType)
            {
                case LoadFileType.OPTIONS:
                    fileName = "config.json";
                    break;
                case LoadFileType.ICON:
                    fileName = "icon.png";
                    break;
            }
            string link = $"https://raw.githubusercontent.com/IlyaChichkov/scripts-box-templates/master/{group}/{folder}/{fileName}";
            SBDebugger.Log($"Loading {folder} folder in github. Link: {link}");
            //https://raw.githubusercontent.com/IlyaChichkov/scripts-box-templates/master/patterns/Singleton/Singleton.cs

            using HttpResponseMessage response = await httpClient.GetAsync(link);

            // получаем ответ
            string content = await response.Content.ReadAsStringAsync();
            getJsonContent = content;

            SBDebugger.Log($"Loaded! Result: {getJsonContent.ToString()}");
            return content;
        }
        catch (System.Exception ex)
        {
            SBDebugger.Log(ex);
        }
        return null;
    }

    async Task LoadFolder(string folder)
    {
        try
        {
            SBDebugger.Log($"Loading {folder} folder in github...");

            using HttpResponseMessage response = await httpClient.GetAsync("https://api.github.com/repos/IlyaChichkov/scripts-box-templates/contents/" + folder);

            // получаем ответ
            getJsonContent = await response.Content.ReadAsStringAsync();
        }
        catch (System.Exception ex)
        {
            SBDebugger.Log(ex);
        }
    }

    public void ClearCache()
    {
        PlayerPrefs.SetString("map_json", "");
        PlayerPrefs.SetString("isMapLoaded", "false");
    }
}