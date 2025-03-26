using System.Collections;
using System.Reflection;

namespace Api.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsExactPrimitive(this Type type)
        {
            return type == typeof(decimal) || type == typeof(DateTime) || type == typeof(string) || type.IsPrimitive || type.IsEnum;
        }

        public static bool IsGenericEnumerable(this Type type) =>
             typeof(IEnumerable<>).IsAssignableFrom(type)
             || typeof(IEnumerable<object>).IsAssignableFrom(type)
             || (typeof(IEnumerable<char>).IsAssignableFrom(type) && type != typeof(string))
             || typeof(IEnumerable<byte>).IsAssignableFrom(type)
             || typeof(IEnumerable<sbyte>).IsAssignableFrom(type)
             || typeof(IEnumerable<ushort>).IsAssignableFrom(type)
             || typeof(IEnumerable<short>).IsAssignableFrom(type)
             || typeof(IEnumerable<uint>).IsAssignableFrom(type)
             || typeof(IEnumerable<int>).IsAssignableFrom(type)
             || typeof(IEnumerable<ulong>).IsAssignableFrom(type)
             || typeof(IEnumerable<long>).IsAssignableFrom(type)
             || typeof(IEnumerable<float>).IsAssignableFrom(type)
             || typeof(IEnumerable<double>).IsAssignableFrom(type)
             || typeof(IEnumerable<decimal>).IsAssignableFrom(type)
             || typeof(IEnumerable<DateTime>).IsAssignableFrom(type)


             || typeof(IEnumerable<object>).IsAssignableFrom(type)
             || (typeof(IEnumerable<char?>).IsAssignableFrom(type) && type != typeof(string))
             || typeof(IEnumerable<byte?>).IsAssignableFrom(type)
             || typeof(IEnumerable<sbyte?>).IsAssignableFrom(type)
             || typeof(IEnumerable<ushort?>).IsAssignableFrom(type)
             || typeof(IEnumerable<short?>).IsAssignableFrom(type)
             || typeof(IEnumerable<uint?>).IsAssignableFrom(type)
             || typeof(IEnumerable<int?>).IsAssignableFrom(type)
             || typeof(IEnumerable<ulong?>).IsAssignableFrom(type)
             || typeof(IEnumerable<long?>).IsAssignableFrom(type)
             || typeof(IEnumerable<float?>).IsAssignableFrom(type)
             || typeof(IEnumerable<double?>).IsAssignableFrom(type)
             || typeof(IEnumerable<decimal?>).IsAssignableFrom(type)
             || typeof(IEnumerable<DateTime?>).IsAssignableFrom(type)
            ;

        public static byte[] ToArray(this Stream sourceStream)
        {
            using var memoryStream = new MemoryStream();
            sourceStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        public static void SetPropertyValue(this object obj, string propertyName, object propertyValue)
        {
            var bindingFlag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
            var info = obj.GetType().GetProperty(propertyName, bindingFlag);
            if (info != null)
            {
                propertyValue = Convert.ChangeType(propertyValue, info.PropertyType);
                if (info.CanWrite)
                    info.SetValue(obj, propertyValue, null);
            }
            else
            {
                var field = obj.GetType().GetField(propertyName, bindingFlag);
                if (field != null)
                {
                    propertyValue = Convert.ChangeType(propertyValue, field.FieldType);
                    field.SetValue(obj, propertyValue);
                }
            }
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var bindingFlag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
            object retval = null;
            var prop = obj.GetType().GetProperty(propertyName, bindingFlag);
            if (prop != null && prop.CanRead)
                retval = prop.GetValue(obj, null);

            if (retval == null)
                retval = obj.GetType().GetField(propertyName, bindingFlag)?.GetValue(obj);

            return retval;
        }

        /// <summary>
        /// Copy property values from one object to another.
        /// </summary>
        /// <typeparam name="T">Destination type.</typeparam>
        /// <param name="source">Source object.</param>
        /// <returns>New object with copied values.</returns>
        public static T CopyProperties<T>(this object source) where T : class
        {
            if (typeof(T).IsExactPrimitive())
                return default;

            return (T)source.CopyProperties(typeof(T));
        }

        /// <summary>
        /// Copy property values from one object to another.
        /// </summary>
        /// <param name="source">Source object.</param>
        /// <param name="type">Destination type.</param>
        /// <returns>New object with copied values.</returns>
        /// <exception cref="InvalidCastException">Exception will be thrown if the data type is not matched.</exception>
        public static object CopyProperties(this object source, Type type)
        {
            if (type.IsExactPrimitive())
                return default;

            var isCollectionDestType = type.IsGenericEnumerable();
            var isCollectionSourceType = source.GetType().IsGenericEnumerable();
            var valid = isCollectionDestType && isCollectionSourceType;

            if (isCollectionDestType || isCollectionSourceType)
            {
                if (!valid)
                    throw new InvalidCastException("Cannot copy non collection object to an object collection");

                var return_type = type.GetGenericArguments().Single();
                var return_list_type = typeof(List<>).MakeGenericType(return_type);
                var return_list = (IList)Activator.CreateInstance(return_list_type);

                var list_source = (IList)source;
                foreach (var src in list_source)
                {
                    var o = src.CopyProperties(return_type);
                    return_list.Add(o);
                }

                return return_list;
                //var instance1 = Activator.CreateInstance(constructedListType);
            }

            var instance = Activator.CreateInstance(type);
            foreach (var sourceProperty in source.GetType().GetProperties())
            {
                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
                PropertyInfo destProperty = type.GetProperty(sourceProperty.Name, flags);
                if (destProperty == null || !destProperty.CanWrite || !sourceProperty.CanRead)
                    continue;

                object value = sourceProperty.GetValue(source, null);
                if (value == null)
                    continue;

                var p = value?.GetType();
                if (p.IsClass)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(p) && p != typeof(string))
                    {
                        destProperty.SetValue(instance, value, null);
                        continue;
                    }

                    if (!p.IsExactPrimitive() && !p.IsValueType)
                    {
                        var objProp = value.CopyProperties(destProperty.PropertyType);
                        destProperty.SetValue(instance, objProp, null);
                    }
                }

                Type sourcePropertyType = Nullable.GetUnderlyingType(sourceProperty.PropertyType) ?? sourceProperty.PropertyType;
                Type destPropertyType = Nullable.GetUnderlyingType(destProperty.PropertyType) ?? destProperty.PropertyType;

                if (value is string)
                    value = Convert.ToString(value).Trim();

                value = value == null ? null : ConvertChangeType(value, destPropertyType);

                if (value == null && Nullable.GetUnderlyingType(sourceProperty.PropertyType) == null)
                    continue;

                destProperty.SetValue(instance, value, null);
            }

            return instance;
        }

        private static object ConvertChangeType(object value, Type toType)
        {
            try
            {
                return Convert.ChangeType(value, toType);
            }
            catch
            {
                return null;
            }
        }
    }
}
