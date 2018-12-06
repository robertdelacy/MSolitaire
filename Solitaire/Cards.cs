using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solitaire
{
    public class Card
    {
        public string face;
        public string suit;

        public Card(string cardFace, string cardSuit)
        {
            suit = cardSuit;
            face = cardFace;
        }

        public override string ToString()
        {
            if (suit != "Joker")
            {
                return face + " of " + suit;
            }
            else
            {
                return "Joker";
            }
        }
    }

    public class Deck
    {
        public Card[] cards;

        public Deck()
        {
            var query =
            from suit in new[] { "hearts", "hearts", "hearts", "hearts", "hearts", "clubs", "clubs", "clubs", "clubs", "spades", "spades", "spades", "diamonds", "joker"}
            from rank in Enumerable.Range(1, 6)
            select new Card(rank.ToString(), suit);

            cards = query.ToArray();
        }
    }
}
