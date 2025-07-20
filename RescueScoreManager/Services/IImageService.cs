using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RescueScoreManager.Services;

public interface IImageService
{
    Task<BitmapImage?> GetImageAsync(string? url, CancellationToken cancellationToken = default);
    Task ClearCacheAsync();
    Task<int> GetCacheSizeAsync();
    bool IsInternetAvailable { get; }
}
