using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Domain.EntityRequests.Dms
{
    public class RequestDocumentAddUpload
    {
        //public int Id { get; set; }
        public int CategoryID { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentDesc { get; set; }
        public int? Owner { get; set; }
        public IFormFile DocFile { get; set; }
        public int? WatermarkID { get; set; }
    }
}
