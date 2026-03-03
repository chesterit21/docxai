using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Api.Extensions
{
    public static class Encryption
    {
        //128 BIT key
        const string key = "&5hU84$3cretK3Y*";

        public static string Encrypt(this string plainText, string pkey = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(plainText))
                    return null;

                bool hex = true;
                byte[] valueBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                if(!string.IsNullOrEmpty(pkey))
					keyBytes = Encoding.UTF8.GetBytes(pkey);

				using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using ICryptoTransform crypt = aes.CreateEncryptor();
                var cipher = crypt.TransformFinalBlock(valueBytes, 0, valueBytes.Length).AsSpan();
                return hex ?  Convert.ToHexString(cipher).ToLower() : Convert.ToBase64String(cipher);
            }
            catch //(Exception ex)
            {
                return null;
            }
        }

        public static string Decrypt(this string cipherText, string pkey = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cipherText))
                    return null;

                bool hex = true;
                byte[] valueBytes = hex ? Convert.FromHexString(cipherText) : Convert.FromBase64String(cipherText);
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                if (!string.IsNullOrEmpty(pkey))
                {
					keyBytes = Encoding.UTF8.GetBytes(pkey);
				}

                using var aes = Aes.Create();
                aes.Key = keyBytes;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using ICryptoTransform crypt = aes.CreateDecryptor();
                byte[] cipher = crypt.TransformFinalBlock(valueBytes, 0, valueBytes.Length);
                return Encoding.UTF8.GetString(cipher);
            }
            catch //(Exception ex)
            {
                return null;
            }
        }

        public static string HashPassword(this string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return password;

            //var saltBytes = Encoding.UTF8.GetBytes(salt);
            var saltBytes = Convert.FromHexString("70651f0715a042a1");
            return Convert.ToHexString(Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 1000, HashAlgorithmName.SHA512, 32)).ToLower();
        }

        private static TEntity[] ToArray<TEntity>(params TEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
                return entities;

            if (entities.ElementAt(0).GetType() == typeof(List<TEntity>))
            {
                return entities.ToArray();
            }

            return entities;
        }

        public static void MaskPassword<TEntity>(this TEntity[] entities)
        {
            foreach (var entity in ToArray(entities))
            {
                foreach (var property in typeof(TEntity).GetProperties())
                {
                    var currentValue = property.GetValue(entity, null);
                    if (property.Name.Contains("password", StringComparison.OrdinalIgnoreCase) && currentValue is string strCurrentValue && !string.IsNullOrWhiteSpace(strCurrentValue))
                    {
                        currentValue = new string('*', 5);
                        property.SetValue(entity, currentValue, null);
                    }
                }
            }
        }

        public static JsonNode MaskPassword(this JsonNode node, bool descendNested = true)
        {
            if (node is JsonArray array)
            {
                foreach (var item in array)
                {
                    MaskPassword(item);
                }
            }
            else if (node is JsonObject obj)
            {
                // We cannot modify obj while iterating though it, so make a list of replacements to set later
                List<(string Key, string Value)> replacements = default;
                foreach (var property in obj)
                {
                    if (property.Value is JsonValue value)
                    {
                        if (property.Key.Contains("password", StringComparison.OrdinalIgnoreCase)
                            && value.GetValueKind() == JsonValueKind.String)
                        {
                            var strValue = value.GetValue<string>();
                            if (!string.IsNullOrWhiteSpace(strValue))
                            {
                                // TODO: this masking algorithm leaks information, specifically the password length.
                                // Decide whether to replace all passwords with some fixed length string instead.
                                (replacements ??= new()).Add((property.Key, new string('*', 5)));
                            }
                        }
                    }
                    else if (property.Value != null && descendNested)
                    {
                        // If you want to recursively descend the hierarchy, do so here:
                        MaskPassword(property.Value);
                    }
                }
                if (replacements != null)
                    foreach (var item in replacements)
                        obj[item.Key] = item.Value;

            }
            return node;
        }

    }
}
