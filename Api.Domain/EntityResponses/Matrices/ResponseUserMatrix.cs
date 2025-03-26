namespace Api.Domain.EntityResponses.Matrices
{
    public class ResponseUserMatrix : BaseResponseData
    {
        public string MenuId { get; set; }
        public string ParentMenuId { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string En { get; set; }
        public int Sequence { get; set; }
        public bool IsInsert { get; set; }
        public bool IsUpdate { get; set; }
        public bool IsDelete { get; set; }
        public bool IsRead { get; set; }
    }
}
