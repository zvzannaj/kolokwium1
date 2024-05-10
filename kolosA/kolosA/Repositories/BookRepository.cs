using Microsoft.Data.SqlClient;

namespace kolosA.Models.DTOs;

public class BookRepository : IBooksRepository
{
    private readonly IConfiguration _configuration;
    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 FROM Books WHERE id = @PK";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@PK", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<bool> DoesGenreExist(int id)
    {
        var query = "SELECT 1 FROM [Genres] WHERE id = @PK";

        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@PK", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();

        return res is not null;
    }

    public async Task<BookDTO> GetBook(int id)
    {
        var query = @"SELECT 
							Book.PK AS BookPK,
							Book.Title AS BookTitle,
							[Genres].Name AS GenresName,
						FROM Books
						JOIN Books_genres ON Books_genres.FK_book = Books.PK
						JOIN [Genres] ON [Genres].PK = Books_genres.FK_genre
						WHERE Books.PK = @PK";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();

	    command.Connection = connection;
	    command.CommandText = query;
	    command.Parameters.AddWithValue("@PK", id);
	    
	    await connection.OpenAsync();

	    var reader = await command.ExecuteReaderAsync();

	    var booksIdOrdinal = reader.GetOrdinal("BooksPK");
	    var booksTitleOrdinal = reader.GetOrdinal("BooksTitle");
	    var genresNameOrdinal = reader.GetOrdinal("GenresName");

	    BookDTO bookDto = null;

	    while (await reader.ReadAsync())
	    {
		    if (bookDto is not null)
		    {
			    bookDto.Genres.Add(new GenreDTO()
			    {
				    Name = reader.GetString(genresNameOrdinal),
			    });
		    }
		    else
		    {
			    bookDto = new BookDTO()
			    {
				    Pk = reader.GetInt32(booksIdOrdinal),
				    Title = reader.GetString(booksTitleOrdinal),
				    Genres = new List<GenreDTO>()
				    {
					    new GenreDTO()
					    {
						    Name = reader.GetString(genresNameOrdinal)
					    }
				    }
			    };
		    }
	    }

	    if (bookDto is null) throw new Exception();
        
        return bookDto;
    }

    public async Task AddNewBookWithGenres(NewBookWithGenresDTO newBookWithGenres)
    {
	    var insert = @"INSERT INTO Books VALUES(@Title);
					   SELECT @@IDENTITY AS ID;";
	    
	    await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
	    await using SqlCommand command = new SqlCommand();
	    
	    command.Connection = connection;
	    command.CommandText = insert;
	    
	    command.Parameters.AddWithValue("@Name", newBookWithGenres.Title);
	    
	    await connection.OpenAsync();

	    var transaction = await connection.BeginTransactionAsync();
	    command.Transaction = transaction as SqlTransaction;
	    
	    try
	    {
		    var id = await command.ExecuteScalarAsync();
    
		    foreach (var genre in newBookWithGenres.Genres)
		    {
			    command.Parameters.Clear();
			    command.CommandText = "INSERT INTO Books_genres VALUES(@BookPk, @GenrePk)";
			    command.Parameters.AddWithValue("@GenrePk", genre.GenrePk);
			    command.Parameters.AddWithValue("@BookPk", id);

			    await command.ExecuteNonQueryAsync();
		    }

		    await transaction.CommitAsync();
	    }
	    catch (Exception)
	    {
		    await transaction.RollbackAsync();
		    throw;
	    }    
    }
}