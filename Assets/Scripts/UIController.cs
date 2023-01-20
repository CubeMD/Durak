using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] 
        private Card trumpCard;
        [SerializeField] 
        private TextMeshProUGUI drawAmount;
        [SerializeField] 
        private RectTransform[] playersHand;
        [SerializeField] 
        private GameObject[] playerTurnHighlights;
        [SerializeField] 
        private RectTransform deskParent;
        [SerializeField] 
        private Card cardTemplate;
        [SerializeField] 
        private Transform turnParentTemplate;

        private readonly Dictionary<DurakAgent, List<Card>> playerCardsTable = new Dictionary<DurakAgent, List<Card>>();
        private readonly Dictionary<Transform, List<Card>> inPlayCardsByTurn = new Dictionary<Transform, List<Card>>();

        public void SetTrumpCard(CardData trump)
        {
            trumpCard.SetupCard(trump);
        }

        public void UpdateDrawAmount(int amount)
        {
            drawAmount.text = $"DRAW: {amount}";
        }
        
        public void UpdatePlayersHands(List<DurakAgent> players, int currentTurn, bool hasHumanPlayer)
        {
            for (int i = 0; i < players.Count; i++)
            {
                DurakAgent player = players[i];
                playerTurnHighlights[i].gameObject.SetActive(currentTurn == i);
                
                if (playersHand.Length > i)
                {
                    RectTransform parent = playersHand[i];
                    
                    if (!playerCardsTable.ContainsKey(player))
                    {
                        playerCardsTable.Add(player, new List<Card>());
                    }

                    bool interactable = hasHumanPlayer && i == 0;
                    int handCount = player.hand.Count;
                    for (int j = 0; j < handCount; j++)
                    {
                        CardData cardData = player.hand[j];
                        if (playerCardsTable[player].Count > j)
                        {
                            Card card = playerCardsTable[player][j];
                            card.SetupCard(cardData, interactable);
                        }
                        else
                        {
                            Card card = SpawnAndSetupCard(cardData, parent, interactable);
                            playerCardsTable[player].Add(card);
                        }
                    }

                    int count = playerCardsTable[player].Count - 1;
                    for (int k = count; k > handCount - 1; k--)
                    {
                        Card card = playerCardsTable[player][k];
                        playerCardsTable[player].Remove(card);
                        Destroy(card.gameObject);
                    }
                }
            }
        }

        private Card SpawnAndSetupCard(CardData cardData, Transform parent, bool interactable = false)
        {
            Card card = Instantiate(cardTemplate, parent);
            card.SetupCard(cardData, interactable);
            return card;
        }

        public void AddInPlayCard(CardData cardData, int turn)
        {
            Transform turnParent = null;
            if (inPlayCardsByTurn.Count > turn)
            {
                turnParent = inPlayCardsByTurn.ElementAt(turn).Key;
            }
            else
            {
                turnParent = Instantiate(turnParentTemplate, deskParent);
                inPlayCardsByTurn.Add(turnParent, new List<Card>());
            }
            
            Card card = SpawnAndSetupCard(cardData, turnParent);
            inPlayCardsByTurn[turnParent].Add(card);
        }
        
        public void ClearDesk()
        {
            foreach (KeyValuePair<Transform,List<Card>> turnCards in inPlayCardsByTurn)
            {
                DestroyAndClearList(turnCards.Value);
                Destroy(turnCards.Key.gameObject);
            }
           
            inPlayCardsByTurn.Clear();
        }

        private void DestroyAndClearList(List<Card> cardList)
        {
            foreach (Card card in cardList)
            {
                Destroy(card.gameObject);
            }
                        
            cardList.Clear();
        }
    }
}