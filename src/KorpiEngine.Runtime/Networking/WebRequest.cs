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