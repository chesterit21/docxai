using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests
{
    public class RequestEmail
    {
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; }

        [Required]
        public string To { get; set; }

        public string Cc { get; set; }

        [Required]
        public string Body { get; set; }

        [Required]
        public bool IsHtml { get; set; } = false;
    }
}
