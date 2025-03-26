using Api.Domain.EntityRequests.Authentications;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityRequests.Masters;

namespace tauxunit
{
    public class CategoryServiceTest
    {
        [Fact]
        public async Task CategoryService_Create()
        {
            using var context = MockInstance.GetSqlDbContext();
            var repo = MockInstance.GetRepositoryInstances(context);
            var service = repo.CategoryService;

            var model = await service.Create(new RequestCategory
            {
                CategoryName = "Legalitas",
                CategoryDesc = "Akta Pendirian, NPWP, SIUP, Surat Keterangan Domisili",
                ParentId = null,
                ParentCategory = null,
                ChildCategories = null
            });

            //var category = await service.GetCategory("Legalitas");
            //Assert.NotNull(category);
            Assert.NotNull(model);
        }

        //[Fact]
        //public async Task UserService_CreateAndDeleteUser()
        //{
        //    using var context = MockInstance.GetSqlDbContext();
        //    var repo = MockInstance.GetRepositoryInstances(context);
        //    var service = repo.UserService;
        //    var roleOther = 3;

        //    await service.DeleteWithChildren([1]);
        //    await service.Create(new RequestUserCreate
        //    {
        //        IsADUser = false,
        //        UserName = "test@shuba.co.id",
        //        FullName = "Test",
        //        //Password = "5HUB4T0p!23",
        //        CompanyId = "shuba",
        //        Roles = [roleOther]
        //    });

        //    var _users = await service.GetUsers(1, 100);
        //    Assert.Equal(4, _users.Count);
        //}

        //[Fact]
        //public async Task Service_ValidateLoginAD()
        //{
        //    using var context = MockInstance.GetSqlDbContext();
        //    var repo = MockInstance.GetRepositoryInstances(context);
        //    var service = repo.UserService;

        //    var validity = await service.Login("aduser", "P@ssw0rd");
        //    Assert.Equal("pertamina", validity?.CompanyId);
        //}

        //[Fact]
        //public async Task Service_ValidateLoginEmail()
        //{
        //    using var context = MockInstance.GetSqlDbContext();
        //    var repo = MockInstance.GetRepositoryInstances(context);
        //    var service = repo.UserService;

        //    var validity = await service.Login("superadmin@shuba.co.id", "5HUB4T0p!23");
        //    Assert.Equal("shuba", validity?.CompanyId);
        //}
    }
}
