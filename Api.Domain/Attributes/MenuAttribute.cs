namespace Api.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class MenuAttribute(string menuId) : Attribute
    {
        public string MenuId { get; } = menuId;
    }
}
