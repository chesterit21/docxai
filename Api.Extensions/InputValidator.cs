using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Api.Extensions
{
    public static class InputValidator
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return Regex.IsMatch(email, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
        }

        public static bool HasEmptyString(this string[] input)
        {
            if (input == null || input.Length == 0)
                return true;

            foreach (var inputItem in input)
            {
                if (string.IsNullOrWhiteSpace(inputItem))
                    return true;
            }

            return false;
        }

        public static bool HasNullProperty<T>(this T entity) where T : class
        {
            foreach (var property in entity.GetType().GetProperties())
            {
                if (!property.CanRead)
                    continue;

                var value = property.GetValue(entity, null);
                if (value == null || (value is string str && string.IsNullOrWhiteSpace(str)))
                    return true;
            }

            return false;
        }

        public static bool HasEmptyRequiredProperty(this object entity)
        {
            var type = entity.GetType();
            if (type.IsExactPrimitive())
            {
                if (entity == null) return true;

                if (entity is string str && string.IsNullOrWhiteSpace(str))
                    return true;

                if (double.TryParse(entity?.ToString(), out double dblvalue) && dblvalue == 0)
                    return true;

                return false;
            }

            if (type.IsGenericEnumerable())
            {
                var list_source = (IList)entity;

                if (list_source.Count == 0)
                    return true;

                foreach (var src in list_source)
                {
                    var empty = HasEmptyRequiredProperty(src);
                    if (empty)
                        return true;
                }
            }
            else
            {
                foreach (var property in entity.GetType().GetProperties())
                {
                    if (property == null)
                        continue;

                    if (!property.CanRead)
                        continue;

                    if (!Attribute.IsDefined(property, typeof(RequiredAttribute)))
                        continue;

                    var value = property.GetValue(entity, null);
                    if (value == null)
                        return true;

                    if (value is string strval && string.IsNullOrWhiteSpace(strval))
                        return true;

                    if (property.PropertyType.IsGenericEnumerable() && value is IList ilist && ilist.Count == 0)
                        return true;

                    if (property.PropertyType.IsGenericEnumerable())
                    {
                        var propType = property.PropertyType.GetGenericArguments().Single();
                        //if (!propType.IsExactPrimitive())
                        //{
                        var empty = HasEmptyRequiredProperty(value);
                        if (empty) return true;
                        //}
                    }
                }
            }

            return false;
        }

        public static (bool strong, string reason) IsStrongPassword(string password)
        {
            if (password.Length < 8 || password.Length > 24)
                return (false, "pass-length");

            if (password.Count(char.IsDigit) < 2)
                return (false, "pass-number");

            if (password.Count(char.IsUpper) < 2)
                return (false, "pass-upper-case");

            var repeatCount = 0;
            var lastChar = '\0';
            foreach (var c in password)
            {
                if (c == lastChar)
                    repeatCount++;
                else
                    repeatCount = 0;
                if (repeatCount >= 3)
                    return (false, "pass-repetitive");

                lastChar = c;
            }

            return (true, null);
        }
    }
}
