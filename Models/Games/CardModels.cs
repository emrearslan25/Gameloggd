namespace GameLoggd.Models.Games;

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum Rank
{
    Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10,
    Jack = 11, Queen = 12, King = 13, Ace = 14
}

public class Card
{
    public Suit Suit { get; set; }
    public Rank Rank { get; set; }
    public bool IsHidden { get; set; }

    public string ImagePath => IsHidden ? "/images/cards/back.png" : $"/images/cards/{Suit.ToString().ToLower()}_{Rank.ToString().ToLower()}.png";
    
    // For simple display without images initially
    public string DisplayName
    {
        get
        {
            if (IsHidden) return "???";
            string suitIcon = Suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => ""
            };
            string rankStr = Rank switch
            {
                Rank.Jack => "J",
                Rank.Queen => "Q",
                Rank.King => "K",
                Rank.Ace => "A",
                _ => ((int)Rank).ToString()
            };
            return $"{rankStr}{suitIcon}";
        }
    }
}
