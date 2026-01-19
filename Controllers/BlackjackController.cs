using Microsoft.AspNetCore.Mvc;
using GameLoggd.Models.Blackjack;
using GameLoggd.Models.Games;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using GameLoggd.Models;

namespace GameLoggd.Controllers;

public class BlackjackController : Controller
{
    // In a real app, use distributed cache like Redis. For this demo, using Session.
    private const string SessionKey = "BlackjackGame";
    private readonly UserManager<ApplicationUser> _userManager;

    public BlackjackController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("/blackjack")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null) ViewBag.Credits = user.Credits;
        return View(new BlackjackGame()); 
    }

    [HttpPost("/blackjack/start")]
    public async Task<IActionResult> Start()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Fixed bet for now, or get from request (later)
        int betAmount = 100;
        if (user.Credits < betAmount)
        {
            return BadRequest($"Not enough credits. You need {betAmount} credits.");
        }

        // Deduct bet immediately
        user.Credits -= betAmount;
        await _userManager.UpdateAsync(user);

        var game = new BlackjackGame();
        game.BetAmount = betAmount;
        game.Deck = CreateDeck();
        Shuffle(game.Deck);

        // Initial Deal
        game.PlayerHand.AddCard(DrawCard(game.Deck));
        game.DealerHand.AddCard(DrawCard(game.Deck));
        game.PlayerHand.AddCard(DrawCard(game.Deck));
        
        var hiddenCard = DrawCard(game.Deck);
        hiddenCard.IsHidden = true;
        game.DealerHand.AddCard(hiddenCard);

        await CheckBlackjack(game, user);
        SaveGame(game);

        return Json(game);
    }

    [HttpPost("/blackjack/hit")]
    public async Task<IActionResult> Hit()
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
            // No payout
        }

        SaveGame(game);
        return Json(game);
    }

    [HttpPost("/blackjack/stand")]
    public async Task<IActionResult> Stand()
    {
        var game = LoadGame();
        if (game == null || game.IsGameOver) return BadRequest("Game not active");
        var user = await _userManager.GetUserAsync(User);
        if(user == null) return Unauthorized();

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
            // Payout 2x
            user.Credits += game.BetAmount * 2;
        }
        else if (playerScore > dealerScore)
        {
            game.Winner = "Player";
            game.Message = "You Win!";
            // Payout 2x
            user.Credits += game.BetAmount * 2;
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
            // Return bet
            user.Credits += game.BetAmount;
        }

        await _userManager.UpdateAsync(user);

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
        if (!deck.Any()) return new Card(); 
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

    private async Task CheckBlackjack(BlackjackGame game, ApplicationUser user)
    {
        if (game.PlayerHand.Score == 21)
        {
            game.IsGameOver = true;
            RevealDealerCard(game);
            if (game.DealerHand.Score == 21)
            {
                game.Winner = "Push";
                game.Message = "Push! Both have Blackjack.";
                user.Credits += game.BetAmount;
            }
            else
            {
                game.Winner = "Player";
                game.Message = "Blackjack! You Win!";
                user.Credits += (int)(game.BetAmount * 2.5); // 3:2 payout
            }
            await _userManager.UpdateAsync(user);
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
