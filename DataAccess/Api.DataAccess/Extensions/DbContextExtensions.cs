using Api.DataAccess.Models.Dms;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Dynamic;
using System.Reflection;

namespace Api.DataAccess.Extensions
{
    public static class DbContextExtensions
    {
        /*
        public static async Task<List<dynamic>> FromSqlAsync(this DbContext db, string sql, params DbParameter[] parameters)
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            sql = sql.Trim();
            cmd.CommandType = sql.StartsWith("exec", StringComparison.OrdinalIgnoreCase) ? CommandType.StoredProcedure : CommandType.Text;
            if (cmd.CommandType == CommandType.StoredProcedure)
                sql = sql[5..];

            cmd.CommandText = sql;
            cmd.CommandTimeout = 60;
            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters.ToArray());

            var list = new List<dynamic>();
            using var dataReader = await cmd.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                var row = new ExpandoObject() as IDictionary<string, object>;
                for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                {
                    object value = null;
                    if (!dataReader.IsDBNull(fieldCount))
                        value = dataReader[fieldCount];

                    row.Add(dataReader.GetName(fieldCount), value);
                }

                //Parallel.For(0, dataReader.FieldCount, (fieldCount) =>
                //{
                //    object value = null;
                //    if (!dataReader.IsDBNull(fieldCount))
                //        value = dataReader[fieldCount];

                //    row.Add(dataReader.GetName(fieldCount), value);
                //});

                list.Add(row);
            }

            return list;
        }

        public static async Task<List<TEntity>> FromSqlAsync<TEntity>(this DbContext db, string sql, params DbParameter[] parameters) where TEntity : class
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            using var cmd = db.Database.GetDbConnection().CreateCommand();

            sql = sql.Trim();
            cmd.CommandTimeout = 60;
            cmd.CommandType = sql.StartsWith("exec", StringComparison.OrdinalIgnoreCase) ? CommandType.StoredProcedure : CommandType.Text;
            if (cmd.CommandType == CommandType.StoredProcedure)
                sql = sql[5..];

            cmd.CommandText = sql;

            if (cmd.Connection.State != ConnectionState.Open)
            {
                await cmd.Connection.OpenAsync();
            }

            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters.ToArray());
            }

            var list = new List<TEntity>();
            using var dataReader = await cmd.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                var type = typeof(TEntity);
                var entity = Activator.CreateInstance<TEntity>();
                for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                {
                    if (dataReader.IsDBNull(fieldCount))
                        continue;
                    string columnName = dataReader.GetName(fieldCount);
                    PropertyInfo prop = type.GetProperty(columnName, flags);
                    if (prop == null || !prop.CanWrite)
                        continue;

                    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    object value = null;
                    if (prop.PropertyType == typeof(DateOnly))
                        value = DateOnly.FromDateTime(dataReader.GetDateTime(fieldCount));
                    else
                        value = Convert.ChangeType(dataReader[fieldCount], propType);

                    prop.SetValue(entity, value);
                }

                //        Parallel.For(0, dataReader.FieldCount, (fieldCount) =>
                //        {
                //            if (dataReader.IsDBNull(fieldCount))
                //                return;
                //            string columnName = dataReader.GetName(fieldCount);
                //            PropertyInfo prop = type.GetProperty(columnName, flags);
                //            if (prop == null || !prop.CanWrite)
                //                return;

                //            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                //            object value = null;
                //            if (prop.PropertyType == typeof(DateOnly))
                //                value = DateOnly.FromDateTime(dataReader.GetDateTime(fieldCount));
                //            else
                //                value = Convert.ChangeType(dataReader[fieldCount], propType);

                //            prop.SetValue(entity, value);
                //        });

                list.Add(entity);
            }

            return list;
        }
        */

        public static IQueryable Set(this DbContext context, Type type)
        {
            var method = typeof(DbContext).GetMethods().Single(p =>
                p.Name == nameof(DbContext.Set) && p.ContainsGenericParameters && !p.GetParameters().Any());

            // Build a method with the specific type argument you're interested in
            method = method.MakeGenericMethod(type);

            return method.Invoke(context, null) as IQueryable;
        }

