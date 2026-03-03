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
using Api.Repository.Dms;
using Api.Repository.Masters;
using Api.Services.Masters;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Api.Services.Dms
{
    public class SharedWorkspaceService : BaseService
    {
        private readonly ILanguageRepository languageRepository;
        private readonly ISharedWorkspaceRepository repositorySharedWorkspac;

        public SharedWorkspaceService(IHttpContextAccessor accessor, ILanguageRepository languageRepository,
			ISharedWorkspaceRepository repositorySharedWorkspac) : base(accessor, repository: languageRepository)
        {
            this.languageRepository = languageRepository;
            this.repositorySharedWorkspac = repositorySharedWorkspac;
        }

		public async Task<object> GetMySharedFiles(RequestWorkspace request)
		{
			await ValidateInputRequestAsync(request);
			var result = await repositorySharedWorkspac.ListMySharedFiles(request.DocumentOrCategoryTitle, request.DocumentOrCategoryDesc, request.Page, request.Limit);
			var resultCount = result.TotalRecord;
			return new ResponsePagination
			{
				TotalRecords = resultCount,
				TotalPages = GetTotalPages(resultCount, request.Limit),
				Data = result.Records
			};

		}


		public async Task<object> GetSharedToMe(RequestWorkspace request)
		{
			await ValidateInputRequestAsync(request);
			var result = await repositorySharedWorkspac.ListSharedToMe(request.DocumentOrCategoryTitle, request.DocumentOrCategoryDesc, request.Page, request.Limit);

			var resultCount = result.TotalRecord;
			return new ResponsePagination
			{
				TotalRecords = resultCount,
				TotalPages = GetTotalPages(resultCount, request.Limit),
				Data = result.Records
			};
		}

		public async Task<int> DeleteCategoryShareToMe(int categoryId)
		{
			await ValidateInputAsync(categoryId);
			Guid trnLogId = await repositorySharedWorkspac.LogTransaction($"Delete Category Share To Me", Domain.Attributes.UserAction.Delete);
			return await repositorySharedWorkspac.RemoveCategoryShare(categoryId);
		}

		public async Task<int> DeleteDocumentShareToMe(int documentId)
		{
			await ValidateInputAsync(documentId);
			await CheckIfDocumentIsShared(documentId);
			Guid trnLogId = await repositorySharedWorkspac.LogTransaction($"Delete Document Share To Me", Domain.Attributes.UserAction.Delete);
			return await repositorySharedWorkspac.RemoveDocumentShare(documentId);
		}

		private async Task CheckIfDocumentIsShared(int documentId) {
			 var isExist = await repositorySharedWorkspac.CheckIfDocumentSharedToLogedUser(documentId);
			if (!isExist)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"Shared Document {message} with Id {documentId} for the user");
			}
		}
	}
}
