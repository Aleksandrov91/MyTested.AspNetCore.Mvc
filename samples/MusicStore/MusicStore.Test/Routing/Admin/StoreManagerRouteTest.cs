namespace MusicStore.Test.Routing.Admin
{
    using MusicStore.Areas.Admin.Controllers;
    using MyTested.AspNetCore.Mvc;
    using Xunit;

    public class StoreManagerRouteTest
    {
        [Fact]
        public void IndexShouldBeRoutedCorrectly()
        {
            //TODO: Set user with ManageStore Policy
            MyRouting
                .Configuration()
                .ShouldMap(request => request
                    .WithLocation("Admin/StoreManager")
                    .WithUser())
                .To<StoreManagerController>(c => c.Index());
        }
    }
}
