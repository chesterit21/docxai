namespace Api.Domain.EntityResponses.Users
{
    public class ResponseLogin
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }

        public List<Company> Companies { get; set; }

        public class Company
        {
            public string CompanyId { get; set; }
            public string CompanyName { get; set; }
        }
    }
}
