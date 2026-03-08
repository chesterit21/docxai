using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.DataAccess.Models.Masters
{
    [Table("TmDocumentTypeAttributes")]
    public class TmDocumentTypeAttributes : BaseEntityDefault
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid DocumentTypeId { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string AttributeName { get; set; }

        [Required]
        [Column(TypeName = "varchar(250)")]
        public string DataType { get; set; }

        [ForeignKey(nameof(DocumentTypeId))]
        public virtual TmDocumentType DocumentType { get; set; }

        public virtual ICollection<TmAttributeSynonyms> Synonyms { get; set; }
    }
}
