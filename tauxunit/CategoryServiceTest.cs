using Xunit;
using Moq;
using Api.Services.Dms;
using Api.Repository.Masters;
using Api.Domain.EntityRequests.Dms;
using Api.Domain.EntityResponses.Dms;
using Api.DataAccess.Models.Dms;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace tauxunit
{
	public class CategoryServiceTest
	{
		private readonly Mock<ICategoryRepository> _repoMock;
		private readonly Mock<ILanguageRepository> _langRepoMock;
		private readonly Mock<IApprovalRepository> _approvalRepoMock;
		private readonly Mock<IHttpContextAccessor> _httpContextMock;
		private readonly CategoriesService _service;

		public CategoryServiceTest()
		{
			_repoMock = new Mock<ICategoryRepository>();
			_langRepoMock = new Mock<ILanguageRepository>();
			_approvalRepoMock = new Mock<IApprovalRepository>();
			_httpContextMock = new Mock<IHttpContextAccessor>();
			_service = new CategoriesService(_httpContextMock.Object, _langRepoMock.Object, _repoMock.Object, _approvalRepoMock.Object);
		}

		[Fact]
		public async Task CreateCategory_ShouldReturnCategory()
		{
			var request = new RequestCategory { CategoryName = "Test", CategoryDesc = "Desc" };
			//_repoMock.Setup(r => r.GetSingleAsync(It.IsAny<Func<Categories, bool>>())).ReturnsAsync((Categories)null);
			_repoMock.Setup(r => r.InsertAsync(It.IsAny<Categories>())).ReturnsAsync(new Categories { Id = 1, CategoryName = "Test" });

			var result = await _service.Create(request);

			Assert.NotNull(result);
		}

		[Fact]
		public async Task GetAll_ShouldReturnPagination()
		{
			var request = new RequestCategoryList { CategoryName = "Test", Page = 1, Limit = 10 };
			_repoMock.Setup(r => r.GetCategories(It.IsAny<string>(), true, null, 1, 10))
				.ReturnsAsync(new ResponseCategoriesAndCount { Records = new List<ResponseCategoryListItem>(), TotalRecord = 0 });
			//_repoMock.Setup(r => r.GetAsync(It.IsAny<Func<Categories, bool>>()))
			//	.ReturnsAsync(new List<Categories>());

			var result = await _service.GetAll(request, true);

			Assert.NotNull(result);
		}

		[Fact]
		public async Task GetParentHierarchy_ShouldReturnParents()
		{
			//_repoMock.Setup(r => r.GetSingleAsync(It.IsAny<Func<Categories, bool>>()))
			//	.ReturnsAsync(new Categories { Id = 1, CategoryName = "Test", ParentId = null });

			var result = await _service.GetParentHierarchy(1);

			Assert.NotNull(result);
			Assert.IsType<List<ParentCategory>>(result);
		}

		[Fact]
		public async Task AddFavorite_ShouldReturnFavorite()
		{
			_repoMock.Setup(r => r.CheckFavorite(It.IsAny<int>())).ReturnsAsync((CategoriesFavorite)null);
			_repoMock.Setup(r => r.AddFavorite(It.IsAny<int>())).ReturnsAsync(new CategoriesFavorite { CategoryID = 1 });

			var result = await _service.AddFavorite(1);

			Assert.NotNull(result);
			Assert.Equal(1, result.CategoryID);
		}

		[Fact]
		public async Task UnFavorite_ShouldReturnInt()
		{
			_repoMock.Setup(r => r.RemoveFavorite(It.IsAny<int>())).ReturnsAsync(1);

			var result = await _service.UnFavorite(1);

			Assert.Equal(1, result);
		}

		[Fact]
		public async Task UpdateCategory_ShouldReturnCategory()
		{
			var request = new RequestCategory { Id = 1, CategoryName = "Test", CategoryDesc = "Desc" };
			//_repoMock.Setup(r => r.GetSingleAsync(It.IsAny<Func<Categories, bool>>())).ReturnsAsync((Categories)null);
			_repoMock.Setup(r => r.UpdateAsync(It.IsAny<Categories>())).ReturnsAsync(new Categories { Id = 1, CategoryName = "Test" });

			var result = await _service.Update(request);

			Assert.NotNull(result);
			Assert.Equal("Test", result.CategoryName);
		}

		[Fact]
		public async Task SoftDelete_ShouldReturnTrue()
		{
			_repoMock.Setup(r => r.MarkAsDeletedAsync(It.IsAny<int>())).ReturnsAsync(true);

			var result = await _service.SoftDelete(1);

			Assert.True(result);
		}

		[Fact]
		public async Task SoftUnDelete_ShouldReturnTrue()
		{
			_repoMock.Setup(r => r.MarkAsUnDeletedAsync(It.IsAny<int>())).ReturnsAsync(true);

			var result = await _service.SoftUnDelete(1);

			Assert.True(result);
		}

		[Fact]
		public async Task HardDelete_ShouldReturnInt()
		{
			//_repoMock.Setup(r => r.GetSingleAsync(It.IsAny<Func<Categories, bool>>())).ReturnsAsync(new Categories { Id = 1 });
			_repoMock.Setup(r => r.DeleteAsync(It.IsAny<int>())).ReturnsAsync(1);

			var result = await _service.HardDelete(1);

			Assert.Equal(1, result);
		}

		[Fact]
		public async Task AddWorkFlow_ShouldReturnInt()
		{
			var workflow = new RequestCategoryWorkflow
			{
				Approval = new RequestCategoryWorkflow.RequestCTApproval { CategoryID = 1, Notes = "Test" },
				Flows = new List<RequestCategoryWorkflow.RequestCTApprovalFlow>
			{
				new RequestCategoryWorkflow.RequestCTApprovalFlow { UserID = 1, Step = 1 }
			}
			};
			//_repoMock.Setup(r => r.GetSingleAsync(It.IsAny<Func<Categories, bool>>())).ReturnsAsync(new Categories { Id = 1 });
			_repoMock.Setup(r => r.AddWorkFlow(It.IsAny<Approvals>(), It.IsAny<List<ApprovalFlows>>())).ReturnsAsync(1);

			var result = await _service.AddWorkFlow(workflow);

			Assert.Equal(1, result);
		}

		[Fact]
		public async Task DeleteWorkFlow_ShouldReturnInt()
		{
			_repoMock.Setup(r => r.DeleteWorkFlow(It.IsAny<int>())).ReturnsAsync(1);

			var result = await _service.DeleteWorkFlow(1);

			Assert.Equal(1, result);
		}

		[Fact]
		public async Task GetNested_ShouldReturnCategories()
		{
			//_repoMock.Setup(r => r.GetNestedCategory()).ReturnsAsync(new List<Categories>());

			var result = await _service.GetNested();

			Assert.NotNull(result);
		}

		[Fact]
		public async Task GetNestedByCategoryId_ShouldReturnCategories()
		{
			_repoMock.Setup(r => r.GetNestedCategory(It.IsAny<int?>())).ReturnsAsync(new List<Categories>());

			var result = await _service.GetNestedByCategoryId(1);

			Assert.NotNull(result);
		}
	}
}
