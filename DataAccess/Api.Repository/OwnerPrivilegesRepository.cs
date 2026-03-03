using Api.DataAccess;
using Api.DataAccess.Models;
using Api.DataAccess.Models.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Repository
{
	public interface IOwnerPrivilegesRepository<TEntity> : IRepository<TEntity>
	where TEntity : BaseEntity, IHasOwner
	{
		Task<bool> CheckOwnerPrivilegesAsync<TPriv>(int entityId, Func<TPriv, bool> privilegePredicate) where TPriv : class;
	}


	public class OwnerPrivilegesRepository<TEntity> : Repository<TEntity>, IOwnerPrivilegesRepository<TEntity>
	where TEntity : BaseEntity, IHasOwner
	{
		public OwnerPrivilegesRepository(DataContext context, IHttpContextAccessor accessor)
			: base(context, accessor) { }

		public async Task<bool> CheckOwnerPrivilegesAsync<TPriv>(int entityId, Func<TPriv, bool> privilegePredicate) where TPriv : class
		{
			var entity = await context.Set<TEntity>().FindAsync(entityId);
			if (entity == null)
				return false;

			int ownerId = entity.Owner;

			if(ownerId == UserId)
				return true;

			if (typeof(TEntity) == typeof(Categories))
			{
				var priv = await context.Set<CategoriesShared>()
					.FirstOrDefaultAsync(p => p.CategoryID == entityId && p.UserID == ownerId);

				return priv != null && privilegePredicate(priv as TPriv);
			}
			else if (typeof(TEntity) == typeof(Documents))
			{
				var priv = await context.Set<DocumentSharedPrivillege>()
					.FirstOrDefaultAsync(p => p.DocumentShared.DocumentID == entityId && p.UserID == ownerId);

				return priv != null && privilegePredicate(priv as TPriv);
			}

			return false;
		}
	}
}
