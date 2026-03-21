using Api.DataAccess;
using Api.DataAccess.Models.Systems;

namespace Api.Repository.Systems
{
    public class AiModelRepository(DataContext context, Microsoft.AspNetCore.Http.IHttpContextAccessor accessor) : Repository<AiModel>(context, accessor), IAiModelRepository
    {
    }
}
