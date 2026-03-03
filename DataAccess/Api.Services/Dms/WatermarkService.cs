using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Ocsp;
using System;

namespace Api.Services.Masters
{
	public class WatermarkService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IWatermarksRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetAll(RequestWatermarkList request)
		{
			await ValidateInputRequestAsync(request);
			//var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			var result = await repository.GetListWatermarks(request.search, request.Page, request.Limit);
			return result;
		}

		public async Task<Watermarks> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Watermarks by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<Watermarks>> Upsert(List<RequestWatermarks> request)
		{
			await ValidateInputRequestAsync(request);
			var entities = request.CopyProperties<List<Watermarks>>();
			entities = entities.DistinctBy(x => x.Id).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<Watermarks> Insert(RequestAddWatermarks request)
		{
			await ValidateInputRequestAsync(request);
			var entity = request.CopyProperties<Watermarks>();
			await repository.LogTransactionAndAuditTrail($"Insert new Watermarks", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<Watermarks> Update(RequestWatermarks request)
		{
			await ValidateInputRequestAsync(request);
			var entity = request.CopyProperties<Watermarks>();
			await repository.LogTransactionAndAuditTrail($"Update existing Request Watermarks with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.UpdateAsync(entity);
		}

		public async Task<bool> Delete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);

			var entity = await repository.GetSingleAsync( x => x.Id == id );
			await repository.LogTransactionAndAuditTrail($"Soft delete existing Watermarks with id {id}", Domain.Attributes.UserAction.Update, entity);
			
			return await repository.SoftDelete(id);
		}

		public async Task<Watermarks> SoftUndelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);

			var entity = new Watermarks { Id = id };

			await repository.LogTransactionAndAuditTrail($"Soft undelete existing Watermarks with id {id}", Domain.Attributes.UserAction.Update, entity);
			entity.IsActive = false;
			entity.UpdatedAt = DateTime.Now;
			entity.UpdatedBy = GetUser();

			return await repository.UpdateAsync(entity);
			//return await repository.MarkAsNotDeletedAsync(entity);
		}

		private async Task CheckIfExist(int id)
		{
			var any = await repository.AnyAsync(x => x.Id == id);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Id {id}");
			}
		}
	}
}
