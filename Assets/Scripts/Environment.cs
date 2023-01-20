using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    public event Action<Environment> OnEnvironmentRequestedDecision;
    public event Action<Environment> OnEnvironmentGameCompleted;

    [SerializeField] 
    private Deck deck;
    
    public List<CardData> drawCards = new List<CardData>();
    public readonly List<CardData> inPlayCards = new List<CardData>();
    public readonly List<CardData> discardedCards = new List<CardData>();
    public List<DurakAgent> players;
    public CardData playedCardData;
    public CardData trumpCard;
    public int numAttacks;
    public int numTurnsThisAttack;
    public int turnsAmount;
    
    [SerializeField] private int defaultCardAmount = 6;
    [SerializeField] private int maxTurnsPerAttack = 100;
    [SerializeField] private int maxAttacksPerGame = 100;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private bool human;
    [SerializeField] public UIController UIController;

    private Coroutine gameRoutine;
    private Coroutine attackRoutine;
    private bool attackerWon;

    private void Awake()
    {
        FillDrawWithAllCards();
    }

    public IEnumerator PlayAGame()
    {
        ShuffleDraw();
        DealCards(players);
        DurakAgent winner = null;
        
        trumpCard = drawCards.Last();

        if (UIController != null)
        {
            UIController.SetTrumpCard(trumpCard);
        }

        int attackingPlayerIndex = Random.Range(0, players.Count);
        int defendingPlayerIndex = NextPlayerIndex(attackingPlayerIndex);
        numAttacks = 0;

        while (numAttacks < maxAttacksPerGame && winner == null)
        {
            if (UIController != null)
            {
                UIController.ClearDesk();
            }

            attackRoutine = StartCoroutine(PlayAnAttack(attackingPlayerIndex, defendingPlayerIndex));
            yield return new WaitUntil(() => attackRoutine == null);
            
            DealCards(new List<DurakAgent>{players[attackingPlayerIndex], players[defendingPlayerIndex]});
            
            if (drawCards.Count < 1)
            {
                foreach (DurakAgent durakAgent in players)
                {
                    if (durakAgent.hand.Count < 1)
                    {
                        winner = durakAgent;
                        //draw case plz 
                    }
                }
            }

            if (!attackerWon)
            {
                attackingPlayerIndex = defendingPlayerIndex;
                defendingPlayerIndex = NextPlayerIndex(defendingPlayerIndex);
            }
            
            numAttacks++;
            
            yield return new WaitForSeconds(waitTime);
        }
        
        ReturnCardsToDraw();
        
        if (winner != null)
        {
            winner.AddReward(2f);
        }
        
        foreach (DurakAgent durakAgent in players)
        {
            durakAgent.AddReward(-1);
            durakAgent.EndEpisode();
            //Academy.Instance.EnvironmentStep();
            //yield return new WaitUntil(() => EnvironmentManager.academyStepped);
        }
        
        OnEnvironmentGameCompleted?.Invoke(this);
    }

    private IEnumerator PlayAnAttack(int attackerIndex, int defenderIndex)
    {
        int currentTurnPlayerIndex = attackerIndex;
        attackerWon = false;
        bool attackerGaveUp = false;
        numTurnsThisAttack = 0;
        turnsAmount = -1;
        
        if (UIController != null)
        {
            UIController.UpdatePlayersHands(players, currentTurnPlayerIndex, human);
        }
        
        while (numTurnsThisAttack < maxTurnsPerAttack && !attackerWon && !attackerGaveUp)
        {
            if (currentTurnPlayerIndex == attackerIndex)
            {
                turnsAmount++;
                players[attackerIndex].Attack(players[defenderIndex]);
                OnEnvironmentRequestedDecision?.Invoke(this);
                yield return new WaitUntil(() => EnvironmentManager.academyStepped);
            }
            else
            {
                players[defenderIndex].Defend(players[attackerIndex]);
                OnEnvironmentRequestedDecision?.Invoke(this);
                yield return new WaitUntil(() => EnvironmentManager.academyStepped);
            }
            
            if (playedCardData == null)
            {
                if (currentTurnPlayerIndex == attackerIndex)
                {
                    attackerGaveUp = true;
                    discardedCards.AddRange(inPlayCards);
                }
                else
                {
                    bool attackerAddedCard = false;
                    
                    do
                    {
                        players[attackerIndex].FinishOff(players[defenderIndex]);
                        OnEnvironmentRequestedDecision?.Invoke(this);
                        yield return new WaitUntil(() => EnvironmentManager.academyStepped);
                        
                        attackerAddedCard = playedCardData != null;

                        if (attackerAddedCard)
                        {
                            AddInPlayCard();
                        }
                        
                        turnsAmount++;
                        
                        if (UIController != null)
                        {
                            UIController.UpdatePlayersHands(players, currentTurnPlayerIndex, human);
                        }
                        
                        yield return new WaitForSeconds(waitTime);
                        
                    } while (attackerAddedCard);
                    
                    attackerWon = true;
                    players[defenderIndex].hand.AddRange(inPlayCards);
                    if (UIController != null)
                    {
                        UIController.ClearDesk();
                    }
                }
                inPlayCards.Clear();
            }
            else
            {
                AddInPlayCard();

                if (currentTurnPlayerIndex == defenderIndex && players[defenderIndex].hand.Count < 1)
                {
                    attackerGaveUp = true;
                    discardedCards.AddRange(inPlayCards);
                    inPlayCards.Clear();
                }
                else
                {
                    currentTurnPlayerIndex = currentTurnPlayerIndex == attackerIndex ? defenderIndex : attackerIndex;
                }
            }
            
            if (UIController != null)
            {
                UIController.UpdatePlayersHands(players, currentTurnPlayerIndex, human);
            }
            
            numTurnsThisAttack++;
            yield return new WaitForSeconds(waitTime);
        }
        
        attackRoutine = null;
    }

    private void AddInPlayCard()
    {
        inPlayCards.Insert(0, playedCardData);
        if (UIController != null)
        {
            UIController.AddInPlayCard(playedCardData, turnsAmount);
        }
        playedCardData = null;
    }
    
    private void FillDrawWithAllCards()
    {
        drawCards = new List<CardData>(deck.AllCards);
    }

    private void ShuffleDraw()
    {
        int shuffledCardIndex = drawCards.Count; 
        
        while (shuffledCardIndex > 1) 
        {
            shuffledCardIndex--;  
            int shuffledCardNewIndex = Random.Range(0, shuffledCardIndex + 1);
            (drawCards[shuffledCardNewIndex], drawCards[shuffledCardIndex]) = (drawCards[shuffledCardIndex], drawCards[shuffledCardNewIndex]);
        }  
    }

    private void DealCards(List<DurakAgent> playersToDealCardsTo)
    {
        foreach (DurakAgent durakAgent in playersToDealCardsTo)
        {
            int cardsToDraw = defaultCardAmount - durakAgent.hand.Count;
            
            for (int i = 0; i < cardsToDraw; i++)
            {
                if (drawCards.Count > 0)
                {
                    durakAgent.hand.Add(drawCards[0]);
                    drawCards.RemoveAt(0);
                }
                else
                {
                    return;
                }
            }
        }
        
        if (UIController != null)
        {
            UIController.UpdateDrawAmount(drawCards.Count);
        }
    }

    private void ReturnCardsToDraw()
    {
        foreach (DurakAgent durakAgent in players)
        {
            drawCards.AddRange(durakAgent.hand);
            durakAgent.hand.Clear();
        }
        
        drawCards.AddRange(discardedCards);
        discardedCards.Clear();
        
        drawCards.AddRange(inPlayCards);
        inPlayCards.Clear();
    }
    
    private int NextPlayerIndex(int index)
    {
        int nextPlayerIndexCandidate = index + 1;
        return nextPlayerIndexCandidate < players.Count ? nextPlayerIndexCandidate : 0;
    }
}
