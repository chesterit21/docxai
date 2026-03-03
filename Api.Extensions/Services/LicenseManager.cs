using Microsoft.Extensions.Configuration;
using NPOI.POIFS.Crypt;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace Api.Extensions.Services
{
    public interface ILicenseManager
    {
        bool ValidateUserCount(int currentCount);
        int GetRemainingLicenses(int currentCount);
        bool IsLicenseValid();
        LicenseInfo GetLicenseInfo();
    }

    public class LicenseManager : ILicenseManager
    {
        private readonly LicenseSettings _licenseSettings;
        private static readonly ReaderWriterLockSlim _cacheLock = new();

        public LicenseManager(IConfiguration configuration)
        {
            _licenseSettings = new LicenseSettings(configuration);
        }

        public bool ValidateUserCount(int currentCount)
        {
            try
            {
                _cacheLock.EnterReadLock();
                // Get all values under single read lock
                var licenseKey = _licenseSettings.License;
				//var decryptedLicenseObj = Encryption.Decrypt(licenseKey);
				var d = JsonSerializer.Deserialize<JsonObject>(licenseKey);

				DateTimeOffset exp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(d["ValidityPeriod"].ToString()));
				DateTime validityPeriod = exp.DateTime;
				var now = DateTime.Now;

				if (now > validityPeriod)
				{
                    return false; // License has expired
				}

				var maxUsers = int.Parse(d["UserCount"].ToString());
                
                // Perform validation with obtained values
                if (string.IsNullOrEmpty(licenseKey))
                    return false;

                return currentCount < maxUsers;
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        public int GetRemainingLicenses(int currentCount)
        {
            try
            {
                _cacheLock.EnterReadLock();
				var licenseKey = _licenseSettings.License;
				var decryptedLicenseObj = Encryption.Decrypt(licenseKey);
				var d = JsonSerializer.Deserialize<JsonObject>(decryptedLicenseObj);
				var maxUsers = int.Parse(d["UserCount"].ToString());

				return Math.Max(0, maxUsers - currentCount);
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        public bool IsLicenseValid()
        {
            try
            {
                _cacheLock.EnterReadLock();
				return IsLicenseValid_NoLock();
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

		private bool IsLicenseValid_NoLock()
		{
			var licenseKey = _licenseSettings.License;
			if (string.IsNullOrEmpty(licenseKey))
				return false;

			var d = JsonSerializer.Deserialize<JsonObject>(licenseKey);

			DateTimeOffset exp = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(d["ValidityPeriod"].ToString()));
			DateTime validityPeriod = exp.DateTime;
			var now = DateTime.Now;

			if (now > validityPeriod)
			{
				return false; // License has expired
			}

			return true;
		}

		public LicenseInfo GetLicenseInfo()
        {
            try
            {
                _cacheLock.EnterReadLock();

				return new LicenseInfo
                {
                    MaxUserCount = _licenseSettings.MaxUserCount,
                    ExpiryDate = _licenseSettings.ExpiryDate,
                    IsValid = IsLicenseValid_NoLock()
				};
            }
            finally
            {
				_cacheLock.ExitReadLock();
			}
        }
    }

    public class LicenseInfo
    {
        public int MaxUserCount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsValid { get; set; }
    }
}