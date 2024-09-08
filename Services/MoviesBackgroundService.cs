using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Services
{
    public class MovieBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MovieBackgroundService> _logger;
        private readonly HttpClient _httpClient;
        private static readonly char[] Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private readonly string _movieApiUrl = "http://www.omdbapi.com/?apikey=7bdd2e4c"; // Düzgün API URL

        public MovieBackgroundService(IServiceProvider serviceProvider, ILogger<MovieBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var letter in Alphabet)
                {
                    await FetchAndStoreMovie(letter.ToString());
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // 5 Saniyə gözlə
                }
            }
        }

        private async Task FetchAndStoreMovie(string letter)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MovieContext>();

            var movie = await FetchMovieFromApi(letter);

            if (movie != null)
            {
                var existingMovie = await dbContext.Movies.FirstOrDefaultAsync(m => m.Title == movie.Title);

                if (existingMovie == null)
                {
                    dbContext.Movies.Add(movie);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation($"Movie '{movie.Title}' added to the database.");
                }
                else
                {
                    _logger.LogInformation($"Movie '{movie.Title}' already exists in the database.");
                }
            }
        }

        private async Task<Movie?> FetchMovieFromApi(string letter)
        {
            try
            {
                // OMDB API sorğusu üçün düzgün URL formatı
                var response = await _httpClient.GetStringAsync($"{_movieApiUrl}&s={letter}");
                var movieData = JsonConvert.DeserializeObject<MovieApiResult>(response);

                // Filmi yoxlayın və varsa, qaytarın
                if (movieData != null && movieData.Search.Any())
                {
                    return new Movie
                    {
                        Title = movieData.Search[0].Title,
                        Description = movieData.Search[0].Year, // Film ili daxil edildi
                        Letter = letter
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching movie for letter {letter}: {ex.Message}");
            }

            return null;
        }

        // OMDB API-dən gələn cavaba uyğun sinif
        private class MovieApiResult
        {
            public List<MovieResult> Search { get; set; }

            public class MovieResult
            {
                public string Title { get; set; }
                public string Year { get; set; } // Film ili
                public string imdbID { get; set; } // IMDB ID
                public string Type { get; set; } // Film növü (məsələn, movie, series)
                public string Poster { get; set; } // Poster URL
            }
        }
    }
}