        public static async Task<List<dynamic>> FromSqlAsync(this DbContext db, string sql, params DbParameter[] parameters) =>
            await FromSqlAsync(db.Database.GetDbConnection() as NpgsqlConnection, sql, parameters);

        public static async Task<TEntity> FromSqlAsync<TEntity>(this DbContext db, string sql, params DbParameter[] parameters) where TEntity : class =>
            await FromSqlAsync<TEntity>(db.Database.GetDbConnection() as NpgsqlConnection, sql, parameters);

        public static async Task<List<dynamic>> FromSqlAsync(this NpgsqlConnection connection, string sql, params DbParameter[] parameters)
        {
            using var cmd = connection.CreateCommand();

            sql = sql.Trim();
            cmd.CommandType = sql.StartsWith("execute", StringComparison.OrdinalIgnoreCase) ? CommandType.StoredProcedure : CommandType.Text;
            if (cmd.CommandType == CommandType.StoredProcedure)
                sql = sql.Substring(5);

            cmd.CommandText = sql;
            cmd.CommandTimeout = 60;
            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            var list = new List<dynamic>();
            using var dataReader = await cmd.ExecuteReaderAsync();
            while (dataReader.Read())
            {
                var row = new ExpandoObject() as IDictionary<string, object>;
                for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
                {
                    object value = null;
                    if (!dataReader.IsDBNull(fieldCount))
                        value = dataReader[fieldCount];

                    row.Add(dataReader.GetName(fieldCount), value);
                }

                list.Add(row);
            }

            return list;
        }

        public static async Task<TEntity> FromSqlAsync<TEntity>(this NpgsqlConnection connection, string sql, params DbParameter[] parameters) where TEntity : class
        {
            var entityType = typeof(TEntity);

            using var cmd = connection.CreateCommand();

            sql = sql.Trim();
            cmd.CommandType = sql.StartsWith("execute", StringComparison.OrdinalIgnoreCase) ? CommandType.StoredProcedure : CommandType.Text;
            if (cmd.CommandType == CommandType.StoredProcedure)
                sql = sql.Substring(5);

            cmd.CommandText = sql;
            cmd.CommandTimeout = 60;
            if (cmd.Connection.State != ConnectionState.Open)
            {
                await cmd.Connection.OpenAsync();
            }

            if (parameters != null && parameters.Length > 0)
            {
                cmd.Parameters.AddRange(parameters.ToArray());
            }

            using var dataReader = await cmd.ExecuteReaderAsync();

            if (entityType.IsPrimitive || entityType == typeof(string))
            {
                dataReader.Read();
                if (dataReader.IsDBNull(0))
                    return default;

                var propType = Nullable.GetUnderlyingType(entityType) ?? entityType;
                object value = Convert.ChangeType(dataReader[0], propType);

                return (TEntity)value;
            }

            if (entityType == typeof(DataTable))
            {
                var table = new DataTable(sql);
                table.Load(dataReader);
                return table as TEntity;
            }

            if (entityType.IsGenericType && entityType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = entityType.GetGenericArguments()[0];
                Type genericListType = typeof(List<>).MakeGenericType(elementType);
                var list = (IList)Activator.CreateInstance(genericListType);
                if (elementType.IsPrimitive || elementType == typeof(string))
                {
                    while (dataReader.Read())
                    {
                        if (dataReader.IsDBNull(0))
                            continue;

                        var propType = Nullable.GetUnderlyingType(elementType) ?? elementType;
                        var value = Convert.ChangeType(dataReader[0], propType);
                        list.Add(value);
                    }
                }
                else
                {
                    while (dataReader.Read())
                    {
                        var entity = MapDataReaderEntity(dataReader, elementType);
                        list.Add(entity);
                    }
                }
                return list as TEntity;
            }
            else
            {
                dataReader.Read();
                var entity = MapDataReaderEntity(dataReader, entityType);
                return (TEntity)entity;
            }

            //return default;
        }

