using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
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
using Api.Repository.Dms;
using Api.Repository.Masters;
using Api.Repository.Systems;
using BitMiracle.LibTiff.Classic;
using iText.Kernel.Geom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Index.HPRtree;
using NetTopologySuite.Mathematics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NPOI.HPSF;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Math;
using SixLabors.Fonts;
//using SixLabors.Fonts;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Drawing.Processing; // For drawing extensions
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Syncfusion.DocIO.DLS;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Environment;
using Path = System.IO.Path;
//using iText.Kernel.Pdf;

namespace Api.Services.Dms
{
	public class DocumentsService(IHttpContextAccessor accessor, ILanguageRepository languageRepository, IConfiguration configuration,
		IDocumentsRepository repository, IDocumentSharedRepository docSharedRepo, IDocumentFilesRepository docFilesRepo, 
		IDocumentAttributesRepository docAttributeRepo, IDocumentSharedRepository docShareRepo, IApprovalRepository approvalRepo, 
		IApprovalFlowsRepository approvalFlowRepo, IApprovalActivityRepository approvalActivityRepo, INotificationRepository notifRepo,
		ICategoriesSharedRepository categorySharedRepo, IDocumentsItemlistRepository docItemListRepository, ICategoryRepository categoryRepo, 
		IEmailRepository emailRepo, IUserRepository userRepo, CategoriesService categoryService) : BaseService(accessor, languageRepository, configuration)

	{
		private static readonly string[] Mimes = { "image/jpg", "image/jpeg", "image/png", "image/bmp" };

		public async Task<object> GetAll(RequestFilter request)
		{
			await ValidateInputRequestAsync(request);

			var result = await repository.GetAsync(null, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			return ObjectFlatter.Flatten(result);
		}

		public async Task<Documents> Get(int Id)
		{
			await ValidateInputAsync(Id);

			await repository.LogTransaction($"Get Documents by Id {Id}", Domain.Attributes.UserAction.Read);

			return await repository.GetSingleAsync(x => x.Id == Id);
		}

		public async Task<List<Documents>> GetAll(int page, int limit)
		{
			await ValidateInputAsync([page, limit]);

			await repository.LogTransaction($"Get Documents page {page} limit {limit}", Domain.Attributes.UserAction.Read);

			return await repository.GetAsync(page, limit);
		}

		public async Task<object> GetDocumentLogs(RequestDocumentLog request)
		{
			await ValidateInputRequestAsync(request);
			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Read Document Log Document ID : {request.DocumentId}", Domain.Attributes.UserAction.Read);

			var records = await repository.GetListDocumentLog(request.DocumentId, request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			var totalRecords = await repository.GetListDocumentLogCount(request.DocumentId, request.FilterBy, request.FilterValue);

			await repository.LogDocumentInsert(trnLogId, request.DocumentId, LogDocumentAction.ViewDocumentLog);

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = records
			};
		}

		public async Task<List<Documents>> Upsert(List<RequestDocuments> request)
		{
			await ValidateInputRequestAsync(request);

			var entities = request.CopyProperties<List<Documents>>();
			entities = entities.DistinctBy(x => x.Id).ToList();

			return await repository.UpsertManyAsync(entities);
		}

		public async Task<Documents> Insert(RequestDocuments request)
		{
			await ValidateInputRequestAsync(request);

			await CheckIfExist(request.Id);

			var entity = request.CopyProperties<Documents>();

			await repository.LogTransactionAndAuditTrail($"Insert new Documents", Domain.Attributes.UserAction.Insert, entity);

			return await repository.InsertAsync(entity);
		}

		public async Task<ResponseDocument> InsertWithDetail(RequestDocuments request)
		{
			await ValidateInputRequestAsync(request);
			//validate title document
			Documents IsTitleExsit = await repository.GetSingleAsync(x => x.DocumentTitle == request.DocumentTitle);
			if (IsTitleExsit != null)
			{
				var message = await GetMessage(LangCodes.DocumentTitleExist);
				throw new ApiException(message);
			}

			//await repository.LogTransactionAndAuditTrail($"Insert new Documents", Domain.Attributes.UserAction.Insert, request);
			return await repository.SaveWithChild(request);
		}

		public static bool IsValidJson(string jsonString)
		{
			try
			{
				JToken.Parse(jsonString);
				return true;
			}
			catch (JsonReaderException)
			{
				return false;
			}
		}

		public async Task<Documents> UpdateDocument(RequestUpdateDocuments request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.Id);
			if (!IsValidJson(request.DocumentAttributes))
				throw new ApiException("Document attributes string was incorrect json");

			var entity = request.CopyProperties<Documents>();

			var jsonDocumentAttributes = JsonConvert.DeserializeObject<List<DocumentAttribute>>(request.DocumentAttributes);
			//foreach (var attr in list)
			//{
			//	var elementObj = JsonConvert.DeserializeObject<dynamic>(attr.attributeElement);
			//	// Use elementObj.type, elementObj.label, etc.
			//}
			DateTime? expiredDate = null;
			int? daysOfReminder = null;

			foreach (var attr in jsonDocumentAttributes)
			{
				var element = JObject.Parse(attr.attributeElement);
				var name = element["name"]?.ToString();

				if (name == "expired_date")
				{
					expiredDate = DateTime.Parse(attr.value.ToString()).Date;
					if (expiredDate < DateTime.Today)
						throw new ApiException("Expired Date cannot be earlier or same than today date.");

					entity.ExpiryDate = expiredDate;
				}
				else if (name == "days_of_reminder")
				{
					if (int.TryParse(attr.value.ToString(), out int days))
					{
						daysOfReminder = days;
						if (daysOfReminder > Int16.MaxValue)
							throw new ApiException($"Days of reminder cannot more than {Int16.MaxValue}");

						entity.ReminderDays = (Int16)daysOfReminder;
					}
				}
			}

			var exisitngDocument = await repository.GetSingleAsync(x => x.Id == entity.Id);
			exisitngDocument.DocumentTitle = entity.DocumentTitle;
			exisitngDocument.DocumentDesc = entity.DocumentDesc;
			exisitngDocument.CategoryID = entity.CategoryID;
			exisitngDocument.Owner = GetUser();
			if (entity.ExpiryDate != null)
				exisitngDocument.ExpiryDate = entity.ExpiryDate;
			exisitngDocument.ReminderDays = entity.ReminderDays;

			//cek status approval, jika on prgoress kasih alert
			var approval = await approvalRepo.GetSingleAsync(d => d.DocumentID == request.Id);
			if (approval != null)
			{
				if (approval.Status == Domain.Enum.ApprovalStatusEnum.Pending)
				{
					var message = await GetMessage(LangCodes.PendingDocument);
					throw new ApiException(message + $". Document ID : {request.Id}");
				}
			}

			List<DocumentRelated> reletedDocumentIds = new List<DocumentRelated>();
			if (request.RelatedDocumentIDs.Count > 0)
			{
				var isSameWithDocument = request.RelatedDocumentIDs.Any(v => v == entity.Id);
				if (isSameWithDocument)
				{
					var message = await GetMessage(LangCodes.DocumentSameReference);
					throw new ApiException(message);
				}

				DocumentRelated docRelated = null;
				foreach (var item in request.RelatedDocumentIDs)
				{
					docRelated = new DocumentRelated()
					{
						DocumentID = entity.Id,
						RelatedDocumentID = item,
						InsertedAt = DateTime.Now,
						InsertedBy = GetUser()
					};
					reletedDocumentIds.Add(docRelated);
				}
			}

			//Api.Domain.Enum.PendingApproval
			//approvalRepo context.ApprovalFlows.Where(x => x.ApprovalID == approvalId).ToList();

			await repository.LogTransactionAndAuditTrail($"Update existing request Documents with id {entity.Id}", Domain.Attributes.UserAction.Update, entity);

			DocumentAttributes attributes = await docAttributeRepo.GetSingleAsync(x => x.DocumentID == entity.Id);
			if (attributes == null)
			{
				attributes = new DocumentAttributes() { DocumentID = entity.Id, AttributeValues = request.DocumentAttributes, InsertedAt = DateTime.Now, InsertedBy = GetUser() };
				await docAttributeRepo.InsertAsync(attributes);
			}
			else
			{
				attributes.AttributeValues = request.DocumentAttributes;
				await docAttributeRepo.UpdateAsync(attributes);
			}

			//var jsonAttr = Newtonsoft.Json.JsonConvert.DeserializeObject(docAttributeRepo.GetSingleAsync(x => x.DocumentIDs == request.Id).Result.AttributeValues);

			//Draft Related Document
			if (reletedDocumentIds.Count > 0)
				await repository.SaveInfoRelatedDocument(reletedDocumentIds, entity.Id);

			if (approval != null)
			{
				//Approval Activity
				List<ApprovalFlows> flows = await approvalFlowRepo.GetAsync(x => x.ApprovalID == approval.Id);
				var SortedApprovalFlow = flows.OrderBy(x => x.Step);
				ApprovalFlows fl = SortedApprovalFlow.FirstOrDefault();
				int targetApproverUserID = fl.ApproverUserID;

				ApprovalActivities appActivity = new ApprovalActivities()
				{
					ApprovalID = approval.Id,
					ApprovalActivity = ApprovalActivity.Submit,
					ApprovalActivityName = ApprovalActivity.Submit.ToString(),
					CurrentStep = 0,
					NextStep = fl.Step,
					InsertedAt = DateTime.Now,
					InsertedBy = GetUser()
				};
				await approvalActivityRepo.InsertAsync(appActivity);

				Notifications notifToNexApprover = new Notifications()
				{
					DocumentID = entity.Id,
					NotificationType = (short)NotificationType.Approval,
					NotifAction = "-",
					NotifContent = "This Document Need Your Approval",
					NotifDescription = "Need your approval asap",
					TargetActor = targetApproverUserID,
					InsertedAt = DateTime.Now,
					InsertedBy = GetUser()
				};
				await notifRepo.InsertAsync(notifToNexApprover);

				approval.CurrentApproverUserID = targetApproverUserID;
				approval.NextApproverUserID = SortedApprovalFlow.Skip(1).FirstOrDefault() != null ? SortedApprovalFlow.Skip(1).FirstOrDefault().ApproverUserID : (int?)null;
				approval.CurrentStep = fl.Step;
				//approval.CurrentApproverUserID = null;
				//approval.NextApproverUserID = targetApproverUserID;
				//approval.CurrentStep = null;
				approval.Status = Domain.Enum.ApprovalStatusEnum.Pending;
				approval.LastActivityDate = DateTime.Now;
				approval.LastRemark = "Submitted";
				await approvalRepo.UpdateAsync(approval);


				User nextAppUser = await userRepo.GetSingleAsync(x => x.UserId == targetApproverUserID);
				User owner = await userRepo.GetSingleAsync(x => x.UserId == exisitngDocument.InsertedBy);

				Email email = new Email()
				{
					Subject = "Approval Notification",
					To = nextAppUser.EmailAddress,
					Cc = owner.EmailAddress,
					Body = $"Please be inform, that any document need your approval. ID document : {exisitngDocument.Id} with title document : {exisitngDocument.DocumentTitle}",
					IsHtml = true
				};
				await emailRepo.InsertAsync(email);

			}

			Documents document = await repository.UpdateAsync(exisitngDocument);
			return document;
		}

		public async Task<bool> SoftDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExist(id);
			//await CheckOwnership(id);
			//await CheckOwnershipAndPrivilegesAsync(id);
			await ValidateOwnerOrFullPrivileges(id);

			var entity = new Documents { Id = id };
			await repository.LogTransactionAndAuditTrail($"Soft delete existing Documents with id {id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsDeletedAsync(id);
		}

		public async Task<bool> SoftUndelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExistInRecycke(id);

			var entity = new Documents { Id = id };
			await repository.LogTransactionAndAuditTrail($"Soft undelete existing Documents with id {id}", Domain.Attributes.UserAction.Update, entity);

			return await repository.MarkAsUnDeletedAsync(id);
			//return await repository.MarkAsNotDeletedAsync(entity);
		}

