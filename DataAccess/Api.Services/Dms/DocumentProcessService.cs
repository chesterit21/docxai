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
using System.Net;

namespace Api.Services.Dms
{
    public class DocumentProcessService : BaseService
    {
        private readonly ILanguageRepository languageRepository;
        private readonly IDropdownRepository repository;
        private readonly IUserRepository repositoryUser;
        private readonly IRoleRepository repositoryRole;
        private readonly ICompanyRepository repositoryCompany;
        private readonly IWatermarksRepository repositoryWatermarks;
		private readonly IGroupRepository repositoryGroup;
		private readonly IDocumentsRepository documentsRepo;
		private readonly DropdownService service;
		private readonly IAttributesRepository attributesRepo;
		private readonly IAttributeCollectionsRepository attributeCollectionsRepo;

        public DocumentProcessService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, 
            IDropdownRepository repository, IUserRepository repositoryUser,
            IRoleRepository repositoryRole, ICompanyRepository repositoryCompany, IWatermarksRepository repositoryWatermarks,
			IGroupRepository repositoryGroup, IDocumentsRepository documentsRepo, IAttributesRepository attributesRepo,
			IAttributeCollectionsRepository attributeCollectionsRepo) : base(accessor, repository: languageRepository)
        {
            this.languageRepository = languageRepository;
            this.repository = repository;
            this.repositoryUser = repositoryUser;
            this.repositoryRole = repositoryRole;
            this.repositoryCompany = repositoryCompany;
            this.repositoryWatermarks = repositoryWatermarks;
			this.repositoryGroup = repositoryGroup;
			this.documentsRepo = documentsRepo;
			this.attributesRepo = attributesRepo;
			this.attributeCollectionsRepo = attributeCollectionsRepo;
		}
		public async Task<bool> CreateFiles(byte[] base64, string docFolder, string oldFilePath, string newFilePath, string currentFilePath, string user, string pass, CancellationToken token)
		{
			try
			{
				//var user = _config["FileOptions:AccessKey"];
				//var pass = _config["FileOptions:SecretKey"];
				if (!string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(pass))
				{
					NetworkCredential testCreds = new(user, pass);
					CredentialCache testCache = new()
					{
						{new Uri(docFolder), "Basic", testCreds}
					};
				}

				if (!Directory.Exists(docFolder) && !string.IsNullOrEmpty(docFolder))
					Directory.CreateDirectory(docFolder);

				if (File.Exists(oldFilePath) && !string.IsNullOrEmpty(oldFilePath) && !string.IsNullOrEmpty(newFilePath))
					File.Move(oldFilePath, newFilePath, true);

				if (!string.IsNullOrEmpty(currentFilePath))
					await File.WriteAllBytesAsync(currentFilePath, base64, token).ConfigureAwait(false);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public async Task<object> GetDropdownAttributeCollection(RequestAttributeCollectionsDropdown request)
		{
			return await attributeCollectionsRepo.GetDropdownAttributeCollection(request.search);
		}
	}
}
