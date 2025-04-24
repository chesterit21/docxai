using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests.Masters
{
    public class RequestCompany
    {
        [Required]
        [MaxLength(15)]
        public string CompanyId { get; set; }

        [MaxLength(15)]
        public string ParentCompanyId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Address { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }
    }
}
