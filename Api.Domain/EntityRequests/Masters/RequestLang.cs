using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Api.Domain.EntityRequests.Masters
{
    public class RequestLang
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }

        [Required]
        [MaxLength(500)]
        public string Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string En { get; set; }

        [JsonIgnore]
        public string Type { get; set; }
    }
}