		public async Task<int> HardDelete(int id)
		{
			await ValidateInputAsync(id);
			await CheckIfExistInRecycke(id);
			await CheckOwnershipAndPrivilegesAsync(id);

			var entity = await repository.GetSingleAsync(x => x.Id == id);
			await repository.LogTransactionAndAuditTrail($"Hard Delete Document with id {id}", Domain.Attributes.UserAction.Delete, entity);
			return await repository.DeleteAsync(id);
		}

		private async Task CheckIfExist(int id)
		{
			var any = await repository.AnyAsync(x => x.Id == id && x.IsActive == true);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"Document {message}. Id {id}");
			}
		}
		private async Task CheckIfExistInRecycke(int id)
		{
			var any = await repository.AnyAsync(x => x.Id == id && x.IsActive == false);
			if (!any)
			{
				var message = await GetMessage(LangCodes.NotFound);
				throw new ApiException($"Document {message}. Id {id}");
			}
		}

		private async Task CheckOwnership(int id)
		{
			var any = await repository.GetSingleAsync(x => x.Id == id);
			if (any != null)
			{
				if (any.Owner != GetUser())
				{
					var message = await GetMessage(LangCodes.NotTheOwner);
					throw new ApiException(message);
				}
			}
		}

		public async Task<bool> ValidateOwnerOrFullPrivileges(int documentId)
		{
			return await repository.CheckOwnerPrivilegesAsync<DocumentSharedPrivillege>(
				documentId,
				priv => priv.IsView && priv.IsEdit && priv.IsDelete
			);
		}
		public async Task<bool> ValidateOwnerOrIsReadPrivileges(int documentId)
		{
			return await repository.CheckOwnerPrivilegesAsync<DocumentSharedPrivillege>(
				documentId,
				priv => priv.IsView
			);
		}
		public async Task<bool> ValidateOwnerOrIsEditPrivileges(int documentId)
		{
			return await repository.CheckOwnerPrivilegesAsync<DocumentSharedPrivillege>(
				documentId,
				priv => priv.IsView && priv.IsEdit
			);
		}

		private async Task CheckOwnershipAndPrivilegesAsync(int id)
		{
			var any = await repository.CheckOwnershipAndPrivilegesAsync(id);
			if (any == false)
			{
				var message = await GetMessage(LangCodes.NotTheOwnerOrHaveNoPriv);
				throw new ApiException(message);
			}
		}

		public async Task<ResponseDocument> GetDocumentDetail(int documentId, string mode = "edit")
		{
			await ValidateInputAsync(documentId);
			await CheckIfExist(documentId);

			//ResponseDocumentAttributes> GetDocumentAttributeAsync(int DocumentIDs)
			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Get Document Detail", Domain.Attributes.UserAction.Read, new Documents() { Id = documentId });
			if(mode == "view")
				await repository.LogDocumentInsert(trnLogId, documentId, LogDocumentAction.ViewDocument);
			var docDetail = await repository.GetDetailDocumentAsync(documentId);

			if (docDetail != null)
			{
				docDetail.ParentsCategory = await categoryService.GetParentHierarchy(docDetail.CategoryID);
				docDetail.SharedTo = await docSharedRepo.GetSharedUsersByDocumentIDAsync(documentId);
				docDetail.Workflow = await approvalRepo.GetApprovalByDocumentIdAsync(documentId);
				docDetail.DocumentFiles = await docFilesRepo.GetFileFromDocumentAsync(documentId);
				docDetail.DocumentAttributes = await docAttributeRepo.GetDocumentAttributeAsync(documentId);
				docDetail.DocumentRelated = await repository.GetRelatedDocumentsAsync(documentId);
			}
			if (docDetail.DocumentFiles.Count > 0)
			{
				docDetail.MainDocumentFile = docDetail.DocumentFiles.FirstOrDefault(x => x.IsMainDocumentFile == true);
				docDetail.FileSize = SizeFormatter.SizeSuffix(docDetail.DocumentFiles.Sum(x => x.DocumentFileSizeInt), 2);
			}

			return docDetail;
		}

		public async Task<DocumentFiles> UploadNewVersionofFile(int documentId, IFormFile file, bool isMainDocumentFile)
		{
			await ValidateInputAsync(documentId);
			//await ValidateInputAsync(isMainDocumentFile);
			await ValidateUploadAsync(file);
			//await IsDocumentFileNameWasExist(documentId, file);
			var ifileName = Path.GetFileNameWithoutExtension(file.FileName);
			if (string.IsNullOrEmpty(ifileName))
			{
				throw new ApiException("File name cannot be empty.");
			}
			await ValidateOwnerOrIsEditPrivileges(documentId);

			var fileName = Path.GetFileNameWithoutExtension(file.FileName);
			var fileExtension = Path.GetExtension(file.FileName);
			string newFileName = Guid.NewGuid().ToString() + fileExtension;
			var model = await repository.GetInfoCategoryDocumentAsync(documentId);			

			var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(GetUser(), UserName, model.Id, model.CategoryName, newFileName, true);
			var resourcePath = DocumentFilesHelper.GetResourcePathForDocumentFile(GetUser(), UserName, model.Id, model.CategoryName, newFileName);

			var existDocs = await docFilesRepo.GetAsync(df => df.DocumentID == documentId && df.DocumentFileName == fileName && df.IsActive == true && df.IsDeleted == false);
			string afileName = string.Empty;
			if (existDocs.Count > 0)
			{
				int versionNo = existDocs.Count;
				if (versionNo > 1)
					versionNo += 1;

				afileName = $"V_{versionNo}_" + fileName;
			}
			else
			{
				afileName = fileName;
			}

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			// 1. Ekstraksi Teks
			string fullText = string.Empty;
			try
			{
				fullText = await FileProcessor.ExtractTextAsync(filePath);
			}
			catch (Exception) { }
			FileInfo fin = new FileInfo(filePath);
			DocumentFiles fl = new DocumentFiles()
			{
				DocumentID = documentId,
				DocumentType = fin.Extension,
				DocumentFileName = afileName,
				NewDocumentFileName = fin.Name,
				//DocumentFilePath = fin.FullName,
				//DocumentFilePath = $"/resource/{model.Id}-{model.CategoryName.Trim()}/{fin.Name}",
				DocumentFilePath = resourcePath,
				DocumentFileSize = (int)fin.Length,
				IsMainDocumentFile = isMainDocumentFile,
				InsertedBy = GetUser(),
				InsertedAt = DateTime.Now,
				UpdatedBy = GetUser(),
				UpdatedAt = DateTime.Now,
				DocumentFileContent = fullText
			};

			if (isMainDocumentFile == true)
			{
				List<DocumentFiles> dcfs = await docFilesRepo.GetAsync(x => x.DocumentID == documentId);
				foreach (var item in dcfs)
				{
					item.IsMainDocumentFile = false;
				}
				await docFilesRepo.UpdateManyAsync(dcfs);
			}

			var mim = file.ContentType;
			if (Mimes.Contains(mim))
			{
				MemoryStream memoryStream;

				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					memoryStream = new MemoryStream();
					fs.CopyTo(memoryStream);
				}

				memoryStream.Position = 0; // Reset position before passing

				var cc = ProcessingOCR(memoryStream, filePath, fileExtension);
				fl.DocumentFileContent = cc;
			}

			fl = await docFilesRepo.InsertAsync(fl);

			Guid trnLogId = await docFilesRepo.LogTransactionAndAuditTrail($"Upload New Version Document for Document ID : {documentId}", Domain.Attributes.UserAction.Update, fl);
			await repository.LogDocumentInsert(trnLogId, documentId, LogDocumentAction.UplaodNewVersion);

			return fl;
		}

		public async Task<int> SetReminderDocument(int DocumentId, short? days, DateTime? datetimeReminder)
		{
			await ValidateInputAsync(DocumentId);
			await CheckIfExist(DocumentId);
			await ValidateOwnerOrIsEditPrivileges(DocumentId);

			Documents doc = new Documents() { Id = DocumentId };
			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Add Reminder For Document ID : {DocumentId}", Domain.Attributes.UserAction.Update, doc);
			await repository.LogDocumentInsert(trnLogId, DocumentId, LogDocumentAction.AddReminder);

			return await repository.SetReminderDocument(DocumentId, days, datetimeReminder);
		}

		public async Task<DocumentFiles> SetMainFileDocument(RequestDocumentFilesUpdateMainDocument dcf)
		{
			await CheckIfExist(dcf.DocumentID);
			await ValidateInputRequestAsync(dcf);
			await ValidateOwnerOrIsEditPrivileges(dcf.DocumentID);

			//validate documentfileid existornot, take from document files repository
			List<DocumentFiles> dcfs = await docFilesRepo.GetAsync(x => x.DocumentID == dcf.DocumentID && x.Id != dcf.DocumentFileID);
			foreach (var item in dcfs)
			{
				item.IsMainDocumentFile = false;
			}
			await docFilesRepo.UpdateManyAsync(dcfs);

			DocumentFiles res = await docFilesRepo.GetSingleAsync(x => x.Id == dcf.DocumentFileID && x.DocumentID == dcf.DocumentID);
			//res.IsMainDocumentFile = dcf.IsMainDocumentFile;
			res.IsMainDocumentFile = true;
			res.UpdatedAt = DateTime.Now;
			res.UpdatedBy = GetUser();
			await docFilesRepo.UpdateAsync(res);

			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Set Main File Document : {dcf.DocumentID}", Domain.Attributes.UserAction.Update, new Documents { Id = dcf.DocumentID });
			await repository.LogDocumentInsert(trnLogId, dcf.DocumentID, LogDocumentAction.AddReminder);

			return res;
		}

		public async Task<int> InsertDocumentShare(RequestDocumentShared request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.DocumentID);
			await CheckOwnershipAndPrivilegesAsync(request.DocumentID);
			DocumentShared share = request.CopyProperties<DocumentShared>();
			Guid trnLogId = await docShareRepo.LogTransactionAndAuditTrail($"Share Document : {request.DocumentID}", Domain.Attributes.UserAction.Update, share);

			int resultCount = 0;
			resultCount = await docShareRepo.UpsertDocumentSharePrivillege(request);
			await repository.LogDocumentInsert(trnLogId, request.DocumentID, LogDocumentAction.AddShared);
			return resultCount;
		}

		public async Task<int> InsertDocumentAttribute(RequestDocumentAttributes request)
		{
			await ValidateInputRequestAsync(request);
			await CheckIfExist(request.DocumentID);
			await ValidateOwnerOrIsEditPrivileges(request.DocumentID);

			//string json = "[{\"id\":13,\"attributeName\":\"Expired Date\",\"attributeElement\":\"{\"type\":\"date\",\"label\":\"Expired Date\",\"className\":\"form-control\",\"name\":\"expired_date\"}\",\"attributeType\":\"date\",\"attributeMaxLength\":0,\"value\":\"2025-06-30T17:00:00.000Z\"}]";

			//var attributes = JsonConvert.DeserializeObject<List<DocumentAttribute>>(json);

			//DateTime? expiredDate = null;
			//int? daysOfReminder = null;

			//foreach (var attr in attributes)
			//{
			//	var element = JObject.Parse(attr.attributeElement);
			//	var name = element["name"]?.ToString();

			//	if (name == "expired_date")
			//	{
			//		expiredDate = DateTime.Parse(attr.value);
			//	}
			//	else if (name == "days_of_reminder")
			//	{
			//		if (int.TryParse(attr.value, out int days))
			//			daysOfReminder = days;
			//	}
			//}

			// expiredDate and daysOfReminder now hold the extracted values

			DocumentAttributes documentAttribute = request.CopyProperties<DocumentAttributes>();
			Guid trnLogId = await docAttributeRepo.LogTransactionAndAuditTrail($"Add Attribute Document ID : {request.DocumentID}", Domain.Attributes.UserAction.Insert, documentAttribute);
			var entity = request.CopyProperties<DocumentAttributes>();
			int returnVal = 0;
			returnVal = await docAttributeRepo.SaveDocumentAttribute(entity);
			await repository.LogDocumentInsert(trnLogId, request.DocumentID, LogDocumentAction.AddAttribute);
			return returnVal;
		}

		public async Task<List<ResponseUserPrivillege>> GetListSharedUsers(int documentId)
		{
			await ValidateInputAsync(documentId);
			//await repository.LogTransactionAndAuditTrail($"Insert new Document Shared", Domain.Attributes.UserAction.Insert, entity);
			var documentShared = await docSharedRepo.GetSharedUsersByDocumentIDAsync(documentId);
			if (documentShared != null)
				return documentShared.SharedUser;
			else
				return new List<ResponseUserPrivillege>();
		}


		private async Task IsDocumentFileNameWasExist(int documentId, IFormFile docFile)
		{
			var fileName = Path.GetFileNameWithoutExtension(docFile.FileName);
			if (string.IsNullOrEmpty(fileName))
			{
				throw new ApiException("File name cannot be empty.");
			}

			bool isExist = await docFilesRepo.AnyAsync(df => df.DocumentID == documentId && df.DocumentFileName == fileName);
			if (isExist)
			{
				var message = await GetMessage(LangCodes.DocumentFileNameExist);
				message = message.Replace("@fileName", fileName);
				throw new ApiException(message);
			}
		}

		//public async Task<ResponseInitialDocument> InsertInitAddWithUploadDocument(RequestDocumentAddUpload request)
		public async Task<int> InsertInitAddWithUploadDocument(RequestDocumentAddUpload request)
		{
			await ValidateInputRequestAsync(request);
			await ValidateUploadAsync(request.DocFile);
			//await IsDocumentFileNameWasExist(documentId, file);

			int result = 0;
			Documents IsTitleExsit = await repository.GetSingleAsync(x => x.DocumentTitle == request.DocumentTitle && x.CategoryID == request.CategoryID);
			if (IsTitleExsit != null)
			{
				var message = await GetMessage(LangCodes.DocumentTitleExist);
				throw new ApiException(message);
			}

			var entity = request.CopyProperties<Documents>();

			//new process here
			DateTime DateNow = DateTime.Now;
			var fileName = Path.GetFileNameWithoutExtension(request.DocFile.FileName);
			var fileExtension = Path.GetExtension(request.DocFile.FileName);

			var model = await categoryRepo.GetSingleAsync(x => x.Id == request.CategoryID);

			string newFileName = Guid.NewGuid().ToString() + fileExtension;

			var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(GetUser(), UserName, model.Id, model.CategoryName, newFileName, true);
			var resourcePath = DocumentFilesHelper.GetResourcePathForDocumentFile(GetUser(), UserName, model.Id, model.CategoryName, newFileName);

			using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await request.DocFile.CopyToAsync(stream);
			}

			// 1. Ekstraksi Teks
			string fullText = string.Empty;
			try
			{
				fullText = await FileProcessor.ExtractTextAsync(filePath);
			}
			catch (Exception){}
			

			FileInfo fin = new FileInfo(filePath);

			entity.Owner = GetUser();
			entity.FileSize = (int)fin.Length;
			entity.InsertedBy = GetUser();
			entity.InsertedAt = DateNow;
			entity.UpdatedBy = GetUser();
			entity.UpdatedAt = DateNow;
			entity = await repository.InsertAsync(entity);
			if (entity != null)
				result++;

			DocumentFiles fl = new DocumentFiles()
			{
				DocumentID = entity.Id,
				DocumentType = fin.Extension,
				DocumentFileName = fileName,
				NewDocumentFileName = fin.Name,
				//DocumentFilePath = fin.FullName,
				DocumentFilePath = resourcePath,
				DocumentFileSize = (int)fin.Length,
				IsMainDocumentFile = true,
				InsertedBy = GetUser(),
				InsertedAt = DateNow,
				UpdatedBy = GetUser(),
				UpdatedAt = DateNow,
				DocumentFileContent = fullText //System.Text.Encoding.UTF8.GetBytes(fullText)
			};

			var mim = request.DocFile.ContentType;
			if (Mimes.Contains(mim))
			{
				MemoryStream memoryStream;

				using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					memoryStream = new MemoryStream();
					fs.CopyTo(memoryStream);
				}

				memoryStream.Position = 0; // Reset position before passing

				//await using var fileStrm = request.OpenReadStream();
				var cc = ProcessingOCR(memoryStream, filePath, fileExtension);
				fl.DocumentFileContent = cc;
			}

			DocumentFiles res = await docFilesRepo.InsertAsync(fl);
			if (res.Id > 0)
				result++;

			//end new process here


			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Insert new Documents and file", Domain.Attributes.UserAction.Insert, entity);


			//ResponseInitialDocument result = await repository.InsertInitAddUploaddocument(request);

			ResponseApprovals approval = await approvalRepo.GetApprovalByCategoryIdAsync(entity.CategoryID);
			if (approval != null)
			{
				List<ResponseApprovalFlow> flows = approval.ApprovalFlows;
				Approvals approvals = new Approvals()
				{
					DocumentID = entity.Id,
					CategoryID = entity.CategoryID,
					Notes = approval.Notes
				};
				List<ApprovalFlows> Flows = new List<ApprovalFlows>();
				foreach (var item in flows)
				{
					ApprovalFlows x = new ApprovalFlows()
					{
						ApproverUserID = (int)item.User.UserID,
						Step = item.Step
					};
					Flows.Add(x);
				}
				await repository.AddWorkFlow(approvals, Flows);
			}

			//replicate user shared from category
			List<ResponseCategoryShared> rCatShare = await categorySharedRepo.GetSharedUsersByCategoryIDAsync(entity.CategoryID);
			if (rCatShare.Count > 0)
			{
				RequestDocumentShared requestDocumentShared = new RequestDocumentShared() { DocumentID = entity.Id };
				List<RequestUserDocumentPrivillege> requestUserDocPrivillege = new List<RequestUserDocumentPrivillege>();
				foreach (var item in rCatShare)
				{
					RequestUserDocumentPrivillege dt = new RequestUserDocumentPrivillege()
					{
						UserID = item.UserID,
						IsView = item.IsView,
						IsEdit = item.IsEdit,
						IsDelete = item.IsDelete,
						ShareType = item.ShareType,
						GroupID = item.GroupID
					};
					requestUserDocPrivillege.Add(dt);
				}
				;
				requestDocumentShared.requestUserDocPrivillege = requestUserDocPrivillege;
				await docShareRepo.UpsertDocumentSharePrivillege(requestDocumentShared);
			}


			await repository.LogDocumentInsert(trnLogId, entity.Id, LogDocumentAction.InsertNewDocument);
			return result;
		}

		public async Task<int> UploadInitDocument(string categoryId, List<IFormFile> request)
		{
			int.TryParse(categoryId, out var CatID);
			foreach (var item in request)
			{
				await ValidateUploadAsync(item);
			}

			foreach (var item in request)
			{
				Documents IsTitleExsit = await repository.GetSingleAsync(x => x.DocumentTitle == Path.GetFileNameWithoutExtension(item.FileName) && x.CategoryID == CatID);
				if (IsTitleExsit != null)
				{
					var message = await GetMessage(LangCodes.DocumentFileNameExist);
					throw new ApiException(message);
				}
			}

			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Upload as new document and file", Domain.Attributes.UserAction.Insert, new Documents() { CategoryID = CatID });

			int result = 0;
			foreach (IFormFile file in request)
			{
				var fileName = Path.GetFileNameWithoutExtension(file.FileName);

				int UserId = GetUser();
				DateTime DateNow = DateTime.Now;

				Documents dc = new Documents() { CategoryID = int.Parse(categoryId) };
				dc.DocumentTitle = fileName;
				dc.DocumentDesc = "";
				dc.Owner = UserId;
				dc.FileSize = (int)file.Length;
				dc.InsertedBy = UserId;
				dc.InsertedAt = DateNow;
				dc.UpdatedBy = UserId;
				dc.UpdatedAt = DateNow;
				dc = await repository.InsertAsync(dc);

				var model = await categoryRepo.GetSingleAsync(x => x.Id == Convert.ToInt32(categoryId));

				var fileExtension = Path.GetExtension(file.FileName);
				string newFileName = Guid.NewGuid().ToString() + fileExtension;

				var filePath = DocumentFilesHelper.GetPhysicalPathForDocumentFile(GetUser(), UserName, model.Id, model.CategoryName, newFileName, true);
				var resourcePath = DocumentFilesHelper.GetResourcePathForDocumentFile(GetUser(), UserName, model.Id, model.CategoryName, newFileName);

				FileInfo fin = new FileInfo(filePath);
				DocumentFiles fl = new DocumentFiles()
				{
					DocumentID = dc.Id,
					DocumentType = Path.GetExtension(file.FileName),
					DocumentFileName = fileName,
					NewDocumentFileName = Path.GetFileName(filePath),
					//DocumentFilePath = fin.FullName,
					//DocumentFilePath = $"/resource/{model.Id}-{model.CategoryName.Trim()}/{Path.GetFileName(filePath)}",
					DocumentFilePath = resourcePath,
					DocumentFileSize = (int)file.Length,
					IsMainDocumentFile = true,
					InsertedBy = UserId,
					InsertedAt = DateNow,
					UpdatedBy = UserId,
					UpdatedAt = DateNow
				};
				fl = await docFilesRepo.InsertAsync(fl);

				if (fl.Id > 0)
				{
					result++;

					using (var stream = new FileStream(filePath, FileMode.Create))
					{

						await file.CopyToAsync(stream);
					}

					// 1. Ekstraksi Teks
					string fullText = string.Empty;
					try
					{
						fullText = await FileProcessor.ExtractTextAsync(filePath);
					}
					catch (Exception) { }
					fl.DocumentFileContent = fullText;// System.Text.Encoding.UTF8.GetBytes(fullText);
					var mim = file.ContentType;
					if (Mimes.Contains(mim))
					{
						MemoryStream memoryStream;

						using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
						{
							memoryStream = new MemoryStream();
							fs.CopyTo(memoryStream);
						}

						memoryStream.Position = 0; // Reset position before passing

						//await using var fileStrm = request.OpenReadStream();
						var cc = ProcessingOCR(memoryStream, filePath, fileExtension);
						fl.DocumentFileContent = cc;
					}

					docFilesRepo.UpdateAsync(fl).Wait();

					await repository.LogDocumentInsert(trnLogId, dc.Id, LogDocumentAction.IntitUploadForNewDocument);
				}
			}

			//ResponseInitialDocument result = await repository.UploadInitDocument(CatID, request);
			//await repository.LogDocumentInsert(trnLogId, result.Id, LogDocumentAction.IntitUploadForNewDocument);
			//return result.Id;

			return result;
		}

		public async Task<object> ListCategoryDocument(RequestDocumentCategory request)
		{
			await ValidateInputRequestAsync(request);
			//var result = await repository.ListCategoryDocument(request.Page, request.Limit, request.SortBy, request.SortOrientation, request.FilterBy, request.FilterValue);
			//return ObjectFlatter.Flatten(result);
			var result = await repository.ListCategoryDocument(request.CategoryId, request.DocumentTile, request.Page, request.Limit);
			//var resultCount = repository.ListCategoryDocumentCount(request.CategoryId, request.DocumentTile);

			return new ResponsePagination
			{
				TotalRecords = result.TotalRecord,
				TotalPages = GetTotalPages(result.TotalRecord, request.Limit),
				Data = result.Record
			};
		}

		public async Task<object> ListDocumentDeleted(RequestDocument request)
		{
			await ValidateInputRequestAsync(request);
			ResponseDocumentPagination result = await repository.ListDocumentDeleted(request.SearchText, request.Page, request.Limit);
			long totalRecord = result.TotalRecord;

			return new ResponsePagination
			{
				TotalRecords = totalRecord,
				TotalPages = GetTotalPages(totalRecord, request.Limit),
				Data = result.Record
			};
		}

		public async Task<bool> EmptyRecyclebin()
		{
			await repository.LogTransaction($"Empty recyclebin of document", Domain.Attributes.UserAction.Delete);
			return await repository.EmptyRecyclebin(20);
		}

		public async Task<DocumentFavorite> AddFavorite(int documentId)
		{
			await ValidateInputAsync(documentId);
			await CheckIfExist(documentId);
			var isany = await repository.CheckFavorite(documentId);
			if (isany != null)
			{
				//var message = await GetMessage(LangCodes.NotFound);
				//throw new ApiException($"Document already in favorite. Id {documentId}");
				//await repository.RemoveFavorite(documentId);
				return new DocumentFavorite() { DocumentId = documentId };
			}

			await repository.LogTransaction($"Add Favorite Document ID : {documentId}", Domain.Attributes.UserAction.Insert);
			return await repository.AddFavorite(documentId);
		}

		public async Task<int> UnFavorite(int documentId)
		{
			await ValidateInputAsync(documentId);
			await CheckIfExist(documentId);
			await ValidateOwnerOrIsEditPrivileges(documentId);

			await repository.LogTransaction($"Remove Favorite Document ID : {documentId}", Domain.Attributes.UserAction.Delete);
			return await repository.RemoveFavorite(documentId);
		}

		public async Task<object> GetHighlightDocument(RequestHighlightDocument request)
		{
			await ValidateInputRequestAsync(request);
			await repository.LogTransaction($"Read Highlight Document", Domain.Attributes.UserAction.Read);

			var records = await repository.GetHighlightDocument(request.DocumentTitle, request.CategoryId, request.Page, request.Limit);
			var totalRecords = await repository.GetHighlightDocumentCount(request.DocumentTitle, request.CategoryId);

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = records
			};
		}

		public async Task<object> GetRecentDocument(RequestRecentDocument request)
		{
			await ValidateInputRequestAsync(request);
			await repository.LogTransaction($"Read Recent Document", Domain.Attributes.UserAction.Read);

			var records = await repository.GetRecentDocument(request.DocumentTitle, request.CategoryId, request.Page, request.Limit);
			var totalRecords = await repository.GetRecentDocumentCount(request.DocumentTitle, request.CategoryId);

			return new ResponsePagination
			{
				TotalRecords = totalRecords,
				TotalPages = GetTotalPages(totalRecords, request.Limit),
				Data = records
			};
		}

		public async Task<int> DeleteWorkFlow(int documentId)
		{
			await ValidateInputAsync(documentId);
			await ValidateOwnerOrIsEditPrivileges(documentId);

			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Delete Workflow With Document Id : {documentId}", Domain.Attributes.UserAction.Delete, new Documents() { Id = documentId });
			int result = await repository.DeleteWorkFlow(documentId);
			await repository.LogDocumentInsert(trnLogId, documentId, LogDocumentAction.DeleteWorkflow);
			return result;
		}

		//private async Task ValidateApprovalStatusAsync(int documentId)
		//{
		//	Approvals approval = await repository.CheckApprovalStatusDocument(documentId);
		//	if (approval != null)
		//	{
		//		if (approval.Status == Domain.Enum.ApprovalStatusEnum.Approved || approval.Status == Domain.Enum.ApprovalStatusEnum.Rejected)
		//		{
		//			//var message = await GetMessage(LangCodes.InputEmpty);
		//			throw new ApiException("Document has been raise final for approval, you cannot add or update workflow");
		//		}			
		//	}
		//}

		public async Task<int> AddWorkFlow(RequestWorkflow workflow)
		{
			await ValidateInputRequestAsync(workflow.Approval);
			await ValidateInputRequestAsync(workflow.Flows);


			int DocumentID = workflow.Approval.DocumentID;
			await ValidateOwnerOrFullPrivileges(DocumentID);

			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Add Workflow For Document Id : {DocumentID}", Domain.Attributes.UserAction.Update, new Documents { Id = DocumentID });
			Approvals approval = workflow.Approval.CopyProperties<Approvals>();
			approval.Status = Domain.Enum.ApprovalStatusEnum.Draft;
			approval.CurrentApproverUserID = null;
			approval.CurrentStep = null;
			approval.NextApproverUserID = null;
			approval.LastActivityDate = null;
			approval.LastRemark = null;
			approval.InsertedBy = GetUser();
			approval.InsertedAt = DateTime.Now;
			approval.UpdatedBy = GetUser();
			approval.UpdatedAt = DateTime.Now;
			//validate approval flows userID exist or not in users table

			List<ApprovalFlows> Flows = new List<ApprovalFlows>();
			foreach (var item in workflow.Flows)
			{
				ApprovalFlows x = new ApprovalFlows()
				{
					ApproverUserID = item.userID,
					Step = item.Step
				};
				Flows.Add(x);
			}

			int result = await repository.AddWorkFlow(approval, Flows);
			await repository.LogDocumentInsert(trnLogId, DocumentID, LogDocumentAction.AddWorkflow);
			//Task<int> 
			return result;
		}

		public async Task<int> DeleteWorkFlowUser(RequestDeleteWorkflowUser flow)
		{
			await ValidateInputRequestAsync(flow);

			int DocumentID = await repository.DeleteWorkFlowUser(flow.ApprovalID, flow.userID);
			Guid trnLogId = await repository.LogTransactionAndAuditTrail($"Delete Workflow user for Document Id : {DocumentID}", Domain.Attributes.UserAction.Update, new Documents { Id = DocumentID });
			await repository.LogDocumentInsert(trnLogId, DocumentID, LogDocumentAction.DeleteWorkflowUser);
			return DocumentID;
		}

		//public List<string> ValidateAddWorkFlow(RequestWorkflow workflow)
		//{
		//	var validationResults = new List<ValidationResult>();
		//	var validationContext = new ValidationContext(this);
		//	// Validate the main object
		//	if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
		//	{
		//		return validationResults.Select(vr => vr.ErrorMessage).ToList();
		//	}
		//	// Validate the Approval property
		//	if (workflow.Approval != null)
		//	{
		//		var approvalResults = new List<ValidationResult>();
		//		var approvalContext = new ValidationContext(workflow.Approval);
		//		Validator.TryValidateObject(workflow.Approval, approvalContext, approvalResults, true);
		//		validationResults.AddRange(approvalResults);
		//	}
		//	// Validate the Flows list
		//	if (Flows != null)
		//	{
		//		foreach (var flow in Flows)
		//		{
		//			var flowResults = new List<ValidationResult>();
		//			var flowContext = new ValidationContext(flow);
		//			Validator.TryValidateObject(flow, flowContext, flowResults, true);
		//			validationResults.AddRange(flowResults);
		//		}
		//	}
		//	return validationResults.Select(vr => vr.ErrorMessage).ToList();
		//}

		public async Task<object> SearchListDocument(RequestTopSearch request)
		{
			await ValidateInputRequestAsync(request);
			var result = await repository.SearchListDocument(request.Textsearch, request.Page, request.Limit);
			var TotalRecord = result.TotalRecord;
			return new ResponsePagination
			{
				TotalRecords = TotalRecord,
				TotalPages = GetTotalPages(TotalRecord, request.Limit),
				Data = result.Records
			};
		}

		public async Task<object> AdvSearchDocument(RequestAdvSearch request)
		{
			await ValidateInputRequestAsync(request);
			DateTime? utcDateFrom = null;
			if (request.DtFrom != null)
			{
				//DateTime rDtFrom = (DateTime)request.DtFrom;
				//utcDateFrom = new DateTime(rDtFrom.Year, rDtFrom.Month, rDtFrom.Day, rDtFrom.Hour, rDtFrom.Minute, rDtFrom.Second, DateTimeKind.Utc);
				utcDateFrom = request.DtFrom;
			}

			DateTime? utcDateTo = null;
			if (request.DtTo != null)
			{
				//DateTime rDateTo = (DateTime)request.DtTo;
				//utcDateTo = new DateTime(rDateTo.Year, rDateTo.Month, rDateTo.Day, rDateTo.Hour, rDateTo.Minute, rDateTo.Second, DateTimeKind.Utc);
				utcDateTo = request.DtTo;
			}

			var result = await repository.AdvSearchDocument(request.DocumentTitle, utcDateFrom, utcDateTo, request.CategoryId, request.Page, request.Limit);
			var TotalRecord = result.TotalRecord;
			return new ResponsePagination
			{
				TotalRecords = TotalRecord,
				TotalPages = GetTotalPages(TotalRecord, request.Limit),
				Data = result.Records
			};
		}

		public async Task<object> ItemList(RequestPagination request)
		{
			await ValidateInputRequestAsync(request);
			var result = await docItemListRepository.ItemList(request.Page, request.Limit);
			var TotalRecord = result.TotalRecord;
			return new ResponsePagination
			{
				TotalRecords = TotalRecord,
				TotalPages = GetTotalPages(TotalRecord, request.Limit),
				Data = result.Records
			};
		}

		public async Task<int> AddItemList(List<int> documentIds)
		{
			await ValidateInputAsync(documentIds);

			int result = await docItemListRepository.AddItemList(documentIds);
			return result;
		}

		public async Task<int> DeleteItemList(List<int> documentIds)
		{
			await ValidateInputAsync(documentIds);
			int result = await docItemListRepository.DeleteItemList(documentIds);
			return result;
		}

		public async Task<int> EmptyItemList()
		{
			return await docItemListRepository.EmptyItemList();
		}

		public async Task<string> SendMailDirect(RequestEmailDocument request)
		{
			bool ishtml = true;
			await ValidateInputRequestAsync(request);

			var model = await repository.GetInfoCategoryDocumentAsync(request.DocumentID);

			var uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(model.Owner, model.OwnerUserName, model.Id, model.CategoryName);
			
			List<ResponseDocumentFiles> files = await docFilesRepo.GetFileFromDocumentAsync(request.DocumentID);
			ResponseDocumentFiles MainFileofDocuments = files.FirstOrDefault(x => x.IsMainDocumentFile == true);
			byte[] fileToAttchOnEmail = null;
			if (MainFileofDocuments != null)
			{
				string filePath = Path.Combine(uploadPath, MainFileofDocuments.NewDocumentFileName);
				if (File.Exists(filePath))
				{
					fileToAttchOnEmail = File.ReadAllBytes(filePath);
				}
			}
			string to = String.Join(";", request.To);
			string cc = String.Join(";", request.Cc);
			var mail = new MailServiceBuilder()
				.Subject(request.Subject)
				.Body(request.Body, ishtml)
				.AddAttachment($"{MainFileofDocuments.DocumentFileName}{MainFileofDocuments.DocumentType}", fileToAttchOnEmail)
				.AddRecipient(to)
				.AddCc(cc)
				.Build();

			await repository.LogTransaction($"Send direct email", Domain.Attributes.UserAction.Insert);
			await repository.LogDocumentInsert(null, request.DocumentID, LogDocumentAction.DeleteWorkflow);

			return mail.Send();
		}

		public async Task<string> SendMailMultiDirect(RequestEmailItemListDocument request)
		{
			bool ishtml = true;
			string reponseSend = null;

			await ValidateInputRequestAsync(request);

			var dictionary = new Dictionary<string, byte[]>();
			//{
			//	{ fileName, File.ReadAllBytes(path) }
			//};

			foreach (var lDocumentId in request.DocumentIDs)
			{
				var model = await repository.GetInfoCategoryDocumentAsync(lDocumentId);
				var uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(model.Owner, model.OwnerUserName, model.Id, model.CategoryName);

				List<ResponseDocumentFiles> files = await docFilesRepo.GetFileFromDocumentAsync(lDocumentId);
				ResponseDocumentFiles MainFileofDocuments = files.FirstOrDefault(x => x.IsMainDocumentFile == true);
				byte[] fileToAttchOnEmail = null;
				if (MainFileofDocuments != null)
				{
					string filePath = Path.Combine(uploadPath, MainFileofDocuments.NewDocumentFileName);
					if (File.Exists(filePath))
					{
						fileToAttchOnEmail = File.ReadAllBytes(filePath);
						dictionary.Add(MainFileofDocuments.DocumentFileName, fileToAttchOnEmail);
					}
				}
			}
			if (dictionary.Count > 0)
			{
				byte[] filesArchieved = FileExtensions.ZipFile(dictionary);

				//string to = String.Join(";", request.To);
				//string cc = String.Join(";", request.Cc);
				var mail = new MailServiceBuilder()
					.Subject(request.Subject)
					.Body(request.Body, ishtml)
					.AddAttachment("Documents", filesArchieved)
					.AddRecipient(request.To.ToArray())
					.AddCc(request.Cc.Count > 0 ? request.Cc.ToArray() : null)
					.Build();

				await repository.LogTransaction($"Send direct item list email", Domain.Attributes.UserAction.Insert);

				reponseSend = mail.Send();
			}
			else
			{
				var message = await GetMessage(LangCodes.AttachmentFilesItemlistEmpty);
				throw new ApiException(message);
			}
			return reponseSend;
		}


		//public async Task SendEmailTable(RequestEmail request)
		//{
		//	await ValidateInputRequestAsync(request);

		//	var email = request.CopyProperties<Email>();

		//	await repository.LogTransactionAndAuditTrail($"Insert new email data for indirect email", Domain.Attributes.UserAction.Insert, email);
		//	await repository.InsertAsync(email);
		//}

		public string ProcessingOCR(Stream fileStrm, string filePath, string extName)
		{
			#region tesseractOcr
			string output = string.Empty;
			string OCRDirectory = @"C:\Program Files\Tesseract-OCR";
			var newId = System.IO.Path.GetRandomFileName();
			var tmpFilePath = System.IO.Path.Combine(GetFolderPath(SpecialFolder.CommonDocuments, SpecialFolderOption.DoNotVerify), newId);

			//var newIdPreProcessing = System.IO.Path.GetRandomFileName();
			//var tmpFilePreProcessingPath = System.IO.Path.Combine(GetFolderPath(SpecialFolder.CommonDocuments, SpecialFolderOption.DoNotVerify), newId);

			if (!Directory.Exists(tmpFilePath))
				Directory.CreateDirectory(tmpFilePath);

			var tempOutputFile = System.IO.Path.Combine(tmpFilePath, "out");
			//if (!File.Exists(tempOutputFile))
			//	File.Create(tempOutputFile);

			string tempImageFile;
			string tempImagePreProcessingFile = null;

			if (string.IsNullOrEmpty(filePath))
			{
				tempImageFile = System.IO.Path.Combine(tmpFilePath, $"in{extName}");
				using var fs = new FileStream(tempImageFile, FileMode.Create);
				fileStrm?.CopyTo(fs);
			}
			else
			{
				tempImageFile = filePath;
				tempImagePreProcessingFile = tmpFilePath + extName;
			}

			OcrImagePreprocessing.PreprocessImage(tempImageFile, tempImagePreProcessingFile);
			tempImageFile = tempImagePreProcessingFile;

			var info = new ProcessStartInfo
			{
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true
			};

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				info.WorkingDirectory = OCRDirectory;
				info.FileName = "cmd.exe";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				info.FileName = "/bin/bash";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return output;

			var process = Process.Start(info);
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				if (process != null)
				{
					//process.StandardInput.WriteLine($@"tesseract.exe {tempImageFile} {tempOutputFile} -c tessedit_do_invert=0 -l eng+ind");
					process.StandardInput.WriteLine($@"tesseract.exe {tempImageFile} {tempOutputFile} l ind+eng --psm 6 tessedit_char_whitelist=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789:-");
					process.StandardInput.WriteLine("exit");
				}
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				//process.StandardInput.WriteLine($"-c \" sudo tesseract {tempImageFile} {tempOutputFile} -l ind+eng\" ");
				if (process != null)
				{
					process.StandardInput.WriteLine($"sudo tesseract {tempImageFile} {tempOutputFile} -l ind+eng");
					process.StandardInput.WriteLine("exit");
				}
			}

			if (process == null) return output;

			var isExited = process.HasExited;
			int i = 0;
			while (!isExited && i < 20)
			{
				isExited = process.HasExited;
				i++;
				Thread.Sleep(500);
			}

			if (!process.HasExited)
			{
				process.Close();
				return output;
			}
			// Exit code: success.
			output = File.ReadAllText($"{tempOutputFile}.txt");
			//output = File.ReadAllText(tempOutputFile);
			if (!string.IsNullOrWhiteSpace(output) || !string.IsNullOrEmpty(output))
				output = Regex.Replace(output, @"\s+", " ");
			Directory.Delete(tmpFilePath, true);
			process.Close();
			return output;

			#endregion
		}

		#region tempdata
		public async Task<bool> CreateTempData()
		{

			Api.Domain.EntityDTO.CategoryDataInitializer td = new Domain.EntityDTO.CategoryDataInitializer();
			var list = td.InitializeFullCategoryList();

			foreach (var item in list)
			{
				await SetDumpbyCategory(item.CategoryName, item);
			}
			return true;
		}

		private async Task SetDumpbyCategory(string category, Domain.EntityDTO.DocumentCategory item)
		{
			int _UserId = 9;
			var dd = repository.GetSingleAsync(x => x.Id == 29).Result;
			var cc = await categoryRepo.GetSingleAsync(x => x.Id == dd.CategoryID);
			var attr = await docAttributeRepo.GetDocumentAttributeAsync(29);
			var dfs = await docFilesRepo.GetFileFromDocumentAsync(29);
			var sdfs = dfs.FirstOrDefault(x => x.IsMainDocumentFile == true);
			var user = await userRepo.GetSingleAsync(x => x.UserId == _UserId);

			var exisit = await categoryRepo.GetSingleAsync(x => x.CategoryName.Contains(category) && x.Owner == _UserId);
			if (exisit != null)
			{
				if (item.Documents.Count > 0)
				{
					foreach (var doc in item.Documents)
					{
						Documents document = new Documents()
						{
							CategoryID = exisit.Id,
							DocumentTitle = doc.DocumentName,
							DocumentDesc = $"({exisit.Id}-{exisit.CategoryName}) >> {doc.DocumentName}",
							FileSize = dd.FileSize,
							Owner = _UserId,
							ReminderDays = dd.ReminderDays,
							ExpiryDate = dd.ExpiryDate,
							InsertedBy = _UserId,
							InsertedAt = DateTime.Now,
							UpdatedBy = _UserId,
							UpdatedAt = DateTime.Now,
							IsActive = true,
							IsDeleted = false
						};
						document = await repository.InsertAsync(document);
						if (document != null)
						{
							DateTime DateNow = DateTime.Now;
							var fileName = Path.GetFileNameWithoutExtension(sdfs.NewDocumentFileName);
							var fileExtension = sdfs.DocumentType;

							var model = await categoryRepo.GetSingleAsync(x => x.Id == document.CategoryID);
							string uploadPath = DocumentFilesHelper.GetPhysicalPathForCategory(_UserId, user.UserName, model.Id, model.CategoryName, true);
							string resource = DocumentFilesHelper.GetResourcePathForDocumentFile(_UserId, user.UserName, model.Id, model.CategoryName, sdfs.NewDocumentFileName);

							
							var srcFile = sdfs.DocumentFilePath.Replace($"/resource/{dd.Id}-{cc.CategoryName}/", "");
							//srcFile = srcFile.Replace("/", "\\");
							string baseDir = AppDomain.CurrentDomain.BaseDirectory;
							var uploadFilePathOld = Path.Combine(baseDir, "upload", $"{dd.Id}-{cc.CategoryName}", srcFile);

							DocumentFiles fl = new DocumentFiles()
							{
								DocumentID = document.Id,
								DocumentType = sdfs.DocumentType,
								DocumentFileName = sdfs.DocumentFileName,
								NewDocumentFileName = sdfs.NewDocumentFileName,
								DocumentFilePath = resource,
								DocumentFileSize = sdfs.DocumentFileSizeInt,
								DocumentFileContent = sdfs.DocumentFileContent,
								IsMainDocumentFile = true,
								InsertedBy = _UserId,
								InsertedAt = DateTime.Now,
								UpdatedBy = _UserId,
								UpdatedAt = DateTime.Now
							};
							var res = await docFilesRepo.InsertAsync(fl);

							string newFileName = Guid.NewGuid().ToString();
							var filePath = Path.Combine(uploadPath, newFileName + fileExtension);
							System.IO.File.Copy(uploadFilePathOld, filePath, true);

							//using (var stream = new FileStream(filePath, FileMode.Create))
							//{
							//	using (var sourceStream = new FileStream(srcFile, FileMode.Open, FileAccess.Read))
							//	{
							//		// 2. Asynchronously copy the content from the source stream to the destination stream
							//		await sourceStream.CopyToAsync(stream);
							//	}
							//}

							if (attr != null)
							{
								docAttributeRepo.InsertAsync(new DocumentAttributes()
								{
									DocumentID = document.Id,
									AttributeValues = attr.AttributeValues,
									InsertedAt = DateTime.Now,
									InsertedBy = _UserId
								}).Wait();
							}							
						}
					}
				}

				if (item.SubCategories.Count > 0)
				{
					foreach (var subCat in item.SubCategories)
					{
						await SetDumpbyCategory(subCat.CategoryName, subCat);
					}
				}
			}
		}
		#endregion
	}
}
