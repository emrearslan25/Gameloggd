namespace GameLoggd.Models.Domain;

public class Game
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Developer { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Web-relative image path, e.g. /uploads/games/<guid>.jpg
    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Slug { get; set; } = string.Empty;

    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<Platform> Platforms { get; set; } = new List<Platform>();
}
