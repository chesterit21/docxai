namespace Api.Domain.Attributes
{
    public enum ActionFlag
    {
        Insert,
        Update,
        Delete,
        Read
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ActionFlagAttribute : Attribute
    {
        public ActionFlagAttribute(ActionFlag flag)
        {
            ActionFlag = flag;
        }

        public ActionFlag ActionFlag { get; }
    }
}
