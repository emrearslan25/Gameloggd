using GameLoggd.Models.Games;

namespace GameLoggd.Models.Poker;

public enum HandRank
{
    HighCard,
    Pair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    RoyalFlush
}

public class PokerHand
{
    public List<Card> Cards { get; set; } = new();
    public HandRank Rank { get; set; }
    public string RankName => Rank.ToString(); // Or human friendly string
    public int Payout { get; set; }
}

public class PokerGame
{
    public List<Card> Deck { get; set; } = new();
    public List<Card> Hand { get; set; } = new(); // Current 5 cards
    public List<bool> HeldCards { get; set; } = new() { false, false, false, false, false }; // Indices 0-4
    public bool IsGameOver { get; set; }
    public bool CanDraw { get; set; } // True after deal, False after draw
    public string Message { get; set; } = string.Empty;
    public int Credits { get; set; } = 100; // Starting credits
    public int Bet { get; set; } = 1;
}

public static class HandEvaluator
{
    public static (HandRank Rank, int Payout) Evaluate(List<Card> cards)
    {
        if (cards.Count != 5) return (HandRank.HighCard, 0);

        var sorted = cards.OrderBy(c => c.Rank).ToList();
        bool flush = cards.All(c => c.Suit == cards[0].Suit);
        
        bool straight = true;
        for (int i = 0; i < 4; i++)
        {
            if (sorted[i + 1].Rank != sorted[i].Rank + 1)
            {
                // Special case: Ace low straight (A, 2, 3, 4, 5)
                // In our Rank enum Ace is 14.
                // If we have 2,3,4,5,14 -> 2,3,4,5,Ace
                if (i == 3 && sorted[3].Rank == Rank.Five && sorted[4].Rank == Rank.Ace)
                {
                     // This is valid 2-3-4-5-A straight
                }
                else
                {
                    straight = false;
                    break;
                }
            }
        }

        if (flush && straight)
        {
            if (sorted[0].Rank == Rank.Ten && sorted[4].Rank == Rank.Ace) return (HandRank.RoyalFlush, 800);
            return (HandRank.StraightFlush, 50);
        }

        var groups = sorted.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).ToList();

        if (groups[0].Count() == 4) return (HandRank.FourOfAKind, 25);
        if (groups[0].Count() == 3 && groups[1].Count() == 2) return (HandRank.FullHouse, 9);
        if (flush) return (HandRank.Flush, 6);
        if (straight) return (HandRank.Straight, 4);
        if (groups[0].Count() == 3) return (HandRank.ThreeOfAKind, 3);
        if (groups[0].Count() == 2 && groups[1].Count() == 2) return (HandRank.TwoPair, 2);
        
        // Jacks or Better
        if (groups[0].Count() == 2 && groups[0].Key >= Rank.Jack) return (HandRank.Pair, 1);

        return (HandRank.HighCard, 0);
    }
}
