using Api.Domain.EntityRequests.Masters;
using Api.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace tauxunit
{
    public class CompanyServiceTest
    {
        [Fact]
        public async Task Company_Add()
        {
            using var context = MockInstance.GetSqlDbContext();
            var repo = MockInstance.GetRepositoryInstances(context);

            var repository = repo.ICompanyRepository;
            var service = repo.CompanyService;

            await service.Delete(["shuba"]);

            //Add new row
            await service.Upsert([
                new RequestCompany
                {
                    CompanyId = "shuba",
                    Address = "Jakarta",
                    Name = "PT. SHUBA MITRA SOLUSI",
                    PhoneNumber = "xxx",
                }
             ]);

            var value = await service.GetAll(1, 100);
            var updated = await service.Get("shuba");
            Assert.NotNull(updated);
        }

        [Fact]
        public async Task GetFromRepository()
        {
            var context = MockInstance.GetSqlDbContext();
            var repository = MockInstance.GetRepositoryInstances(context);

            var company = repository.ICompanyRepository;

            var result = await company.GetAsync(1, 100);

            var flatted = ObjectFlatter.Flatten(result);
            var json = JsonSerializer.Serialize(flatted, new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.NotNull(result);
        }
    }
}
