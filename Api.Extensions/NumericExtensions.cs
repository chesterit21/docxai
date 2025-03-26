namespace Api.Extensions
{
    public static class NumericExtensions
    {
        private static readonly HashSet<Type> NumericTypes = new()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(decimal),
            typeof(double),
            typeof(float)
        };

        public static bool IsNumericType(this Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return NumericTypes.Contains(type) || NumericTypes.Contains(underlyingType);
        }
    }
}
