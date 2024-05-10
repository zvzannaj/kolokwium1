namespace kolosA.Models.DTOs;

public class BookDTO
{
    public int Pk { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<GenreDTO> Genres { get; set; } = null!;
}

public class GenreDTO
{
    public string Name { get; set; } = string.Empty;
}