using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.Domain;
using Api.Domain.Constants;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.EntityResponses;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;
using Api.Domain.Formatters;
using Api.Extensions;
using Api.Repository.Masters;
using Api.Services.Masters;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Http;
using NPOI.POIFS.Properties;
using Pipelines.Sockets.Unofficial.Buffers;
using System.Collections.Generic;
using System.Reflection;
using static NPOI.HSSF.Util.HSSFColor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Services.Dms
{
	public class CategoriesService : BaseService
	{
		private readonly ILanguageRepository languageRepository;
		private readonly ICategoryRepository repository;
		private readonly CategoriesService service;
		private readonly IApprovalRepository approvalRepository;
		private readonly int UserId;

		public CategoriesService(IHttpContextAccessor accessor, ILanguageRepository languageRepository,
			ICategoryRepository repository, IApprovalRepository approvalRepository) : base(accessor, repository: languageRepository)
		{
			this.languageRepository = languageRepository;
			this.repository = repository;
			UserId = GetUser();
			this.approvalRepository = approvalRepository;
		}

		public async Task<object> GetAll(RequestCategoryList request, bool isActive)
		{
			await ValidateInputRequestAsync(request);

			List<ResponseCategoryListItem> activeCategories = new List<ResponseCategoryListItem>();
			ResponseCategoriesAndCount response = await repository.GetCategories(request.CategoryName, isActive, request.ParentCategoryId, request.Page, request.Limit);

			var categories = await repository.GetAsync(x => x.IsActive != false);
			activeCategories = categories.ToList().Select(x => new ResponseCategoryListItem() { Id = x.Id, CategoryName = x.CategoryName, CategoryDesc = x.CategoryDesc, Owner = x.Owner, ParentId = x.ParentId }).ToList();

			FillParents(activeCategories, response.Records);

			return new ResponsePagination
			{
				TotalRecords = response.TotalRecord,
				TotalPages = GetTotalPages(response.TotalRecord, request.Limit),
				Data = response.Records
			};
		}

		private void FillParents(List<ResponseCategoryListItem> ownedCategory, List<ResponseCategoryListItem> paginatedCategory, ResponseCategoryListItem categoryTofill = null, ResponseCategoryListItem parent = null)
		{
			if (categoryTofill == null)
			{
				foreach (var category in paginatedCategory)
				{
					if (category.ParentId.HasValue)
					{
						ResponseCategoryListItem x = new ResponseCategoryListItem();
						if (parent != null)
						{
							x = parent;
							x = ownedCategory.Find(c => c.Id == x.ParentId);
						}
						else
						{
							x = ownedCategory.Find(c => c.Id == category.ParentId);
						}

						if (x != null)
						{
							if (categoryTofill == null)
							{
								category.Parents.Add(new ParentCategory { Id = x.Id, Name = x.CategoryName });
							}
							else
							{
								categoryTofill.Parents.Add(new ParentCategory { Id = x.Id, Name = x.CategoryName });
							}

							//Recursively fill parents
							FillParents(ownedCategory, paginatedCategory, category, x);
						}
					}
				}
			}
			else
			{
				if (parent.ParentId.HasValue)
				{
					ResponseCategoryListItem x = new ResponseCategoryListItem();
					if (parent != null)
					{
						x = parent;
						x = ownedCategory.Find(c => c.Id == x.ParentId);
					}

					if (x != null)
					{
						if (categoryTofill != null)
						{
							categoryTofill.Parents.Add(new ParentCategory { Id = x.Id, Name = x.CategoryName });
						}

						x = ownedCategory.Find(c => c.Id == x.Id);
						//Recursively fill parents
						FillParents(ownedCategory, paginatedCategory, categoryTofill, x);
					}
				}
			}
		}

		public async Task<List<ParentCategory>> GetParentHierarchy(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			var parentHierarchy = new List<ParentCategory>();

			var currentCategory = await repository.GetSingleAsync(x => x.Id == categoryId);
			if (currentCategory == null)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"{message}. Category Id {categoryId}");
			}

			while (currentCategory?.ParentId != null)
			{
				currentCategory = await repository.GetSingleAsync(x => x.Id == currentCategory.ParentId);
				if (currentCategory != null)
				{
					parentHierarchy.Add(new ParentCategory
					{
						Id = currentCategory.Id,
						Name = currentCategory.CategoryName
					});
				}
			}

			// Reverse to get root -> child order
			//parentHierarchy.Reverse();
			return parentHierarchy;
		}

		public async Task<ResponseCategoryListItem> Get(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			await repository.LogTransaction($"Get category by id {categoryId}", Domain.Attributes.UserAction.Read);

			//List<Categories> categories = await repository.GetAsync(x => x.Owner == UserId && x.IsActive == true);
			//List<ResponseCategoryListItem> ownedCategories = categories.Select(x => new ResponseCategoryListItem() { Id = x.Id, CategoryName = x.CategoryName, CategoryDesc = x.CategoryDesc, Owner = x.Owner, ParentId = x.ParentId }).ToList();
			
			Categories ctp = await repository.GetSingleAsync(x => x.Id == categoryId);
			ResponseCategoryListItem current = ctp.CopyProperties<ResponseCategoryListItem>();
			current.Parents = await GetParentHierarchy(categoryId);

			//if (ctp.ParentId != null)
			//{
			//	Categories parentctp = await repository.GetSingleAsync(x => x.Id == ctp.ParentId);
			//	ResponseCategoryListItem parentcurrent = parentctp.CopyProperties<ResponseCategoryListItem>();

			//	current.Parents = new List<ParentCategory>()
			//	{
			//		new ParentCategory(){ Id =  parentctp.Id, Name = parentctp.CategoryName }
			//	};

			//	//FillParents(ownedCategories, null, current, parentcurrent);
			//	current.Parents = await GetParentHierarchy(categoryId);
			//}

			return current;
			//return await repository.GetSingleAsync(x => x.Id == categoryId);
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
			return await repository.GetSingleAsync(x => x.CategoryName.Contains(categoryname));
		}


		public async Task<object> Create(RequestCategory request)
		{
			await ValidateInputRequestAsync(request);
			//await CheckIfExistDuplicate(request.Id);
			Categories IsTitleExsit = await repository.GetSingleAsync(x => x.CategoryName.ToLower() == request.CategoryName.ToLower() && x.ParentId == request.ParentId);
			if (IsTitleExsit != null)
			{
				var message = await GetMessage(LangCodes.CategoryTitleExist);
				throw new ApiException(message);
			}

			var entity = request.CopyProperties<Categories>();
			entity.Owner = UserId;
			await repository.LogTransactionAndAuditTrail($"Insert new category", Domain.Attributes.UserAction.Insert, entity);
			Categories model = await repository.InsertAsync(entity);

			DocumentFilesHelper.GetPhysicalPathForCategory(UserId, UserName, model.Id, model.CategoryName, true);
			return model;
		}

		public async Task<CategoriesFavorite> AddFavorite(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			await CheckIfExist(categoryId);
			var isany = await repository.CheckFavorite(categoryId);
			if (isany != null)
			{
				//var message = await GetMessage(LangCodes.NotFound);
				//throw new ApiException($"Category already in favorite. Id {categoryId}");
				return isany;
			}
			await repository.LogTransactionAndAuditTrail($"Add Favorite Category", Domain.Attributes.UserAction.Insert, new Categories() { Id = categoryId });
			return await repository.AddFavorite(categoryId);
		}

		public async Task<int> UnFavorite(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			await CheckIfExist(categoryId);
			await repository.LogTransactionAndAuditTrail($"Remove Favorite Category", Domain.Attributes.UserAction.Insert, new Categories() { Id = categoryId });
			return await repository.RemoveFavorite(categoryId);
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
			Categories categoryExistWithTitle = await repository.GetSingleAsync(x => x.CategoryName.ToLower() == request.CategoryName.ToLower() && x.ParentId == request.ParentId);
			if (categoryExistWithTitle != null )
			{
				if (categoryExistWithTitle.Id != request.Id)
				{
					var message = await GetMessage(LangCodes.CategoryTitleExist);
					throw new ApiException(message);
				}
			}
			var entity = request.CopyProperties<Categories>();
			if (entity.Owner == 0)
				entity.Owner = UserId;
			await repository.LogTransactionAndAuditTrail($"Update new categories", Domain.Attributes.UserAction.Update, entity);
			return await repository.UpdateAsync(entity);
		}


		public async Task<int> AddWorkFlow(RequestCategoryWorkflow workflow)
		{
			await ValidateInputRequestAsync(workflow.Approval);
			await ValidateInputRequestAsync(workflow.Flows);

			int? CategoryID = workflow.Approval.CategoryID;
			Categories ct = await repository.GetSingleAsync(x => x.Id == workflow.Approval.CategoryID);
			ct.Approvals = new List<Approvals>(){ new Approvals()
			{
				CategoryID = CategoryID,
				DocumentID = null,
				Notes = workflow.Approval.Notes,
				MaxStep = workflow.Flows.Count
			} };

			List<ApprovalFlows> listflow = new List<ApprovalFlows>();
			ApprovalFlows flow = null;
			foreach (var item in workflow.Flows)
			{
				flow = new ApprovalFlows { ApproverUserID = item.UserID, Step = item.Step };
				listflow.Add(flow);
			}
			ct.Approvals.FirstOrDefault().ApprovalFlows = listflow;

			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Add Workflow For Category Id : {CategoryID}", Domain.Attributes.UserAction.Update, ct);
			Approvals approval = workflow.Approval.CopyProperties<Approvals>();
			List<ApprovalFlows> Flows = new List<ApprovalFlows>();
			foreach (var item in workflow.Flows)
			{
				ApprovalFlows x = new ApprovalFlows()
				{
					ApproverUserID = item.UserID,
					Step = item.Step,
					InsertedAt = DateTime.Now,
					UpdatedBy = GetUser()
				};

				Flows.Add(x);
			}

			int result = await repository.AddWorkFlow(approval, Flows);
			//Task<int> 
			return result;
		}

		public async Task<List<ResponseApprovalFlow>> GetApprovalFlow(int CategoryId)
		{
			await ValidateInputAsync(CategoryId);
			await CheckIfExist(CategoryId);
			ResponseApprovals approval = await approvalRepository.GetApprovalByCategoryIdAsync(CategoryId);
			List<ResponseApprovalFlow> flows = approval.ApprovalFlows;
			return flows;
		}

		public async Task<ResponseApprovals> GetWorkFlow(int CategoryId)
		{
			await ValidateInputAsync(CategoryId);
			await CheckIfExist(CategoryId);
			ResponseApprovals approval = await approvalRepository.GetApprovalByCategoryIdAsync(CategoryId);
			return approval;
		}

		public async Task<int> DeleteWorkFlow(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			int result = await repository.DeleteWorkFlow(categoryId);
			return result;
		}

		//public async Task<int> DeleteWithChildren(List<string> ids)
		//{
		//    ids.RemoveAll(x => string.IsNullOrWhiteSpace(x));
		//    await ValidateInputAsync(ids);

		//    return await repository.DeleteWithChildren(ids);
		//}

		public async Task<bool> SoftDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);
			await CheckOwnership(id);

			var entity = new Categories { Id = Convert.ToInt32(id) };
			await repository.LogTransactionAndAuditTrail($"Soft delete existing category with id {id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsDeletedAsync(id);
			//return await repository.MarkAsDeletedAsync(new Categories { Id = Convert.ToInt32(id) });
		}

		public async Task<bool> SoftUnDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);
			await CheckOwnership(id);

			var entity = new Categories { Id = Convert.ToInt32(id) };
			await repository.LogTransactionAndAuditTrail($"Undelete category with id {id}", Domain.Attributes.UserAction.Update, entity);
			return await repository.MarkAsUnDeletedAsync(id);
		}

		public async Task<int> HardDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);
			await CheckOwnership(id);

			var entity = await repository.GetSingleAsync(x => x.Id == id);
			await repository.LogTransactionAndAuditTrail($"Flag IsDelete  = 1 for category with id {id}", Domain.Attributes.UserAction.Delete, entity);
			return await repository.DeleteAsync(id);
		}

		public async Task<bool> EmptyRecyclebin()
		{
			await repository.LogTransaction($"Empty recyclebin of category", Domain.Attributes.UserAction.Delete);
			return await repository.EmptyRecyclebin(20);
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

		public async Task<bool> ValidateOwnerPrivileges(int categoryId)
		{
			return await repository.CheckOwnerPrivilegesAsync<CategoriesShared>(
			categoryId,
			priv => priv.IsView && priv.IsEdit && priv.IsDelete);
		}

		private async Task CheckOwnership(int id)
		{
			bool isOwnerNhaveARight = await ValidateOwnerPrivileges(id);
			if (!isOwnerNhaveARight)
			{
				var message = await GetMessage(LangCodes.NotTheOwnerOrHaveNoPriv);
				throw new ApiException(message);
			}
		}

		public async Task<List<Categories>> GetNested()
		{
			await repository.LogTransaction($"Get Category as nested json result", Domain.Attributes.UserAction.Read);
			return await repository.GetNestedCategory();
		}

		public async Task<List<Categories>> GetNestedByCategoryId(int? CategoryId = null)
		{
			await repository.LogTransaction($"Get Category as nested json result", Domain.Attributes.UserAction.Read);
			return await repository.GetNestedCategory(CategoryId);
		}
	}
}
