namespace Api.Extensions
{
    public static class StringExtensions
    {
        public static int ToInt32(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            if (int.TryParse(value, out var result))
                return result;

            return 0;
        }

        public static DateTime? ToDateTime(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value, out var result))
                return result;

            return null;
        }

        public static string ConvertToStringTime(this double value)
        {
            int hours = (int)value / 3600;
            int minutes = ((int)value % 3600) / 60;
            int seconds = (int)value % 60;

            return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }
    }
}
