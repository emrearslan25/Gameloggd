namespace GameLoggd.Models.Admin;

public class AdminGame
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string Developer { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Web-relative image path (e.g. "/uploads/games/<id>.jpg").
    /// </summary>
    public string? ImagePath { get; set; }
}
