namespace MusicStore.Test.Data
{
    using MusicStore.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class StoreManagerTestData
    {
        public static List<Album> GetAlbums(int count)
            => Enumerable
                .Range(1, count)
                .Select(i => new Album
                {
                    AlbumId = i,
                    Title = $"Album {i}",
                    AlbumArtUrl = $"http://test.com/{i}",
                    Price = i,
                    Created = new DateTime(2019, 1, 1),
                    Artist = new Artist
                    {
                        Name = $"User_{i}"
                    },
                    Genre = new Genre
                    {
                        Name = $"Genre{i}"
                    }
                })
                .ToList();

        public static List<Genre> GetGenres(int count)
            => Enumerable
                .Range(1, count)
                .Select(i => new Genre
                {
                    Name = $"Genre {i}",
                    Description = $"Description {i}",                    
                })
                .ToList();

        public static List<Artist> GetArtists(int count)
            => Enumerable
                .Range(1, count)
                .Select(i => new Artist
                {
                    Name = $"Artist {i}"
                })
                .ToList();
    }
}
