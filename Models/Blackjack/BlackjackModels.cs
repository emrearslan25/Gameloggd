using GameLoggd.Models.Games;

namespace GameLoggd.Models.Blackjack;

public class Hand
{
    public List<Card> Cards { get; set; } = new();

    public int Score
    {
        get
        {
            int score = Cards.Where(c => !c.IsHidden).Sum(c => 
            {
                if (c.Rank == Rank.Ace) return 11;
                if ((int)c.Rank >= 10) return 10;
                return (int)c.Rank;
            });

            int aces = Cards.Count(c => !c.IsHidden && c.Rank == Rank.Ace);

            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }
            return score;
        }
    }

    public void AddCard(Card card)
    {
        Cards.Add(card);
    }
}

public class BlackjackGame
{
    public Hand PlayerHand { get; set; } = new();
    public Hand DealerHand { get; set; } = new();
    public List<Card> Deck { get; set; } = new();
    public bool IsGameOver { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Winner { get; set; } = string.Empty; // "Player", "Dealer", "Push"
    public int BetAmount { get; set; } = 100;
}
