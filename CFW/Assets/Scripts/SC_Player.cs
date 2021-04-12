using Card;
using System;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using static SC_Global;
using MLAPI.Messaging;
using System.Collections;

public class SC_Player : NetworkBehaviour {

    public static string deckName;

	public SC_Deck Deck { get; set; }

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

    #region Setup
    public override void NetworkStart () {

        if (IsLocalPlayer) {

            localPlayer = this;

            StartCoroutine (SetupCoroutine ());

        } else
            otherPlayer = this;

    }

    public void SetupPlayerValues () {

        Health = GM.baseHealth;

        Stamina = GM.baseStamina;

        BodyPartsHealth = new Dictionary<BodyPart, int>();

        foreach (BodyPart bP in Enum.GetValues(typeof(BodyPart)))
            if (bP != BodyPart.None)
                BodyPartsHealth.Add(bP, GM.baseBodyPartHealth);

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

    [ServerRpc]
    void DecksReadyServerRpc () {

        DecksReadyClientRpc ();

    }

    [ClientRpc]
    void DecksReadyClientRpc () {

        decksReady = true;

    }

    bool decksReady, decksShuffled;

    IEnumerator SetupCoroutine () {

        while (!otherPlayer || !GM)
            yield return new WaitForEndOfFrame ();

        SetupPlayerValues ();

        while (!Deck || !otherPlayer.Deck)
            yield return new WaitForEndOfFrame ();

        DecksReadyServerRpc ();

        while (!decksReady || !otherPlayer.decksReady)
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
    public void DrawServerRpc (int nbr) {

        DrawClientRpc (nbr);

    }

    [ClientRpc]
    void DrawClientRpc (int nbr) {

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

        (IsLocalPlayer ? this : otherPlayer).Turn = start;

        (IsLocalPlayer ? otherPlayer : this).Turn = !start;

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

    #region Base usage
    [ServerRpc]
    public void UseBaseCardServerRpc (string id) {

        BaseUseCardClientRpc (id);

    }

    [ClientRpc]
    void BaseUseCardClientRpc (string id) {

        ActionOnCard(id, (t) => { t.GetComponent<SC_UI_Card>().Card.Use(this); });

        BaseCardUsage(id);

    }

    void BaseCardUsage (string id) {

        RectTransform h = IsLocalPlayer ? GM.localHand : GM.otherHand;

        ActionOnCard(id, (t) => { Destroy(t.gameObject); });

        SC_Deck.OrganizeHand(h);

    }
    #endregion

    #region Offensive move with body part choice
    [ServerRpc]
    public void UseOffensiveMoveCardServerRpc (string id, bool choice) {

        UseOffensiveMoveCardClientRpc (id, choice);

    }

    [ClientRpc]
    void UseOffensiveMoveCardClientRpc (string id, bool choice) {

        ActionOnCard(id, (t) => { (t.GetComponent<SC_UI_Card>().Card as SC_OffensiveMove).Use(this, choice); });

        BaseCardUsage(id);

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

            localPlayer.DrawServerRpc (1);

        }

    }
    #endregion

}
