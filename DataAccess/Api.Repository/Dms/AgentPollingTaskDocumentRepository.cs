using Api.DataAccess;
using Api.DataAccess.Models.Dms;

namespace Api.Repository.Dms
{
    public class AgentPollingTaskDocumentRepository : Repository<AgentPollingTaskDocument>, IAgentPollingTaskDocumentRepository
    {
        public AgentPollingTaskDocumentRepository(DataContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor accessor) : base(context, accessor)
        {
        }
    }
}
