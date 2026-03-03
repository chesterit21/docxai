using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Api.Extensions
{
    public class LicenseSettings
    {
        private readonly IConfiguration _configuration;
        private const string LICENSE_SECTION = "License";

		private const string LCK = "25hU84M5K3Y*";

		public LicenseSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string License
        {
            get => Encryption.Decrypt(_configuration[$"{LICENSE_SECTION}:AppsLicense"]);
            set => _configuration[$"{LICENSE_SECTION}:AppsLicense"] = Encryption.Encrypt(value);
        }

        public int MaxUserCount
        {
            get
            {
				var licenseKey = _configuration[$"{LICENSE_SECTION}:AppsLicense"];
				var decryptedLicenseObj = Encryption.Decrypt(licenseKey);
				var d = JsonSerializer.Deserialize<JsonObject>(decryptedLicenseObj);

				var maxUsers = int.Parse(d["UserCount"].ToString());
				return maxUsers;
            }
            //set => _configuration[$"{LICENSE_SECTION}:EncryptedMaxUserCount"] = 
            //    Encryption.Encrypt(value.ToString(), License);
        }

        public DateTime? ExpiryDate
        {
            get
            {
				var licenseKey = _configuration[$"{LICENSE_SECTION}:AppsLicense"];
				var decryptedLicenseObj = Encryption.Decrypt(licenseKey);
				var d = JsonSerializer.Deserialize<JsonObject>(decryptedLicenseObj);

				if (string.IsNullOrEmpty(licenseKey))
					return null;

				DateTimeOffset exp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(d["ValidityPeriod"].ToString()));
				DateTime validityPeriod = exp.DateTime;

                return validityPeriod;
            }
            //set => _configuration[$"{LICENSE_SECTION}:EncryptedExpiryDate"] = 
            //    value.HasValue ? Encryption.Encrypt(value.Value.ToString("O")) : "";
        }
    }
}