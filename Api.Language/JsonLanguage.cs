using System.Reflection;
using System.Text.Json;

namespace Api.Language
{
    public class JsonLanguage
    {
        static string pathLang = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lang.json");
        static string pathMenu = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "lang_menu.json");

        public string Code { get; set; }
        public string Id { get; set; }
        public string En { get; set; }

        #region LANG
        public static List<JsonLanguage> GetLanguages()
        {
            //var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
            if (!File.Exists(pathLang))
            {
                return [];
            }

            var json = File.ReadAllText(pathLang);
            return JsonSerializer.Deserialize<List<JsonLanguage>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public static JsonLanguage GetLanguage(string code)
        {
            var languages = GetLanguages();
            return languages?.FirstOrDefault(x => x.Code == code);
        }

        public static void UpdateLanguage(string code, string id, string en)
        {
            var languages = GetLanguages();
            var language = languages?.FirstOrDefault(y => y.Code == code);
            if (language == null)
            {
                languages.Add(new JsonLanguage { Code = code, Id = id, En = en });
            }
            else
            {
                language.Id = id;
                language.En = en;
            }

            languages = languages.OrderBy(y => y.Code).ToList();

            var json = JsonSerializer.Serialize(languages);
            File.WriteAllText(pathLang, json);
        }

        public static void UpdateLanguage(List<JsonLanguage> jsonLanguages)
        {
            var languages = GetLanguages();

            foreach (var lang in jsonLanguages)
            {
                var language = languages?.FirstOrDefault(y => y.Code == lang.Code);
                if (language == null)
                {
                    languages.Add(new JsonLanguage { Code = lang.Code, Id = lang.Id, En = lang.En });
                }
                else
                {
                    language.Id = lang.Id;
                    language.En = lang.En;
                }
            }

            languages = languages.OrderBy(y => y.Code).ToList();

            var json = JsonSerializer.Serialize(languages);
            File.WriteAllText(pathLang, json);
        }
        #endregion

        #region MENU
        public static List<JsonLanguage> GetMenus()
        {
            //var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory);
            if (!File.Exists(pathMenu))
            {
                return [];
            }

            var json = File.ReadAllText(pathMenu);
            return JsonSerializer.Deserialize<List<JsonLanguage>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public static JsonLanguage GetMenu(string code)
        {
            var languages = GetMenus();
            return languages?.FirstOrDefault(x => x.Code == code);
        }

        public static void UpdateMenu(string code, string id, string en)
        {
            var languages = GetMenus();
            var language = languages?.FirstOrDefault(y => y.Code == code);
            if (language == null)
            {
                languages.Add(new JsonLanguage { Code = code, Id = id, En = en });
            }
            else
            {
                language.Id = id;
                language.En = en;
            }

            var json = JsonSerializer.Serialize(languages);
            File.WriteAllText(pathMenu, json);
        }

        public static void UpdateMenu(List<JsonLanguage> jsonLanguages)
        {
            var languages = GetMenus();

            foreach (var lang in jsonLanguages)
            {
                var language = languages?.FirstOrDefault(y => y.Code == lang.Code);
                if (language == null)
                {
                    languages.Add(new JsonLanguage { Code = lang.Code, Id = lang.Id, En = lang.En });
                }
                else
                {
                    language.Id = lang.Id;
                    language.En = lang.En;
                }
            }

            languages = languages.OrderBy(y => y.Code).ToList();

            var json = JsonSerializer.Serialize(languages);
            File.WriteAllText(pathMenu, json);
        }
        #endregion

    }
}
