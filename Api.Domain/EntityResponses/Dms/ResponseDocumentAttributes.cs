using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocumentAttributes
    {
        public int Id { get; set; }
        public int DocumentID { get; set; }
        public string AttributeValues { get; set; }
    }
}
