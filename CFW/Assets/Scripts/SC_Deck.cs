using System.Collections.Generic;
using UnityEngine;

public class SC_Deck : MonoBehaviour {

	public List<SC_BaseCard> cards;

    public int Size { get { return cards.Count; } }

    public void Shuffle() {

        cards.Shuffle();

        /*int[] newOrder = new int[cards.Count];

        for (int i = 0; i < newOrder.Length; i++)
            newOrder[i] = i;

        newOrder.Shuffle();

        List<SC_BaseCard> oldCards = new List<SC_BaseCard>(cards);

        for (int i = 0; i < newOrder.Length; i++)
            cards[i] = oldCards[newOrder[i]];*/

    }

}
