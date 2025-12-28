namespace GameLoggd.Models.Domain;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<Game> Games { get; set; } = new List<Game>();
}
