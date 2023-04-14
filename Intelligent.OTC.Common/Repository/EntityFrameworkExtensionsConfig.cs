using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intelligent.OTC.Common.Repository
{
    public static class EntityFrameworkExtensionsConfig
    {
        public static void Register()
        {
            string licenseName = "85;100-ACN";//... PRO license name
            string licenseKey = "D200C9293A3372152C864860C1944DD8";//... PRO license key
            Z.EntityFramework.Extensions.LicenseManager.AddLicense(licenseName, licenseKey);
            string errorMessage = "";
            bool isValidateLicense = Z.EntityFramework.Extensions.LicenseManager.ValidateLicense(out errorMessage,Z.BulkOperations.ProviderType.SqlServer);
        }
    }
}
