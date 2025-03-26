using Api.Domain.EntityRequests.Authentications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tauxunit
{
    public class UserServiceTest
    {
        [Fact]
        public async Task UserService_CreateUserTest()
        {
            using var context = MockInstance.GetSqlDbContext();
            var repo = MockInstance.GetRepositoryInstances(context);
            var service = repo.UserService;

            var admnistrator = 1;

            await service.Create(new RequestUserCreate
            {
                IsADUser = false,
                UserName = "taadmin",
                FullName = "Admin Taufiq",
                //Password = "P@ssW0rd123", //P@ssw0rd
                CompanyId = "shuba",
                Roles = [admnistrator]
            });

            var user = await service.GetUser("taadmin");
            Assert.NotNull(user);
        }
    }
}
