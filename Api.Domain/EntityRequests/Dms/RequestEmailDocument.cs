using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests
{
    public class RequestEmailDocument
    {
		[Required]
		public int DocumentID { get; set; }

		[Required]
        [MaxLength(150)]
        public string Subject { get; set; }

        [Required]
        public List<string> To { get; set; }

        public List<string> Cc { get; set; }

        [Required]
        public string Body { get; set; }
    }
}
