using Api.Domain.EntityRequests.Authentications;
using Api.Domain.EntityRequests.Masters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tauxunit
{
    public class RoleServiceTest
    {
        [Fact]
        public async Task CreateRoleTest()
        {
            using var context = MockInstance.GetSqlDbContext();
            var repo = MockInstance.GetRepositoryInstances(context);
            var service = repo.RoleService;
            var role = await service.Insert(new RequestRoleCreate { Name = "Administrator" });
            Assert.NotNull(role);
        }

        [Fact]
        public async Task GetRoleByNameTest()
        {
            using var context = MockInstance.GetSqlDbContext();
            var repo = MockInstance.GetRepositoryInstances(context);
            var service = repo.RoleService;
            var role = await service.GetByName("Administrator");
            Assert.NotNull(role);
        }
    }
}
