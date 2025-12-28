namespace GameLoggd.Models.Domain;

public class Platform
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? IconClass { get; set; } // e.g. "fa-brands fa-playstation"

    public ICollection<Game> Games { get; set; } = new List<Game>();
}
