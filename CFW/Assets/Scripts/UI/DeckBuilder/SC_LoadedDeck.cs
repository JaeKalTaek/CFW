using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class SC_LoadedDeck : MonoBehaviour, IPointerClickHandler {

    public TextMeshProUGUI deckName;

    string deck;

    public void Setup (string name, string d) {

        deckName.text = name;

        deck = d;

    }

    public void OnPointerClick (PointerEventData eventData) {

        SC_DecksManager.Instance.SetChosenDeck (deck);

    }

}
