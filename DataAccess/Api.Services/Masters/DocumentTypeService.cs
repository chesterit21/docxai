using Api.DataAccess.Models.Masters;
using Api.Repository.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Services.Masters
{
    public class DocumentTypeService
    {
        private readonly IDocumentTypeRepository _documentTypeRepo;
        private readonly IDocumentTypeAttributesRepository _documentTypeAttrRepo;
        private readonly IAttributeSynonymsRepository _attrSynonymsRepo;

        public DocumentTypeService(
            IDocumentTypeRepository documentTypeRepo,
            IDocumentTypeAttributesRepository documentTypeAttrRepo,
            IAttributeSynonymsRepository attrSynonymsRepo)
        {
            _documentTypeRepo = documentTypeRepo;
            _documentTypeAttrRepo = documentTypeAttrRepo;
            _attrSynonymsRepo = attrSynonymsRepo;
        }

        public async Task<TmDocumentType> InsertAsync(TmDocumentType entity)
        {
            return await _documentTypeRepo.InsertAsync(entity);
        }

        public async Task<TmDocumentType> UpdateAsync(TmDocumentType entity)
        {
            return await _documentTypeRepo.UpdateAsync(entity);
        }

        public async Task<bool> DeleteAsync(TmDocumentType entity)
        {
            await _documentTypeRepo.DeleteAsync(entity);
            return true;
        }

        public async Task<List<TmDocumentType>> GetAllAsync()
        {
            return await _documentTypeRepo.GetAsync(null);
        }

        public async Task<TmDocumentType> GetByIdAsync(Guid id)
        {
            return await _documentTypeRepo.GetSingleAsync(x => x.Id == id);
        }
    }
}
