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
using static Card.SC_BaseCard;

public class SC_Player : NetworkBehaviour {

    public static string deckName;

	public SC_Deck Deck { get; set; }

    public List<SC_BaseCard> Hand { get; set; }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public static SC_Player localPlayer, otherPlayer;

    public ShiFuMi ShifumiChoice { get; set; }

    public bool Turn { get; set; }

    public bool Busy { get; set; }

    public ChoosingCard ChoosingCard { get; set; }

    public Dictionary<string, int> IntChoices { get; set; }

    public int GetIntChoice (string id) { IntChoices.TryGetValue (id, out int v); return v; }

    public Dictionary<string, string> StringChoices { get; set; }

    public string GetStringChoice (string id) { StringChoices.TryGetValue (id, out string v); return v; }

    private int health;
    public int Health { get => health; set => health = Mathf.Clamp (value, 0, GM.baseHealth); }

    private int stamina;
    public int Stamina { get => stamina; set => stamina = Mathf.Clamp (value, 0, GM.baseStamina); }

    private int alignment;
    public int Alignment { get => alignment; set => alignment = Mathf.Clamp (value, -GM.maxAlignment, GM.maxAlignment); }

    public bool Heel { get { return alignment < 0; } }
    public bool Neutral { get { return alignment == 0; } }
    public bool Face { get { return alignment > 0; } }

    public Dictionary<BodyPart, int> BodyPartsHealth;

    NetworkVariable<bool> hasOtherPlayer = new NetworkVariable<bool> (new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly }, false);

    public NetworkVariable<Locked> locked = new NetworkVariable<Locked> (new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly }, Locked.Unlocked);

    public bool Unlocked { get { return locked.Value == Locked.Unlocked; } }

    public bool Pinned { get { return locked.Value == Locked.Pinned; } }

    public bool Submitted { get { return locked.Value == Locked.Submission; } }

    public static bool NoLock { get { return localPlayer.Unlocked && otherPlayer.Unlocked; } }

    public bool SpecialUsed { get; set; }

    #region Setup
    public override void NetworkStart () {

        Turn = false;

        IntChoices = new Dictionary<string, int> ();

        StringChoices = new Dictionary<string, string> ();

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

        GM.waitPanel.SetActive (false);

        if ((start && !IsLocalPlayer) || (!start && IsLocalPlayer))
            localPlayer.NextTurnServerRpc ();

    }
    #endregion

    #region Card usage
    public delegate void CardAction (SC_BaseCard c);

    public void ActionOnCard (string id, CardAction a) {

        foreach (SC_BaseCard c in IsLocalPlayer ? Hand : otherPlayer.Hand) {

            if (c.UICard.name == id) {

                a (c);

                return;

            }

        }

        Debug.LogError ("CARD NOT FOUND FOR ACTION");

    }

    #region Set choices
    [ServerRpc]
    public void SetIntChoiceServerRpc (string id, int choice) {

        SetIntChoiceClientRpc (id, choice);

    }

    [ClientRpc]
    void SetIntChoiceClientRpc (string id, int choice) {

        IntChoices[id] = choice;

    }

    [ServerRpc]
    public void SetStringChoiceServerRpc (string id, string choice) {

        SetStringChoiceClientRpc (id, choice);

    }

    [ClientRpc]
    void SetStringChoiceClientRpc (string id, string choice) {

        StringChoices[id] = choice;

    }
    #endregion

    [ServerRpc]
    public void PlayCardServerRpc (string id) {

        PlayCardClientRpc (id);

    }

    [ClientRpc]
    void PlayCardClientRpc (string id) {

        ActionOnCard (id, (c) => { c.Play (this); });

    }    

    public SC_BaseCard CopyAndStartUsing (SC_UI_Card original) {

        SC_UI_Card c = Instantiate (original, IsLocalPlayer ? GM.localHand : GM.otherHand);        

        c.gameObject.SetActive (true);

        c.Card.Ephemeral = true;

        c.name = c.Card.Path;

        c.RecT.anchorMin = c.RecT.anchorMax = c.RecT.pivot = c.BigRec.anchorMin = c.BigRec.anchorMax = c.BigRec.pivot = new Vector2 (.5f, IsLocalPlayer ? 0 : 1);

        SC_Deck.OrganizeHand (IsLocalPlayer ? GM.localHand : GM.otherHand);

        Hand.Add (c.Card);

        if (!IsLocalPlayer)
            c.GetComponent<Image> ().sprite = Resources.Load<Sprite> ("Sprites/CardBack");
        else
            StartCoroutine (c.Card.StartPlaying ());

        return c.Card;

    }

    [ServerRpc]
    public void CopyAndStartUsingServerRpc () {

        CopyAndStartUsingClientRpc ();

    }

    [ClientRpc]
    void CopyAndStartUsingClientRpc () {

        CopyAndStartUsing (originalCard.UICard);

    }

    [ServerRpc]
    public void StartUsingBasicServerRpc (int id) {

        StartUsingBasicClientRpc (id);

    }

    [ClientRpc]
    void StartUsingBasicClientRpc (int id) {

        CopyAndStartUsing (UI.basicsPanel.transform.GetChild (id).GetComponent<SC_UI_Card> ());

    }

    [ServerRpc]
    public void MaintainSubmissionServerRpc () {

        MaintainSubmissionClientRpc ();

    }

    [ClientRpc]
    void MaintainSubmissionClientRpc () {

        localPlayer.Busy = true;

        (lockingCard as SC_Submission).Maintain ();

    }

    [ServerRpc]
    public void DoubleTapServerRpc () {

        DoubleTapClientRpc ();

    }

    [ClientRpc]
    void DoubleTapClientRpc () {

        originalCard.DoubleTapEffect ();

    }
    #endregion

    #region Next turn
    [ServerRpc]
    public void NextTurnServerRpc () {

        NextTurnClientRpc ();

    }

    [ClientRpc]
    void NextTurnClientRpc () {

        localPlayer.Busy = false;

        if (IsLocalPlayer)
            localPlayer.Turn = false;           

        StartCoroutine ((IsLocalPlayer ? otherPlayer : localPlayer).Deck.Draw (true));

    }

    public void StartTurn () {

        SpecialUsed = false;

        Turn = true;

        if (IsLocalPlayer)
            (NoLock ? UI.showBasicsButton : (otherPlayer.Submitted ? UI.maintainSubmissionButton : UI.showLockedBasicsButton)).SetActive (true);

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

    #region Discard
    [ServerRpc]
    public void DiscardServerRpc (string id) {

        DiscardClientRpc (id);

    }

    [ClientRpc]
    void DiscardClientRpc (string id) {

        ActionOnCard (id, (c) => { c.Discard (this); });

    }
    #endregion    

}
