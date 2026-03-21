using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests.Systems
{
    public class RequestAiModel
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ModelName { get; set; }

        [Required]
        [MaxLength(500)]
        public string UrlApi { get; set; }

        [Required]
        [MaxLength(500)]
        public string ApiKey { get; set; }

        [Required]
        [MaxLength(50)]
        public string Provider { get; set; }

        [Required]
        public int MaxToken { get; set; }
    }
}
