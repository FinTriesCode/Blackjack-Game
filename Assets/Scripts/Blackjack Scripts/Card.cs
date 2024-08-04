using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//scriptable object for creating cards in-engine

[CreateAssetMenu(fileName = "Card_", menuName = "Cards")]
public class Card : ScriptableObject
{
    //class for a scriptable object - used for setting up card(s)
    public CardNum cardNum;
    public CardSuit cardSuit;

    public Mesh cardModel;
}