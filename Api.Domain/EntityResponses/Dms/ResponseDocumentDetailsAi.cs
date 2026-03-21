using System;
using System.Collections.Generic;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocumentDetailsAi
    {
        public int DocumentId { get; set; }
        public string Title { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string DocumentTypeName { get; set; }
        public string DocumentSummary { get; set; }
        public long FileSize { get; set; }
        public List<ResponseEntityItem> Entities { get; set; }
    }

    public class ResponseEntityItem
    {
        public string AttributeName { get; set; }
        public string Value { get; set; }
    }
}
