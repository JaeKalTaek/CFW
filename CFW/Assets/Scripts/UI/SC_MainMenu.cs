using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.SceneManagement;
using TMPro;
using MLAPI.Transports.PhotonRealtime;
using System.Collections.Generic;
using Card;
using static SC_Global;

public class SC_MainMenu : MonoBehaviour {

    public GameObject deckBuilderPanel;

    Dictionary<string, string> decks;

    void Start () {

        allCards = new List<SC_BaseCard> (Resources.LoadAll<SC_BaseCard> (""));

        SC_SavedDataManager.LoadDecks ();

        decks = new Dictionary<string, string> ();

        foreach (SC_Deck d in Resources.LoadAll<SC_Deck> ("Decks/"))
            decks[d.name.Replace (" Deck", "")] = CardsListToCode (d.cards);

        foreach (KeyValuePair<string, string> deck in SC_SavedDataManager.savedData.decks)
            decks[deck.Key] = deck.Value;                

        foreach (KeyValuePair<string, string> deck in decks)
            deckChoice.options.Add (new TMP_Dropdown.OptionData (deck.Key));

        deckChoice.RefreshShownValue ();

    }

    #region Online Play
    public InputField matchCode;
    public TMP_Dropdown deckChoice;

    public void Host () {

        SC_Player.deckCode = decks[deckChoice.options[deckChoice.value].text];

        (NetworkManager.Singleton.NetworkConfig.NetworkTransport as PhotonRealtimeTransport).RoomName = "CFW_Room_" + matchCode.text;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost ();        
        NetworkSceneManager.SwitchScene ("Play_Scene");      

    }

    private void ApprovalCheck (byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback) {

        callback (true, null, true, null, null);

    }

    public void Join () {

        SC_Player.deckCode = decks[deckChoice.options[deckChoice.value].text];

        (NetworkManager.Singleton.NetworkConfig.NetworkTransport as PhotonRealtimeTransport).RoomName = "CFW_Room_" + matchCode.text;
        NetworkManager.Singleton.StartClient ();

    }
    #endregion

    public void ShowDeckBuilder () {

        deckBuilderPanel.SetActive (true);

    }

    public void ShowOnlinePlay () {

        deckBuilderPanel.SetActive (false);

    }

}
