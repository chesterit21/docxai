using Api.DataAccess;
using Api.DataAccess.Extensions;
using Api.DataAccess.Models.Masters;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Api.DataAccess.Models.Dms;
using Api.Domain.EntityRequests.Dms;
using Api.DataAccess.Models.Systems;
using Api.Repository.Systems;

namespace Api.Repository.Masters
{
	public interface IDropdownRepository: IRepository<Dropdown>
	{
		List<Dropdown> GetDropdownSharedPrivillege();
	}

	public class DropdownRepository(DataContext context, IHttpContextAccessor accessor) : Repository<Dropdown>(context, accessor), IDropdownRepository
	{
		public List<Dropdown> GetDropdownSharedPrivillege()
		{
			var list = new List<Dropdown>();
			list.Add(new Dropdown { Text = "All", Value = EnumPrivillege.All });
			list.Add(new Dropdown { Text = "View", Value = EnumPrivillege.View });
			list.Add(new Dropdown { Text = "Edit", Value = EnumPrivillege.Edit });
			list.Add(new Dropdown { Text = "Delete", Value = EnumPrivillege.Delete });

			return list;
		}

		//public async Task<CategoriesShared> SubmitShare(RequestCategoriesShared request)
		//{
		//	foreach (var x in request.User)
		//	{
		//		var modelCategoriesShared = new CategoriesShared();
		//		var modelCategoriesSharedPrivillege = new CategoriesSharedPrivillege();
		//		modelCategoriesShared.Id = request.Id;
		//		modelCategoriesShared.CategoryID = request.CategoryID;
		//		modelCategoriesShared.ApproverUserID = x.ApproverUserID;

		//		await InsertAsync(modelCategoriesShared);

		//		modelCategoriesSharedPrivillege.Id = 0;
		//		modelCategoriesSharedPrivillege.CategoriesSharedID = modelCategoriesShared.Id;
		//		modelCategoriesSharedPrivillege.IsView = x.UserPrivillege.Select(x => x.IsView).FirstOrDefault();
		//		modelCategoriesSharedPrivillege.IsEdit = x.UserPrivillege.Select(x => x.IsEdit).FirstOrDefault();
		//		modelCategoriesSharedPrivillege.IsDelete = x.UserPrivillege.Select(x => x.IsDelete).FirstOrDefault();

		//		return modelCategoriesShared;
		//		//await InsertAsync(modelCategoriesSharedPrivillege);
		//	}
		//}
	}
}