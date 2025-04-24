using ProtoBuf;
using System.Collections;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Api.Extensions
{
    public static class HttpClientExtensions
    {
        #region PRIVATE METHOD
        private static JsonSerializerOptions options => new()
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        private static bool ValidJson(string json)
        {
            json = json.Trim();
            return json.StartsWith("{") || json.StartsWith("[");
        }

        private static bool IsDictionary(object o)
        {
            if (o == null) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }

        private static bool IsKeyValuePair(object o)
        {
            if (o == null) return false;
            return o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(KeyValuePair<,>));
        }

        private static string GetPropertyValue(PropertyInfo prop, object source)
        {
            if (prop != null && prop.CanRead)
            {
                var value = prop.GetValue(source, null);
                if (value != null || string.IsNullOrWhiteSpace(value?.ToString()))
                    return null;

                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (type == typeof(DateTime) || type == typeof(DateTime?))
                    return Convert.ToDateTime(value).ToString("yyyy-MM-ddHH:mm:ss");

                return value?.ToString();
            }

            return null;
        }
        #endregion

        #region GENERAL
        public static async Task<T> CreateResponseAsync<T>(this HttpResponseMessage response, string nodeName = null)
        {
            try
            {
                Type type = typeof(T);

                byte[] contentBytes = Array.Empty<byte>(); //default(byte[]);

                if (response.Content.Headers.ContentEncoding.Contains("br", StringComparer.OrdinalIgnoreCase))
                {
                    using BrotliStream bs = new(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress);
                    using MemoryStream msOutput = new();

                    bs.CopyTo(msOutput);
                    msOutput.Seek(0, System.IO.SeekOrigin.Begin);
                    using StreamReader reader = new(msOutput);
                    //json = reader.ReadToEnd();

                    using (var memstream = new MemoryStream())
                    {
                        reader.BaseStream.CopyTo(memstream);
                        contentBytes = memstream.ToArray();
                    }
                }
                else
                {
                    contentBytes = await response.Content.ReadAsByteArrayAsync();
                }

                var contentType = response.Content.Headers?.ContentType?.MediaType?.ToLower();

                if (contentType == "application/proto" || contentType == "application/x-protobuf")
                {
                    using var stream = new MemoryStream(contentBytes);
                    return Serializer.Deserialize<T>(stream);
                }

                if (contentType == "application/xml" || contentType == "text/xml")
                {
                    var serializer = new XmlSerializer(type);
                    using var stringReader = new StringReader(Encoding.UTF8.GetString(contentBytes));
                    using var xmlTextReader = new XmlTextReader(stringReader)
                    {
                        Normalization = false
                    };

                    return (T)serializer.Deserialize(xmlTextReader);
                }

                if (contentType == "application/json" || contentType == "text/json")
                {
                    string text = Encoding.UTF8.GetString(contentBytes);

                    if (!string.IsNullOrWhiteSpace(nodeName))
                    {
                        text = JsonObject.Parse(text)?.GetNode(nodeName)?.ToJsonString() ?? text;
                    }

                    if (type.IsExactPrimitive())
                    {
                        var t = (T)Convert.ChangeType(text, type);
                        return t;
                    }

                    if (ValidJson(text))
                    {
                        return JsonSerializer.Deserialize<T>(text, options); //JsonConvert.DeserializeObject<T>(json);
                    }
                    else
                    {
                        return default;
                        //return Activator.CreateInstance<T>();
                    }
                }

                return default;
            }
            finally
            {
                response.Dispose();
            }
        }

        public static async Task<T> GetAsync<T>(this HttpClient client, string url, string nodeName = null) =>
            await SendAsync<T>(client, HttpMethod.Get, url, null, nodeName);

        public static async Task<T> SendAsync<T>(this HttpClient client, HttpMethod method, string url, HttpContent content, string nodeName = null)
        {
            using var request = new HttpRequestMessage(method, url);

            if (content != null)
                request.Content = content;

            var r = await client.SendAsync(request);

            content?.Dispose();

            return await CreateResponseAsync<T>(r, nodeName);
        }
        #endregion

        #region PROTOCOL BUFFER CONTENT
        public static async Task<T> PostAsProtoAsync<T>(this HttpClient client, string url, object request, string nodeName = null)
        {
            using var content = CreateProtoContent(request);
            return await client.SendAsync<T>(HttpMethod.Post, url, content, nodeName);
        }

        public static async Task<T> PutAsProtoAsync<T>(this HttpClient client, string url, object request, string nodeName = null)
        {
            using var content = CreateProtoContent(request);
            return await client.SendAsync<T>(HttpMethod.Put, url, content, nodeName);
        }

        public static async Task<T> PatchAsProtoAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateProtoContent(request);
            return await client.SendAsync<T>(HttpMethod.Patch, url, content, nodeName);
        }

        public static async Task<T> DeleteAsProtoAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateProtoContent(request);
            return await SendAsync<T>(client, HttpMethod.Delete, url, content, nodeName);
        }

        private static ByteArrayContent CreateProtoContent(object request)
        {
            if (request == null)
                return null;

            if (request is HttpContent)
            {
                var arrContent = request as ByteArrayContent;
                arrContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
                return arrContent;
            }

            using var content = new ByteArrayContent(request.SerializeProtobuf());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");
            return content;
        }

        #endregion

        #region STRING CONTENT
        public static async Task<T> PostAsStringAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateStringContent(request);
            return await SendAsync<T>(client, HttpMethod.Post, url, content, nodeName);
        }

        public static async Task<T> PutAsStringAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateStringContent(request);
            return await SendAsync<T>(client, HttpMethod.Put, url, content, nodeName);
        }

        public static async Task<T> PatchAsStringAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateStringContent(request);
            return await client.SendAsync<T>(HttpMethod.Patch, url, content, nodeName);
        }

        public static async Task<T> DeleteAsStringAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateStringContent(request);
            return await SendAsync<T>(client, HttpMethod.Delete, url, content, nodeName);
        }

        private static StringContent CreateStringContent(object request)
        {
            if (request == null)
                return null;

            if (request is HttpContent)
                return request as StringContent;

            var content = new StringContent(request?.ToString(), Encoding.UTF8, "text/plain");
            content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            //content.Headers.ContentType.CharSet = string.Empty;
            return content;
        }
        #endregion

        #region JSON CONTENT
        public static async Task<T> PostAsJsonAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateJsonContent(request);
            return await SendAsync<T>(client, HttpMethod.Post, url, content, nodeName);
        }

        public static async Task<T> PutAsJsonAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateJsonContent(request);
            return await SendAsync<T>(client, HttpMethod.Put, url, content, nodeName);
        }

        public static async Task<T> PatchAsJsonAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateJsonContent(request);
            return await client.SendAsync<T>(HttpMethod.Patch, url, content, nodeName);
        }

        public static async Task<T> DeleteAsJsonAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateJsonContent(request);
            return await SendAsync<T>(client, HttpMethod.Delete, url, content, nodeName);
        }

        private static StringContent CreateJsonContent(object request)
        {
            if (request == null)
                return null;

            if (request is HttpContent)
                return request as StringContent;

            if (request is string json)
            {
                json = request.ToString();
            }
            else
            {
                //var jsonSettings = new JsonSerializerSettings();
                //jsonSettings.DateFormatString = "yyyy-MM-dd";
                //json = JsonConvert.SerializeObject(request, jsonSettings);

                json = JsonSerializer.Serialize(request, options);
            }

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentType.CharSet = "UTF-8";
            return content;
        }
        #endregion

        #region FORM CONTENT
        public static async Task<T> PostAsFormAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateFormContent(request);
            return await SendAsync<T>(client, HttpMethod.Post, url, content, nodeName);
        }

        public static async Task<T> PutAsFormAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateFormContent(request);
            return await SendAsync<T>(client, HttpMethod.Put, url, content, nodeName);
        }

        public static async Task<T> PatchAsFormAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateFormContent(request);
            return await client.SendAsync<T>(HttpMethod.Patch, url, content, nodeName);
        }

        public static async Task<T> DeleteAsFormAsync<T>(this HttpClient client, string url, object request = null, string nodeName = null)
        {
            using var content = CreateFormContent(request);
            return await SendAsync<T>(client, HttpMethod.Delete, url, content, nodeName);
        }

        private static FormUrlEncodedContent CreateFormContent(object request)
        {
            if (request == null)
                return null;

            if (request is HttpContent)
            {
                return request as FormUrlEncodedContent;
            }
            else if (request is KeyValuePair<string, string>)
            {
                var dict = request.GetType().GetProperties().ToDictionary(property => property.Name, property => property.GetValue(request)?.ToString());
                return new FormUrlEncodedContent(dict);
            }
            else if (request is Dictionary<string, string>)
            {
                var dict = request as Dictionary<string, string>;
                return new FormUrlEncodedContent(dict);
            }
            else
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(JsonSerializer.Serialize(request));
                //var dict = request.GetType()
                //    .GetProperties()
                //    .ToDictionary(property => property.Name, property => GetPropertyValue(property, request));
                return new FormUrlEncodedContent(dict);
            }
        }
        #endregion
    }
}
