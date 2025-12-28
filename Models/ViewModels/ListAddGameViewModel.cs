using GameLoggd.Models.Domain;

namespace GameLoggd.Models.ViewModels;

public class ListAddGameViewModel
{
    public int ListId { get; set; }
    public string ListTitle { get; set; } = string.Empty;
    public string? Query { get; set; }
    public List<Game> Results { get; set; } = new();
}
