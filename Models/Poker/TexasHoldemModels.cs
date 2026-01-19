using GameLoggd.Models.Games;

namespace GameLoggd.Models.Poker;

public enum HoldemStage
{
    PreFlop,
    Flop,
    Turn,
    River,
    Showdown,
    GameOver
}

public class TexasHoldemGame
{
    public List<Card> Deck { get; set; } = new();
    
    // Cards
    public List<Card> PlayerHoleCards { get; set; } = new();
    public List<Card> DealerHoleCards { get; set; } = new();
    public List<Card> CommunityCards { get; set; } = new();

    // Chips & Bets
    public int PlayerChips { get; set; } = 0;
    public int DealerChips { get; set; } = 1000;
    public int Pot { get; set; } = 0;
    public int CurrentBet { get; set; } = 0; // Amount to call for the current round leader
    public int PlayerBet { get; set; } = 0;  // Amount player put in THIS round
    public int DealerBet { get; set; } = 0;  // Amount dealer put in THIS round
    
    // State
    public HoldemStage Stage { get; set; }
    public bool IsPlayerTurn { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty; // "Player", "Dealer", "Split"
    public string WinningHandName { get; set; } = string.Empty;

    public void ResetRound()
    {
        Deck = new List<Card>();
        PlayerHoleCards = new List<Card>();
        DealerHoleCards = new List<Card>();
        CommunityCards = new List<Card>();
        Pot = 0;
        CurrentBet = 0;
        PlayerBet = 0;
        DealerBet = 0;
        Stage = HoldemStage.PreFlop;
        Message = "";
        Winner = "";
    }
}

public class BestHand
{
    public HandRank Rank { get; set; }
    public List<int> ScoreKey { get; set; } = new(); // For comparing same rank hands (Kickiers etc)
    public string Description { get; set; } = string.Empty;
}

public static class HoldemHandEvaluator
{
    public static BestHand Evaluate(List<Card> holeCards, List<Card> communityCards)
    {
        var allCards = holeCards.Concat(communityCards).ToList();
        // Since we need best 5 out of 7, we can iterate all 5-combinations
        // But that's 21 combinations. Fast enough.

        // Or we can be smart.
        // Let's optimize slightly:
        // 1. Check Flush (>= 5 cards of same suit). If exists, find highest 5. Check Straight flush.
        // 2. Check Straights.
        // 3. Check Groups (Quads, Full House, Trips, Pairs).
        
        // Actually, reusing the 5-card evaluator from Video Poker is easiest if we update it to return a score key.
        // simpler: generate all 21 combinations of 5 cards, evaluate each, pick best.
        
        var best = new BestHand { Rank = HandRank.HighCard, ScoreKey = new List<int> { 0 }, Description = "High Card" };
        
        var combinations = GetCombinations(allCards, 5);
        foreach (var combo in combinations)
        {
            var result = Evaluate5(combo);
            if (Compare(result, best) > 0)
            {
                best = result;
            }
        }
        return best;
    }

    private static int Compare(BestHand a, BestHand b)
    {
        if (a.Rank > b.Rank) return 1;
        if (a.Rank < b.Rank) return -1;
        
        // Tie break using ScoreKey (lexicographical comparison)
        for(int i=0; i < Math.Min(a.ScoreKey.Count, b.ScoreKey.Count); i++)
        {
            if (a.ScoreKey[i] > b.ScoreKey[i]) return 1;
            if (a.ScoreKey[i] < b.ScoreKey[i]) return -1;
        }
        return 0;
    }

    // Standard 5 card eval returning comparable score keys
    private static BestHand Evaluate5(List<Card> cards)
    {
        var sorted = cards.OrderByDescending(c => c.Rank).ToList(); // Ace is 14
        bool flush = cards.All(c => c.Suit == cards[0].Suit);
        
        bool straight = true;
        // Check standard straight
        for (int i = 0; i < 4; i++)
        {
            if (sorted[i].Rank != sorted[i + 1].Rank + 1)
            {
                // Check Ace Low Straight: 5-4-3-2-A (A=14)
                if (i == 3 && sorted[3].Rank == Rank.Two && sorted[0].Rank == Rank.Ace) 
                {
                    // Treat Ace as 1 for this check visual, but sorted list has it at top.
                    // Correct sequence in desc order for Wheel: A, 5, 4, 3, 2
                    // Wait, sorted[0] is A(14). sorted[1] should be 5.
                    // Let's re-check specifically for Wheel
                    straight = false;
                }
                else
                {
                    straight = false; 
                }
                break;
            }
        }
        
        // Special check for A-2-3-4-5 (Wheel)
        // In Descending it looks like: Ace(14), 5, 4, 3, 2
        bool wheel = false;
        if (!straight && sorted[0].Rank == Rank.Ace && sorted[1].Rank == Rank.Five && sorted[2].Rank == Rank.Four && sorted[3].Rank == Rank.Three && sorted[4].Rank == Rank.Two)
        {
            straight = true;
            wheel = true;
        }

        if (flush && straight)
        {
            if (!wheel && sorted[0].Rank == Rank.Ace) return new BestHand { Rank = HandRank.RoyalFlush, Description = "Royal Flush" };
            return new BestHand { Rank = HandRank.StraightFlush, ScoreKey = new List<int> { wheel ? 5 : (int)sorted[0].Rank }, Description = "Straight Flush" };
        }

        var groups = sorted.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();

        if (groups[0].Count() == 4) 
            return new BestHand { Rank = HandRank.FourOfAKind, ScoreKey = new List<int> { (int)groups[0].Key, (int)groups[1].Key }, Description = "Four of a Kind" };
            
        if (groups[0].Count() == 3 && groups[1].Count() == 2) 
            return new BestHand { Rank = HandRank.FullHouse, ScoreKey = new List<int> { (int)groups[0].Key, (int)groups[1].Key }, Description = "Full House" };

        if (flush)
            return new BestHand { Rank = HandRank.Flush, ScoreKey = sorted.Select(c => (int)c.Rank).ToList(), Description = "Flush" };

        if (straight)
            return new BestHand { Rank = HandRank.Straight, ScoreKey = new List<int> { wheel ? 5 : (int)sorted[0].Rank }, Description = "Straight" };

        if (groups[0].Count() == 3)
            return new BestHand { Rank = HandRank.ThreeOfAKind, ScoreKey = new List<int> { (int)groups[0].Key, (int)groups[1].Key, (int)groups[2].Key }, Description = "Three of a Kind" };

        if (groups[0].Count() == 2 && groups[1].Count() == 2)
            return new BestHand { Rank = HandRank.TwoPair, ScoreKey = new List<int> { (int)groups[0].Key, (int)groups[1].Key, (int)groups[2].Key }, Description = "Two Pair" };

        if (groups[0].Count() == 2)
            return new BestHand { Rank = HandRank.Pair, ScoreKey = new List<int> { (int)groups[0].Key, (int)groups[1].Key, (int)groups[2].Key, (int)groups[3].Key }, Description = "Pair" };

        return new BestHand { Rank = HandRank.HighCard, ScoreKey = sorted.Select(c => (int)c.Rank).ToList(), Description = "High Card" };
    }

    private static IEnumerable<List<Card>> GetCombinations(List<Card> list, int length)
    {
        if (length == 1) return list.Select(t => new List<Card> { t });
        return GetCombinations(list, length - 1)
            .SelectMany(t => list.Where(e => t.All(c => c != e) && list.IndexOf(e) > list.IndexOf(t.Last())), 
                        (t1, t2) => t1.Concat(new List<Card> { t2 }).ToList());
    }
}
