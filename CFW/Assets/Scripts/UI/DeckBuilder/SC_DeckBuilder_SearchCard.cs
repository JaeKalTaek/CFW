using Card;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SC_DeckBuilder_SearchCard : MonoBehaviour, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    public Image image;

    bool selected;

    void Start () {

        SC_DeckBuilder.filteredCards[Card] = this;

        selected = SC_DeckBuilder.cardsInDeck.Contains (Card);

        image.color = new Color (1, 1, 1, selected ? .5f : 1);

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (!Card.Is (SC_Global.CardType.Basic)) {

            selected ^= true;

            image.color = new Color (1, 1, 1, selected ? .5f : 1);

            if (selected)
                SC_DeckBuilder.AddCard (Card);
            else
                SC_DeckBuilder.RemoveCard (Card);

        }

    }

}
