using Api.DataAccess;
using Api.DataAccess.Models.Dms;
using Api.DataAccess.Models.Masters;
using Api.DataAccess.Models.Systems;
using Api.Domain.EntityResponses.Dms;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Api.Repository.Masters
{
    public interface IMigrationJobRepository
    {
		Task<MigrationJob> CheckStatus(Guid jobId);
	}

    public class MigrationJobRepository(DataContext context, IHttpContextAccessor accessor) : IMigrationJobRepository
	{
		public async Task<MigrationJob> CheckStatus(Guid jobId)
		{
			var migrationJob = await context.MigrationJobs.Where(x => x.Id == jobId).FirstOrDefaultAsync();
			return migrationJob;
		}
    }
}
