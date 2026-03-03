using Api.DataAccess;
using Api.DataAccess.Models.Masters;
using Api.Domain.EntityRequests.Authentications;
using Api.Domain.EntityRequests.Masters;
using Api.Domain.Enum;
using Api.Extensions;
using Api.Services.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Dml.Diagram;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Api.Services
{
	public static class Seeder
	{
		//public static async Task SeedItAsync(IServiceProvider serviceProvider, string userConn)
		public static async Task SeedItAsync(DataContext mydbcontext)
		{
			//company
			//role
			//user
			//user role
			//usercompany	
			//tblmsuserroles

			//var options = new DbContextOptionsBuilder<DataContext>()
			//.UseNpgsql(userConn)
			//.Options;

			//var httpAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
			//await using var mydbcontext = new DataContext(options, httpAccessor);

			//using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
			//await using var mydbcontext = serviceScope.ServiceProvider.GetService<DataContext>();
				////context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
			if (mydbcontext == null) return;
			
			await mydbcontext.Database.MigrateAsync();
			await mydbcontext.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");


			var isThere = await mydbcontext.Company.AsNoTracking().Where(x => "shuba".Contains(x.CompanyId)).ToListAsync().ConfigureAwait(false);
			//if (isThere.Count < 0) return;
			//mydbcontext.RemoveRange(isThere);
			//await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			if (isThere.Count < 1)
			{
				await mydbcontext.Company.AddAsync(new Company()
				{
					CompanyId = "shuba",
					Name = "PT. Shuba Mitra Solusi",
					Address = "Jakarta",
					PhoneNumber = "021339478"
				});
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			}

			string roleAdminStr = "System Administrator";
			var isRoleAdminExist = await mydbcontext.Role.AsNoTracking().Where(x => roleAdminStr.Contains(x.Name)).ToListAsync().ConfigureAwait(false);
			//if (isRoleAdminExist.Count < 0) return;
			//mydbcontext.RemoveRange(isRoleAdminExist);
			Role AdminRole = null;
			if (isRoleAdminExist.Count < 1)
			{
				AdminRole = new Role() { Name = roleAdminStr };
				await mydbcontext.Role.AddAsync(AdminRole).ConfigureAwait(false);
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			}
			else 
			{
				AdminRole = await mydbcontext.Role.AsNoTracking().FirstOrDefaultAsync(x => roleAdminStr.Contains(x.Name));
			}

				string grouEveryone = "Everyone";
			var isGroupEveryoneExist = await mydbcontext.Group.AsNoTracking().Where(x => grouEveryone.Contains(x.GroupName)).ToListAsync().ConfigureAwait(false);

			if (isGroupEveryoneExist.Count < 1)
			{
				Group grp = new Group() { GroupName = grouEveryone, GroupDescription = "Default group by system", IsSystem = true };
				await mydbcontext.Group.AddAsync(grp).ConfigureAwait(false);
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			}

			DateTime userDateNow = DateTime.Now;
			User user = new User()
			{
				UserName = "superadmin",
				CompanyId = "shuba",
				FullName = "System Administrator",
				IsADUser = false,
				UserPassword = "P@ssw0rd".HashPassword(),
				IsActive = true,
				EmailAddress = "taufiq.iman@shuba.co.id",
				PhoneNumber = "62085759802620",
				EmailVerified = true,
				IsAdmin = true,
				UserType = "superadmin",
				IsDeleted = false,
				InsertedAt = userDateNow,
				UpdatedAt= userDateNow,
				UserStatus = UserStatus.Active.GetHashCode()
			};
			var isUserAdminExist = await mydbcontext.User.AsNoTracking().Where(x => x.UserName.Contains(user.UserName)).ToListAsync().ConfigureAwait(false);
			//if (isRoleAdminExist.Count < 0) return;
			//mydbcontext.RemoveRange(isUserAdminExist);
			if (isUserAdminExist.Count < 1)
			{
				await mydbcontext.User.AddAsync(user).ConfigureAwait(false);
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			}
			else
			{
				user = await mydbcontext.User.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == "superadmin");
			}

			//var UserCompanyRes = await mydbcontext.UserCompany.AsNoTracking().Where(x => x.UserId == user.UserId && x.CompanyId == "shuba").ToListAsync().ConfigureAwait(false);
			//if (UserCompanyRes.Count < 0)
			//{
			//	UserCompany userComp = new UserCompany() { UserId = user.UserId, CompanyId = "shuba" };
			//	await mydbcontext.UserCompany.AddAsync(userComp).ConfigureAwait(false);
			//	await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			//}

			var UserRole = await mydbcontext.UserRole.AsNoTracking().Where(x => x.UserId == user.UserId).ToListAsync().ConfigureAwait(false);
			if (UserRole.Count < 1)
			{
				UserRole userRole = new UserRole()
				{
					UserId = user.UserId,
					RoleId = AdminRole.RoleId,
					InsertedAt = DateTime.Now,
					InsertedBy = user.UserId,
					UpdatedAt = DateTime.Now,
					UpdatedBy = user.UserId
				};
				await mydbcontext.UserRole.AddAsync(userRole).ConfigureAwait(false);
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			}

			//language
			var mslanguage = await mydbcontext.Language.AsNoTracking().ToListAsync().ConfigureAwait(false);
			//if (mslanguage.Count < 0) return;
			//mydbcontext.RemoveRange(mslanguage);
			int Actor = user.UserId;
			DataAccess.Models.Masters.Language[] langs = new DataAccess.Models.Masters.Language[]
			{
				new DataAccess.Models.Masters.Language(){ Code = "lang-back", Id = "Kembali", En = "Back", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "lang-approve", Id = "Setuju", En = "Approve", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "lang-send", Id = "Kirim", En = "Send", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "lang-cancel", Id = "Batal", En = "Cancel", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "pass-invalid", Id = "User ID atau password yang Anda masukkan salah", En = "Invalid User ID or password", Type="Message", },
				new DataAccess.Models.Masters.Language(){ Code = "pass-length", Id = "Jumlah karater password tidak memenuhi kriteria", En = "Your password does not match max or min characters", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "pass-number", Id = "Password harus terdiri setidaknya dua angka", En = "Your password should contains at least two numbers", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "pass-upper-case", Id = "Password harus terdiri setidaknya dua huruf kapital", En = "Your password should contains at least two upper characters"},
				new DataAccess.Models.Masters.Language(){ Code = "pass-repetitive", Id = "Password mengandung pengulangan karakter", En = "Your password contains too many repetitive characters", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "input-empty", Id = "Semua field mandatory harus terisi", En = "One or more fields are required", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "input-invalid", Id = "Data yang Anda masukkan salah", En = "You have entered an invalid value", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "input-invalid-type", Id = "Tipe data yang Anda masukkan salah", En = "You have entered an invalid type", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "input-invalid-format", Id = "Format yang Anda masukkan salah", En = "You have entered an invalid format", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "input-invalid-code", Id = "Kode yang Anda masukkan salah", En = "You have entered an invalid code", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "input-invalid-daterange", Id = "Rentang tanggal yang Anda masukkan tidak sesuai", En = "You have entered an invalid date range", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "message-sent", Id = "Pesan telah terkirim", En = "Your message was sent", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "message-unsent", Id = "Pesan gagal terkirim", En = "Your message was not sent", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "email-invalid", Id = "Email yang Anda masukkan tidak valid", En = "Invalid email", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "email-verified", Id = "Email sudah diverifikasi", En = "Email has been verified", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "email-unverified", Id = "Email belum terverifikasi", En = "Email is not verified", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "email-expired", Id = "Kode verifikasi email Anda tidak berlaku. Hubungi Administrator", En = "Your email verfication has expired. Contact your Administrator", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "process-success", Id = "Berhasil", En = "Successfull", Type="Message" },
				new DataAccess.Models.Masters.Language(){ Code = "process-failed", Id = "Failed", En = "Failed", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "duplicate", Id = "Anda memasukkan duplikasi data. Masukkan data yang berbeda", En = "The data you provide has been set. Enter another one", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "unknown-exception", Id = "Error tidak diketahui", En = "Unknown error", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "not-found", Id = "Data yang Anda cari tidak ditemukan", En = "The data you are looking for was not found", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "login-exeeded", Id = "Anda sudah melakukan kesalahan login melebihi batas maksimal. Hubungi Andmistrator", En = "Your login attempt exeeded. Please, contact your Administrator", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "login-invalid", Id = "Login gagal. Username atau password yang Anda masukkan salah", En = "Login failed. You've' entered invalid username or password", Type="Message"},
				new DataAccess.Models.Masters.Language(){ Code = "login-already", Id = "sedang login", En = "already login", Type="Message" },
				new DataAccess.Models.Masters.Language(){ Code = "date-expired", Id = "Tanggal sudah kadaluarsa", En = "Date expired", Type="Message" },
				new DataAccess.Models.Masters.Language(){ Code = "document-same-references", Id = "tidak bisa merelasikan dengan document yang sama, pilih dokumen lain untuk relasinya", En = "Cannot relating with same document, choose another document as related document", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "document-file-name-exist", Id = "A file named @fileName already exists for this document.", En = "File dengan nama @fileName telah ada untuk dokumen ini.", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "upload-ext-invalid", Id = "File Not Allowed to be upload", En = "File tidak diizinkan, ekstensi diizinkan (.jpg, .pdf, .doc, .docx, .xls, .xlsx, .txt, .jpeg, .png, .ppt, .pptx)", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "category-title-exist", Id = "Judul Document Telah terdaftar", En = "Document Title Alrady exist", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "password-changed", Id = "Password Anda Berhasil Diubah", En = "Your Password successfully change", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "itemlist-files-empty", Id = "Lampiran kosong untuk dokumen-dokumen yang terpilih", En = "Attachment is mepty for selected documents", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "password-not-match", Id = "Password tidak sama", En = "Password not match", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "pending-approval", Id = "Dokumen pending, tidak bisa mengubah dokumen.", En = "Document is on pending, cannot update the document.", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "duplicate-username", Id = "Username yang anda masukan telah terdaftar, masukan username lain", En = "Username you provide has been set. Enter another username", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "duplicate-email-address", Id = "Email Address yang anda masukan telah terdaftar, masukan email address lain", En = "Email Address you provide has been set. Enter another email address", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "group-everyone", Id = "Group  ini tidak bisa diedit ataupun dihapus, karena group ini dugunakan secara default oleh sistem", En = "This Group Cannot Be Modify or Deleted, because this default group used by system", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "not-the-owner", Id = "Anda bukan pemilik dari kategori atau dokumen ini, menghapus tidak diizinkan", En = "You are not the owner of this category or document, delete is not permitted", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "document-title-exist", Id = "Judul dokumen telah ada, masukan judul yang lain", En = "Document title already exist", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "attribute-title-exist", Id = "Nama Attribute telah terdaftar, silakan masukan nama attribute yang lain", En = "Attribute name already exist", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "user-max-reached", Id = "Maksimum user telah mencapai batas.", En = "Max user register has reach the limit.", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "duplicate-phone-number", Id = "Nomor Telepon sudah digunakan oleh pengguna lain.", En = "Phone Numebr Already used by another user", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "invalid-code-verification", Id = "Kode verifikasi tidak valid", En = "invalid code verification", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code = "not-the-owner-no-priv", Id = "Anda bukan pemilik dari kategori atau dokumen ini, menghapus tidak diizinkan", En = "You are not the owner of this category or document.", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
				new DataAccess.Models.Masters.Language(){ Code="attribute-name-exist", Id="Nama Atribut Telah Terdaftar", En = "Attribut name already exist", Type="Message", InsertedBy = Actor, InsertedAt=DateTime.Now }
				//new DataAccess.Models.Masters.Language(){ Code = "", Id = "", En = "", Type="Message", InsertedBy = Actor, InsertedAt = DateTime.Now },
			};

			var duplicateCodes = langs
				.GroupBy(lang => lang.Code)
					.Where(g => g.Count() > 1)
				.Select(g => new { Code = g.Key, Count = g.Count() })
				.ToList();

			if (mslanguage.Count == 0)
			{
				await mydbcontext.Language.AddRangeAsync(langs).ConfigureAwait(false);
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			}

			var Attributes = await mydbcontext.Attributtes.AsNoTracking().Where(x => x.IsActive == true).ToListAsync().ConfigureAwait(false);
			if (Attributes.Count < 1)
			{
				//string json = "{\"type\":\"number\",\"required\":\"true\",\"label\":\"Days Of Reminder\",\"placeholder\":\"days of reminder\",\"helptext\":\"digunakan untuk mengitung waktu mundur s.d dokumen kadaluarsa\",\"min\":1,\"max\":30,\"name\":\"days_of_reminder\"}";
				Api.DataAccess.Models.Dms.Attributes att = new Api.DataAccess.Models.Dms.Attributes()
				{
					AttributeName = "Days Of Reminder",
					AttributeElement = "{\"type\":\"number\",\"required\":true,\"label\":\"Days Of Reminder\",\"placeholder\":\"days of reminder\",\"helptext\":\"digunakan untuk mengitung waktu mundur s.d dokumen kadaluarsa\",\"min\":0,\"max\":10000,\"name\":\"days_of_reminder\"}",
					AttributeType = "number",
					InsertedBy = user.UserId,
					InsertedAt = DateTime.Now,
					UpdatedAt = DateTime.Now,
					UpdatedBy = user.UserId,
					IsSystem = true,
				};
				//var json1 = JsonConvert.DeserializeObject<Api.Domain.EntityRequests.Dms.JsonAttribute>(att.AttributeElement);
				//att.AttributeElement = json1.ToString();
				await mydbcontext.Attributtes.AddAsync(att).ConfigureAwait(false);
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);

				Api.DataAccess.Models.Dms.Attributes att2 = new Api.DataAccess.Models.Dms.Attributes()
				{
					AttributeName = "Expired Date",
					AttributeElement = "{\"type\":\"date\",\"required\":true,\"label\":\"Expired Date\",\"placeholder\":\"Date expiration of the document\",\"helptext\":\"tanggal kadaluarsa\",\"format\":\"MM-DD-YYYY\",\"name\":\"expired_date\"}",
					AttributeType = "date",
					InsertedBy = user.UserId,
					InsertedAt = DateTime.Now,
					UpdatedAt = DateTime.Now,
					UpdatedBy = user.UserId,
					IsSystem = true,
				};
				//var json2 = JsonConvert.DeserializeObject<Api.Domain.EntityRequests.Dms.JsonAttribute>(att2.AttributeElement);
				//att2.AttributeElement = json2.ToString();
				//att2.AttributeElement = JsonConvert.DeserializeObject<Api.Domain.EntityRequests.Dms.JsonAttribute>(att2.AttributeElement).ToString();
				await mydbcontext.Attributtes.AddAsync(att2).ConfigureAwait(false);
				await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
			}
		}
	}
}
