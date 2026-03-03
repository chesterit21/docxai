using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseDocumentShared
    {
        public int Id { get; set; }
        public int DocumentID { get; set; }
        //public int DocumentFilesID { get; set; }
        //public int UserID { get; set; }
        public List<ResponseUserPrivillege> SharedUser { get; set; }
    }
}
