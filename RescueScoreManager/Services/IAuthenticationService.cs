using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IAuthenticationService
{
    Task<bool> LoginAsync(string username, string password);
    Task<bool> ValidateAndRefreshTokenAsync();
    bool IsAuthenticated { get; }
    UserInfo CurrentUser { get; }
    void Logout();
}
