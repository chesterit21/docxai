using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Api.DataAccess.Models.Dms;

namespace Api.Repository.Masters
{
    public interface ICategoryRepository : IRepository<Categories> { }

    public class CategoryRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Categories>(context, accessor), ICategoryRepository { }
}