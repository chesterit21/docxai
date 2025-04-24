using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Api.DataAccess
{
    public class BaseEntity
    {
        [NotMapped]
        public string InsertedByUserName { get; set; }

        [NotMapped]
        public string InsertedByFullName { get; set; }

        [NotMapped]
        public string UpdatedByUserName { get; set; }

        [NotMapped]
        public string UpdatedByFullName { get; set; }

        public int InsertedBy { get; set; } = 0;

        [Column(TypeName = "timestamp")]
        public DateTime InsertedAt { get; set; }

        #region EQUALITY BASED ON PRIMARY KEY
        public bool Equals(object entity, DbContext dbContext)
        {
            if (entity == null || entity.GetType() != this.GetType())
                return false;

            var entityType = dbContext.Model.FindEntityType(this.GetType());

            if (entityType == null)
                throw new InvalidOperationException($"Entity type {this.GetType().Name} is not part of the DbContext model.");

            var primaryKeyProperties = entityType.FindPrimaryKey()?.Properties;

            if (primaryKeyProperties == null || !primaryKeyProperties.Any())
                throw new InvalidOperationException($"No primary key is defined for entity type {this.GetType().Name}.");

            foreach (var property in primaryKeyProperties)
            {
                var propertyInfo = property.PropertyInfo;

                if (propertyInfo == null)
                {
                    throw new InvalidOperationException($"Primary key property {property.Name} does not have a corresponding PropertyInfo.");
                }

                var thisValue = propertyInfo.GetValue(this);
                var otherValue = propertyInfo.GetValue(entity);

                if (!object.Equals(thisValue, otherValue))
                {
                    return false;
                }
            }

            return true;
        }

        public new bool Equals(object entity)
        {
            if (entity == null || entity.GetType() != this.GetType())
                return false;

            var primaryKeyProperties = GetPrimaryKeyProperties(this.GetType());
            if (primaryKeyProperties != null && primaryKeyProperties.Any())
            {
                foreach (var property in primaryKeyProperties)
                {
                    var thisValue = property.GetValue(this);
                    var otherValue = property.GetValue(entity);

                    if (!object.Equals(thisValue, otherValue))
                        return false;
                }

                return true;
            }

            var keyProperties = GetKeyProperties(this.GetType());
            if (keyProperties != null && keyProperties.Any())
            {
                foreach (var property in keyProperties)
                {
                    var thisValue = property.GetValue(this);
                    var otherValue = property.GetValue(entity);

                    if (!object.Equals(thisValue, otherValue))
                        return false;
                }
            }

            return true;
        }

        private static PropertyInfo[] GetPrimaryKeyProperties(Type type)
        {
            var primaryKeyAttribute = type.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).FirstOrDefault() as PrimaryKeyAttribute;

            if (primaryKeyAttribute == null)
                return Array.Empty<PropertyInfo>();

            return primaryKeyAttribute.PropertyNames.Select(propertyName => type.GetProperty(propertyName)).Where(property => property != null).ToArray();
        }

        private static PropertyInfo[] GetKeyProperties(Type type) => type.GetProperties().Where(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Any()).ToArray();
        #endregion
    }

    public class BaseEntityUpdate : BaseEntity
    {
        public int UpdatedBy { get; set; } = 0;

        [Column(TypeName = "timestamp")]
        public DateTime UpdatedAt { get; set; }
    }

    public class BaseEntityDefault : BaseEntityUpdate
    {
        public bool IsActive { get; set; } = true;
    }

}
