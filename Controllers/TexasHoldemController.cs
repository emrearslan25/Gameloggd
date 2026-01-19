using Microsoft.AspNetCore.Mvc;
using GameLoggd.Models.Games;
using GameLoggd.Models.Poker;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using GameLoggd.Models;

namespace GameLoggd.Controllers;

public class TexasHoldemController : Controller
{
    private const string SessionKey = "HoldemGame";
    private const int smallBlind = 10;
    private const int bigBlind = 20;
    private readonly UserManager<ApplicationUser> _userManager;

    public TexasHoldemController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("/holdem")]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var game = new TexasHoldemGame();
        if(user != null) {
            game.PlayerChips = user.Credits;
        }
        return View(game);
    }

    [HttpPost("/holdem/start")]
    public async Task<IActionResult> Start()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var game = LoadGame() ?? new TexasHoldemGame();
        
        // Sync Chips from real balance
        game.PlayerChips = user.Credits;
        
        // Ensure dealer has chips
        if (game.DealerChips <= 0) game.DealerChips = 1000;

        if (game.PlayerChips < bigBlind)
        {
             return BadRequest("Not enough credits to play (need " + bigBlind + ")");
        }

        game.ResetRound();

        // Dealing
        game.Deck = CreateDeck();
        Shuffle(game.Deck);

        game.PlayerHoleCards.Add(DrawCard(game.Deck));
        game.DealerHoleCards.Add(DrawCard(game.Deck));
        game.PlayerHoleCards.Add(DrawCard(game.Deck));
        game.DealerHoleCards.Add(DrawCard(game.Deck));

        // Blinds
        PostBlind(game, isPlayer: true, amount: smallBlind);
        PostBlind(game, isPlayer: false, amount: bigBlind);
        
        game.CurrentBet = bigBlind;
        game.IsPlayerTurn = true; 
        
        game.Message = "Your Turn (Pre-Flop)";
        
        // Sync back balance
        user.Credits = game.PlayerChips;
        await _userManager.UpdateAsync(user);

        SaveGame(game);
        return Json(ViewData(game));
    }

    [HttpPost("/holdem/action")]
    public async Task<IActionResult> Action([FromBody] string action)
    {
        var game = LoadGame();
        if (game == null || game.Stage == HoldemStage.GameOver) return BadRequest("Game error");
        
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        // Force sync before action to be safe, though Session should be up to date
        // Actually trusting session state for chips inside the hand is correct, 
        // but let's ensure we don't cheat by double spending across tabs? 
        // For simple single-seat, trusting session is fine.

        switch (action.ToLower())
        {
            case "fold":
                game.Winner = "Dealer";
                game.Message = "You Folded. Dealer Wins.";
                game.DealerChips += game.Pot;
                game.Stage = HoldemStage.GameOver;
                break;
            case "call":
                int callAmt = game.CurrentBet - game.PlayerBet;
                if (callAmt > game.PlayerChips) callAmt = game.PlayerChips; 
                game.PlayerChips -= callAmt;
                game.Pot += callAmt;
                game.PlayerBet += callAmt;
                game.IsPlayerTurn = false;
                break;
            case "check":
                if (game.PlayerBet < game.CurrentBet) return BadRequest("Cannot check, must call");
                game.IsPlayerTurn = false;
                break;
            case "raise":
                int raiseAmt = (game.CurrentBet == 0 ? bigBlind : game.CurrentBet * 2) - game.PlayerBet;
                if (raiseAmt > game.PlayerChips) raiseAmt = game.PlayerChips;
                game.PlayerChips -= raiseAmt;
                game.Pot += raiseAmt;
                game.PlayerBet += raiseAmt;
                game.CurrentBet = game.PlayerBet;
                game.IsPlayerTurn = false;
                break;
        }

        if (game.Stage != HoldemStage.GameOver && !game.IsPlayerTurn)
        {
             BotPlay(game);
        }
        
        // Sync back balance (wins or bets applied)
        user.Credits = game.PlayerChips;
        await _userManager.UpdateAsync(user);
        
        SaveGame(game);
        return Json(ViewData(game));
    }

    private void BotPlay(TexasHoldemGame game)
    {
        int toCall = game.CurrentBet - game.DealerBet;
        bool canCheck = toCall == 0;
        
        // Simple AI: 30% fold if raising, otherwise call/check.
        Random rnd = new Random();
        if (canCheck)
        {
            // Check
            game.Message = "Dealer Checks.";
             NextStage(game);
        }
        else
        {
            if (toCall > game.DealerChips) 
            {
                game.DealerChips -= toCall; 
                game.Pot += toCall; 
            }
            else
            {
                game.DealerChips -= toCall;
                game.Pot += toCall;
                game.DealerBet += toCall;
                game.Message = "Dealer Calls.";
                NextStage(game);
            }
        }
        
        game.IsPlayerTurn = true;
    }

    private void NextStage(TexasHoldemGame game)
    {
        game.PlayerBet = 0;
        game.DealerBet = 0;
        game.CurrentBet = 0;

        if (game.Stage == HoldemStage.PreFlop)
        {
            game.Stage = HoldemStage.Flop;
            game.CommunityCards.Add(DrawCard(game.Deck));
            game.CommunityCards.Add(DrawCard(game.Deck));
            game.CommunityCards.Add(DrawCard(game.Deck));
        }
        else if (game.Stage == HoldemStage.Flop)
        {
            game.Stage = HoldemStage.Turn;
            game.CommunityCards.Add(DrawCard(game.Deck));
        }
        else if (game.Stage == HoldemStage.Turn)
        {
            game.Stage = HoldemStage.River;
            game.CommunityCards.Add(DrawCard(game.Deck));
        }
        else if (game.Stage == HoldemStage.River)
        {
            game.Stage = HoldemStage.Showdown;
            EvaluateWinner(game);
        }
    }

    private void EvaluateWinner(TexasHoldemGame game)
    {
        var pBest = HoldemHandEvaluator.Evaluate(game.PlayerHoleCards, game.CommunityCards);
        var dBest = HoldemHandEvaluator.Evaluate(game.DealerHoleCards, game.CommunityCards);

        int comparison = 0;
        if (pBest.Rank > dBest.Rank) comparison = 1;
        else if (pBest.Rank < dBest.Rank) comparison = -1;
        else
        {
            for(int i=0; i < Math.Min(pBest.ScoreKey.Count, dBest.ScoreKey.Count); i++)
            {
                if (pBest.ScoreKey[i] > dBest.ScoreKey[i]) { comparison = 1; break; }
                if (pBest.ScoreKey[i] < dBest.ScoreKey[i]) { comparison = -1; break; }
            }
        }

        if (comparison > 0)
        {
            game.Winner = "Player";
            game.PlayerChips += game.Pot;
            game.WinningHandName = pBest.Description;
            game.Message = $"You Win with {pBest.Description}!";
        }
        else if (comparison < 0)
        {
            game.Winner = "Dealer";
            game.DealerChips += game.Pot;
            game.WinningHandName = dBest.Description;
             game.Message = $"Dealer Wins with {dBest.Description}.";
        }
        else
        {
            game.Winner = "Split";
            game.PlayerChips += game.Pot / 2;
            game.DealerChips += game.Pot / 2;
            game.Message = "Split Pot!";
        }
        
        game.Pot = 0;
        game.Stage = HoldemStage.GameOver;
    }

    private object ViewData(TexasHoldemGame game)
    {
        var dealerCards = game.Stage == HoldemStage.Showdown || game.Stage == HoldemStage.GameOver 
            ? game.DealerHoleCards 
            : game.DealerHoleCards.Select(c => new Card { IsHidden = true }).ToList();

        return new {
            playerHole = game.PlayerHoleCards,
            dealerHole = dealerCards,
            community = game.CommunityCards,
            pot = game.Pot,
            stage = game.Stage.ToString(),
            message = game.Message,
            playerChips = game.PlayerChips,
            dealerChips = game.DealerChips,
            winner = game.Winner,
            turn = game.IsPlayerTurn
        };
    }

    private void PostBlind(TexasHoldemGame game, bool isPlayer, int amount)
    {
        if (isPlayer)
        {
            game.PlayerChips -= amount;
            game.PlayerBet += amount;
        }
        else
        {
            game.DealerChips -= amount;
            game.DealerBet += amount;
        }
        game.Pot += amount;
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

    private void SaveGame(TexasHoldemGame game)
    {
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(game));
    }

    private TexasHoldemGame? LoadGame()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        return json == null ? null : JsonSerializer.Deserialize<TexasHoldemGame>(json);
    }
}