        private static object MapDataReaderEntity(DbDataReader dataReader, Type entityType)
        {
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
            var entity = Activator.CreateInstance(entityType);

            for (var fieldCount = 0; fieldCount < dataReader.FieldCount; fieldCount++)
            {
                if (dataReader.IsDBNull(fieldCount))
                    continue;

                string columnName = dataReader.GetName(fieldCount);
                PropertyInfo prop = entityType.GetProperty(columnName, flags);
                if (prop == null || !prop.CanWrite)
                    continue;

                var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                object value = Convert.ChangeType(dataReader[fieldCount], propType);

                prop.SetValue(entity, value);
            }

            //Parallel.For(0, dataReader.FieldCount, (fieldCount) =>
            //{
            //    if (dataReader.IsDBNull(fieldCount))
            //        return;

            //    string columnName = dataReader.GetName(fieldCount);
            //    PropertyInfo prop = entityType.GetProperty(columnName, flags);
            //    if (prop == null || !prop.CanWrite)
            //        return;

            //    var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            //    object value = Convert.ChangeType(dataReader[fieldCount], propType);

            //    prop.SetValue(entity, value);
            //});

            return entity;
        }

        public static async Task<int> InsertIntoTableAsync(this DbContext db, string tableName, object value)
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();

            Dictionary<string, object> pairs = new();

            var isDictionary = value is Dictionary<string, object> || value is KeyValuePair<string, object>;

            if (isDictionary)
            {
                pairs = value as Dictionary<string, object>;
            }
            else
            {
                var isArray = typeof(IEnumerable).IsAssignableFrom(value.GetType());
                if (isArray)
                {
                    Type type = value.GetType().GetGenericArguments().ElementAtOrDefault(0);
                    if (type == typeof(DbParameter))
                    {
                        var arr = value as List<DbParameter>;
                        foreach (var a in arr)
                        {
                            pairs.Add(a.ParameterName, a.Value);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException("The data value type is not supported");
                    }
                }
                else
                {
                    pairs = value.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToDictionary(prop => prop.Name, prop => prop.GetValue(value, null));
                }
            }

            var tableColumn = new List<string>();
            var tableValue = new List<string>();

            foreach (var p in pairs)
            {
                if (p.Value == null)
                    continue;

                var parameter = cmd.CreateParameter();
                parameter.ParameterName = "@" + p.Key;
                parameter.Value = p.Value;
                cmd.Parameters.Add(parameter);

                tableColumn.Add(p.Key);
                tableValue.Add("@" + p.Key);
            }

            var col = string.Join(",", tableColumn);
            var val = string.Join(",", tableValue);

            cmd.CommandText = $"INSERT INTO {tableName}({col}) VALUES({val})";

            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            return await cmd.ExecuteNonQueryAsync();
        }

		public static bool IsEntityTracked<T>(this DbContext context, T entity) where T : class
		{
			var entry = context.ChangeTracker.Entries<T>()
				.FirstOrDefault(e => e.Entity == entity ||
									 context.Entry(e.Entity).Property("Id").CurrentValue.Equals(
										 context.Entry(entity).Property("Id").CurrentValue));

			return entry != null;
		}
        //use above that
        //if (!context.IsEntityTracked(parent))
        //{
        //    context.Attach(parent);
        //}
	    // context.Entry(parent).State = EntityState.Modified;


		public static bool IsEntityTrackedById<T>(this DbContext context, object id) where T : class
		{
			var keyProperty = typeof(T).GetProperty("Id");
			if (keyProperty == null) throw new InvalidOperationException("Entity must have an 'Id' property.");

			return context.ChangeTracker.Entries<T>()
				.Any(e => keyProperty.GetValue(e.Entity)?.Equals(id) == true);
		}
        //How to use above extension
        //if (!context.IsEntityTrackedById<Documents>(documentId))
        //{
        //    var parent = new Documents { Id = documentId };
		      //  context.Attach(parent);
        //}

}
}
