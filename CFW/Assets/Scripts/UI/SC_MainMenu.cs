using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.SceneManagement;
using TMPro;
using MLAPI.Transports.PhotonRealtime;

public class SC_MainMenu : MonoBehaviour {

    public GameObject deckBuilderPanel;

    void Start () {

        foreach (SC_Deck d in Resources.LoadAll<SC_Deck> ("Decks/"))
            deckChoice.options.Add (new TMP_Dropdown.OptionData (d.name.Replace (" Deck", "")));

        deckChoice.RefreshShownValue ();

    }

    #region Online Play
    public InputField matchCode;
    public TMP_Dropdown deckChoice;

    public void Host () {

        SC_Player.deckName = deckChoice.options[deckChoice.value].text;

        (NetworkManager.Singleton.NetworkConfig.NetworkTransport as PhotonRealtimeTransport).RoomName = "CFW_Room_" + matchCode.text;
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost ();        
        NetworkSceneManager.SwitchScene ("Play_Scene");      

    }

    private void ApprovalCheck (byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback) {

        callback (true, null, true, null, null);

    }

    public void Join () {

        SC_Player.deckName = deckChoice.options[deckChoice.value].text;

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
