using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck")]
public class Deck : ScriptableObject
{
    [SerializeField] 
    private List<CardData> allCards;
    public List<CardData> AllCards => allCards;
    
    [SerializeField] 
    private Sprite cardBack;
}