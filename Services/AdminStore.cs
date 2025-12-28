using GameLoggd.Models.Admin;

namespace GameLoggd.Services;

public class AdminStore
{
    private readonly object _gate = new();
    private readonly List<AdminGame> _games = new();
    private readonly List<AdminUser> _users = new();

    public AdminStore()
    {
        // Seed demo data
        _games.AddRange(new[]
        {
            new AdminGame { Title = "Cyberpunk Legends: Neon Rising", Year = 2024, Developer = "Neon Studios", Description = "Neon-soaked open world with a narrative focus." },
            new AdminGame { Title = "Fantasy Quest", Year = 2023, Developer = "Mythworks", Description = "Classic RPG adventure with party building." },
            new AdminGame { Title = "Speed Racer X", Year = 2024, Developer = "Circuit Labs", Description = "Arcade racer with tight handling and synthwave vibes." },
        });

        _users.AddRange(new[]
        {
            new AdminUser { Username = "alexchen", Email = "alex@example.com", IsBanned = false },
            new AdminUser { Username = "sarahw", Email = "sarah@example.com", IsBanned = false },
            new AdminUser { Username = "marcusj", Email = "marcus@example.com", IsBanned = true },
        });
    }

    public (List<AdminGame> games, List<AdminUser> users) Snapshot()
    {
        lock (_gate)
        {
            return (
                _games.Select(g => new AdminGame { Id = g.Id, Title = g.Title, Year = g.Year, Developer = g.Developer, Description = g.Description, ImagePath = g.ImagePath }).ToList(),
                _users.Select(u => new AdminUser { Id = u.Id, Username = u.Username, Email = u.Email, IsBanned = u.IsBanned }).ToList()
            );
        }
    }

    public AdminGame AddGame(string title, int? year, string developer, string description, string? imagePath)
    {
        lock (_gate)
        {
            var game = new AdminGame
            {
                Title = title.Trim(),
                Year = year,
                Developer = developer.Trim(),
                Description = description.Trim(),
                ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath,
            };

            _games.Insert(0, game);
            return game;
        }
    }

    public AdminGame? GetGame(Guid id)
    {
        lock (_gate)
        {
            var g = _games.FirstOrDefault(x => x.Id == id);
            if (g is null) return null;
            return new AdminGame { Id = g.Id, Title = g.Title, Year = g.Year, Developer = g.Developer, Description = g.Description, ImagePath = g.ImagePath };
        }
    }

    public bool UpdateGame(Guid id, string title, int? year, string developer, string description, string? imagePathOrNullToKeep)
    {
        lock (_gate)
        {
            var g = _games.FirstOrDefault(x => x.Id == id);
            if (g is null) return false;

            g.Title = title.Trim();
            g.Year = year;
            g.Developer = developer.Trim();
            g.Description = description.Trim();
            if (imagePathOrNullToKeep is not null)
            {
                g.ImagePath = string.IsNullOrWhiteSpace(imagePathOrNullToKeep) ? null : imagePathOrNullToKeep;
            }

            return true;
        }
    }

    public bool DeleteGame(Guid id)
    {
        lock (_gate)
        {
            var idx = _games.FindIndex(g => g.Id == id);
            if (idx < 0) return false;
            _games.RemoveAt(idx);
            return true;
        }
    }

    public bool SetUserBanned(Guid id, bool isBanned)
    {
        lock (_gate)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user is null) return false;
            user.IsBanned = isBanned;
            return true;
        }
    }
}
