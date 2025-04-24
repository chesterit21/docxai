using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Services.Masters;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Dms
{
    public class CategoriesService : BaseService
    {
        private readonly ILanguageRepository languageRepository;
        private readonly ICategoryRepository repository;
        private readonly CategoriesService service;

        public CategoriesService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, ICategoryRepository repository) : base(accessor, repository: languageRepository)
        {
            this.languageRepository = languageRepository;
            this.repository = repository;
        }

        public async Task<object> GetAll(ReqestFilter request)
        {
            await ValidateInputRequestAsync(request);
            var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
            return ObjectFlatter.Flatten(result);
        }

        public async Task<Categories> Get(int categoryId)
        {
            await ValidateInputAsync(categoryId);
            await repository.LogTransaction($"Get category by id {categoryId}", Domain.Attributes.UserAction.Read);
            return await repository.GetSingleAsync(x => x.Id == categoryId);
        }

        public async Task<List<Categories>> Get()
        {
            await repository.LogTransaction($"Get all category", Domain.Attributes.UserAction.Read);
            return await repository.GetAsync();
        }

        public async Task<Categories> GetCategory(string categoryname)
        {
            await ValidateInputAsync(categoryname);
            await repository.LogTransaction($"Get category by categoryname {categoryname}", Domain.Attributes.UserAction.Read);
            return await repository.GetSingleAsync( x => x.CategoryName.Contains(categoryname));
        }


        public async Task<object> Create(RequestCategory request)
        {
            await ValidateInputRequestAsync(request);
            await CheckIfExistDuplicate(request.Id);
            var entity = request.CopyProperties<Categories>();
            await repository.LogTransactionAndAuditTrail($"Insert new category", Domain.Attributes.UserAction.Insert, entity);
            return await repository.InsertAsync(entity);
        }
        public async Task<List<Categories>> Upsert(List<Categories> request)
        {
            await ValidateInputRequestAsync(request);
            var entities = request.CopyProperties<List<Categories>>();
            entities = entities.DistinctBy(x => x.Id).ToList();
            await repository.LogTransactionAndAuditTrail($"Upsert new and existing Categories", Domain.Attributes.UserAction.Insert, [.. entities]);

            return await repository.UpsertManyAsync(entities);
        }


        public async Task<Categories> Update(RequestCategory request)
        {
            await ValidateInputRequestAsync(request);
            await CheckIfExistDuplicate(request.Id);

            var entity = request.CopyProperties<Categories>();
            await repository.LogTransactionAndAuditTrail($"Update new categories", Domain.Attributes.UserAction.Update, entity);
            return await repository.UpdateAsync(entity);
        }

        //public async Task<int> DeleteWithChildren(List<string> ids)
        //{
        //    ids.RemoveAll(x => string.IsNullOrWhiteSpace(x));
        //    await ValidateInputAsync(ids);

        //    return await repository.DeleteWithChildren(ids);
        //}

        public async Task<Categories> SoftDelete(int id)
        {
            await ValidateInputAsync(id);
            await CheckIfExist(id);

            var entity = new Categories { Id = Convert.ToInt32(id) };
            await repository.LogTransactionAndAuditTrail($"Soft delete existing category with id {id}", Domain.Attributes.UserAction.Update, entity);

            return await repository.MarkAsDeletedAsync(new Categories { Id = Convert.ToInt32(id) });
        }

        private async Task CheckIfExist(int id)
        {
            var any = await repository.AnyAsync(x => x.Id == id);
            if (!any)
            {
                var message = await GetMessage(LangCodes.NotFound);
                throw new ApiException($"{message}. Category Id {id}");
            }
        }

        private async Task CheckIfExistDuplicate(int id)
        {
            var any = await repository.AnyAsync(x => x.Id == id);
            if (any)
            {
                var message = await GetMessage(LangCodes.Duplicate);
                throw new ApiException($"{message}. Categories Id {id}");
            }
        }
    }
}
