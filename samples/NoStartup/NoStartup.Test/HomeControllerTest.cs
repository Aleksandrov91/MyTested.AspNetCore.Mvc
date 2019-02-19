﻿namespace NoStartup.Test
{
    using Controllers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MyTested.AspNetCore.Mvc;

    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void IndexShouldReturnOkWithCorrectModel()
            => MyController<HomeController>
                .Instance()
                .WithServices(services => services
                    .With(ServiceMock.GetInstance()))
                .Calling(c => c.Index())
                .ShouldReturn()
                .Ok()
                .WithModel(new[] { "Mock", "Test" });

        [TestMethod]
        public void RedirectToIndexShouldRedirectToIndex()
            => MyController<HomeController>
                .Instance()
                .WithServices(services => services
                    .With(ServiceMock.GetInstance()))
                .Calling(c => c.RedirectToIndex())
                .ShouldReturn()
                .Redirect()
                .To<HomeController>(c => c.Index());
    }
}
