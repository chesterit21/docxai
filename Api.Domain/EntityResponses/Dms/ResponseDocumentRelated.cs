using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocumentRelated
	{
        public int Id { get; set; }
		public int RelatedDocumentID { get; set; }
		public string Text { get; set; }
		public int Value { get; set; }
	}
}
