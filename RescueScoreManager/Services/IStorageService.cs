using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IStorageService
{
    void SaveAuthenticationInfo(AuthenticationInfo authInfo);
    AuthenticationInfo LoadAuthenticationInfo();
}
