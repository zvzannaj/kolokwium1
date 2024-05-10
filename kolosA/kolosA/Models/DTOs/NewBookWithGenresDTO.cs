namespace kolosA.Models.DTOs;

public class NewBookWithGenresDTO
{
    public string Title { get; set; } = string.Empty;
    public IEnumerable<Genre> Genres { get; set; } = new List<Genre>();
}
public class Genre
{
    public int GenrePk { get; set; }
}