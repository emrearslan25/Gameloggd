using Microsoft.AspNetCore.Mvc;
using GameLoggd.Models.Blackjack;
using GameLoggd.Models.Games;
using System.Text.Json;

namespace GameLoggd.Controllers;

public class BlackjackController : Controller
{
    // In a real app, use distributed cache like Redis. For this demo, using Session.
    private const string SessionKey = "BlackjackGame";

    [HttpGet("/blackjack")]
    public IActionResult Index()
    {
        return View(new BlackjackGame()); // Start with empty/new game view structure
    }

    [HttpPost("/blackjack/start")]
    public IActionResult Start()
    {
        var game = new BlackjackGame();
        game.Deck = CreateDeck();
        Shuffle(game.Deck);

        // Initial Deal
        game.PlayerHand.AddCard(DrawCard(game.Deck));
        game.DealerHand.AddCard(DrawCard(game.Deck));
        game.PlayerHand.AddCard(DrawCard(game.Deck));
        
        var hiddenCard = DrawCard(game.Deck);
        hiddenCard.IsHidden = true;
        game.DealerHand.AddCard(hiddenCard);

        CheckBlackjack(game);
        SaveGame(game);

        return Json(game);
    }

    [HttpPost("/blackjack/hit")]
    public IActionResult Hit()
    {
        var game = LoadGame();
        if (game == null || game.IsGameOver) return BadRequest("Game not active");

        game.PlayerHand.AddCard(DrawCard(game.Deck));

        if (game.PlayerHand.Score > 21)
        {
            game.IsGameOver = true;
            game.Winner = "Dealer";
            game.Message = "Bust! You went over 21.";
            RevealDealerCard(game);
        }

        SaveGame(game);
        return Json(game);
    }

    [HttpPost("/blackjack/stand")]
    public IActionResult Stand()
    {
        var game = LoadGame();
        if (game == null || game.IsGameOver) return BadRequest("Game not active");

        RevealDealerCard(game);

        // Dealer plays
        while (game.DealerHand.Score < 17)
        {
            game.DealerHand.AddCard(DrawCard(game.Deck));
        }

        // Determine Winner
        int playerScore = game.PlayerHand.Score;
        int dealerScore = game.DealerHand.Score;

        if (dealerScore > 21)
        {
            game.Winner = "Player";
            game.Message = "Dealer Busts! You Win!";
        }
        else if (playerScore > dealerScore)
        {
            game.Winner = "Player";
            game.Message = "You Win!";
        }
        else if (dealerScore > playerScore)
        {
            game.Winner = "Dealer";
            game.Message = "Dealer Wins!";
        }
        else
        {
            game.Winner = "Push";
            game.Message = "Push (Tie)!";
        }

        game.IsGameOver = true;
        SaveGame(game);
        return Json(game);
    }

    private List<Card> CreateDeck()
    {
        var deck = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                deck.Add(new Card { Suit = suit, Rank = rank });
            }
        }
        return deck;
    }

    private void Shuffle(List<Card> deck)
    {
        var rng = new Random();
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (deck[k], deck[n]) = (deck[n], deck[k]);
        }
    }

    private Card DrawCard(List<Card> deck)
    {
        if (!deck.Any()) return new Card(); // Should handle shuffle if empty ideally
        var card = deck[0];
        deck.RemoveAt(0);
        return card;
    }

    private void RevealDealerCard(BlackjackGame game)
    {
        if (game.DealerHand.Cards.Count > 1)
        {
            game.DealerHand.Cards[1].IsHidden = false;
        }
    }

    private void CheckBlackjack(BlackjackGame game)
    {
        if (game.PlayerHand.Score == 21)
        {
            game.IsGameOver = true;
            RevealDealerCard(game);
            if (game.DealerHand.Score == 21)
            {
                game.Winner = "Push";
                game.Message = "Push! Both have Blackjack.";
            }
            else
            {
                game.Winner = "Player";
                game.Message = "Blackjack! You Win!";
            }
        }
    }

    private void SaveGame(BlackjackGame game)
    {
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(game));
    }

    private BlackjackGame? LoadGame()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        return json == null ? null : JsonSerializer.Deserialize<BlackjackGame>(json);
    }
}
