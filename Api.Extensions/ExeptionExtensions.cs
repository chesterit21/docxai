using System.Text.RegularExpressions;

namespace Api.Extensions
{
    public static class ExeptionExtensions
    {
        public static string GetExceptionMessages(this Exception exception)
        {
            if (exception == null)
                return null;

            string innerExceptionMessage = "";

            //if (exception is DbEntityValidationException dbEx) //DbUnexpectedValidationException
            //{
            //    innerExceptionMessage = string.Join("; ", dbEx.EntityValidationErrors
            //        .SelectMany(x => x.ValidationErrors)
            //        .Select(x =>  "Property: " + x.PropertyName + " Error: " + x.ErrorMessage));

            //    //var sb = new StringBuilder();
            //    //foreach (var validationErrors in dbEx.EntityValidationErrors)
            //    //{
            //    //    foreach (var validationError in validationErrors.ValidationErrors)
            //    //    {
            //    //        sb.AppendLine(string.Format("Property: {0} Error: {1}",
            //    //        validationError.PropertyName, validationError.ErrorMessage));
            //    //    }
            //    //}

            //    innerExceptionMessage = sb.ToString();
            //}
            //else
            //{
            while (exception != null)
            {
                innerExceptionMessage += " " + exception.Message;
                exception = exception.InnerException;
            }
            //}

            innerExceptionMessage = Regex.Replace(innerExceptionMessage, @"\t+", " ");
            innerExceptionMessage = Regex.Replace(innerExceptionMessage, @"\s+", " ");

            return innerExceptionMessage.Trim();
        }
    }
}
