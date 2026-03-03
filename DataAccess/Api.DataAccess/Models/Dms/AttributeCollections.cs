using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Api.DataAccess.Models.Dms
{
	[Table("TblAttributeCollections")]
	public class AttributeCollections : BaseEntitySoftDelete
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		//public Guid Id { get; set; } = Guid.NewGuid();
		public Guid Id { get; set; } = Guid.NewGuid();

		[Required]
		[Column(TypeName = "varchar(150)")]
		public string CollectionName { get; set; }

		[Required]
		[Column(TypeName = "varchar(500)")]
		public string CollectionDescription { get; set; }

		[Column(TypeName = "json")]
		public string AttributeElementCollection { get; set; }
	}
}
