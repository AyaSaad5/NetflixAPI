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
    public class GenresController : ControllerBase
    {
        private readonly IGenresService _genreService;

        public GenresController(IGenresService genreService)
        {
           _genreService = genreService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var genres = await _genreService.GetAll();
            return Ok(genres);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateGenreDTO createGenreDto)
        {
            var genre = new Genre
            {
                Name = createGenreDto.Name,
            };
            await _genreService.Add(genre);
            
            return Ok(genre);
        }

        [HttpPut("{id}")] //update
        //api/genres/1
        public async Task<IActionResult> UpdateAsync(byte id, [FromBody] CreateGenreDTO createGenreDto)
        {
            //اول حاجه هتاكد ان ال id ال اليوزر بعتهولى فاليد لا
            var genre = await _genreService.GetById(id);
            if (genre == null) return NotFound($"No genre found with this id : {id}");

            genre.Name = createGenreDto.Name;
            _genreService.Update(genre);

            return Ok(genre);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(byte id)
        {
            var genre = await _genreService.GetById(id);
            if (genre == null) return NotFound($"No genre found with this id : {id}");

            _genreService.Delete(genre);
       

            return Ok(genre);
        }
    }
}
