using Api.DataAccess.Models.Masters;
using Microsoft.Identity.Client;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using NpgsqlTypes;
//using Api.DataAccess.Models.Common;

namespace Api.DataAccess.Models.Dms
{
    [Table("TblDocumentFiles")]
    public class DocumentFiles : BaseEntitySoftDelete
	{
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "integer")]
        public int DocumentID { get; set; }
        [Required]
        [Column(TypeName = "varchar(15)")]
        public string DocumentType { get; set; }
        [Required]
        [Column(TypeName = "varchar(150)")]
        public string DocumentFileName{ get; set; }
		[Column(TypeName = "varchar(150)")]
		public string NewDocumentFileName { get; set; }

		[Required]
        [Column(TypeName = "integer")]
        public int DocumentFileSize { get; set; }

        //[Required]
        [Column(TypeName = "text")]
        public string DocumentFileContent { get; set; }

        [Required]
        [Column(TypeName = "varchar(1000)")]
        public string DocumentFilePath { get; set; }

        [Required]
        [Column(TypeName = "boolean")]
        public bool IsMainDocumentFile { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(DocumentID))]
        public virtual Documents Documents { get; set; }

        public string DocumentSummary { get; set; }

		public string Attributtes { get; set; }

        public new int InsertedBy { get; set; }
		public new int? UpdatedBy { get; set; }

		[JsonIgnore]
		[ForeignKey(nameof(InsertedBy))]
		public virtual User InsertedByUser { get; set; }
		[JsonIgnore]
		[ForeignKey(nameof(UpdatedBy))]
		public virtual User UpdatedByUser { get; set; }
		
		[JsonIgnore]
		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public NpgsqlTsVector SearchVector { get; set; }		
	}
}
