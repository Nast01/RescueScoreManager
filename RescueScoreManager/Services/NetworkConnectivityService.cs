using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace RescueScoreManager.Services;

public class NetworkConnectivityService : INetworkConnectivityService
{
    private readonly ILogger<NetworkConnectivityService> _logger;
    private bool _isAvailable = true;

    public NetworkConnectivityService(ILogger<NetworkConnectivityService> logger)
    {
        _logger = logger;
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
    }

    public bool IsAvailable => _isAvailable;

    public async Task<bool> CheckConnectivityAsync()
    {
        try
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return false;
            }

            using var ping = new Ping();
            var reply = await ping.SendPingAsync("8.8.8.8", 3000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }

    public event EventHandler<bool>? ConnectivityChanged;

    private void OnNetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs e)
    {
        _isAvailable = e.IsAvailable;
        ConnectivityChanged?.Invoke(this, e.IsAvailable);
    }
}
