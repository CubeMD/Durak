using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

public class DurakAgent : Agent
{
    public readonly List<Card> hand = new List<Card>();

    [SerializeField] private Environment environment;

    private DurakAgent currentOpponent;
    private bool attacking;
    private bool finishingOff;
    private List<bool> mask = new List<bool>(37);

    private void Awake()
    {
        for (int i = 0; i < 37; i++)
        {
            mask.Add(false);
        }
    }

    public override void OnEpisodeBegin()
    {
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        List<float[]> cardObservations = new List<float[]>();
        
        for (int i = 0; i < 36; i++)
        {
            cardObservations.Add(new float[4]);
        }

        foreach (Card card in hand)
        {
            cardObservations[CardToObservationIndex(card)][0] = 1;
        }
        foreach (Card card in environment.discardedCards)
        {
            cardObservations[CardToObservationIndex(card)][1] = 1;
        }
        foreach (Card card in environment.inPlayCards)
        {
            cardObservations[CardToObservationIndex(card)][2] = 1;
        }

        if (environment.inPlayCards.Count > 0)
        {
            cardObservations[CardToObservationIndex(environment.inPlayCards[0])][3] = 1;
        }

        foreach (float[] observation in cardObservations)
        {
            sensor.AddObservation(observation);
        }
        sensor.AddObservation(ConvertIndexToOneHot((int)environment.trumpSuit, 4));
        sensor.AddObservation(finishingOff);
        sensor.AddObservation(attacking);
        sensor.AddObservation(environment.drawCards.Count / 36f);
        sensor.AddObservation(currentOpponent.hand.Count / 6f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        for (int i = 0; i < 37; i++)
        {
            mask[i] = false;
        }
        
        GetValidCardsFromHand(out List<Card> validCards);
        
        foreach (Card card in validCards)
        {
            mask[CardToObservationIndex(card)] = true;
        }
        
        mask[36] = environment.numTurnsThisAttack > 0;

        for (int i = 0; i < mask.Count; i++)
        {
            actionMask.SetActionEnabled(0, i, mask[i]);
        }
    }

    private void GetValidCardsFromHand(out List<Card> validCards)
    {
        validCards = new List<Card>();

        if (attacking)
        {
            if (environment.numTurnsThisAttack > 0)
            {
                foreach (Card card in hand)
                {
                    if (environment.inPlayCards.Any(x => x.cardValue == card.cardValue))
                    {
                        validCards.Add(card);
                    }
                }
            }
            else
            {
                validCards.AddRange(hand);
            }
        }
        else
        {
            foreach (Card card in hand)
            {
                if (card.CanDefeat(environment.inPlayCards[0], environment.trumpSuit))
                {
                    validCards.Add(card);
                }
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int actionIndex = actions.DiscreteActions[0];

        if (!mask[actionIndex])
        {
            List<int> validActionIndexes = new List<int>();

            for (int i = 0; i < mask.Count; i++)
            {
                if (mask[i])
                {
                    validActionIndexes.Add(i);
                }
            }

            actionIndex = validActionIndexes[Random.Range(0, validActionIndexes.Count)];
            Debug.Log("Attempted invalid action");
        }
        
        string action = "skip";
        
        if (actionIndex < 36)
        {
            int cardInHandIndex = ObservationIndexToHandCardIndex(actionIndex);
            action = hand[cardInHandIndex].cardValue + " " + hand[cardInHandIndex].cardSuit;
            environment.playedCard = hand[cardInHandIndex];
            hand.RemoveAt(cardInHandIndex);
        }
        
        string attackingString = attacking ? "attacking" : "defending";
        string handString = hand.Aggregate(String.Empty, (current, card) => current + (card.cardValue + " " + card.cardSuit + ", "));
        string tableCard = environment.inPlayCards.Count > 0
            ? environment.inPlayCards[0].cardValue + " " + environment.inPlayCards[0].cardSuit
            : "None";
        
        Debug.Log($"Trump: {environment.trumpSuit}, Agent {gameObject.GetInstanceID()}, {attackingString}, hand: {handString}, table card {tableCard}, action {action}");
    }

    public void Attack(DurakAgent opponent)
    {
        attacking = true;
        finishingOff = false;
        currentOpponent = opponent;
        RequestDecision();
    }

    public void Defend(DurakAgent opponent)
    {
        attacking = false;
        finishingOff = false;
        currentOpponent = opponent;
        RequestDecision();
    }

    public void FinishOff(DurakAgent opponent)
    {
        attacking = true;
        finishingOff = true;
        currentOpponent = opponent;
        RequestDecision();
    }

    private int CardToObservationIndex(Card card)
    {
        return (int)card.cardValue + (int)card.cardSuit * 9;
    }
    
    private int ObservationIndexToHandCardIndex(int cardIndex)
    {
        CardSuit cardSuit = (CardSuit)(cardIndex / 9);
        CardValue cardValue = (CardValue)(cardIndex % 9);

        for (int index = 0; index < hand.Count; index++)
        {
            if (hand[index].cardSuit == cardSuit && hand[index].cardValue == cardValue)
            {
                return index;
            }
        }

        return -1;
    }
    
    private float[] ConvertIndexToOneHot(int index, int indexCount)
    {
        float[] result = new float[indexCount];
        result[index] = 1;
        
        return result;
    }
}
