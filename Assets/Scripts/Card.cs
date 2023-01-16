using System.Collections;
using System.Collections.Generic;
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

public class Card
{
    public CardSuit cardSuit;
    public CardValue cardValue;

    public Card(CardSuit cardSuit, CardValue cardValue)
    {
        this.cardSuit = cardSuit;
        this.cardValue = cardValue;
    }

    public bool CanDefeat(Card otherCard, CardSuit trumpSuit)
    {
        if (cardSuit != trumpSuit && otherCard.cardSuit == trumpSuit)
        {
            return false;
        }
        
        if (cardSuit == trumpSuit && otherCard.cardSuit != trumpSuit)
        {
            return true;
        }
        
        if ((cardSuit != trumpSuit && otherCard.cardSuit != trumpSuit) ||
            (cardSuit == trumpSuit && otherCard.cardSuit == trumpSuit))
        {
            return cardValue > otherCard.cardValue;
        }

        Debug.Log("Wat");
        return false;
    }
}
