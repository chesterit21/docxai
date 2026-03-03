using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
    public class RequestDocumentAttributes
	{
        public int DocumentID { get; set; }

        public string AttributeValues { get; set; }
    }

	public class RequestUpdateDocumentAttributes : RequestDocumentAttributes
	{
		public int Id { get; set; }
	}

	public class DocumentAttribute
	{
		//public int id { get; set; }
		public string attributeName { get; set; }
		public string attributeElement { get; set; }
		public string attributeType { get; set; }
		public int attributeMaxLength { get; set; }
		public object value { get; set; }
	}
}
