using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RescueScoreManager.Data;

public class UserInfo
{
    public string? Label { get; set; }
    public string? Type { get; set; }
    public string? Role { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
}
