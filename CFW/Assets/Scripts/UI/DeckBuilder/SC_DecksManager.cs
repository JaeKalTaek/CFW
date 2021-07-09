using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Card;
using System.Collections.Generic;
using System.Linq;

public class SC_DecksManager : MonoBehaviour {

    public Transform decks;

    public TMP_InputField deckName;

    public Button saveButton, loadButton;

    public static SC_DecksManager Instance;

    public Dictionary<string, SC_LoadedDeck> loadedDecks;

    void Start () {

        Instance = this;

        loadedDecks = new Dictionary<string, SC_LoadedDeck> ();

        deckName.onValueChanged.AddListener ((s) => { UpdateCanSaveDeck (); });        

        foreach (KeyValuePair<string, string> deck in SC_SavedDataManager.savedData.decks)
            CreateDeck (deck.Key, deck.Value);

    }

    void CreateDeck (string n, string d) {

        SC_LoadedDeck l = Instantiate (Resources.Load<SC_LoadedDeck> ("Prefabs/DeckBuilder/P_LoadedDeck"), decks);

        l.Setup (n, d);

        loadedDecks.Add (n, l);

    }

    public void Show () { 

        gameObject.SetActive (true);

    }

    public void UpdateCanSaveDeck () {

        saveButton.interactable = deckName.text != "" && SC_DeckBuilder.deckCards.Count == SC_DeckBuilder.Instance.deckSize;

    }

    public void Save () {

        string deck = SC_Global.CardsListToCode (new List<SC_BaseCard> (SC_DeckBuilder.deckCards.Keys));

        if (loadedDecks.ContainsKey (deckName.text))
            loadedDecks[deckName.text].Setup (deckName.text, deck);
        else
            CreateDeck (deckName.text, deck);

        SC_SavedDataManager.SaveDeck (deckName.text, deck);

    }

    string chosenDeck;

    public void SetChosenDeck (string d) {

        chosenDeck = d;

        loadButton.interactable = true;

    }

    public void Load () {

        for (int i = SC_DeckBuilder.deckCards.Count; i > 0; i--)
          SC_DeckBuilder.TryAddRemove (SC_DeckBuilder.deckCards.Keys.ElementAt (0), false);

        foreach (SC_BaseCard c in SC_Global.CodeToCardsList (chosenDeck))
            SC_DeckBuilder.TryAddRemove (c, true);

    }

    public void Back () {

        deckName.text = "";

        loadButton.interactable = false;

        gameObject.SetActive (false);

    }

}
