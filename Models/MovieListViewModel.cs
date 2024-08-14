﻿namespace MovieDatabase.Models
{
    public class MovieListViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public List<int> Actors { get; set; }

        public string Language { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string CoverImage { get; set; }
    }
}
