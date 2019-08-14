namespace MusicStore.Test.Controllers.Admin
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Caching.Memory;
    using MusicStore.Areas.Admin.Controllers;
    using MusicStore.Models;
    using MyTested.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Xunit;

    public class StoreManagerControllerTests
    {
        private const string ManagerStorePolicy = "ManageStore";

        [Fact]
        public void ControllerShouldBeInAdminArea()
        {
            const string AdminArea = "Admin";

            MyMvc
                .Controller<StoreManagerController>()
                .ShouldHave()
                .Attributes(attrs => attrs
                    .SpecifyingArea(AdminArea)
                    .PassingFor<AuthorizeAttribute>(authorize => authorize.Policy == ManagerStorePolicy)
                    .RestrictingForAuthorizedRequests());
        }

        [Fact]
        public void GetIndexShouldReturnViewWithAlbums()
        {
            //TODO: View should have data
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Index())
                .ShouldReturn()
                .View(view => view
                    .WithModelOfType<Album>());
        }

        [Fact]
        public void GetDetailsShouldReturnNotFoundWithInvalidAlbum()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Details(
                    From.Services<IMemoryCache>(),
                    With.No<int>()))
                .ShouldReturn()
                .NotFound();
        }

        [Fact]
        public void GetDetailsShouldReturnView()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Details(
                    From.Services<IMemoryCache>(),
                    1))
                .ShouldReturn()
                .NotFound();
        }

        [Fact]
        public void GetCreateContainsArtistsAndGenresInViewBagAndReturnView()
        {
            //TODO: Check ViewBag data
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Create())
                .ShouldHave()
                .ViewBag(viewBag => viewBag
                    .ContainingEntry("Genres", new List<Genre>())
                    .ContainingEntry("Artists", new List<Album>()))
                .AndAlso()
                .ShouldReturn()
                .View();
        }

        [Fact]
        public void PostCreateShouldContainsCorrectActionAttributes()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Create(
                    With.Default<Album>(),
                    From.Services<IMemoryCache>(),
                    CancellationToken.None))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethod(HttpMethod.Post)
                    .ValidatingAntiForgeryToken());
        }

        [Fact]
        public void PostCreateWithInvalidModelShouldReturnView()
        {
            //TODO: Check ViewBag data
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Create(
                    With.No<Album>(),
                    From.Services<IMemoryCache>(),
                    CancellationToken.None))
                .ShouldHave()
                .ModelState(modelState => modelState
                    .For<Album>()
                    .ContainingErrorFor(m => m.Title)
                    .ContainingErrorFor(m => m.Price)
                    .ContainingErrorFor(m => m.Created))
                .AndAlso()
                .ShouldReturn()
                .View();
        }

        [Fact]
        public void PostCreateWithValidModelShouldReturnRedirectToAction()
        {
            var album = new Album()
            {
                Title = "Test Album",
                AlbumArtUrl = "mytest.com/testalbum",
                Created = DateTime.UtcNow
            };

            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Create(
                    album,
                    From.Services<IMemoryCache>(),
                    CancellationToken.None))
                .ShouldHave()
                .ValidModelState()
                .AndAlso()
                .ShouldReturn()
                .RedirectToAction("Index");
        }

        [Fact]
        public void GetEditWithInvalidAlbumIdShouldReturnNotFound()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Edit(With.No<int>()))
                .ShouldReturn()
                .NotFound();
        }

        [Fact]
        public void GetEditWithValidAlbumIdShouldReturnView()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void PostEditShouldContainsCorrectActionAttributes()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Edit(
                    With.No<IMemoryCache>(),
                    With.No<Album>(),
                    CancellationToken.None))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethod(HttpMethod.Post)
                    .ValidatingAntiForgeryToken());
        }

        [Fact]
        public void PostEditWithInvalidModelShouldReturnView()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void PostEditWithValidModelShouldReturnRedirectToAction()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void GetRemoveAlbumWithInvalidIdShouldReturnNotFound()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.RemoveAlbum(With.No<int>()))
                .ShouldReturn()
                .NotFound();
        }

        [Fact]
        public void GetRemoveAlbumWithValidAlbumIdShouldReturnView()
        {
            var album = new Album();

            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.RemoveAlbum(album.AlbumId))
                .ShouldReturn()
                .View(album);
        }

        [Fact]
        public void PostRemoveAlbumShouldHaveCorrectActionAttributes()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.RemoveAlbumConfirmed(
                    With.No<IMemoryCache>(),
                    With.No<int>(),
                    CancellationToken.None))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethod(HttpMethod.Post));
        }

        [Fact]
        public void PostRemoveAlbumWithInvalidAlbumIdShouldReturnNotFound()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void PostRemoveAlbumWithValidAlbumIdShouldRedirectToAction()
        {
            throw new NotImplementedException();
        }
    }
}
