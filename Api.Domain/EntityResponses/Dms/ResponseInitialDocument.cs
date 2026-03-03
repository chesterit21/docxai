using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.Enum;

namespace Api.Domain.EntityResponses.Dms
{
    public class ResponseInitialDocument
	{
        public int Id { get; set; }
        public int CategoryID { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentDesc { get; set; }
        public int Owner { get; set; }
		public string FileSize { get; set; }

    }
}
