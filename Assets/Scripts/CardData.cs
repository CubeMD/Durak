using System;
using UnityEngine;

public enum CardSuit
{
    Heart,
    Club,
    Diamond,
    Spade
}

public enum CardValue
{
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}

[Serializable]
public class CardData
{
    public CardSuit cardSuit;
    public CardValue cardValue;
    public Sprite cardIcon;

    public CardData(CardSuit cardSuit, CardValue cardValue)
    {
        this.cardSuit = cardSuit;
        this.cardValue = cardValue;
    }

    public bool CanDefeat(CardData otherCardData, CardSuit trumpSuit)
    {
        if (cardSuit == otherCardData.cardSuit)
        {
            return cardValue > otherCardData.cardValue;
        }
        
        if (cardSuit == trumpSuit)
        {
            if (otherCardData.cardSuit == trumpSuit)
            {
                return cardValue > otherCardData.cardValue;
            }

            return true;
        }

        return false;
    }
}
