using GameLoggd.Models.Domain;

namespace GameLoggd.Models.ViewModels;

public class GamesIndexViewModel
{
    public List<Game> Games { get; set; } = new List<Game>();
    public List<Genre> AvailableGenres { get; set; } = new List<Genre>();
    public List<Platform> AvailablePlatforms { get; set; } = new List<Platform>();

    // Current filters
    public string? SearchQuery { get; set; }
    public string? SelectedGenre { get; set; }
    public string? SelectedPlatform { get; set; }
    public string? SelectedSort { get; set; }
}
