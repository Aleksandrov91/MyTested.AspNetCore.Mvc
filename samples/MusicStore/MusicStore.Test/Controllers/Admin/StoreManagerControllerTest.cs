namespace MusicStore.Test.Controllers.Admin
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using MusicStore.Areas.Admin.Controllers;
    using MusicStore.Models;
    using MusicStore.Test.Data;
    using MyTested.AspNetCore.Mvc;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Xunit;

    public class StoreManagerControllerTests
    {
        private const string ManagerStorePolicy = "ManageStore";
        private const string AdminArea = "Admin";

        [Fact]
        public void ControllerShouldBeInAdminArea()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .ShouldHave()
                .Attributes(attrs => attrs
                    .SpecifyingArea(AdminArea)
                    .PassingFor<AuthorizeAttribute>(authorize => authorize.Policy == ManagerStorePolicy));
        }

        //TODO: View should have data
        [Fact]
        public void GetIndexShouldReturnViewWithAlbums()
        {            
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Index())
                .ShouldReturn()
                .View(view => view
                    .WithModelOfType<List<Album>>());
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
                .WithData(StoreManagerTestData.GetAlbums(1))
                .Calling(c => c.Details(
                    From.Services<IMemoryCache>(),
                    1))
                .ShouldReturn()
                .View(view => view
                    .WithModelOfType<Album>()
                    .Passing(album => album.AlbumId == 1));
        }

        //TODO: Check ViewBag data
        [Fact]
        public void GetCreateContainsArtistsAndGenresInViewBagAndReturnView()
        {
            //var genres = AlbumTestData.GetGenres(3);
            //var artists = AlbumTestData.GetArtists(3);

            MyMvc
                .Controller<StoreManagerController>()
                //.WithData(data => data
                //    .WithEntities(entities => entities
                //        .AddRange(genres, artists)))
                .Calling(c => c.Create())
                //.ShouldHave()
                //.ViewBag(viewBag => viewBag
                //    .ContainingEntry("Genres", genres.Count)
                //    .ContainingEntry("Artists", artists.Count))
                //.AndAlso()
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

        //TODO: Check ViewBag data
        [Fact]
        public void PostCreateWithInvalidModelShouldReturnView()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Create(
                    With.Default<Album>(),
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
                .View(view => view
                    .WithModelOfType<Album>());
        }

        [Fact]
        public void PostCreateWithValidModelShouldReturnRedirectToAction()
        {
            var album = StoreManagerTestData.GetAlbums(1).First();

            MyMvc
                .Controller<StoreManagerController>()
                .WithData(album)
                .Calling(c => c.Create(
                    album,
                    From.Services<IMemoryCache>(),
                    CancellationToken.None))
                .ShouldHave()
                .ValidModelState()
                .AndAlso()
                .ShouldHave()
                .Data(data => data
                    .WithSet<Album>(set =>
                    {
                        set.ShouldNotBeEmpty();
                        set.SingleOrDefault(a => a.Title == album.Title).ShouldNotBeNull();
                    }))
                .AndAlso()
                .ShouldReturn()
                .Redirect(redirect => redirect
                    .To<StoreManagerController>(c => c.Index()));
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
            MyMvc
                .Controller<StoreManagerController>()
                .WithData(StoreManagerTestData.GetAlbums(1))
                .Calling(c => c.Edit(1))
                .ShouldReturn()
                .View(view => view
                    .WithModelOfType<Album>()
                    .Passing(album => album.AlbumId == 1));
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
                    .ValidatingAntiForgeryToken()
                    .RestrictingForAuthorizedRequests());
        }

        [Fact]
        public void PostEditWithInvalidModelShouldReturnView()
        {
            //TODO: Investigate ModelState is aways valid.
            var album = new Album
            {
                Title = "Title",
                AlbumArtUrl = "http://testalbum.com",
                Price = -10.0m
            };

            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.Edit(
                    From.Services<IMemoryCache>(),
                    album,
                    CancellationToken.None))
                .ShouldHave()
                .ModelState(modelState => modelState
                    .For<Album>()
                    .ContainingErrorFor(m => m.Title))
                .AndAlso()
                .ShouldReturn()
                .View(album);
        }

        [Fact]
        public void PostEditWithValidModelShouldReturnRedirectToAction()
        {
            var album = StoreManagerTestData.GetAlbums(1).First();

            MyMvc
                .Controller<StoreManagerController>()
                .WithData(album)
                .Calling(c => c.Edit(
                    From.Services<IMemoryCache>(),
                    album,
                    CancellationToken.None))
                .ShouldHave()
                .ValidModelState()
                .AndAlso()
                .ShouldHave()
                .Data(data => data
                    .WithSet<Album>(set =>
                    {
                        set.ShouldNotBeEmpty();
                        set.SingleOrDefault(a => a.Title == album.Title).ShouldNotBeNull();
                    }))
                .AndAlso()
                .ShouldReturn()
                .Redirect(redirect => redirect
                    .To<StoreManagerController>(c => c.Index()));
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
            var album = StoreManagerTestData.GetAlbums(1).First();

            MyMvc
                .Controller<StoreManagerController>()
                .WithData(album)
                .Calling(c => c.RemoveAlbum(album.AlbumId))
                .ShouldReturn()
                .View(view => view
                    .WithModelOfType<Album>()
                    .Passing(result =>
                    {
                        result.Title.ShouldNotBeNullOrEmpty();
                        result.Price.ShouldNotBeNull();
                    }));
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
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.RemoveAlbumConfirmed(
                    With.No<IMemoryCache>(),
                    With.No<int>(),
                    CancellationToken.None))
                .ShouldReturn()
                .NotFound();
        }

        [Fact]
        public void PostRemoveAlbumWithValidAlbumIdShouldRedirectToAction()
        {
            var album = StoreManagerTestData.GetAlbums(1).First();

            MyMvc
                .Controller<StoreManagerController>()
                .WithData(album)
                .Calling(c => c.RemoveAlbumConfirmed(
                    From.Services<IMemoryCache>(),
                    album.AlbumId,
                    CancellationToken.None))
                .ShouldHave()
                .Data(data => data
                    .WithSet<Album>(set => set.ShouldBeEmpty()))
                .AndAlso()
                .ShouldReturn()
                .Redirect(redirect => redirect
                    .To<StoreManagerController>(c => c.Index()));
        }

        [Fact]
        public void GetAlbumIdFromNameShouldHaveCorrectActionAttributes()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.GetAlbumIdFromName(
                    With.No<string>()))
                .ShouldHave()
                .ActionAttributes(attrs => attrs
                    .RestrictingForHttpMethod(HttpMethod.Get)
                    .SkippingStatusCodePages());
        }

        [Fact]
        public void GetAlbumIdFromNameWithInvalidAlbumNameShouldReturnNotFound()
        {
            MyMvc
                .Controller<StoreManagerController>()
                .Calling(c => c.GetAlbumIdFromName(
                    With.No<string>()))
                .ShouldReturn()
                .NotFound();
        }

        [Fact]
        public void GetAlbumIdFromNameWithValidAlbumNameShouldReturnAlbumId()
        {
            var album = StoreManagerTestData.GetAlbums(1).First();

            MyMvc
                .Controller<StoreManagerController>()
                .WithData(album)
                .Calling(c => c.GetAlbumIdFromName(
                    album.Title))
                .ShouldReturn()
                .Content(album.AlbumId.ToString());
        }
    }
}
