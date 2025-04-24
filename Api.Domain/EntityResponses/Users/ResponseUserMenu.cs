namespace Api.Domain.EntityResponses.Users
{
    public class ResponseMenuMatrix : IComparable<ResponseMenuMatrix>
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public bool IsADUser { get; set; }
        public bool EmailVerified { get; set; }
        public bool IsActive { get; set; }
        public List<Role> Roles { get; set; }
        public List<Matrix> Matrices { get; set; }

        public int CompareTo(ResponseMenuMatrix other)
        {
            if (UserName == other.UserName)
                return 1;

            return 0;
        }

        public class Role
        {
            public int RoleId { get; set; }
            public string Name { get; set; }
        }

        public class Matrix : IComparable<Matrix>
        {
            public string ParentMenuId { get; set; }
            public string MenuId { get; set; }
            public string Description { get; set; }
            public string Id { get; set; }
            public string En { get; set; }
            public int Sequence { get; set; }
            public int Level { get; set; }
            public string Url { get; set; }
            public string Icon { get; set; }
            public bool IsInsert { get; set; }
            public bool IsUpdate { get; set; }
            public bool IsDelete { get; set; }
            public bool IsRead { get; set; }

            public int CompareTo(Matrix other)
            {
                if (MenuId == other.MenuId)
                    return 1;

                return 0;
            }
        }
    }
}
