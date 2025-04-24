namespace Api.Domain.EntityResponses.Users
{
    public class EncryptedForgetPassword
    {
        public string Email { get; set; }
        public int UserId { get; set; }
        public long ValidityPeriod { get; set; }
    }

    public class ResponseForgetPassword
    {
        public string Reason { get; set; }
        public long ValidityPeriod { get; set; }
    }
}
