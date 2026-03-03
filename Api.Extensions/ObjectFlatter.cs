using System.Collections;
using System.Linq.Expressions;

namespace Api.Extensions
{
    public static class ObjectFlatter
    {
        public static object Flatten<T>(T obj)
        {
            if (obj is IEnumerable enumerable)
            {
                var flattenedList = new List<object>();
                foreach (var item in enumerable)
                {
                    flattenedList.Add(FlattenObject(item));
                }

                return flattenedList;
            }
            else
            {
                return FlattenObject(obj);
            }
        }

        private static Dictionary<string, object> FlattenObject(object obj)
        {
			var result = new Dictionary<string, object>();
			try
            {
				var properties = obj.GetType().GetProperties();

				foreach (var prop in properties)
				{
					var value = prop.GetValue(obj);

					if (value != null && !IsSimpleType(prop.PropertyType))
					{
						var nestedProperties = FlattenObject(value);
						foreach (var kvp in nestedProperties)
						{
							result[kvp.Key] = kvp.Value;
						}
					}
					else
					{
						result[prop.Name] = value;
					}
				}
			}
            catch (Exception e)
            {

                throw;
            }

            return result;
        }

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(DateTime) || type == typeof(decimal) || type == typeof(Guid);
        }
    }

}
