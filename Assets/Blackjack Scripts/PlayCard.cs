using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCard : MonoBehaviour
{
    //update card info

    //reference card 
    public Card card;

    //update card's mesh by taking a new card and applying this to card
    public void SetCard(Card c)
    {
        card = c;

        gameObject.GetComponent<MeshFilter>().mesh = card.cardModel;
    }

    public Card GetCard() { return card; }
}
