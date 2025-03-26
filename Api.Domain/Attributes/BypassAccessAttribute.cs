namespace Api.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BypassAccessAttribute : Attribute { }
}
