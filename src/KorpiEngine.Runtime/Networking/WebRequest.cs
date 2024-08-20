using System.Collections;
using KorpiEngine.Core;
using KorpiEngine.Core.API.AssetManagement;

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

public sealed class WebAssetLoadOperation<T> : IDisposable where T : Resource
{
    private readonly string? _relativeSavePath;
    private readonly WebRequest _request;

    public bool IsDone => _request.IsDone;
    public T? Result { get; private set; }


    public WebAssetLoadOperation(string url, string? relativeSavePath)
    {
        _relativeSavePath = relativeSavePath;
        
        _request = WebRequest.Get(url);
        _request.DownloadHandler = new DownloadHandler();
    }
    
    
    public IEnumerator SendWebRequest()
    {
        Console.WriteLine("sponzaaaaaaa!");
        yield return _request.SendWebRequest();

        if (!string.IsNullOrEmpty(_request.Error))
        {
            Application.Logger.Error(_request.Error);
            yield break;
        }

        string savePath = string.IsNullOrEmpty(_relativeSavePath) ?
            Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()) :
            Path.Combine(Application.Directory, _relativeSavePath);
        
        File.WriteAllText(savePath, _request.DownloadHandler!.Text);

        Result = AssetDatabase.LoadAssetFile<T>(savePath);
    }


    public void Dispose()
    {
        _request.Dispose();
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
    
    
    
    public static WebAssetLoadOperation<T> LoadWebAsset<T>(string url, string? relativeSavePath = null) where T : Resource
    {
        return new WebAssetLoadOperation<T>(url, relativeSavePath);
    }


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