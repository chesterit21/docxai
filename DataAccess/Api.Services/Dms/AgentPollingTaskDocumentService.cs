using Api.DataAccess.Models.Dms;
using Api.Repository.Dms;
using Api.Repository.Masters;
using Api.Domain.Constants;
using Api.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Services.Dms
{
    public class AgentPollingTaskDocumentService : BaseService
    {
        private readonly IAgentPollingTaskDocumentRepository _taskRepo;
        private readonly IDocumentExtractedEntitiesRepository _extractedRepo;

        public AgentPollingTaskDocumentService(
            IHttpContextAccessor accessor,
            ILanguageRepository languageRepository,
            IConfiguration configuration,
            IAgentPollingTaskDocumentRepository taskRepo,
            IDocumentExtractedEntitiesRepository extractedRepo)
            : base(accessor, languageRepository, configuration)
        {
            _taskRepo = taskRepo;
            _extractedRepo = extractedRepo;
        }

        public async Task<List<AgentPollingTaskDocument>> GetAllTasks()
        {
            return await _taskRepo.GetAsync();
        }

        public async Task<AgentPollingTaskDocument> GetTaskById(int id)
        {
            return await _taskRepo.GetSingleAsync(x => x.Id.ToString() == id.ToString()); // Handling Guid vs int if needed, but the signature uses Guid.
        }

        public async Task<AgentPollingTaskDocument> InsertTask(AgentPollingTaskDocument entity)
        {
            entity.InsertedBy = GetUser();
            entity.InsertedAt = System.DateTime.Now;
            entity.UpdatedBy = GetUser();
            entity.UpdatedAt = System.DateTime.Now;

            return await _taskRepo.InsertAsync(entity);
        }

        public async Task<AgentPollingTaskDocument> UpdateTask(AgentPollingTaskDocument entity)
        {
            var existing = await _taskRepo.GetSingleAsync(x => x.Id == entity.Id);
            if (existing == null) throw new ApiException("Task not found");

            existing.Status = entity.Status;
            existing.FullPath = entity.FullPath;
            existing.UpdatedBy = GetUser();
            existing.UpdatedAt = System.DateTime.Now;

            return await _taskRepo.UpdateAsync(existing);
        }

        public async Task<DocumentExtractedEntities> InsertExtractedEntity(DocumentExtractedEntities entity)
        {
            return await _extractedRepo.InsertAsync(entity);
        }

        public async Task<List<DocumentExtractedEntities>> GetExtractedEntitiesByDocumentId(int documentId)
        {
            return await _extractedRepo.GetAsync(x => x.DocumentId == documentId);
        }
    }
}
