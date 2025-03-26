namespace Api.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GroupTagAttribute : Attribute
    {
        public string Name { get; }

        public GroupTagAttribute(string name)
        {
            Name = name;
        }
    }
}
