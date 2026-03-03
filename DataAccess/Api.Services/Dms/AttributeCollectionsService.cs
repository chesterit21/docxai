using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Extensions;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Api.Services.Masters
{
	public class AttributeCollectionsService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IAttributeCollectionsRepository repository) : BaseService(accessor, languageRepository)
	{
		public async Task<object> GetListAsync(RequestAttrCollectionList request, bool isActive)
		{
			await ValidateInputRequestAsync(request);

			//var result = await repository.GetListAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			//return ObjectFlatter.Flatten(result);
			ResponseAttributeCollectionPagination respose = await repository.GetListAsync(request.search, isActive, request.Page, request.Limit);
			var totalRecords = respose.TotalRecord;

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = respose.Record
			};
		}

		public async Task<AttributeCollections> Get(Guid Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Attributes by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<AttributeCollections>> GetList()
		{
			await repository.LogTransaction($"Get List Attributes", Domain.Attributes.UserAction.Read);

			return await repository.GetListAttributs();
		}

		public async Task<List<AttributeCollections>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get Attributes page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}

		public async Task<List<AttributeCollections>> Upsert(List<RequestAttributeCollections> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<AttributeCollections>>();
			entities = entities.DistinctBy(x => x.Id).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<AttributeCollections> Insert(RequestAttributeCollections request)
		{
			await ValidateInputRequestAsync(request);

			var entity = request.CopyProperties<AttributeCollections>();
			entity.AttributeElementCollection = request.AttributeElementCollection.ToString();

			await repository.LogTransactionAndAuditTrail($"Insert new Attribute Collections", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<AttributeCollections> Update(RequestUpdateAttributeCollections request)
		{
			await ValidateInputRequestAsync(request);

			var entity = request.CopyProperties<AttributeCollections>();
			entity.AttributeElementCollection = request.AttributeElementCollection.ToString();
			await repository.LogTransactionAndAuditTrail($"Update existing Request Attributes with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.UpdateAsync(entity);
		}

		public async Task<AttributeCollections> SoftDelete(Guid id)
		{
			await ValidateInputAsync(id);

			var entity = new AttributeCollections { Id = id };

			await repository.LogTransactionAndAuditTrail($"Soft delete existing Attributes with id {id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsDeletedAsync(id);
			//return await repository.DeleteAsync(entity);
		}

		public async Task<AttributeCollections> SoftUndelete(Guid id)
		{
			await ValidateInputAsync(id);

			await CheckIfExist(id);

			var entity = new AttributeCollections { Id = id };

			await repository.LogTransactionAndAuditTrail($"Soft undelete existing Attributes with id {id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsUnDeletedAsync(id);
			//return await repository.MarkAsNotDeletedAsync(entity);
		}

		private async Task CheckIfExist(Guid id)
		{
			var any = await repository.AnyAsync(x => x.Id == id);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Id {id}");
			}
		}

		public async Task<int> Harddelete(Guid id)
		{
			await ValidateInputAsync(id);

			await CheckIfExist(id);

			//var entity = new AttributeCollections { Id = id };
			var entity = await repository.GetSingleAsync(x => x.Id == id);
			await repository.LogTransactionAndAuditTrail($"Soft undelete existing Collections Attribute with id {id}", Domain.Attributes.UserAction.Update, entity);
			//return await repository.DeleteAsync(entity);
			return await repository.DeleteAsync(id);
			//return await repository.MarkAsNotDeletedAsync(entity);
		}

		public async Task<bool> EmptyRecyclebin()
		{
			await repository.LogTransaction($"Empty recyclebin of collection attribute", Domain.Attributes.UserAction.Delete);
			return await repository.EmptyRecyclebin(20);
		}
	}
}
