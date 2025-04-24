using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class FormFileValidationsAttribute(string[] extensions, string maxFileSize) : ValidationAttribute
    {
        //public override bool IsValid(object value)
        //{
        //    return base.IsValid(value);
        //}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file == null)
                throw new ApiException("File was not found"); //return new ValidationResult(ErrorMessage ?? "File was not found");

            var extension = Path.GetExtension(file.FileName);
            if (!extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                throw new ApiException("File extensions are not allowed"); //return new ValidationResult(ErrorMessage ?? "File extensions are not allowed");

            string[] sizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            string numberPart = new string(maxFileSize.TakeWhile(char.IsDigit).Concat(maxFileSize.TakeWhile(c => c == '.' || c == '-')).ToArray());
            string unitPart = maxFileSize.Substring(numberPart.Length).Trim();

            if (!decimal.TryParse(numberPart, out decimal numericValue))
                throw new ApiException("Invalid numeric value in the size string");//return new ValidationResult("Invalid numeric value in the size string.");

            int suffixIndex = Array.FindIndex(sizeSuffixes, suffix => suffix.Equals(unitPart, StringComparison.OrdinalIgnoreCase));
            if (suffixIndex < 0)
                throw new ApiException("Invalid size suffix"); //return new ValidationResult("Invalid size suffix.");

            // Calculate the size in bytes
            var maxSize =(long)(numericValue * (1L << (suffixIndex * 10)));

            if (file.Length > maxSize)
                throw new ApiException("File size exceeded"); //return new ValidationResult("File size exceeded");

            return ValidationResult.Success;
        }
    }
}
