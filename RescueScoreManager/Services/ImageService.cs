using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using Microsoft.Extensions.Logging;

namespace RescueScoreManager.Services;

public class ImageService : IImageService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ImageService> _logger;
    private readonly INetworkConnectivityService _networkService;
    private readonly Dictionary<string, BitmapImage> _imageCache = new();
    private readonly SemaphoreSlim _cacheSemaphore = new(1, 1);
    private bool _disposed = false;

    public ImageService(
        ILogger<ImageService> logger,
        INetworkConnectivityService networkService)
    {
        // Create and configure HttpClient
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _networkService = networkService ?? throw new ArgumentNullException(nameof(networkService));
    }

    public bool IsInternetAvailable => _networkService.IsAvailable;

    public async Task<BitmapImage?> GetImageAsync(string? url, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate URL
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogDebug("No URL provided for image download");
                return null;
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                _logger.LogWarning("Invalid URL provided: {Url}", url);
                return null;
            }

            // Check internet connectivity
            if (!IsInternetAvailable)
            {
                _logger.LogWarning("No internet connection available for URL: {Url}", url);
                return null;
            }

            // Check cache first
            await _cacheSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (_imageCache.TryGetValue(url, out BitmapImage? cachedImage))
                {
                    _logger.LogDebug("Retrieved image from cache for URL: {Url}", url);
                    return cachedImage;
                }
            }
            finally
            {
                _cacheSemaphore.Release();
            }

            // Download the image
            _logger.LogDebug("Downloading image from URL: {Url}", url);

            using var response = await _httpClient.GetAsync(uri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to download image from {Url}. HTTP Status: {StatusCode}",
                    url, response.StatusCode);
                return null;
            }

            // Check if content is actually an image
            string? contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType == null || !contentType.StartsWith("image/"))
            {
                _logger.LogWarning("URL does not return an image. Content-Type: {ContentType}, URL: {Url}",
                    contentType, url);
                return null;
            }

            // Read image data
            byte[] imageData = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (imageData.Length == 0)
            {
                _logger.LogWarning("Empty image data received from URL: {Url}", url);
                return null;
            }

            // Create BitmapImage from downloaded data
            var bitmapImage = CreateBitmapImageFromBytes(imageData);

            if (bitmapImage != null)
            {
                // Cache the image
                await _cacheSemaphore.WaitAsync(cancellationToken);
                try
                {
                    if (!_imageCache.ContainsKey(url))
                    {
                        _imageCache[url] = bitmapImage;
                        _logger.LogDebug("Cached image for URL: {Url}", url);
                    }
                }
                finally
                {
                    _cacheSemaphore.Release();
                }

                _logger.LogDebug("Successfully loaded image from URL: {Url}", url);
            }

            return bitmapImage;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Image download cancelled for URL: {Url}", url);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error downloading image from URL: {Url}", url);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error downloading image from URL: {Url}", url);
            return null;
        }
    }

    private static BitmapImage? CreateBitmapImageFromBytes(byte[] imageData)
    {
        try
        {
            using var stream = new MemoryStream(imageData);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task ClearCacheAsync()
    {
        await _cacheSemaphore.WaitAsync();
        try
        {
            _imageCache.Clear();
            _logger.LogDebug("Image cache cleared");
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    public async Task<int> GetCacheSizeAsync()
    {
        await _cacheSemaphore.WaitAsync();
        try
        {
            return _imageCache.Count;
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _cacheSemaphore?.Dispose();
            _disposed = true;
        }
    }
}
