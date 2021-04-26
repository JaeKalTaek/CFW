using Card;
using System;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using static SC_Global;
using MLAPI.Messaging;
using System.Collections;
using MLAPI.NetworkVariable;
using UnityEngine.UI;

public class SC_Player : NetworkBehaviour {

    public static string deckName;

	public SC_Deck Deck { get; set; }

    public List<SC_BaseCard> Hand { get; set; }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public static SC_Player localPlayer, otherPlayer;

    public ShiFuMi ShifumiChoice { get; set; }

    public bool Turn { get; set; }

    public bool CanPlay { get; set; }

    public bool Busy { get; set; }

    public bool Assessing { get; set; }

    public int CurrentChoice { get; set; }

    private int health;
    public int Health { get => health; set => health = Mathf.Clamp (value, 0, GM.baseHealth); }

    private int stamina;
    public int Stamina { get => stamina; set => stamina = Mathf.Clamp (value, 0, GM.baseStamina); }

    private int alignment;
    public int Alignment { get => alignment; set => alignment = Mathf.Clamp (value, -GM.maxAlignment, GM.maxAlignment); }

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

    #region Shuffle Deck
    [ServerRpc]
    public void ShuffleDeckServerRpc (int[] newOrder) {

        ShuffleDeckClientRpc (newOrder);

    }

    [ClientRpc]
    void ShuffleDeckClientRpc (int[] newOrder) {

        Deck.Shuffle (newOrder);

    }
    #endregion

    #region Draw
    [ServerRpc]
    public void DrawServerRpc (int nbr, bool startTurn) {

        DrawClientRpc (nbr, startTurn);

    }

    [ClientRpc]
    void DrawClientRpc (int nbr, bool startTurn) {
        
        if (startTurn)
            ApplySingleEffect ("Stamina", GM.staminaPerTurn);

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
    public delegate void CardAction (SC_UI_Card c);

    public void ActionOnCard (string id, CardAction a) {

        foreach (Transform t in IsLocalPlayer ? GM.localHand : GM.otherHand)
            if (t.name == id)
                a (t.GetComponent <SC_UI_Card> ());

    }

    #region Use card
    [ServerRpc]
    public void UseCardServerRpc (string id, int choice = 0) {

        UseCardClientRpc (id, choice);

    }

    [ClientRpc]
    void UseCardClientRpc (string id, int choice) {

        localPlayer.CurrentChoice = choice;        

        ActionOnCard (id, (c) => { c.Card.Play (this); });

    }

    [ServerRpc]
    public void UseBasicCardServerRpc (int id, int choice = 0) {

        UseBasicCardClientRpc (id, choice);

    }

    [ClientRpc]
    void UseBasicCardClientRpc (int id, int choice) {

        CurrentChoice = choice;

        SC_UI_Card c = Instantiate (UI.basicsPanel.transform.GetChild (id)).GetComponent <SC_UI_Card> ();

        c.name = c.Card.Path;

        c.transform.SetParent (IsLocalPlayer ? GM.localHand : GM.otherHand, false);

        if (!IsLocalPlayer)
            c.GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Sprites/CardBack");

        c.Card.Play (this);

    }
    #endregion
    #endregion

    #region Skip turn
    public void SkipTurn () {

        Turn = false;

        CanPlay = false;

        SkipTurnServerRpc ();

    }

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

    #region Apply effects
    public void ApplySingleEffect (string field, int? value = null, object effect = null) {

        typeof (SC_Player).GetProperty (field).SetValue (this, ((int) typeof (SC_Player).GetProperty (field).GetValue (this)) + (value ?? -(int) effect.GetType ().GetField (field.ToLower ()).GetValue (effect)));

        UI.SetValue (IsLocalPlayer, field, (int) typeof (SC_Player).GetProperty (field).GetValue (this));

    }

    public void ApplySingleBodyEffect (BodyPart part, int effect) {

        BodyPartsHealth[part] = Mathf.Clamp (BodyPartsHealth[part] - effect, 0, GM.baseBodyPartHealth);

        UI.SetValue (IsLocalPlayer, part.ToString (), BodyPartsHealth[part]);

    }
    #endregion

}
