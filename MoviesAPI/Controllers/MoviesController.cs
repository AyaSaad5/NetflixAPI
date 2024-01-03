using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.Data;
using MoviesAPI.DTOs;
using MoviesAPI.Services;

namespace MoviesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMoviesService _moviesService;
        private readonly IGenresService _genresService;
        private readonly IMapper _mapper;

        private new List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;

        public MoviesController(IMoviesService moviesService, IGenresService genresService,
                                 IMapper mapper)
        {
            _moviesService = moviesService;
            _genresService = genresService;
            _mapper = mapper;
        }
        [Authorize(Roles ="User")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _moviesService.GetAll();

            var mappedMovies = _mapper.Map<IEnumerable< Movie>, IEnumerable<MovieDetailsDTO>>(movies);
            return Ok(mappedMovies);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null) return NotFound("No id found");
            
            var mappedMovie = _mapper.Map<Movie,MovieDetailsDTO>(movie);
            return Ok(mappedMovie);
        }

        [HttpGet("{genreId}")]
        public async Task<IActionResult> GetByGenreId(byte genreId)
        {
            var movies = await _moviesService.GetAll(genreId);

            var mappedMovies = _mapper.Map<IEnumerable<Movie>, IEnumerable<MovieDTO>>(movies);
            return Ok(mappedMovies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDTO movieDTO)
        {
            if (movieDTO.Poster == null)
                return BadRequest("Poster is Required");
            if (!_allowedExtensions.Contains(Path.GetExtension(movieDTO.Poster.FileName).ToLower())) ;
               return BadRequest("only .jpg or .png");

            if (movieDTO.Poster.Length > _maxAllowedPosterSize)
               return BadRequest("allowed size is 1MB");

            var isValidGenre = await _genresService.isValidGenre(movieDTO.GenreId);
            if (!isValidGenre)
                return BadRequest("invalid Id");

             using var dataStream = new MemoryStream();
             await movieDTO.Poster.CopyToAsync(dataStream);

            var mappedMovie = _mapper.Map<MovieDTO,Movie>(movieDTO); 

            mappedMovie.Poster = dataStream.ToArray();

            await _moviesService.Add(mappedMovie);

            return Ok(mappedMovie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync([FromForm] MovieDTO movieDTO, int id)
        {
            var movie = await _moviesService.GetById(id);
            if (movie == null) return NotFound($"No movie found with ID: {id}");


            var isValidGenre = await _genresService.isValidGenre(movieDTO.GenreId);
            if (!isValidGenre)
                return BadRequest("invalid GenreId");

            if(movieDTO.Poster != null)
            {
                //if (!_allowedExtensions.Contains(Path.GetExtension(movieDTO.Poster.FileName).ToLower())) ;
                //return BadRequest("only .jpg or .png");

                //if (movieDTO.Poster.Length > _maxAllowedPosterSize)
                //    return BadRequest("allowed size is 1MB");

                var dataStream = new MemoryStream();
                await movieDTO.Poster.CopyToAsync(dataStream);

                movie.Poster = dataStream.ToArray();
            }
            movie.Title = movieDTO.Title;
            movie.Year = movieDTO.Year;
            movie.StoryLine = movieDTO.StoryLine;
            movie.Rate = movieDTO.Rate;
            movie.GenreId = movieDTO.GenreId;

             _moviesService.Update(movie);

            return Ok(movie);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesService.GetById(id);
            if (movie == null) return NotFound($"No movie found with ID: {id}");

           _moviesService.Delete(movie);

            return Ok();
        }
    }
}
