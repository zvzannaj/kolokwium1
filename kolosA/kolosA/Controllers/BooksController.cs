using System.Transactions;
using kolosA.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace kolosA
{

    [Route("api/books")]
    [ApiController]
    
    public class BooksController : Controller
    {
        private readonly IBooksRepository _booksRepository;
        public BooksController(IBooksRepository booksRepository)
        {
            _booksRepository = booksRepository;
        }
        
        [HttpGet("{id}/genres")]
        public async Task<IActionResult> GetBook(int id)
        {
            if (!await _booksRepository.DoesBookExist(id))
                return NotFound($"Book with given ID - {id} doesn't exist");

            var animal = await _booksRepository.GetBook(id);
            
            return Ok(animal);
        }
        
        // Version with implicit transaction
        [HttpPost]
        public async Task<IActionResult> AddAnimal(NewBookWithGenresDTO newBookWithGenres)
        {

            foreach (var genre in newBookWithGenres.Genres)
            {
                if (!await _booksRepository.DoesGenreExist(genre.GenrePk))
                    return NotFound($"Genre with given ID - {genre.GenrePk} doesn't exist");
            }

            await _booksRepository.AddNewBookWithGenres(newBookWithGenres);

            return Created(Request.Path.Value ?? "api/books", newBookWithGenres);
        }
        
    }
}