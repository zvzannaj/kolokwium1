namespace kolosA.Models.DTOs;

public interface IBooksRepository
{
    Task<bool> DoesBookExist(int id);
    Task<bool> DoesGenreExist(int id);
    Task<BookDTO> GetBook(int id);
    Task AddNewBookWithGenres(NewBookWithGenresDTO newBookWithGenres);
}