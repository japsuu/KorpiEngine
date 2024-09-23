using System.Collections;

namespace KorpiEngine.Networking;

public sealed class DownloadHandler
{
    /// <summary>
    /// Returns the raw bytes downloaded from the remote server, or null.
    /// </summary>
    public byte[]? Bytes { get; private set; }
    
    /// <summary>
    /// Convenience property. Returns the bytes from data interpreted as a UTF8 string.
    /// </summary>
    public string? Text { get; private set; }
    
    
    public void SetBytes(byte[] bytes)
    {
        Bytes = bytes;
        Text = System.Text.Encoding.UTF8.GetString(bytes);
    }


    public void SaveToFile(string path)
    {
        if (Bytes == null)
            throw new InvalidOperationException("No bytes to save to a file.");
        
        File.WriteAllBytes(path, Bytes);
    }
}

public sealed class WebAssetLoadOperation
{
    private readonly bool _force;
    private readonly string _baseUrl;
    private readonly string _relativeSavePath;
    private readonly string[] _savePaths;
    private readonly string[] _assets;
    
    public bool IsDone { get; private set; }
    public IReadOnlyList<string> SavePaths => _savePaths;


    /// <param name="saveDirectory">The subfolder inside "WebAssets" in which to save the downloaded assets.</param>
    /// <param name="baseUrl">The base URL to download assets from.</param>
    /// <param name="force">If true, the operation will download the assets even if they already exist.</param>
    /// <param name="assets">The names of the assets to download, relative to the base URL.</param>
    /// <returns>A new WebAssetLoadOperation instance.</returns>
    public WebAssetLoadOperation(string saveDirectory, string baseUrl, bool force = false, params string[] assets)
    {
        _force = force;
        _baseUrl = baseUrl;
        _relativeSavePath = Path.Combine(Application.WebAssetsDirectory, saveDirectory);
        _savePaths = new string[assets.Length];
        _assets = assets;
        
        for (int i = 0; i < assets.Length; i++)
        {
            _savePaths[i] = Path.Combine(_relativeSavePath, assets[i]);
        }
    }
    
    
    /// <summary>
    /// Sends a web request to download the assets.
    /// </summary>
    /// <returns>An IEnumerator that can be yielded in a coroutine.</returns>
    public IEnumerator SendWebRequest()
    {
        int index = 0;
        int count = _assets.Length;
        foreach (string asset in _assets)
        {
            index++;
            string url = Path.Combine(_baseUrl, asset);
            using WebRequest request = WebRequest.Get(url);
            request.DownloadHandler = new DownloadHandler();

            // Check if the asset already exists.
            string destinationFile = Path.Combine(_relativeSavePath, asset);
            if (!_force)
            {
                bool fileExists = File.Exists(destinationFile);
                if (fileExists)
                {
                    Application.Logger.Info($"Asset {index}/{count} '{url}' already exists. Skipping download.");
                    continue;
                }
            }
        
            Application.Logger.Info($"Downloading asset {index}/{count} '{url}'...");
            yield return request.SendWebRequest();

            if (!string.IsNullOrEmpty(request.Error))
            {
                Application.Logger.Error($"Failed to download asset '{url}': {request.Error}");
                continue;
            }
            
            // Ensure the directory exists.
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
        
            request.DownloadHandler.SaveToFile(destinationFile);
            
            Application.Logger.Info($"Downloaded asset to '{destinationFile}'.");
        }

        IsDone = true;
    }
}

public sealed class WebRequest : IDisposable
{
    private static readonly HttpClient HttpClient = new();
    private HttpResponseMessage? _response;
    private bool _disposed;

    public string Url { get; private set; }
    public string? Error { get; private set; }
    public bool IsDone { get; private set; }
    public DownloadHandler? DownloadHandler { get; set; }


    private WebRequest(string url)
    {
        Url = url;
    }


    public static WebRequest Get(string url) => new(url);


    public IEnumerator SendWebRequest()
    {
        Task<HttpResponseMessage> requestTask;
        try
        {
            requestTask = HttpClient.GetAsync(Url);
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            IsDone = true;
            yield break;
        }
        
        while (!requestTask.IsCompleted)
            yield return null;

        _response = requestTask.Result;
        IsDone = true;

        if (!_response.IsSuccessStatusCode)
        {
            Error = _response.ReasonPhrase;
        }
        else if (DownloadHandler != null)
        {
            Task<byte[]> readTask = _response.Content.ReadAsByteArrayAsync();
            
            while (!readTask.IsCompleted)
                yield return null;
            
            DownloadHandler.SetBytes(readTask.Result);
        }
    }


    public void Dispose()
    {
        if (_disposed)
            return;
        
        _response?.Dispose();
        _disposed = true;
    }
}