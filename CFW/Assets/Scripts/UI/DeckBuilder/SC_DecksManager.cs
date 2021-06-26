using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Card;
using System.Collections.Generic;

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

        SC_SavedDataManager.LoadDecks ();

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

    public static void UpdateCanSaveDeck () {

        Instance.saveButton.interactable = Instance.deckName.text != "" && SC_DeckBuilder.Instance.deck.childCount == SC_DeckBuilder.Instance.deckSize;

    }

    public void Save () {

        string deck = "";

        foreach (SC_BaseCard c in SC_DeckBuilder.cardsInDeck)
            deck += c.Path + ";";

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

        foreach (Transform t in SC_DeckBuilder.Instance.deck)
            Destroy (t.gameObject);

        foreach (string d in chosenDeck.Split (new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries))
            SC_DeckBuilder.AddCard (Resources.Load<SC_BaseCard> (d));

    }

    public void Back () {

        deckName.text = "";

        loadButton.interactable = false;

        gameObject.SetActive (false);

    }

}
