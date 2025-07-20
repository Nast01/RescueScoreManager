using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RescueScoreManager.Services;

public interface INetworkConnectivityService
{    bool IsAvailable { get; }
    Task<bool> CheckConnectivityAsync();
    event EventHandler<bool> ConnectivityChanged;
}
