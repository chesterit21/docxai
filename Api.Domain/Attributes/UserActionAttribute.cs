namespace Api.Domain.Attributes
{
    public enum UserAction
    {
        Insert, Update, Delete, Read
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class UserActionAttribute(UserAction action) : Attribute
    {
        public UserAction Action { get; } = action;
    }
}
