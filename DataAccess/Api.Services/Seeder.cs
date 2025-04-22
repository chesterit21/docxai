using Api.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Api.Domain.EntityRequests.Masters;
using Api.Services.Masters;
using Api.Domain.EntityRequests.Authentications;
using Api.DataAccess.Models.Masters;
using NPOI.OpenXmlFormats.Dml.Diagram;
using System.Xml.Linq;
using Api.Extensions;

namespace Api.Services
{
    public static class Seeder
    {
        public static async void SeedIt(IServiceProvider serviceProvider)
        {
            //company
            //role
            //user
            //usercompany
            //tblmsuserroles
            using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            await using var mydbcontext = serviceScope.ServiceProvider.GetService<DataContext>();
            //context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            if (mydbcontext == null) return;
            await mydbcontext.Database.MigrateAsync();
            var isThere = await mydbcontext.Company.AsNoTracking().Where(x => "shuba".Contains(x.CompanyId)).ToListAsync().ConfigureAwait(false);
            //if (isThere.Count < 0) return;
            //mydbcontext.RemoveRange(isThere);
            //await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
            if (isThere.Count == 0)
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
            if (isRoleAdminExist.Count == 0)
            {
                Role role = new Role() { Name = roleAdminStr };
                await mydbcontext.Role.AddAsync(role).ConfigureAwait(false);
                await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
            }


            User user = new User()
            { UserName = "superadmin", CompanyId = "shuba", FullName = "System Administrator", IsADUser = false, UserPassword = "P@ssw0rd".HashPassword(), IsActive = true, EmailAddress = "taufiq.iman@shuba.co.id", EmailVerified = true };
            var isUserAdminExist = await mydbcontext.User.AsNoTracking().Where(x => x.UserName.Contains(user.UserName)).ToListAsync().ConfigureAwait(false);
            //if (isRoleAdminExist.Count < 0) return;
            //mydbcontext.RemoveRange(isUserAdminExist);
            if (isUserAdminExist.Count == 0)
            {
                await mydbcontext.User.AddAsync(user).ConfigureAwait(false);
                await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
            }

            var UserCompanyRes = await mydbcontext.UserCompany.AsNoTracking().Where(x => x.UserId == user.UserId && x.CompanyId == "shuba").ToListAsync().ConfigureAwait(false);
            //if (UserCompanyRes.Count < 0) return;
            //mydbcontext.RemoveRange(UserCompanyRes);
            //await mydbcontext.SaveChangesAsync().ConfigureAwait(false);

            if (UserCompanyRes.Count > 0)
            {
                UserCompany userComp = new UserCompany() { UserId = user.UserId, CompanyId = "shuba" };
                await mydbcontext.UserCompany.AddAsync(userComp).ConfigureAwait(false);
                await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
            }

            //language
            var mslanguage = await mydbcontext.Language.AsNoTracking().ToListAsync().ConfigureAwait(false);
            //if (mslanguage.Count < 0) return;
            //mydbcontext.RemoveRange(mslanguage);

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
                new DataAccess.Models.Masters.Language(){ Code = "date-expired", Id = "Tanggal sudah kadaluarsa", En = "Date expired", Type="Message" }
            };
            if (mslanguage.Count == 0)
            {
                await mydbcontext.Language.AddRangeAsync(langs).ConfigureAwait(false);
                await mydbcontext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
