using Microsoft.AspNetCore.Authentication;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Sasl;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.Cms;
using System.Security.Authentication;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Api.Extensions
{
    public static class ActiveDirectory
    {
        static HashSet<string> SearchForGroup(LdapConnection connection, string entryPoint, string searchFilter, string[] requiredAttributes, bool typesOnly)
        {
            var result = connection.Search(entryPoint, LdapConnection.ScopeSub, searchFilter, requiredAttributes, typesOnly);

            var groups = new HashSet<string>();
            foreach (var group in result)
            {
                var attribute = group.GetAttribute("cn");
                groups.Add(attribute.StringValue);
            }

            return groups;
        }

        public static (bool Valid, string Message) Authenticate(string username, string password)
        {
            try
            {
                //JsonElement doc = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("appsettings.json"));
                //var domain = doc.GetElement("ActiveDirectory.Domain").GetString();
                //var address = doc.GetElement("ActiveDirectory.Host").GetString();
                //var port = doc.GetElement("ActiveDirectory.Port").GetInt16();

                var setting = AppSettings.Read();
                var domain = setting.ActiveDirectory.Domain;
                var address = setting.ActiveDirectory.Host;
                var port = setting.ActiveDirectory.Port;


                string userDn = $"{username}@{domain}";

                using var connection = new LdapConnection { SecureSocketLayer = false };

                connection.Connect(address, port);
                connection.Bind(userDn, password);
                return (connection.Bound, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static string GetAttributeValue(this LdapAttributeSet attributeSet, string attributeName)
        {
            try
            {
                return attributeSet.GetAttribute(attributeName)?.StringValue;
            }
            catch
            {
                return null;
            }
        }

        public static string GetAttributeValue(this LdapEntry entry, string attributeName)
        {
            try
            {
                return entry.GetAttribute(attributeName)?.StringValue;
            }
            catch
            {
                return null;
            }
        }

        public static ActiveDirectoryAttribute GetUserAttribute(string username, string password)
        {
            //string[] attributes = new string[] { "cn", "userPrincipalName", "st", "givenname", "samaccountname","description", "telephonenumber", "department", "displayname", "name", "mail", "givenName", "sn" };

            //JsonElement doc = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("appsettings.json"));
            //var domain = doc.GetElement("ActiveDirectory.Domain").GetString();
            //var address = doc.GetElement("ActiveDirectory.Host").GetString();
            //var port = doc.GetElement("ActiveDirectory.Port").GetInt16();
            var setting = AppSettings.Read();
            var domain = setting.ActiveDirectory.Domain;
            var address = setting.ActiveDirectory.Host;
            var port = setting.ActiveDirectory.Port;


            // CN = Common Name
            // OU = Organizational Unit
            // DC = Domain Component

            var dom = domain.Split('.')[0];
            var tld = domain.Split(".")[1];

            string searchBase = $"CN=Users,DC={dom},DC={tld}";
            //string searchFilter = "(&(objectClass=user)(objectCategory=person))";
            string searchFilter = $"(samaccountname=*{username}*)";

            string loginDn = $"{username}@{domain}";
            string[] attributes = [ "cn", "userPrincipalName", "st", "givenname", "samaccountname", "description", "telephonenumber", "department",
                "displayname", "name", "mail", "givenName", "distinguishedName", "sAMAccountName",
                "sn", "mailNickname", "memberOf", "homeDirectory", "msExchUserCulture" ];

            using var con = new LdapConnection();

            con.Connect(address, port);
            con.Bind(loginDn, password);

            if (con.Bound == false)
                return null;

            LdapSearchQueue queue = con.Search(
                searchBase,
                LdapConnection.ScopeSub,
                searchFilter,
                attributes,
                false,
                null,
                null);

            LdapMessage message;

            while ((message = queue.GetResponse()) != null)
            {
                if (message is LdapSearchResult)
                {
                    LdapEntry entry = ((LdapSearchResult)message).Entry;

                    LdapAttributeSet attributeSet = entry.GetAttributeSet();

                    return new ActiveDirectoryAttribute
                    {
                        Cn = attributeSet.GetAttributeValue("cn"),
                        UserPrincipalName = attributeSet.GetAttributeValue("userPrincipalName"),
                        St = attributeSet.GetAttributeValue("st"),
                        GivenName = attributeSet.GetAttributeValue("givenname") ?? attributeSet.GetAttributeValue("givenName"),
                        SamAccountName = attributeSet.GetAttributeValue("samaccountname"),
                        Description = attributeSet.GetAttributeValue("description"),
                        TelephoneNumber = attributeSet.GetAttributeValue("telephonenumber"),
                        Department = attributeSet.GetAttributeValue("department"),
                        DisplayName = attributeSet.GetAttributeValue("displayname"),
                        Name = attributeSet.GetAttributeValue("name"),
                        Mail = attributeSet.GetAttributeValue("mail"),
                        Sn = attributeSet.GetAttributeValue("sn")
                    };
                }
            }

            return null;
        }

        public static bool CheckIfUserExists(string username)
        {
            var setting = AppSettings.Read();
            var domain = setting.ActiveDirectory.Domain;
            var address = setting.ActiveDirectory.Host;
            var port = setting.ActiveDirectory.Port;
            var usr = setting.ActiveDirectory.User;
            var pwd = setting.ActiveDirectory.Password.Decrypt();

            var dom = domain.Split('.')[0];
            var tld = domain.Split(".")[1];

            string searchBase = $"CN=Users,DC={dom},DC={tld}"; //$"dc={dom},dc=com"
            string searchFilter = $"(samaccountname=*{username}*)";

            string loginDn = $"{usr}@{domain}";

            using (var connection = new LdapConnection())
            {
                connection.Connect(address, port);
                connection.Bind(loginDn, pwd);

                if (connection.Bound == false)
                    throw new InvalidCredentialException("The username or password is incorrect");

                LdapSearchConstraints cons = connection.SearchConstraints;
                cons.ReferralFollowing = true;
                connection.Constraints = cons;

                // To enable referral following, use LDAPConstraints.setReferralFollowing passing TRUE to enable referrals, or FALSE(default) to disable referrals.

                var searchResults = connection.Search(
                    searchBase,
                    LdapConnection.ScopeSub,
                    searchFilter,
                    null,
                    false,
                    (LdapSearchConstraints)null);

                if (searchResults.HasMore())
                {
                    var entry = searchResults.Next();

                    return entry != null && (entry.GetAttribute("sAMAccountName")?.StringValue?.Equals(username, StringComparison.OrdinalIgnoreCase) ?? false);
                }

                return false;
            }
        }

        public static ActiveDirectoryAttribute QueryUser(string username)
        {
            var setting = AppSettings.Read();
            var domain = setting.ActiveDirectory.Domain;
            var address = setting.ActiveDirectory.Host;
            var port = setting.ActiveDirectory.Port;
            var usr = setting.ActiveDirectory.User;
            var pwd = setting.ActiveDirectory.Password.Decrypt();

            var dom = domain.Split('.')[0];
            var tld = domain.Split(".")[1];

            string searchBase = $"CN=Users,DC={dom},DC={tld}"; //$"dc={dom},dc=com"
            string searchFilter = $"(samaccountname=*{username}*)";

            string loginDn = $"{usr}@{domain}";

            using var connection = new LdapConnection();

            connection.Connect(address, port);
            connection.Bind(loginDn, pwd);

            if (connection.Bound == false)
                throw new InvalidCredentialException("The username or password is incorrect");

            LdapSearchConstraints cons = connection.SearchConstraints;
            cons.ReferralFollowing = true;
            connection.Constraints = cons;

            // To enable referral following, use LDAPConstraints.setReferralFollowing passing TRUE to enable referrals, or FALSE(default) to disable referrals.

            var searchResults = connection.Search(
                searchBase,
                LdapConnection.ScopeSub,
                searchFilter,
                null,
                false,
                (LdapSearchConstraints)null);

            if (searchResults.HasMore())
            {
                var entry = searchResults.Next();

                return new ActiveDirectoryAttribute
                {
                    Cn = entry.GetAttributeValue("cn"),
                    UserPrincipalName = entry.GetAttributeValue("userPrincipalName"),
                    St = entry.GetAttributeValue("st"),
                    GivenName = entry.GetAttributeValue("givenname") ?? entry.GetAttributeValue("givenName"),
                    SamAccountName = entry.GetAttributeValue("samaccountname"),
                    Description = entry.GetAttributeValue("description"),
                    TelephoneNumber = entry.GetAttributeValue("telephonenumber"),
                    Department = entry.GetAttributeValue("department"),
                    DisplayName = entry.GetAttributeValue("displayname"),
                    Name = entry.GetAttributeValue("name"),
                    Mail = entry.GetAttributeValue("mail"),
                    Sn = entry.GetAttributeValue("sn")
                };

            }

            return null;

        }


        public class ActiveDirectoryAttribute
        {
            public string Cn { get; set; }
            public string UserPrincipalName { get; set; }
            public string St { get; set; }
            public string GivenName { get; set; }
            public string SamAccountName { get; set; }
            public string Description { get; set; }
            public string TelephoneNumber { get; set; }
            public string Department { get; set; }
            public string DisplayName { get; set; }
            public string Name { get; set; }
            public string Mail { get; set; }
            public string Sn { get; set; }
        }


        ////if using System.DirectoryServices.AccountManagement via NUGET and working on windows only
        //public static bool AuthenticateWindows(string username, string password)
        //{
        //    if (OperatingSystem.IsWindows())
        //    {
        //        var setting = AppSettings.Read();
        //        using (var adContext = new PrincipalContext(ContextType.Domain, setting.ActiveDirectory.Host))
        //        {
        //            var result = adContext.ValidateCredentials(username, password);
        //            return result;
        //        }
        //    }

        //    return false;
        //}

        //public static bool CheckIfUserExistsWindows(string username)
        //{
        //    if (!OperatingSystem.IsWindows())
        //        return false;

        //    var setting = AppSettings.Read();
        //    var address = setting.ActiveDirectory.Host;

        //    var usr = setting.ActiveDirectory.AdminUser;
        //    var pwd = setting.ActiveDirectory.AdminPassword.Decrypt();

        //    using (var context = new PrincipalContext(ContextType.Domain, address, usr, pwd))
        //    {
        //        using (var user = UserPrincipal.FindByIdentity(context, username))
        //        {
        //            return user != null;
        //        }
        //    }
        //}
    }
}
