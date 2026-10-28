using ClientFlow.Application.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientFlow.Tests.Application;

[TestClass]
public class PagedResultTests
{
    [TestMethod]
    public void PagedResult_PaginationMath_ComputesTotalPagesAndNavigationCorrectly()
    {
        var items = new List<string> { "item1", "item2" };
        var paged = new PagedResult<string>(items, totalCount: 25, pageIndex: 2, pageSize: 10);

        Assert.AreEqual(3, paged.TotalPages);
        Assert.IsTrue(paged.HasPreviousPage);
        Assert.IsTrue(paged.HasNextPage);
        Assert.AreEqual(2, paged.Items.Count);
    }

    [TestMethod]
    public void PagedResult_BoundaryPage_EnforcesPreviousAndNextBounds()
    {
        var items = new List<string> { "item1" };
        var pagedFirst = new PagedResult<string>(items, totalCount: 5, pageIndex: 1, pageSize: 5);

        Assert.AreEqual(1, pagedFirst.TotalPages);
        Assert.IsFalse(pagedFirst.HasPreviousPage);
        Assert.IsFalse(pagedFirst.HasNextPage);
    }
}
