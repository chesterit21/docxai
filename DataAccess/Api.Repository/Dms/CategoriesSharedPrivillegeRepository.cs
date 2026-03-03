using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Api.DataAccess.Models.Dms;
using Api.Domain.EntityRequests.Dms;

namespace Api.Repository.Masters
{
	public interface ICategoriesSharedPrivillegeRepository //: IRepository<CategoriesSharedPrivillege>
	{
		//Task<CategoriesSharedPrivillege> InsertCategoriesSharedPriv(int categoriesSharedID, List<Users> user);
	}

	public class CategoriesSharedPrivillegeRepository(DataContext context, IHttpContextAccessor accessor) //: Repository<CategoriesSharedPrivillege>(context, accessor), ICategoriesSharedPrivillegeRepository
	{

		//public async Task<CategoriesSharedPrivillege> InsertCategoriesSharedPriv(int categoriesSharedID,List<Users> user)
		//{
		//	var modelCategoriesShared = new CategoriesSharedPrivillege();
		//	foreach (var x in user)
		//	{
		//		modelCategoriesShared.Id = 0;
		//		modelCategoriesShared.CategoriesSharedID = categoriesSharedID;
		//		if (user.Select(x => x.UserPrivillege.Text == "All").FirstOrDefault())
		//		{
		//			modelCategoriesShared.IsView = true;
		//			modelCategoriesShared.IsEdit = true;
		//			modelCategoriesShared.IsDelete = true;
		//		}
		//		else if (user.Select(x => x.UserPrivillege.Text == "View").FirstOrDefault())
		//			modelCategoriesShared.IsView = true;
		//		else if (user.Select(x => x.UserPrivillege.Text == "Edit").FirstOrDefault())
		//			modelCategoriesShared.IsEdit = true;
		//		else if (user.Select(x => x.UserPrivillege.Text == "Delete").FirstOrDefault())
		//			modelCategoriesShared.IsDelete = true;

		//		await InsertAsync(modelCategoriesShared);

		//	}
		//	return modelCategoriesShared;
		//}
	}
}