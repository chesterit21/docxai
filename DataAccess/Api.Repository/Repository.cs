using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Systems;
using Api.Domain;
using Api.Domain.Attributes;
using Api.Extensions;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using NPOI.SS.Formula.Functions;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Api.Repository
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
	{
        int UserId { get; }
        string UserName { get; }

        IQueryable<TEntity> AsQueryable();
        IQueryable GetQueryableAsync(Expression<Func<TEntity, bool>> predicate);
        DbSet<TEntity> AsDbSet();

        void ChangeTrackingBehavior(QueryTrackingBehavior behavior);

        Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<bool> AnyAsync();

        Task<long> CountAsync();
        Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<long> CountAsync(DateTime insertedDateStart, DateTime insertedDateEnd);

        Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetAsync();
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, int page, int limit);
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, int page, int limit, string orderBy, string orderOrientation);
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, int page, int limit, string orderBy, string orderOrientation, string filterBy, string filterValue);

        Task<List<TEntity>> GetAsync(int page, int limit);
        Task<List<TEntity>> GetAsync(int page, int limit, string orderBy, string orderOrientation);


        Task<TEntity> InsertAsync(TEntity entity);
        Task<TEntity> UpdateAsync(TEntity entity);
        Task<TEntity> DeleteAsync(TEntity entity);

        Task<TEntity> MarkAsDeletedAsync(TEntity entity);
        Task<TEntity> MarkAsNotDeletedAsync(TEntity entity);


        Task<int> ExecuteSqlRawAsync(string commandText);
        Task<int> ExecuteSqlRawAsync(string commandText, params SqlParameter[] parameters);
        Task<List<TEntity>> ExecuteSqlQueryAsync(string sqlQuery, params SqlParameter[] parameters);

        IQueryable<TEntity> FromSql(string sql, params object[] parameters);

        Task<List<TEntity>> InsertManyAsync(List<TEntity> entities);
        Task<List<TEntity>> InsertManyAsync(List<TEntity> entities, BulkConfig config);
        Task<List<TEntity>> UpdateManyAsync(List<TEntity> entities);
        Task<List<TEntity>> DeleteManyAsync(List<TEntity> entities);
        Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> UpsertAsync(TEntity entity);
        Task<List<TEntity>> UpsertManyAsync(List<TEntity> entities);
        Task<List<TEntity>> UpsertManyAsync(List<TEntity> entities, BulkConfig config);
        Task<List<TEntity>> UpsertDeleteManyAsync(List<TEntity> entities);
        Task<List<TEntity>> UpsertDeleteManyAsync(List<TEntity> entities, BulkConfig config);

        Task<int> LogAuditTrailInsert(Guid? transLogId, params TEntity[] entities);
        Task<int> LogAuditTrailUpdate(Guid? transLogId, params TEntity[] entities);
        Task<int> LogAuditTrailUpsert(Guid? transLogId, params TEntity[] entities);
        Task<int> LogAuditTrailDelete(Guid? transLogId, params TEntity[] entities);
        Task<Guid> LogTransaction(string description, UserAction userAction);
        Task<Guid> LogTransactionAndAuditTrail(string description, UserAction userAction, params TEntity[] entities);
        int GetTotalPages(int totalRecords, int limit);
        //Task<bool> CheckOwnerPrivilegesAsync<TPriv>(int entityId, Func<TPriv, bool> privilegePredicate) where TPriv : class;
	}

    public class Repository<TEntity>(DataContext context, IHttpContextAccessor accessor) : IDisposable, IRepository<TEntity> where TEntity : BaseEntity
	{
        private enum CommandAction
        {
            Insert, Update
        };

        protected readonly DataContext context = context;
        protected readonly HttpContext httpContext = accessor?.HttpContext;

        private readonly DbSet<TEntity> dbset = context.Set<TEntity>();		

		public int UserId => int.Parse(accessor?.HttpContext?.User?.Identity?.Name ?? "0");
		public string UserRoleInApp => accessor?.HttpContext?.GetClaim<string>("user_type");

		public string UserName => accessor?.HttpContext?.GetClaim<string>("given_name");//GivenName

		public int Skip(int page, int limit) => (page - 1) * limit;

        public int GetTotalPages(int totalRecords, int limit)
        {
            if (totalRecords < 1)
                return 0;

            return (int)Math.Ceiling((double)totalRecords / limit);
        }

        public IQueryable<TEntity> AsQueryable() => dbset.AsQueryable<TEntity>();

        public DbSet<TEntity> AsDbSet() => dbset;

        public void ChangeTrackingBehavior(QueryTrackingBehavior behavior)
        {
            context.ChangeTrackingBehavior(behavior);
        }

		private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
		{
			WriteIndented = true,
			ReferenceHandler = ReferenceHandler.Preserve // or Preserve if needed
		};

		public static string SerializeArray<T>(T[] array)
		{
			return JsonSerializer.Serialize(array, _options);
		}

		public async Task<long> CountAsync(DateTime insertedDateStart, DateTime insertedDateEnd)
        {
            return await dbset.Where(x => x.InsertedAt >= insertedDateStart && x.InsertedAt <= insertedDateEnd).CountAsync();
            //var dateColumn = nameof(BaseEntityDefault.InsertedAt);
            //return await dbset.Where($"{dateColumn} >= @0 && {dateColumn} <= @1", insertedDateStart, insertedDateEnd).CountAsync();
        }

        public virtual async Task<long> CountAsync() => await dbset.CountAsync();

        public virtual async Task<long> CountAsync(Expression<Func<TEntity, bool>> predicate) => await dbset.CountAsync(predicate);

        public virtual async Task<bool> AnyAsync() => await dbset.AnyAsync();

        public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate) => await dbset.AnyAsync(predicate);

        private async Task<List<TEntity>> GetJoinedQuery(Expression<Func<TEntity, bool>> predicate = null, int page = 0, int limit = 0, string orderBy = null, string orderOrientation = null, string filterBy = null, string filterValue = null)
        {
            var skip = Skip(page, limit);

            var query = dbset
                //dbset.Where($"{dateColumn} >= @0 && {dateColumn} <= @1", insertedDateStart, insertedDateEnd)
                .WhereIf(predicate != null, predicate)
                .WhereIf(!string.IsNullOrWhiteSpace(filterBy) && !string.IsNullOrWhiteSpace(filterValue), $"{filterBy}.Contains(@0)", filterValue)
                .LeftJoin(context.User,
                    left => left.InsertedBy,
                    user => user.UserId,
                    (left, userInsert) => new
                    {
                        Entity = left,
                        InsertedByUserName = userInsert.UserName,
                        InsertedByFullName = userInsert.FullName,
                        UpdatedByUserName = "",
                        UpdatedByFullName = ""
                    });

            if (typeof(TEntity).GetProperty("UpdatedBy", BindingFlags.Public | BindingFlags.Instance) != null)
            {
                query = query.LeftJoin(context.User,
                    "Entity.UpdatedBy",//left => left.Entity.UpdatedBy,
                    "UserId",//user => user.UserId,
                    (left, userUpdate) => new
                    {
                        left.Entity,
                        left.InsertedByUserName,
                        left.InsertedByFullName,
                        UpdatedByUserName = userUpdate.UserName,
                        UpdatedByFullName = userUpdate.FullName
                    });
            }

            if (!string.IsNullOrWhiteSpace(orderBy))
                query = query.OrderBy(x => $"x.{orderBy} {orderOrientation}");

            if (page > 0 && limit > 0)
                query = query.Skip(skip).Take(limit);

            var list = await query.ToListAsync();

            return list.Select(x =>
            {
                x.Entity.InsertedByUserName = x.InsertedByUserName;
                x.Entity.InsertedByFullName = x.InsertedByFullName;
                x.Entity.UpdatedByUserName = x.UpdatedByUserName;
                x.Entity.UpdatedByFullName = x.UpdatedByFullName;
                return x.Entity;
            }).ToList();
        }

        public virtual async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var query = await GetJoinedQuery(predicate);
            return query.FirstOrDefault();
        }

        public async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, int page, int limit, string orderBy, string orderOrientation) => await GetJoinedQuery(predicate, page, limit, orderBy, orderOrientation);

        public async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, int page, int limit, string orderBy, string orderOrientation, string filterBy, string filterValue) => await GetJoinedQuery(predicate, page, limit, orderBy, orderOrientation, filterBy, filterValue);


        public virtual async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate) => await GetJoinedQuery(predicate);

        public virtual IQueryable GetQueryableAsync(Expression<Func<TEntity, bool>> predicate) => dbset.Where(predicate);

        public virtual async Task<List<TEntity>> GetAsync() => await GetJoinedQuery();

        public async Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, int page, int limit) => await GetJoinedQuery(predicate, page, limit);

        public async Task<List<TEntity>> GetAsync(int page, int limit, string orderBy, string order) => await GetJoinedQuery(null, page, limit, orderBy, order);

        public async Task<List<TEntity>> GetAsync(int page, int limit) => await GetJoinedQuery(null, page, limit);

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            await SetBaseEntity([entity], CommandAction.Insert);
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            await SetBaseEntity([entity], CommandAction.Update);
            context.Update(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> DeleteAsync(TEntity entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> MarkAsDeletedAsync(TEntity entity)
        {
            await SetBaseEntity([entity], CommandAction.Update);
            context.MarkAsDeleted(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> MarkAsNotDeletedAsync(TEntity entity)
        {
            await SetBaseEntity([entity], CommandAction.Update);
            context.MarkAsUndeleted(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task DeleteManyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await dbset.Where(predicate).ToListAsync();
            dbset.RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task<int> ExecuteSqlRawAsync(string commandText) => await context.Database.ExecuteSqlRawAsync(commandText);

        public virtual async Task<int> ExecuteSqlRawAsync(string commandText, params SqlParameter[] parameters) => await context.Database.ExecuteSqlRawAsync(commandText, parameters);

        public async Task<List<TEntity>> ExecuteSqlQueryAsync(string sqlQuery, params SqlParameter[] parameters) => await context.Database.SqlQueryRaw<TEntity>(sqlQuery, parameters).ToListAsync();

        public IQueryable<TEntity> FromSql(string sql, params object[] parameters) => dbset.FromSqlRaw(sql, parameters);

        #region BULK

        readonly BulkConfig bulkConfig = new()
        {
            //BatchSize = 200,
            //SqlBulkCopyOptions = SqlBulkCopyOptions.TableLock,
            WithHoldlock = true,
            UseOptionLoopJoin = false,
            //PreserveInsertOrder = true,
            //SetOutputIdentity = true,
        };

        public virtual async Task<List<TEntity>> InsertManyAsync(List<TEntity> entities)
        {
            await SetBaseEntity(entities, CommandAction.Insert);
            await context.BulkInsertAsync(entities, bulkConfig);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        public virtual async Task<List<TEntity>> InsertManyAsync(List<TEntity> entities, BulkConfig config)
        {
            await SetBaseEntity(entities, CommandAction.Insert);
            await context.BulkInsertAsync(entities, config);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        public virtual async Task<List<TEntity>> UpdateManyAsync(List<TEntity> entities)
        {
            await SetBaseEntity(entities, CommandAction.Update);
            await context.BulkUpdateAsync(entities);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        public virtual async Task<List<TEntity>> DeleteManyAsync(List<TEntity> entities)
        {
            await context.BulkDeleteAsync(entities);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        public async Task<TEntity> UpsertAsync(TEntity entity)
        {
            await SetBaseEntity([entity], CommandAction.Update);
            var list = new List<TEntity>([entity]);
            await UpsertManyAsync(list);
            return entity;
        }

        public virtual async Task<List<TEntity>> UpsertManyAsync(List<TEntity> entities)
        {
            await SetBaseEntity(entities, CommandAction.Update);
            await context.BulkInsertOrUpdateAsync(entities, bulkConfig);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        public virtual async Task<List<TEntity>> UpsertManyAsync(List<TEntity> entities, BulkConfig config)
        {
            await SetBaseEntity(entities, CommandAction.Update);
            await context.BulkInsertOrUpdateAsync(entities, config);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        public virtual async Task<List<TEntity>> UpsertDeleteManyAsync(List<TEntity> entities)
        {
            await SetBaseEntity(entities, CommandAction.Update);
            await context.BulkInsertOrUpdateOrDeleteAsync(entities, bulkConfig);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        public virtual async Task<List<TEntity>> UpsertDeleteManyAsync(List<TEntity> entities, BulkConfig config)
        {
            await SetBaseEntity(entities, CommandAction.Update);
            await context.BulkInsertOrUpdateOrDeleteAsync(entities, config);
            await context.BulkSaveChangesAsync();
            return entities;
        }

        #endregion

        #region DATABASE LOGGER

        private string GetUserAgent() => accessor?.HttpContext?.Request?.Headers?.UserAgent.ToString();

        private string GetIpAddress() => accessor?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();

        private TEntity[] ToArray(params TEntity[] entities)
        {
            if (entities == null || entities.Length == 0)
                return entities;

            if (entities.ElementAt(0).GetType() == typeof(List<TEntity>))
            {
                return entities.ToArray();
            }

            return entities;
        }

        private async Task<List<TEntity>> GetOldDataFromDbAsync(params TEntity[] entities)
        {
            var type = typeof(TEntity);
            var entityType = context.Model.FindEntityType(type);
            var primaryKey = entityType.FindPrimaryKey();
            var schema = entityType.GetSchema();
            var tableName = entityType.GetTableName();
            var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, schema);
            //var primaryKeyColumns = primaryKey.Properties.Select(x => x.GetColumnName(storeObjectIdentifier)).ToList();
            var primaryKeyNames = primaryKey.Properties.Select(x => x.Name).ToList();

            var where = "";

            foreach (var entity in ToArray(entities))
            {
                where += "(";

                int ix = 0;
                foreach (var propName in primaryKeyNames)
                {
                    var value = type.GetProperty(propName).GetValue(entity, null);

                    if (value == null || (value is string strval && string.IsNullOrWhiteSpace(strval)))
                        break;

                    if (value is string || value is Guid)
                        value = "\"" + value + "\"";

                    if (ix == 0)
                        where += $"{propName} == {value}";
                    else
                        where += $" && {propName} == {value}";

                    ix++;
                }

                where += ") || ";
            }

            where = where[..^4];

            return await dbset.Where(where).AsNoTracking().ToListAsync();
        }

        public async Task<int> LogAuditTrailInsert(Guid? transLogId, params TEntity[] entities)
        {
            entities = ToArray(entities);

            if (entities == null || entities.Length == 0) return default;

            var classType = entities[0];
            if (classType is AuditTrail || classType is ApplicationLog || classType is TransactionLog)
                return default;

            Encryption.MaskPassword(entities);

            var type = typeof(TEntity);
            var entityType = context.Model.FindEntityType(type);
            var tableName = entityType.GetTableName();


			var auditTrail = new AuditTrail
            {
                TransactionLogId = transLogId,
				//After = JsonSerializer.Serialize(entities, options),
				After = SerializeArray(entities),
				Before = null,
                Command = "Insert",
                TableName = tableName,
                InsertedBy = UserId,
                InsertedAt = DateTime.Now,
            };

            context.AuditTrail.Add(auditTrail);
            return await context.SaveChangesAsync();
        }

        public async Task<int> LogAuditTrailUpdate(Guid? transLogId, params TEntity[] entities)
        {
            entities = ToArray(entities);

            if (entities == null || entities.Length == 0) return default;

            var classType = entities[0];
            if (classType is AuditTrail || classType is ApplicationLog || classType is TransactionLog)
                return default;

			context.Entry(entities[0]).State = EntityState.Detached;
			Encryption.MaskPassword(entities);

			var entityType = context.Model.FindEntityType(typeof(TEntity));
			var tableName = entityType.GetTableName();

            var entitiesFromDb = await GetOldDataFromDbAsync(entities);
			
			var auditTrail = new AuditTrail
            {
                TransactionLogId = transLogId,
				//After = JsonSerializer.Serialize(entities, options),
				//Before = JsonSerializer.Serialize(entitiesFromDb, options),
				After = SerializeArray(entities),
				Before = SerializeArray(entitiesFromDb.ToArray()),
				Command = "Update",
                TableName = tableName,
                InsertedBy = UserId,
                InsertedAt = DateTime.Now,
            };

            context.AuditTrail.Add(auditTrail);
            return await context.SaveChangesAsync();
        }

        public async Task<int> LogAuditTrailUpsert(Guid? transLogId, params TEntity[] entities)
        {
            entities = ToArray(entities);

            if (entities == null || entities.Length == 0) return default;

            var classType = entities[0];
            if (classType is AuditTrail || classType is ApplicationLog || classType is TransactionLog)
                return default;

            Encryption.MaskPassword(entities);

            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var tableName = entityType.GetTableName();

            var entitiesFromDb = await GetOldDataFromDbAsync(entities);
			
			var auditTrail = new AuditTrail
            {
                TransactionLogId = transLogId,
               // After = JsonSerializer.Serialize(entities),
                //Before = JsonSerializer.Serialize(entitiesFromDb),
                After = SerializeArray(entities),
				Before = SerializeArray(entitiesFromDb.ToArray()),
				Command = "Update | Insert",
                TableName = tableName,
                InsertedBy = UserId,
                InsertedAt = DateTime.Now,
            };

            context.AuditTrail.Add(auditTrail);
            return await context.SaveChangesAsync();
        }

        public async Task<int> LogAuditTrailDelete(Guid? transLogId, params TEntity[] entities)
        {
            entities = ToArray(entities);

            if (entities == null || entities.Length == 0) return 0;

            var classType = entities[0];
            if (classType is AuditTrail || classType is ApplicationLog || classType is TransactionLog)
                return 0;

            Encryption.MaskPassword(entities);

            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var tableName = entityType.GetTableName();

            var entitiesFromDb = await GetOldDataFromDbAsync(entities);

            var auditTrail = new AuditTrail
            {
                TransactionLogId = transLogId,
                After = null,
                //Before = JsonSerializer.Serialize(entitiesFromDb),
				Before = SerializeArray(entitiesFromDb.ToArray()),
				Command = "Delete",
                TableName = tableName,
                Id = Guid.NewGuid(),
                InsertedBy = UserId,
                InsertedAt = DateTime.Now,
            };

            context.AuditTrail.Add(auditTrail);
            return await context.SaveChangesAsync();
        }

        public async Task<Guid> LogTransaction(string description, UserAction userAction)
        {
            var request = accessor?.HttpContext?.Request;
            var path = request?.Path;
            var body = request?.QueryString.Value?.ToString().Trim();
            if (string.IsNullOrWhiteSpace(body))
                body = request?.GetRawStringBody();

            if (!string.IsNullOrWhiteSpace(body) && body.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                if (body.Trim().StartsWith("{") || body.Trim().EndsWith("}"))
                    body = Encryption.MaskPassword(JsonNode.Parse(body), true)?.ToJsonString();
            }

            var log = new TransactionLog
            {
                Id = Guid.NewGuid(),
                IPAddress = GetIpAddress(),
                UserAgent = GetUserAgent(),
                Action = userAction.ToString(),
                Path = $"[{request?.Method} {request?.Protocol}] {path}",
                Parameter = body,
                Description = $"[{UserName}] {description}",
                InsertedBy = UserId,
                InsertedAt = DateTime.Now,
            };

            context.TransactionLog.Add(log);
            await context.SaveChangesAsync();
            return log.Id;
        }

        public async Task<Guid> LogTransactionAndAuditTrail(string description, UserAction userAction, params TEntity[] entities)
        {
            entities = ToArray(entities);

            var id = await LogTransaction(description, userAction);

            if (userAction == UserAction.Read)
                return id;

            if (entities == null || entities.Length == 0)
                throw new ApiException($"Entities of {typeof(TEntity).Name} was not provided", System.Net.HttpStatusCode.InternalServerError);

            if (entities.ElementAt(0).GetType() == typeof(List<TEntity>))
            {
                entities = entities.ToArray();
            }

            switch (userAction)
            {
                case UserAction.Delete:
                    await LogAuditTrailDelete(id, entities);
                    break;
                case UserAction.Insert:
                    await LogAuditTrailInsert(id, entities);
                    break;
                case UserAction.Update:
                    await LogAuditTrailUpdate(id, entities);
                    break;
            }

            return id;
        }

        //private void SetAuditTrail()
        //{
        //    var entities = context.ChangeTracker.Entries()
        //        .Where(e => e.State == EntityState.Added ||
        //                    e.State == EntityState.Modified ||
        //                    e.State == EntityState.Deleted)
        //        .ToList();

        //    foreach (var entity in entities)
        //    {
        //        var classType = entity.Entity;
        //        if (classType is AuditTrail || classType is ApplicationLog || classType is TransactionLog)
        //            return;

        //        var entityType = context.Model.FindEntityType(entity.Entity.GetType());
        //        //var primaryKey = entityType.FindPrimaryKey();
        //        //var schema = entityType.GetSchema();
        //        var tableName = entityType.GetTableName();
        //        //var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, schema);
        //        //var primaryKeyColumns = primaryKey.Properties.Select(x => x.GetColumnName(storeObjectIdentifier)).ToList();
        //        //var primaryKeyNames = primaryKey.Properties.Select(x => x.Name).ToList();

        //        string after = null, before = null;
        //        if (entity.OriginalValues != null)
        //            before = JsonSerializer.Serialize(entity.OriginalValues.ToObject());

        //        if (entity.CurrentValues != null)
        //            after = JsonSerializer.Serialize(entity.CurrentValues.ToObject());


        //        var auditTrail = new AuditTrail
        //        {
        //            After = entity.State == EntityStateasync.Added ? after : entity.State == EntityState.Deleted ? null : GetChanges(entity),
        //            Before = entity.State == EntityState.Added ? null : before,
        //            Command = entity.State.ToString(),
        //            TableName = tableName,
        //            InsertedBy = GetUserId(),
        //            InsertedAt = DateTime.Now,
        //        };

        //        AuditTrail.Add(auditTrail);
        //    }
        //}

        //private static string GetChanges(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entity)
        //{
        //    dynamic expando = new System.Dynamic.ExpandoObject();
        //    //var changes = new StringBuilder();
        //    foreach (var property in entity.OriginalValues.Properties)
        //    {
        //        var originalValue = entity.OriginalValues[property];
        //        var currentValue = entity.CurrentValues[property];
        //        if (!Equals(originalValue, currentValue))
        //        {
        //            if (property.Name.Contains("password", StringComparison.OrdinalIgnoreCase) && currentValue is string strCurrentValue && !string.IsNullOrWhiteSpace(strCurrentValue))
        //                currentValue = new string('*', strCurrentValue.Length);

        //            ((IDictionary<string, object>)expando).Add(property.Name, currentValue);

        //            //changes.AppendLine($"{property.GetColumnName()}: from '{originalValue}' to '{currentValue}'");
        //        }
        //    }
        //    //return changes.Length == 0 ? null : changes.ToString();
        //    return JsonSerializer.Serialize(expando);
        //}

        #endregion

        private async Task SetBaseEntity(List<TEntity> entities, CommandAction action)
        {
            var name = accessor?.HttpContext?.User?.Identity?.Name ?? "0";
            int userId = int.Parse(name);

            var type = entities.ElementAt(0).GetType();
            var insertedBy = nameof(BaseEntityDefault.InsertedBy);
            var insertedAt = nameof(BaseEntityDefault.InsertedAt);

            var updatedBy = nameof(BaseEntityDefault.UpdatedBy);
            var updatedAt = nameof(BaseEntityDefault.UpdatedAt);

            var hasInsert = type.GetProperty(insertedBy) != null;
            var hasUpdate = type.GetProperty(updatedBy) != null;

            if (action == CommandAction.Insert && hasInsert)
            {
                foreach (var entity in entities)
                {
                    type.GetProperty(insertedBy).SetValue(entity, userId);
                    //type.GetProperty(insertedAt).SetValue(entity, DateTime.Now);
                    type.GetProperty(insertedAt).SetValue(entity, DateTime.Now);

                    if (hasUpdate)
                    {
                        type.GetProperty(updatedBy).SetValue(entity, userId);
                        //type.GetProperty(updatedAt).SetValue(entity, DateTime.Now);
                        type.GetProperty(updatedAt).SetValue(entity, DateTime.Now);
                    }
                }

                return;
            }

            if (action == CommandAction.Update)
            {
                if (hasUpdate)
                {
                    foreach (var entity in entities)
                    {
                        type.GetProperty(updatedBy).SetValue(entity, userId);
                        //type.GetProperty(updatedAt).SetValue(entity, DateTime.Now);
                        type.GetProperty(updatedAt).SetValue(entity, DateTime.Now);
                    }
                }

                if (hasInsert)
                {
                    var ls = await GetOldDataFromDbAsync(entities.ToArray());

                    foreach (var entity in entities)
                    {
                        var e = ls.FirstOrDefault(x => x.Equals(entity, context));
                        if (e == null)
                        {
                            type.GetProperty(insertedBy).SetValue(entity, userId);
                            //type.GetProperty(insertedAt).SetValue(entity, DateTime.Now);
                            type.GetProperty(insertedAt).SetValue(entity, DateTime.Now);
                        }
                        else
                        {
                            var insertedByValue = e.GetType().GetProperty(insertedBy).GetValue(e, null);
                            var insertedAtValue = e.GetType().GetProperty(insertedAt).GetValue(e, null);

                            type.GetProperty(insertedBy).SetValue(entity, insertedByValue);
                            type.GetProperty(insertedAt).SetValue(entity, insertedAtValue);
                        }
                    }
                }
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }
    }

}
