using Card;
using System;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using static SC_Global;
using MLAPI.Messaging;
using System.Collections;
using MLAPI.NetworkVariable;

public class SC_Player : NetworkBehaviour {

    public static string deckName;

	public SC_Deck Deck { get; set; }

    public List<SC_BaseCard> Hand { get; set; }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public static SC_Player localPlayer, otherPlayer;

    public ShiFuMi ShifumiChoice { get; set; }

    public bool Turn { get; set; }

    public bool CanPlay { get; set; }

    public bool Busy { get; set; }

    public int Health { get; set; }

    public int Stamina { get; set; }

    public int Alignment { get; set; }

    public Dictionary<BodyPart, int> BodyPartsHealth;

    private NetworkVariable<bool> hasOtherPlayer = new NetworkVariable<bool> (new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly }, false);

    #region Setup
    public override void NetworkStart () {

        Hand = new List<SC_BaseCard> ();

        if (IsLocalPlayer) {

            localPlayer = this;

            if (otherPlayer)
                SetHasOtherPlayer ();

            StartCoroutine (SetupCoroutine ());

        } else {

            otherPlayer = this;

            localPlayer?.SetHasOtherPlayer ();

        }

    }

    void SetHasOtherPlayer () {

        hasOtherPlayer.Value = true;

    }

    public void SetupPlayerValues () {

        Health = GM.baseHealth;

        Stamina = GM.baseStamina;

        BodyPartsHealth = new Dictionary<BodyPart, int>();

        foreach (BodyPart bP in Enum.GetValues(typeof(BodyPart)))
            if (bP != BodyPart.None)
                BodyPartsHealth.Add(bP, GM.baseBodyPartHealth);

        if (IsLocalPlayer)
            SetDeckServerRpc (deckName);

    }

    [ServerRpc]
    void SetDeckServerRpc (string deckName) {

        SetDeckClientRpc (deckName);

    }

    [ClientRpc]
    void SetDeckClientRpc (string deckName) {

         Deck = Instantiate (Resources.Load<SC_Deck> ("Decks/" + deckName + "Deck"), GM.background);

    }

    bool decksShuffled;

    IEnumerator SetupCoroutine () {

        while (!otherPlayer || !otherPlayer.hasOtherPlayer.Value || !GM)
            yield return new WaitForEndOfFrame ();

        SetupPlayerValues ();
        otherPlayer.SetupPlayerValues ();
    
        while (!Deck || !otherPlayer.Deck)
            yield return new WaitForEndOfFrame ();

        Deck.Shuffle ();

        SetupDeckServerRpc ();

        while (!decksShuffled || !otherPlayer.decksShuffled)
            yield return new WaitForEndOfFrame ();

        GM.waitingPanel.SetActive (false);

    }

    [ServerRpc]
    void SetupDeckServerRpc () {

        SetupDeckClientRpc ();

    }

    [ClientRpc]
    void SetupDeckClientRpc () {        
        
        Deck.Setup (IsLocalPlayer);

        decksShuffled = true;

    }
    #endregion

    #region Deck
    [ServerRpc]
    public void ShuffleDeckServerRpc (int[] newOrder) {

        ShuffleDeckClientRpc (newOrder);

    }

    [ClientRpc]
    void ShuffleDeckClientRpc (int[] newOrder) {

        Deck.Shuffle (newOrder);

    }

    [ServerRpc]
    public void DrawServerRpc (int nbr, bool startTurn) {

        DrawClientRpc (nbr, startTurn);

    }

    [ClientRpc]
    void DrawClientRpc (int nbr, bool startTurn) {

        Deck.Draw (nbr, true);

    }
    #endregion

    #region ShiFuMi
    [ServerRpc]
    public void ShiFuMiChoiceServerRpc (int s) {

        ShiFuMiChoiceClientRpc (s);

    }

    [ClientRpc]
    void ShiFuMiChoiceClientRpc (int s) {

        ShifumiChoice = (ShiFuMi)s;

        if (IsLocalPlayer) {

            if (otherPlayer.ShifumiChoice != ShiFuMi.None) {

                if (Win(ShifumiChoice, otherPlayer.ShifumiChoice)) {

                    ChooseStartingPlayerServerRpc (true);

                } else if (ShifumiChoice == otherPlayer.ShifumiChoice) {

                    ResetShiFuMiServerRpc ();                        

                } else {

                    ChooseStartingPlayerServerRpc (false);

                }

            } else {

                GM.shifumiText.text = "Waiting for other player to choose...";

            }

        }

    }

    [ServerRpc]
    void ResetShiFuMiServerRpc () {

        ResetShiFuMiClientRpc ();

    }

    [ClientRpc]
    void ResetShiFuMiClientRpc () {

        ShifumiChoice = ShiFuMi.None;

        otherPlayer.ShifumiChoice = ShiFuMi.None;

        SC_ShiFuMiChoice.Draw();

    }
    #endregion

    #region Start Game
    [ServerRpc]
    void ChooseStartingPlayerServerRpc (bool won) {

        ChooseStartingPlayerClientRpc (won);

    }

    [ClientRpc]
    void ChooseStartingPlayerClientRpc (bool won) {

        GM.shifumiPanel.SetActive(false);

        GM.ShowTurnPanel(IsLocalPlayer ? won : !won);

    }    

    [ServerRpc]
    public void StartGameServerRpc (bool start) {

        StartGameClientRpc (start);

    }

    [ClientRpc]
    void StartGameClientRpc (bool start) {        

        (IsLocalPlayer ? localPlayer : otherPlayer).Turn = start;

        (IsLocalPlayer ? otherPlayer : localPlayer).Turn = !start;

        GM.StartGame();

    }
    #endregion

    #region Card usage
    delegate void CardAction (Transform t);

    void ActionOnCard (string id, CardAction a) {

        foreach (Transform t in IsLocalPlayer ? GM.localHand : GM.otherHand)
            if (t.name == id)
                a(t);

    }

    #region Use card
    [ServerRpc]
    public void UseCardServerRpc (string id, bool choice) {

        UseCardClientRpc (id, choice);

    }

    [ClientRpc]
    void UseCardClientRpc (string id, bool choice) {

        SC_OffensiveMove.currentChoice = choice;        

        ActionOnCard (id, (t) => { t.GetComponent<SC_UI_Card> ().Card.Use (this); });

    }
    #endregion
    #endregion

    #region Skip turn
    [ServerRpc]
    public void SkipTurnServerRpc () {

        SkipTurnClientRpc ();

    }

    [ClientRpc]
    void SkipTurnClientRpc () {

        if (!IsLocalPlayer) {

            localPlayer.Turn = true;

            localPlayer.DrawServerRpc (1, true);

        }

    }
    #endregion

}
