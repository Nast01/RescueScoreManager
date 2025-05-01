using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RescueScoreManager.Data;

public class AuthenticationInfo
{
    public string? Token { get; set; }
    public DateTime ExpirationDate { get; set; }
    public UserInfo UserInfo { get; set; }

    public bool IsTokenValid => !string.IsNullOrEmpty(Token) && ExpirationDate > DateTime.Now;
}
