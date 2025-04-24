using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Api.DataAccess
{
    public class JsonPrimitiveTypeConverter : ValueConverter<List<string>, string>
    {
        //JsonSerializerOptions options = new()
        //{
        //    WriteIndented = true,
        //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        //};
        /// <summary>
        /// Creates a new instance of this converter.
        /// </summary>
        public JsonPrimitiveTypeConverter() : base(
                d => d == null
                ? default
                : JsonSerializer.Serialize(d, new JsonSerializerOptions
                {
                    WriteIndented = true
                }), //Newtonsoft.Json.JsonConvert.SerializeObject(d),
            d => d == null
                ? default
                : JsonSerializer.Deserialize<List<string>>(d, new JsonSerializerOptions
                {
                    WriteIndented = true
                }))//Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(d))
        {
        }
    }
}
