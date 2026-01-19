using Microsoft.AspNetCore.Mvc;
using GameLoggd.Models.Games;
using GameLoggd.Models.Poker;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using GameLoggd.Models;

namespace GameLoggd.Controllers;

public class PokerController : Controller
{
    private const string SessionKey = "PokerGame";
    private readonly UserManager<ApplicationUser> _userManager;

    public PokerController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("/poker")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user != null) 
        {
             // Pass credits to view for initial load
             var game = new PokerGame();
             game.Credits = user.Credits; 
             return View(game);
        }
        return View(new PokerGame { Credits = 0 });
    }

    [HttpPost("/poker/start")]
    public async Task<IActionResult> Start([FromBody] int bet)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (bet < 1) bet = 1;
        if (bet > 5) bet = 5; 

        if (user.Credits < bet)
        {
             return BadRequest("Not enough credits");
        }

        user.Credits -= bet;
        await _userManager.UpdateAsync(user);

        var game = new PokerGame();
        game.Credits = user.Credits; // Sync for response
        game.Bet = bet;
        game.Deck = CreateDeck();
        Shuffle(game.Deck);

        for (int i = 0; i < 5; i++)
        {
            game.Hand.Add(DrawCard(game.Deck));
        }

        game.CanDraw = true;
        SaveGame(game);
        return Json(game);
    }

    [HttpPost("/poker/draw")]
    public async Task<IActionResult> Draw([FromBody] List<bool> heldIndices)
    {
        var game = LoadGame();
        if (game == null || !game.CanDraw) return BadRequest("Game not active or draw already made");
        
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        if (heldIndices == null || heldIndices.Count != 5) 
            heldIndices = new List<bool> { false, false, false, false, false };

        game.HeldCards = heldIndices;

        // Replace unheld cards
        for (int i = 0; i < 5; i++)
        {
            if (!heldIndices[i])
            {
                game.Hand[i] = DrawCard(game.Deck);
            }
        }

        var result = HandEvaluator.Evaluate(game.Hand);
        int payout = result.Payout * game.Bet;
        
        if (payout > 0)
        {
            user.Credits += payout;
            await _userManager.UpdateAsync(user);
        }
        
        game.Credits = user.Credits; // Sync for response
        game.Message = payout > 0 ? $"{result.Rank}! You won {payout} credits." : "Game Over";
        
        game.CanDraw = false;
        game.IsGameOver = true;
        SaveGame(game);

        return Json(new { 
            hand = game.Hand, 
            credits = game.Credits, 
            message = game.Message, 
            rank = result.Rank.ToString(),
            payout = payout
        });
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

    private void SaveGame(PokerGame game)
    {
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(game));
    }

    private PokerGame? LoadGame()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        return json == null ? null : JsonSerializer.Deserialize<PokerGame>(json);
    }
}
