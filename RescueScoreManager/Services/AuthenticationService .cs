using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RescueScoreManager.Data;
using Windows.Services.Maps;

namespace RescueScoreManager.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IApiService _apiService;
    private readonly IStorageService _storageService;
    private AuthenticationInfo _authInfo;

    public UserInfo? CurrentUser => _authInfo?.UserInfo;
    public AuthenticationInfo? AuthenticationInfo => _authInfo;
    public bool IsAuthenticated => _authInfo != null && _authInfo.IsTokenValid;

    public AuthenticationService(IApiService apiService, IStorageService storageService)
    {
        _apiService = apiService;
        _storageService = storageService;

        // Load stored user information
        _authInfo = _storageService.LoadAuthenticationInfo();
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        // Request new token
        var tokenResponse = await _apiService.RequestTokenAsync(username, password);

        if (!tokenResponse.Success)
        {
            return false;
        }

        // Get user info
        var userInfo = await _apiService.GetUserInfoAsync(tokenResponse.Token);

        if (!userInfo.Success)
        {
            return false;
        }

        // Store user information
        _authInfo = new AuthenticationInfo
        {
            Token = tokenResponse.Token,
            ExpirationDate = tokenResponse.Expiration,
            UserInfo = userInfo
        };

        // Save user info
        _storageService.SaveAuthenticationInfo(_authInfo);

        return true;
    }

    public async Task<bool> ValidateAndRefreshTokenAsync()
    {
        if (_authInfo == null)
        {
            return false;
        }

        // If Token still valid then check if it's still working
        if (_authInfo.IsTokenValid)
        {
            try
            {
                var userInfo = await _apiService.GetUserInfoAsync(_authInfo.Token);

                if (userInfo.Success)
                {
                    // Update user info
                    _authInfo.UserInfo = userInfo;
                    _storageService.SaveAuthenticationInfo(_authInfo);
                    return true;
                }
            }
            catch
            {
                // If call failed, then the token is not valid 
                return false;
            }
        }

        return false;
    }

    public void Logout()
    {
        _authInfo = null;
        _storageService.SaveAuthenticationInfo(new AuthenticationInfo());
    }
}
