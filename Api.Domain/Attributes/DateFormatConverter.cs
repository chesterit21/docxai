using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateFormatConverterAttribute : JsonConverterAttribute
    {
        string readFormat;
        string writeFormat;
        public DateFormatConverterAttribute(string readFormat = null, string writeFormat = null)
        {
            this.readFormat = readFormat;
            this.writeFormat = writeFormat;
        }

        public override JsonConverter CreateConverter(Type typeToConvert)
        {
            if (typeToConvert != typeof(DateTime) && typeToConvert != typeof(DateTime?))
            {
                throw new ArgumentException(
                    $"This converter only works with datetime, and it was provided {typeToConvert.Name}.");
            }

            if (typeToConvert == typeof(DateTime))
                return new DateFormatConverter(readFormat, writeFormat);

            return new DateFormatConverterNullable(readFormat, writeFormat);
        }
    }

    public class DateFormatConverter : JsonConverter<DateTime>
    {
        string readFormat;
        string writeFormat;
        public DateFormatConverter(string readFormat = null, string writeFormat = null)
        {
            this.readFormat = readFormat;
            this.writeFormat = writeFormat;
        }

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateValue = reader.GetString();
            if (dateValue == null)
                return DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(readFormat))
            {
                if (DateTime.TryParse(dateValue, out DateTime result))
                    return result;

                if (reader.TryGetDateTime(out DateTime result2))
                    return result2;
            }
            else
            {
                if (DateTime.TryParseExact(dateValue, readFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;
            }

            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime dateTimeValue, JsonSerializerOptions options)
        {
            var format = string.IsNullOrWhiteSpace(writeFormat) ? "yyyy-MM-ddTHH:mm:ss.fffZ" : writeFormat;
            writer.WriteStringValue(dateTimeValue.ToString(format, CultureInfo.InvariantCulture));
        }
    }

    public class DateFormatConverterNullable : JsonConverter<DateTime?>
    {
        string readFormat;
        string writeFormat;
        public DateFormatConverterNullable(string readFormat = null, string writeFormat = null)
        {
            this.readFormat = readFormat;
            this.writeFormat = writeFormat;
        }

        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            //return reader.GetDateTime();

            var dateValue = reader.GetString();
            if (dateValue == null)
                return null;

            if (string.IsNullOrWhiteSpace(readFormat))
            {
                if (DateTime.TryParse(dateValue, out DateTime result))
                    return result;

                if (reader.TryGetDateTime(out DateTime result2))
                    return result2;
            }
            else
            {
                if (DateTime.TryParseExact(dateValue, readFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, DateTime? dateTimeValue, JsonSerializerOptions options)
        {
            if (dateTimeValue == null)
                return;

            var format = string.IsNullOrWhiteSpace(writeFormat) ? "yyyy-MM-ddTHH:mm:ss.fffZ" : writeFormat;
            writer.WriteStringValue(dateTimeValue.Value.ToString(format, CultureInfo.InvariantCulture));
        }
    }

}