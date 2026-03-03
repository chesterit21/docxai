using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain.EntityRequests;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Api.Domain.Enum;
using Api.Domain.Formatters;
using Api.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Dynamic.Core;
using static NPOI.HSSF.Util.HSSFColor;

namespace Api.Repository.Masters
{
	public interface IApprovalRepository : IRepository<Approvals>
	{
		Task<ResponseApprovals> GetApprovalByDocumentIdAsync(int id);
		Task<ResponseApprovalTaskAndCount> GetMyApprovalTask(int page, int limit);
		Task<ResponseApprovalRequestAndCount> GetMyApprovalRequest(int page, int limit);
		Task<int> ApprovalAction(int approvalId, ApprovalActivities activity);
		Task<ResponseCheckApproval> GetInfoCheckForApprovalPageView(int approvalId);
		Task<ResponseApprovals> GetApprovalByCategoryIdAsync(int categoryId);

	}

	public class ApprovalRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Approvals>(context, accessor), IApprovalRepository
	{
		public async Task<ResponseApprovals> GetApprovalByDocumentIdAsync(int documentId)
		{
			var result = await context.Approvals.Include(a => a.ApprovalFlows).ThenInclude(af => af.User).Include(sts => sts.ApprovalActivities)
				.Where(d => d.DocumentID == documentId)
				.Select(d => new ResponseApprovals()
				{
					Id = d.Id,
					DocumentID = d.DocumentID,
					CategoryID = d.CategoryID,
					Notes = d.Notes,
					MaxStep = d.MaxStep,
					ApprovalFlows = d.ApprovalFlows
						.Select(u => new ResponseApprovalFlow
						{
							Id = u.Id,
							ApprovalID = u.ApprovalID,
							Step = u.Step,
							ApprovalActivityCode = (int)(d.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step).ApprovalActivity ?? ApprovalActivity.PendingApproval),
							ApprovalActivity = d.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step).ApprovalActivityName ?? "Waiting Approval",
							User = new ResponseUser
							{
								UserID = u.User.UserId,
								UserName = u.User.UserName,
								FullName = u.User.FullName,
								Email = u.User.EmailAddress
							}
						}).ToList()
				}).FirstOrDefaultAsync();
			return result;
		}

		public async Task<ResponseApprovals> GetApprovalByCategoryIdAsync(int categoryId)
		{
			var result = await context.Approvals.Include(a => a.ApprovalFlows).ThenInclude(af => af.User).Include(sts => sts.ApprovalActivities)
				.Where(d => d.DocumentID == null && d.CategoryID == categoryId)
				.Select(d => new ResponseApprovals()
				{
					Id = d.Id,
					CategoryID = d.CategoryID,
					Notes = d.Notes,
					MaxStep = d.ApprovalFlows.Count, //d.MaxStep,
					ApprovalFlows = d.ApprovalFlows.Select(u => new ResponseApprovalFlow
					{
						Id = u.Id,
						ApprovalID = u.ApprovalID,
						Step = u.Step,
						ApprovalActivity = d.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step).ApprovalActivityName,
						User = new ResponseUser
						{
							UserID = u.User.UserId,
							UserName = u.User.UserName,
							FullName = u.User.FullName,
							Email = u.User.EmailAddress
						}
					}).ToList()
				}).FirstOrDefaultAsync();
			return result;
		}

		public async Task<ResponseApprovalTaskAndCount> GetMyApprovalTask(int page = 0, int limit = 0)
		{
			var skip = Skip(page, limit);

			//Cari dari approval yg statusapproval masih pending
			//look up ke activity berdasarkan approval ID, ambil next step
			//look up ke appflow berdasarkan approval ID dan next step activity. jika user id sama dengan login brrti pending thdp login

			//a.LastRemark,
			//		a.Notes,
			//		a.DocumentID,
			//		a.CurrentStep,
			//		a.MaxStep,
			//		a.InsertedBy,
			//		a.Status,

			var query = context.Approvals
				//.Where(x => x.Status == Domain.Enum.ApprovalStatusEnum.Pending && x.NextApproverUserID == UserId)
				.Where(x => x.Status == Domain.Enum.ApprovalStatusEnum.Pending && x.CurrentApproverUserID == UserId)
				.Select(a => new ResponseApprovalTask
				{
					RequestID = a.Id,
					Initiator = a.InsertedUser.FullName,
					Approver = a.CurrentApproverUser.FullName,
					NextApprover = a.NextApproverUser.FullName,
					ApprovalStatus = a.Status,
					ApprovalStatusName = a.Status.ToString(),
					ApprovalDate = a.LastActivityDate,
					ApprovalActivities = a.ApprovalActivities
						.OrderByDescending(act => act.InsertedAt)
						.Select(act => new
							ResponseApprovalActivities
						{
							Remark = act.Remark,
							Reason = act.Reason,
							ApprovalActivity = act.ApprovalActivity,
							ApprovalActivityDesc = act.ApprovalActivity.ToString(),
							CurrentStep = act.CurrentStep,
							NextStep = act.NextStep,
							InsertedBy = act.InsertedBy,
						})
						.ToList(),
				});

			ResponseApprovalTaskAndCount result = new ResponseApprovalTaskAndCount()
			{
				PageCount = query.Count()
			};

			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			result.ResponseApprovalTasks = await query.ToListAsync();

			return result;
		}

		public async Task<ResponseApprovalRequestAndCount> GetMyApprovalRequestold(int page = 0, int limit = 0)
		{
			ApprovalActivity activity = ApprovalActivity.PendingApproval;
			string activityName = activity.ToString().SplitCamelCase();

			var skip = Skip(page, limit);

			var query = context.Approvals
				.Where(x => x.InsertedBy == UserId).Include(y => y.ApprovalFlows).ThenInclude(yy => yy.User).Include(z => z.ApprovalActivities).AsSplitQuery()
				.Join(context.Documents.Include(x => x.DocumentFiles), approval => approval.DocumentID, document => document.Id,
					(approval, document) => new ResponseApprovalRequest
					{
						RequestID = approval.Id,
						RequestDate = approval.Documents.InsertedAt,
						DocumentTitle = approval.Documents.DocumentTitle,
						DocumentID = approval.DocumentID,
						UserID = approval.InsertedBy,
						Approver = approval.ApprovalFlows.Where(a => a.Step == (approval.ApprovalActivities.Where(aa => aa.ApprovalID == approval.Id).Max(b => b.CurrentStep))).First().User.UserName,
						NextApprover = approval.ApprovalFlows.Where(a => a.Step == (approval.ApprovalActivities.Where(aa => aa.ApprovalID == approval.Id).Max(b => b.CurrentStep) + 1)).First().User.UserName,
						ApprovalStep = approval.ApprovalActivities.Where(a => a.ApprovalID == approval.Id).Max(b => b.CurrentStep).ToString() + "/" + approval.MaxStep.ToString(),
						ApprovalStatus = approval.Status,
						ApprovalStatusName = approval.Status.ToString(),
						ResponseDate = approval.ApprovalActivities.Where(a => a.ApprovalID == approval.Id).Max(b => (DateTime?)b.InsertedAt),
						Detail = new ResponseDetailMyApprovalRequest
						{
							ApprovalID = approval.Id,
							DocumentID = (int)approval.DocumentID,
							Notes = approval.Notes,
							ApprovalFlows = approval.ApprovalFlows
								.Select(u =>
									 new ResponseApprovalFlowMyRequest
									 {
										 Id = u.Id,
										 Step = u.Step,
										 Status = approval.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step) == null
											? activityName
											: approval.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step).ApprovalActivity.ToString().SplitCamelCase(),
										 User = new ResponseUser
										 {
											 UserID = u.User.UserId,
											 UserName = u.User.UserName,
											 FullName = u.User.FullName,
											 Email = u.User.EmailAddress
										 },
										 AcitivityDate = approval.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step) == null ? (DateTime?)null : approval.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step).InsertedAt,
									 }).ToList(),
							MainFile = document.DocumentFiles.Where(df => df.IsMainDocumentFile == true)
								.Select(f => new ResponseApprovalDocumentFiles
								{
									Id = f.Id,
									DocumentID = f.DocumentID,
									DocumentFileName = f.DocumentFileName,
									DocumentFilePath = f.DocumentFilePath,
									DocumentFileSize = SizeFormatter.SizeSuffix(f.DocumentFileSize, 2),
									DocumentType = f.DocumentType,
									UpdateAt = f.UpdatedAt,
									UpdatedByFullName = f.UpdatedByFullName
								}).FirstOrDefault(),
							ApproveActivity = approval.ApprovalActivities.Where(y => y.ApprovalID == approval.Id)
								.Select(ip => new ResponseApprovalActivities
								{
									Id = ip.Id,
									ApprovalID = ip.ApprovalID,
									ApprovalActivity = ip.ApprovalActivity == null ? activity : ip.ApprovalActivity,
									ApprovalActivityDesc = ip.ApprovalActivity == null ? activityName : ip.ApprovalActivity.ToString().SplitCamelCase(),
									ApprovalDate = ip.InsertedAt,
									Remark = ip.Remark,
									Reason = ip.Reason,
									CurrentStep = ip.CurrentStep,
									NextStep = ip.NextStep
								}).ToList()
						}
					});//.Where(v => v.ApprovalStatus == Domain.Enum.ApprovalStatusEnum.Pending);

			ResponseApprovalRequestAndCount result = new ResponseApprovalRequestAndCount() { PageCount = query.Count() };
			if (page > 0 && limit > 0)
				query = query.Skip(skip).Take(limit);

			result.ResponseApprovalRequests = await query.ToListAsync();

			return result;

		}

		public async Task<ResponseApprovalRequestAndCount> GetMyApprovalRequest(int page = 0, int limit = 0)
		{
			ApprovalActivity activity = ApprovalActivity.PendingApproval;
			string activityName = activity.ToString().SplitCamelCase();

			var skip = Skip(page, limit);
			// && x.Status == Domain.Enum.ApprovalStatusEnum.Pending
			var approvalsRequest = context.Approvals
			.Where(x => x.InsertedBy == UserId)
			.Select(a => new ResponseApprovalRequest
			{
				RequestID = a.Id,
				RequestDate = a.LastActivityDate,
				DocumentTitle = a.Documents.DocumentTitle,
				DocumentID = a.Documents.Id,
				UserID = a.InsertedBy,
				Approver = a.CurrentApproverUser.FullName,
				NextApprover = a.NextApproverUser.FullName,
				ApprovalStep = (a.CurrentStep ?? 0) > 1 ? a.CurrentStep.ToString() + "/" + a.MaxStep.ToString() : (a.MaxStep.ToString() + "/" + a.MaxStep.ToString()),
				ApprovalStatus = (ApprovalStatusEnum)a.Status,
				ApprovalStatusName = ((ApprovalStatusEnum)a.Status).ToString().SplitCamelCase(),
				ResponseDate = a.LastActivityDate,
				LastRemark = a.LastRemark
				//Detail = new ResponseDetailMyApprovalRequest
				//{
				//	Notes = a.Notes,
				//	ApprovalFlows = a.ApprovalFlows
				//				.Select(u => new ResponseApprovalFlow
				//				{
				//					Id = u.Id,
				//					ApprovalID = u.ApprovalID,
				//					Step = u.Step,
				//					ApprovalActivity = a.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step) == null ? activityName : a.ApprovalActivities.FirstOrDefault(t => t.CurrentStep == u.Step).ApprovalActivity.ToString().SplitCamelCase(),
				//					User = new ResponseUser
				//					{
				//						UserID = u.User.UserId,
				//						UserName = u.User.UserName,
				//						FullName = u.User.FullName,
				//						Email = u.User.EmailAddress
				//					}
				//				}).ToList(),
				//	MainFile = a.Documents.DocumentFiles.Where(df => df.IsMainDocumentFile == true)
				//				.Select(f => new ResponseDocumentFiles
				//				{
				//					Id = f.Id,
				//					DocumentID = f.DocumentID,
				//					DocumentFileName = f.DocumentFileName,
				//					DocumentFileContent = f.DocumentFileContent,
				//					DocumentFilePath = f.DocumentFilePath,
				//					DocumentFileSize = SizeFormatter.SizeSuffix(f.DocumentFileSize, 2),
				//					DocumentFileSizeInt = f.DocumentFileSize,
				//					DocumentType = f.DocumentType,
				//					IsMainDocumentFile = f.IsMainDocumentFile,
				//					UpdateAt = f.UpdatedAt,
				//					UpdatedByUserName = f.UpdatedByUserName,
				//					UpdatedByFullName = f.UpdatedByFullName
				//				}).FirstOrDefault(),
				//	ApproveActivity = a.ApprovalActivities.Where(y => y.ApprovalID == a.Id)
				//				.Select(ip => new ResponseApprovalActivities
				//				{
				//					Id = ip.Id,
				//					ApprovalID = ip.ApprovalID,
				//					ApprovalActivity = ip.ApprovalActivity == null ? activity : ip.ApprovalActivity,
				//					ApprovalActivityDesc = ip.ApprovalActivity == null ? activityName : ip.ApprovalActivity.ToString().SplitCamelCase(),
				//					ApprovalDate = ip.InsertedAt,
				//					Remark = ip.Remark,
				//					Reason = ip.Reason,
				//					CurrentStep = ip.CurrentStep,
				//					NextStep = ip.NextStep
				//				}).ToList()
				//}
				// 3. Activity History requires a collection load (Still easy)
				//ActivityHistory = a.ApprovalActivities
				//	.OrderByDescending(act => act.InsertedAt)
				//	.Select(act => new { act.Remark, act.ApprovalActivity, act.InsertedAt, act.CurrentStep })
				//	.ToList()
			});


			ResponseApprovalRequestAndCount result = new ResponseApprovalRequestAndCount() { PageCount = await approvalsRequest.CountAsync() };
			if (page > 0 && limit > 0)
				approvalsRequest = approvalsRequest.OrderByDescending(a => a.RequestID).Skip(skip).Take(limit);

			result.ResponseApprovalRequests = await approvalsRequest.ToListAsync();

			return result;
		}

		public async Task<ResponseCheckApproval> GetInfoCheckForApprovalPageView(int approvalId)
		{
			ApprovalActivity activity = ApprovalActivity.PendingApproval;
			string activityName = activity.ToString().SplitCamelCase();

			return await context.Approvals.Where(x => x.Id == approvalId).Include(x => x.ApprovalActivities).ThenInclude(x => x.InsertedUser)
			.Select(t => new ResponseCheckApproval()
			{
				ApprovalID = t.Id,
				DocumentID = t.DocumentID,
				Notes = t.Notes,
				DocumentFiles = t.Documents.DocumentFiles
					.Select(f => new ResponseDocumentFiles
					{
						Id = f.Id,
						DocumentID = f.DocumentID,
						DocumentFileName = f.DocumentFileName,
						DocumentType = f.DocumentType,
						DocumentFilePath = f.DocumentFilePath,
						DocumentFileSize = SizeFormatter.SizeSuffix(f.DocumentFileSize, 2),
						DocumentFileSizeInt = f.DocumentFileSize,
						IsMainDocumentFile = f.IsMainDocumentFile,
						UpdateAt = f.UpdatedAt,
						UpdatedByFullName = f.UpdatedByUser.FullName
					}).ToList(),
				ApprovalActivities = t.ApprovalActivities
					.Select(ip => new ResponseApprovalActivities
					{
						Id = ip.Id,
						ApprovalID = ip.ApprovalID,
						ApprovalActivity = ip.ApprovalActivity == null ? activity : ip.ApprovalActivity,
						ApprovalActivityDesc = ip.ApprovalActivity == null ? activityName : ip.ApprovalActivity.ToString().SplitCamelCase(),
						ApprovalDate = ip.InsertedAt,
						Remark = ip.Remark,
						Reason = ip.Reason == null ? ip.Remark : ip.Reason,
						CurrentStep = ip.CurrentStep,
						NextStep = ip.NextStep,
						UserName = ip.InsertedUser.UserName
					}).ToList()

			}).FirstOrDefaultAsync();
		}

		public async Task<int> ApprovalAction(int approvalId, ApprovalActivities activity) //, int? documentId = null, int? categoryId = null
		{
			int result = 0;
			var strategy = context.Database.CreateExecutionStrategy();
			await strategy.ExecuteAsync(async () =>
			{
				using var transaction = context.Database.BeginTransaction();
				{

					try
					{
						Approvals approval = await GetSingleAsync(x => x.Id == approvalId);
						List<ApprovalFlows> appFlow = context.ApprovalFlows.Where(x => x.ApprovalID == approvalId).ToList();

						ApprovalFlows info = appFlow.FirstOrDefault(x => x.ApproverUserID == UserId);
						int MaxStep = 0;
						int currentStep = 0;
						if (info != null)
						{
							MaxStep = approval.MaxStep;
							currentStep = info.Step;
						}

						ApprovalFlows infoNextActor = appFlow.FirstOrDefault(x => x.Step == (currentStep + 1));


						ApprovalActivity ApprovalActivityEnum = 0;
						string ApprovalActivityName = "";

						if (activity.ApprovalActivityName.ToLower() == "reject")
						{
							ApprovalActivityEnum = ApprovalActivity.Reject;
							ApprovalActivityName = "Reject";
						}
						if (activity.ApprovalActivityName.ToLower() == "approve")
						{
							ApprovalActivityEnum = ApprovalActivity.Approve;
							ApprovalActivityName = "Approve";
						}

						User NextApprover = null;
						if (infoNextActor != null)
						{
							NextApprover = await context.User.FirstOrDefaultAsync(x => x.UserId == infoNextActor.ApproverUserID);
						}

						User CurrentApprover = await context.User.FirstOrDefaultAsync(x => x.UserId == UserId);
						var document = await context.Documents.FirstOrDefaultAsync(x => x.Id == approval.DocumentID);
						User Requestor = await context.User.FirstOrDefaultAsync(x => x.UserId == document.InsertedBy);

						Notifications notif = new Notifications()
						{
							DocumentID = approval.DocumentID,
							NotificationType = (short)NotificationType.Approval,
							InsertedBy = UserId,
							InsertedAt = DateTime.Now
						};

						info.IsFinalStep = info.Step == MaxStep;
						//jika telah tahap akhir / maxstep
						//if (info.Step == MaxStep)
						if (info.IsFinalStep)
						{
							if (ApprovalActivityName == "Approve")
							{
								approval.Status = Domain.Enum.ApprovalStatusEnum.Approved;

								notif.NotifDescription = $"Document Approved";
								notif.TargetActor = Requestor.UserId;
							}
							else if (ApprovalActivityName == "Reject")
							{
								approval.Status = Domain.Enum.ApprovalStatusEnum.Rejected;

								notif.NotifDescription = $"Document Rejected";
								notif.TargetActor = Requestor.UserId;
							}

							approval.CurrentStep = null;
							approval.CurrentApproverUserID = null;
							approval.NextApproverUserID = null;

							DateTime ActivityDate = DateTime.Now;

							approval.LastRemark = activity.Remark;
							approval.LastActivityDate = ActivityDate;
							approval.UpdatedBy = UserId;
							approval.UpdatedAt = DateTime.Now;

							context.Update(approval);
							result += context.SaveChanges();

							ApprovalActivities activities = new ApprovalActivities()
							{
								ApprovalID = approvalId,
								ApprovalActivity = ApprovalActivityEnum,
								InsertedAt = ActivityDate,
								ApprovalActivityName = ApprovalActivityName,
								CurrentStep = info.Step,
								Remark = activity.Remark,
								Reason = activity.Reason,
								InsertedBy = UserId
							};

							await context.ApprovalActivities.AddAsync(activities);
							result += context.SaveChanges();

							await context.Notifications.AddAsync(notif);
							result += context.SaveChanges();
						}
						else
						{
							if (ApprovalActivityName == "Reject")
							{
								approval.Status = Domain.Enum.ApprovalStatusEnum.Rejected;

								notif.NotifDescription = $"Document Rejected by {CurrentApprover.FullName}, Step {info.Step}/{info.Step + 1} ";
								notif.TargetActor = Requestor.UserId;
							}
							else
							{
								approval.Status = Domain.Enum.ApprovalStatusEnum.Pending;
								if (infoNextActor != null)
								{
									notif.NotifDescription = $"Document Approve by {CurrentApprover.FullName}, Step {info.Step}/{info.Step + 1}";
									notif.TargetActor = infoNextActor.ApproverUserID;
								}
							}

							approval.CurrentStep = infoNextActor.Step;
							if (infoNextActor != null)
								approval.CurrentApproverUserID = infoNextActor.ApproverUserID;
							//if ((infoNextActor.Step + 1) > MaxStep)
							//{
							//	approval.NextApproverUserID = null;
							//}
							//else
							//{
							//	ApprovalFlows infoNextNextActor = appFlow.FirstOrDefault(x => x.Step == (infoNextActor.Step + 1));
							//	approval.NextApproverUserID = infoNextNextActor.ApproverUserID;
							//}
							ApprovalFlows infoNextNextActor = appFlow.FirstOrDefault(x => x.Step == (infoNextActor.Step + 1));
							if (infoNextNextActor != null)
								approval.NextApproverUserID = infoNextNextActor.ApproverUserID;
							else
								approval.NextApproverUserID = null;

							DateTime ActivityDate = DateTime.Now;
							approval.LastRemark = activity.Remark == null ? activity.Reason : activity.Remark;
							approval.LastActivityDate = ActivityDate;
							approval.UpdatedBy = UserId;
							approval.UpdatedAt = DateTime.Now;

							context.Update(approval);
							result += context.SaveChanges();

							ApprovalActivities activities = new ApprovalActivities()
							{
								ApprovalID = approvalId,
								ApprovalActivity = ApprovalActivityEnum,
								InsertedAt = DateTime.Now,
								ApprovalActivityName = ApprovalActivityName,
								CurrentStep = info.Step,
								Remark = activity.Remark,
								Reason = activity.Reason,
								NextStep = infoNextActor == null ? null : infoNextActor.Step,
								InsertedBy = UserId
							};

							await context.ApprovalActivities.AddAsync(activities);
							result += context.SaveChanges();

							await context.Notifications.AddAsync(notif);
							result += context.SaveChanges();
						}

						//======== Emailing ========
						Email email = null;

						if (ApprovalActivityName == "Approve")
						{
							if (infoNextActor == null)
							{
								email = new Email()
								{
									Subject = "DMS Approval",
									To = Requestor.EmailAddress,
									Body = $"Please be inform, that your request document have been approved. ID document : {document.Id} with title document : {document.DocumentTitle}",
									IsHtml = true
								};
								//await context.Email.AddAsync(email);
								//result += context.SaveChanges();
							}
							else
							{
								email = new Email()
								{
									Subject = "DMS Approval",
									To = NextApprover.EmailAddress,
									Cc = Requestor.EmailAddress,
									Body = $"Please be inform, that any document need your approval. ID document : {document.Id} with title document : {document.DocumentTitle}",
									IsHtml = true
								};
							}

							await context.Email.AddAsync(email);
							result += context.SaveChanges();
						}
						else if (ApprovalActivityName == "Reject")
						{
							if (infoNextActor == null)
							{
								email = new Email()
								{
									Subject = "DMS Rejection",
									To = Requestor.EmailAddress,
									Body = $"Please be inform, that your request document have been rejected. ID document : {document.Id} with title document : {document.DocumentTitle}, Rejected Reason : {activity.Reason}",
									IsHtml = true
								};
								await context.Email.AddAsync(email);
								result += context.SaveChanges();
							}
							else
							{
								email = new Email()
								{
									To = Requestor.EmailAddress,
									Body = $"Please be inform, that your request document have been rejected. ID document : {document.Id} with title document : {document.DocumentTitle}, Rejected Reason : {activity.Reason}",
									IsHtml = true
								};
								await context.Email.AddAsync(email);
								result += context.SaveChanges();
							}
							//jika reject harus dipikirkan juga ketika requester sudah revisi kemudian submit ulang, maka approver2 yang sudah approve sebelumnya harus bisa approve lagi.
						}

						//cek current step == maxstep
						//jika sama ubah :
						//status approval approve
						//jika tidak sama
						//ubah status approval jd pending/reject
						//status pada documents jd reject/pending

						//masukan notifikasi kepada next step & requestor
						//send email kepada next step & requestor
						//jika approve kirim email & notifikasi kepada approver (have been approve) & cc ke step dibawahnya, kirim email to ke requestor

						await transaction.CommitAsync();
					}
					catch (Exception ex)
					{
						await transaction.RollbackAsync();
						throw;
					}
				}
			});

			return result;

		}

	}
}
