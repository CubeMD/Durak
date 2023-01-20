using System;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class Card : MonoBehaviour
    {
        public static event Action<Card, CardData> OnAnyCardPressed;
        
        [SerializeField] 
        private Button cardButton;
        [SerializeField] 
        private Image cardIcon;
        
        private CardData cardData;

        private void Awake()
        {
            cardButton.onClick.AddListener(HandleCardPressed);
        }

        private void OnDestroy()
        {
            cardButton.onClick.RemoveListener(HandleCardPressed);
        }

        public void SetupCard(CardData data, bool interactable = false)
        {
            cardData = data;
            cardButton.interactable = interactable;
            cardIcon.sprite = cardData.cardIcon;
        }
        
        private void HandleCardPressed()
        {
            OnAnyCardPressed?.Invoke(this, cardData);
        }
    }
}