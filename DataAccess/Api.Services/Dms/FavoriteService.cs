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
using Api.Repository;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Api.Services.Masters
{
	public class FavoriteService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, ICategoriesFavoriteRepository categoriesFavRepo, IDocumentFavoriteRepository documentFavRepo) :
		BaseService(accessor, languageRepository)
	{
		public async Task<object> GetFavoriteCategories(RequestFavoritesCategories request)
		{
			await ValidateInputRequestAsync(request);

			//var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			//return ObjectFlatter.Flatten(result);

			long totalRecords = 0;
			//List<ResponseCategoriesFavorite> records = null;

			ResponseCategoriesFavoriteAndCount recordsAndCount = await categoriesFavRepo.GetAsync(request.CategoryName, request.Page, request.Limit);
			//totalRecords = await categoriesFavRepo.GetCountAsync(request.CategoryName);
			totalRecords = recordsAndCount.TotalRecord;

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = recordsAndCount.Records
			};
		}

		public async Task<object> GetFavoriteDocument(RequestFavoritesDocument request)
		{
			await ValidateInputRequestAsync(request);

			//var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			//return ObjectFlatter.Flatten(result);
			long totalRecords = 0;
			//List<ResponseDocumentsFavorite> records = null;

			ResponseDocumentsFavoriteAndCount result = await documentFavRepo.GetAsync(request.DocumentTitle, request.Page, request.Limit);
			totalRecords = result.TotalRecord;
			//totalRecords = await documentFavRepo.GetCountAsync(request.DocumentTitle);

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = result.Records
			};
		}

		//public async Task<Attributes> Get(int Id)
		//{
		//	await ValidateInputAsync(Id);

		//	await repository.LogTransaction($"Get Attributes by Id {Id}", Domain.Attributes.UserAction.Read);

		//	return await repository.GetSingleAsync(x => x.Id == Id);
		//}

		//public async Task<List<Attributes>> GetList()
		//{
		//	await repository.LogTransaction($"Get List Attributes", Domain.Attributes.UserAction.Read);

		//	return await repository.GetListAttributs();
		//}

		//public async Task<List<Attributes>> GetAll(int page, int limit)
		//{
		//	await ValidateInputAsync([page, limit]);

		//	await repository.LogTransaction($"Get Attributes page {page} limit {limit}", Domain.Attributes.UserAction.Read);

		//	return await repository.GetAsync(page, limit);
		//}

		//public async Task<List<Attributes>> Upsert(List<RequestAttributes> request)
		//{
		//	await ValidateInputRequestAsync(request);

		//	var entities = request.CopyProperties<List<Attributes>>();
		//	entities = entities.DistinctBy(x => x.Id).ToList();

		//	return await repository.UpsertManyAsync(entities);
		//}

		//public async Task<Attributes> Insert(RequestAttributes request)
		//{
		//	await ValidateInputRequestAsync(request);

		//	var json = JsonConvert.DeserializeObject<JsonAttribute>(request.AttributeElement.ToString());
		//	var entity = request.CopyProperties<Attributes>();
		//	entity.AttributeType = json.type;
		//	entity.AttributeElement = request.AttributeElement.ToString();
		//	entity.AttributeMaxLength = json.maxlength ?? 0;

		//	await repository.LogTransactionAndAuditTrail($"Insert new Attributes", Domain.Attributes.UserAction.Insert, entity);

		//	return await repository.InsertAsync(entity);
		//}

		//public async Task<Attributes> UpdateDocument(RequestAttributes request)
		//{
		//	await ValidateInputRequestAsync(request);

		//	var json = JsonConvert.DeserializeObject<JsonAttribute>(request.AttributeElement.ToString());
		//	var entity = request.CopyProperties<Attributes>();
		//	entity.AttributeType = json.type;
		//	entity.AttributeElement = request.AttributeElement.ToString();
		//	entity.AttributeMaxLength = json.maxlength ?? 0;
		//	await repository.LogTransactionAndAuditTrail($"UpdateDocument existing Request Attributes with id {entity.Id}", Domain.Attributes.UserAction.UpdateDocument, entity);

		//	return await repository.UpdateAsync(entity);
		//}

		//public async Task<Attributes> SoftDelete(int id)
		//{
		//	await ValidateInputAsync(id);

		//	var entity = new Attributes { Id = id };

		//	await repository.LogTransactionAndAuditTrail($"Soft delete existing Attributes with id {id}", Domain.Attributes.UserAction.UpdateDocument, entity);

		//	return await repository.MarkAsDeletedAsync(entity);
		//}

		//public async Task<Attributes> SoftUndelete(int id)
		//{
		//	await ValidateInputAsync(id);

		//	await CheckIfExist(id);

		//	var entity = new Attributes { Id = id };

		//	await repository.LogTransactionAndAuditTrail($"Soft undelete existing Attributes with id {id}", Domain.Attributes.UserAction.UpdateDocument, entity);

		//	return await repository.MarkAsNotDeletedAsync(entity);
		//}

		//private async Task CheckIfExist(int id)
		//{
		//	var any = await repository.AnyAsync(x => x.Id == id);
		//	if (!any)
		//	{
		//		var message = await GetMessage(LangCodes.NotFound);
		//		throw new ApiException($"{message}. Id {id}");
		//	}
		//}
	}
}
