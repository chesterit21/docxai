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
	public class AttributesService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IAttributesRepository repository) : BaseService(accessor, languageRepository)
	{
		//public async Task<object> GetAll(RequestFilter request)
		//{
		//	await ValidateInputRequestAsync(request);

		//	//var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
		//	//return ObjectFlatter.Flatten(result);
		//	var totalRecords = await repository.CountAsync(x => x.IsActive == true);
		//	var records = await repository.GetAsync(x => x.IsActive == true, request.Page, request.Limit);

		//	return new ResponsePagination
		//	{
		//		TotalRecords = totalRecords,
		//		TotalPages = GetTotalPages(totalRecords, request.Limit),
		//		Data = records
		//	};
		//}

		public async Task<object> GetAll(RequestAttrList request, bool isActive)
		{
			await ValidateInputRequestAsync(request);
			ResponseAttributePagination respose = await repository.GetListAsync(request.search, isActive, request.Page, request.Limit);
			var totalRecords = respose.TotalRecord;

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = respose.Record
			};

			//var result = await repository.GetAsync(x => x.IsActive == true, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			//return ObjectFlatter.Flatten(result);
		}

		public async Task<Attributes> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Attributes by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<Attributes>> GetList()
		{
			await repository.LogTransaction($"Get List Attributes", Domain.Attributes.UserAction.Read);

			return await repository.GetListAttributs();
		}

		public async Task<List<Attributes>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get Attributes page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}

		public async Task<List<Attributes>> Upsert(List<RequestAttributes> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<Attributes>>();
			entities = entities.DistinctBy(x => x.Id).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<Attributes> Insert(RequestAddAttributes request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfAttributeNameExist(request.AttributeName);

			var json = JsonConvert.DeserializeObject<JsonAttribute>(request.AttributeElement.ToString());
			var entity = request.CopyProperties<Attributes>();
			entity.AttributeType = json.type;
			entity.AttributeElement = request.AttributeElement.ToString();
			//entity.AttributeMaxLength = json.maxlength ?? 0;

			await repository.LogTransactionAndAuditTrail($"Insert new Attributes", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<Attributes> Update(RequestAttributes request)
		{
			await ValidateInputRequestAsync(request);

			var json = JsonConvert.DeserializeObject<JsonAttribute>(request.AttributeElement.ToString());
			var entity = request.CopyProperties<Attributes>();
			entity.AttributeType = json.type;
			entity.AttributeElement = request.AttributeElement.ToString();
			//entity.AttributeMaxLength = json.maxlength ?? 0;
			await repository.LogTransactionAndAuditTrail($"Update existing Request Attributes with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);
			var exA = repository.GetSingleAsync(x => x.Id == request.Id).Result;
			if (exA.AttributeName != request.AttributeName)
			{
				await CheckIfAttributeNameExist(request.AttributeName);
			}

			entity.IsSystem = exA.IsSystem;

			return await repository.UpdateAsync(entity);
		}

		public async Task<Attributes> SoftDelete(int id)
		{
			await ValidateInputAsync(id);
			var entity = new Attributes { Id = id, IsActive = false };

			var existEntity = await repository.GetSingleAsync(x => x.Id == id);
			var jsonAttribute = JsonConvert.DeserializeObject<JsonAttribute>(existEntity.AttributeElement);
			var name = jsonAttribute.name;

			var element = JObject.Parse(existEntity.AttributeElement);
			if (name == "expired_date")
			{
				throw new ApiException("Expired Date cannot be delete, used by system.");
			}
			else if (name == "days_of_reminder")
			{
				throw new ApiException($"Days of reminder cannot be delete, used by system.");
			}

			await repository.LogTransactionAndAuditTrail($"Inactivated Attributes with id {id}", Domain.Attributes.UserAction.Delete, entity);
			//Attributes existData = await repository.GetSingleAsync(x => x.Id == id);
			//if (existData != null)
			//{
			//	existData.IsActive = false;
			//}
			return await repository.MarkAsDeletedAsync(id);
			//return await repository.UpdateAsync(existData);
		}

		public async Task<Attributes> SoftUndelete(int id)
		{
			await ValidateInputAsync(id);

			await CheckIfExist(id);

			var entity = new Attributes { Id = id };

			await repository.LogTransactionAndAuditTrail($"Activated Attributes with id {id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsUnDeletedAsync(id);
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

		private async Task CheckIfAttributeNameExist(string attributeName)
		{
			var any = await repository.AnyAsync(x => x.AttributeName.ToLower() == attributeName.ToLower());
			if (any)
			{
				var message = await GetMessage(LangCodes.AttributeNameExist);
				throw new ApiException($"{message}. ({attributeName})");
			}
		}


		public async Task<int> HardDelete(int id)
		{
			await ValidateInputAsync(id);

			await CheckIfExist(id);

			//var entity = new Attributes { Id = id };
			var entity = await repository.GetSingleAsync(x => x.Id == id);
			await repository.LogTransactionAndAuditTrail($"Hard Delete Attributes with id {id}", Domain.Attributes.UserAction.Delete, entity);
			//return await repository.DeleteAsync(entity);
			return await repository.DeleteAsync(id);
		}

		public async Task<bool> EmptyRecyclebin()
		{
			await repository.LogTransaction($"Empty recyclebin of collection attribute", Domain.Attributes.UserAction.Delete);
			return await repository.EmptyRecyclebin(20);
		}
	}
}
