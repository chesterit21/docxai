using System.ComponentModel.DataAnnotations;

namespace Api.Domain.EntityRequests
{

    public class BaseRequest
    {

    }

    public class RequestPagination : BaseRequest
    {
        [Required]
        public int Page { get; set; }

        [Required]
        public int Limit { get; set; }
    }

    public class ReqestFilter : RequestPagination
    {
        [MaxLength(20)]
        public string FilterBy { get; set; }

        [MaxLength(50)]
        public string FilterValue { get; set; }

        [MaxLength(20)]
        public string SortBy { get; set; } = "InsertedAt";

        [MaxLength(4)]
        public string SortOrientation { get; set; } = "desc";

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
