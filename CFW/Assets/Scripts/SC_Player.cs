using Card;
using System;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using static SC_Global;
using MLAPI.Messaging;
using System.Collections;
using MLAPI.NetworkVariable;
using static Card.SC_BaseCard;
using static SC_UI_Manager;

public class SC_Player : NetworkBehaviour {

    public static string deckCode;

	public SC_Deck Deck { get; set; }

    public SC_Graveyard Graveyard { get; set; }

    public List<SC_BaseCard> Hand { get; set; }

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return Instance; } }

    public static SC_Player localPlayer, otherPlayer;

    public ShiFuMi ShifumiChoice { get; set; }

    public bool Turn { get; set; }

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
    public bool FirstAttackPlayed { get; set; }

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

        Alignment = GM.startingAlignment;

        if (IsLocalPlayer)
            SetDeckServerRpc (deckCode);

    }

    [ServerRpc]
    void SetDeckServerRpc (string deckCodeParam) {

        SetDeckClientRpc (deckCodeParam);

    }

    [ClientRpc]
    void SetDeckClientRpc (string deckCodeParam) {

        Deck = Instantiate (Resources.Load<SC_Deck> ("Prefabs/P_Deck"), GM.background);
        Deck.cards = CodeToCardsList (deckCodeParam);

    }

    bool decksShuffled;

    IEnumerator SetupCoroutine () {

        while (!otherPlayer || !otherPlayer.hasOtherPlayer.Value || !GM)
            yield return new WaitForEndOfFrame ();

        SetupPlayerValues ();
        otherPlayer.SetupPlayerValues ();
    
        while (!Deck || !otherPlayer.Deck)
            yield return new WaitForEndOfFrame ();

        if (!Deck.ordered)
            Deck.Shuffle ();

        SetupDeckServerRpc ();

        while (!decksShuffled || !otherPlayer.decksShuffled)
            yield return new WaitForEndOfFrame ();

        Graveyard = GM.localGraveyard;
        otherPlayer.Graveyard = GM.otherGraveyard;

        shifumiAction = (b) => { GM.ShowTurnPanel (b); };

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
    public static BooleanChoice shifumiAction;

    [ServerRpc]
    public void ShiFuMiChoiceServerRpc (int s) {

        ShiFuMiChoiceClientRpc (s);

    }

    [ClientRpc]
    void ShiFuMiChoiceClientRpc (int s) {

        ShifumiChoice = (ShiFuMi)s;

        if (IsLocalPlayer) {

            if (otherPlayer.ShifumiChoice != ShiFuMi.None) {

                if (ShifumiChoice == otherPlayer.ShifumiChoice)
                    ResetShiFuMiServerRpc ();
                else
                    ShiFuMiFinishedServerRpc (Win (ShifumiChoice, otherPlayer.ShifumiChoice));

            } else
                SC_ShiFuMiChoice.text.text = "Waiting for other player to choose...";

        }

    }

    [ServerRpc]
    void ResetShiFuMiServerRpc () {

        ResetShiFuMiClientRpc ();

    }

    [ClientRpc]
    void ResetShiFuMiClientRpc () {

        SC_ShiFuMiChoice.Draw ();

    }

    [ServerRpc]
    void ShiFuMiFinishedServerRpc (bool won) {

        ShiFuMiFinishedClientRpc (won);

    }

    [ClientRpc]
    void ShiFuMiFinishedClientRpc (bool won) {

        GM.shifumiPanel.SetActive (false);

        shifumiAction (IsLocalPlayer ? won : !won);

    }
    #endregion

    #region Start Game
    [ServerRpc]
    public void StartGameServerRpc (bool start) {

        StartGameClientRpc (start);

    }

    [ClientRpc]
    void StartGameClientRpc (bool start) {        

        GM.waitPanel.SetActive (false);

        UI.turnIndicator.rotation = Quaternion.Euler (0, 0, UI.turnIndicatorRotation.F (start == IsLocalPlayer));

        UI.turnIndicator.gameObject.SetActive (true);

        (start == IsLocalPlayer ? localPlayer : otherPlayer).StartCoroutine ((start == IsLocalPlayer ? localPlayer : otherPlayer).StartTurn ());

    }
    #endregion

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

    #region Card usage
    public delegate void CardAction (SC_BaseCard c);

    public void ActionOnCard (int id, CardAction a) {

        a ((IsLocalPlayer ? Hand : otherPlayer.Hand)[id]);

    }    

    [ServerRpc]
    public void PlayCardServerRpc (int id, bool onTheRing = false) {

        PlayCardClientRpc (id, onTheRing);

    }

    [ClientRpc]
    void PlayCardClientRpc (int id, bool onTheRing) {

        if (onTheRing)
            (IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots)[0].slot.parent.GetChild (id).GetComponentInChildren<SC_BaseCard> ().Play (this);
        else
            ActionOnCard (id, (c) => { c.Play (this); });

    }    

    public SC_BaseCard CopyAndStartUsing (SC_BaseCard original) {

        SC_UI_Card c = Create (original, IsLocalPlayer ? GM.localHand : GM.otherHand);

        c.Card.Ephemeral = true;        

        c.RecT.anchorMin = c.RecT.anchorMax = c.RecT.pivot = c.BigRec.anchorMin = c.BigRec.anchorMax = c.BigRec.pivot = new Vector2 (.5f, IsLocalPlayer ? 0 : 1);

        SC_Deck.OrganizeHand (IsLocalPlayer ? GM.localHand : GM.otherHand);

        AddCard (c.Card);

        if (IsLocalPlayer) {

            c.SetImages ();

            c.Card.StartCoroutine (c.Card.StartPlaying ());

        }

        return c.Card;

    }

    [ServerRpc]
    public void CopyAndStartUsingServerRpc () {

        CopyAndStartUsingClientRpc ();

    }

    [ClientRpc]
    void CopyAndStartUsingClientRpc () {

        CopyAndStartUsing (originalCard);

    }

    [ServerRpc]
    public void StartUsingBasicServerRpc (int id) {

        StartUsingBasicClientRpc (id);

    }

    [ClientRpc]
    void StartUsingBasicClientRpc (int id) {

        CopyAndStartUsing (UI.basicsPanel.transform.GetChild (id).GetComponent<SC_UI_Card> ().Card);

    }

    [ServerRpc]
    public void InterceptCardFinishServerRpc () {

        InterceptCardFinishClientRpc ();

    }

    [ClientRpc]
    void InterceptCardFinishClientRpc () {

        interceptNext = true;

    }

    #region Specific cards
    [ServerRpc]
    public void FinishStrikesComboServerRpc (int nbr) {

        FinishStrikesComboClientRpc (nbr);

    }

    [ClientRpc]
    void FinishStrikesComboClientRpc (int nbr) {

        originalCard.StartCoroutine ((originalCard as SC_StrikesCombo).FinishCombo (nbr));

    }
    #endregion
    #endregion

    #region Next turn
    public static event Action OnNewTurn;

    public static event Action OnNoAttackTurn;

    [ServerRpc]
    public void NextTurnServerRpc (bool noAttack = false) {

        NextTurnClientRpc (noAttack);

    }

    [ClientRpc]
    void NextTurnClientRpc (bool noAttack) {

        if (noAttack)
            OnNoAttackTurn?.Invoke ();

        Turn = false;

        (IsLocalPlayer ? otherPlayer : localPlayer).StartCoroutine ((IsLocalPlayer ? otherPlayer : localPlayer).StartTurn ());

    }

    bool firstTurnDone;

    public bool SkipDraw { get; set; }

    public IEnumerator StartTurn () {

        yield return WaitForPlayers ();

        if (localPlayer.firstTurnDone || otherPlayer.firstTurnDone)
            yield return StartCoroutine (UI.UpdateTurn (IsLocalPlayer));

        ApplySingleEffect ("Stamina", GM.staminaPerTurn);

        Turn = true;

        if (OnNewTurn != null && OnNewTurn.GetInvocationList ().Length > 0) {

            foreach (Delegate d in OnNewTurn.GetInvocationList ()) {

                d.Method.Invoke (d.Target, null);

                while (activeCard?.ApplyingEffects ?? false)
                    yield return new WaitForEndOfFrame ();

                activeCard = null;

            }

        }

        if (SkipDraw)
            SkipDraw = false;
        else {

            Turn = false;

            yield return StartCoroutine (Deck.Draw ());

            Turn = true;

        }

        SpecialUsed = FirstAttackPlayed = false;            

        if (!firstTurnDone) {

            firstTurnDone = true;

            foreach (SC_BaseCard c in new List<SC_BaseCard> (Hand)) {

                c.Caller = this;

                c.Receiver = IsLocalPlayer ? otherPlayer : localPlayer;

                yield return c.StartCoroutine (c.FirstTurnEffect ());

            }

        }

        GM.Turn++;

        if (IsLocalPlayer)
            BecomeActive ();

    }
    #endregion

    #region Apply effects
    [ServerRpc]
    public void FinishedApplyingEffectsServerRpc () {

        FinishedApplyingEffectsClientRpc ();

    }

    [ClientRpc]
    void FinishedApplyingEffectsClientRpc () {

        activeCard.ApplyingEffects = false;

    }

    public void ApplySingleEffect (string field, int value, bool hit = false) {        

        if (value != 0) {

            int before = (int) typeof (SC_Player).GetProperty (field).GetValue (this);

            if (field == "Health" && !hit && Health + value <= 0)
                Health = 1;
            else
                typeof (SC_Player).GetProperty (field).SetValue (this, ((int) typeof (SC_Player).GetProperty (field).GetValue (this)) + value);

            UI.SetValue (IsLocalPlayer, field, (int) typeof (SC_Player).GetProperty (field).GetValue (this), before != (int) typeof (SC_Player).GetProperty (field).GetValue (this));

        }

    }

    public void ApplySingleBodyEffect (BodyPart part, int effect, bool hit = false) {

        if (effect != 0) {

            ApplySingleEffect ("Health", Mathf.Min (BodyPartsHealth[part] - effect, 0), hit);

            int before = BodyPartsHealth[part];

            BodyPartsHealth[part] = Mathf.Clamp (BodyPartsHealth[part] - effect, 0, GM.baseBodyPartHealth);

            UI.SetValue (IsLocalPlayer, part.ToString (), BodyPartsHealth[part], before != BodyPartsHealth[part]);

        }

    }
    #endregion

    #region Discard
    Action DoubleDiscardEffect;

    public void StartDoubleDiscard (Action a) {

        DoubleDiscardEffect = a;

        IntChoices["DoubleDiscard"] = -1;

        StartChoosingCard (ChoosingCard.DoubleDiscard);

    }

    [ServerRpc]
    public void DoubleDiscardServerRpc () {

        DoubleDiscardClientRpc ();

    }

    [ClientRpc]
    void DoubleDiscardClientRpc () {

        ActionOnCard (GetIntChoice ("DoubleDiscard"), (c) => {

            c.Discard (this, () => {

                ActionOnCard (GetIntChoice ("DoubleDiscard2") - (GetIntChoice ("DoubleDiscard") < GetIntChoice ("DoubleDiscard2") ? 1 : 0), (ca) => {

                    ca.Discard (this, () => {

                        if (IsLocalPlayer)
                            DoubleDiscardEffect ();

                    });

                });

            });

        });

    }

    [ServerRpc]
    public void DiscardServerRpc (int id) {

        DiscardClientRpc (id);

    }

    [ClientRpc]
    void DiscardClientRpc (int id) {

        ActionOnCard (id, (c) => { c.Discard (this); });

    }
    #endregion

    #region Response
    [ServerRpc]
    public void StartResponseServerRpc () {

        StartResponseClientRpc ();

    }

    [ClientRpc]
    void StartResponseClientRpc () {

        if (!IsLocalPlayer) {

            respondedCards.Push (activeCard);

            UI.ShowMessage ("Responding");

        }

        respondedCards.Peek ().StopAllCoroutines ();

    }
    #endregion

    #region Boost
    [ServerRpc]
    public void StartBoostServerRpc (int id) {

        StartBoostClientRpc (id);

    }

    [ClientRpc]
    void StartBoostClientRpc (int id) {

        if (!IsLocalPlayer)
            respondedCards.Push (Hand[id]);      

    }
    #endregion

    #region Hand related
    public void AddCard (SC_BaseCard c) {

        Hand.Add (c);

        c.ApplyAllModifiersToCard (this, true);

    }

    public static void UpdateHighlightedCards (bool show) {

        List<SC_BaseCard> cards = new List<SC_BaseCard> (localPlayer.Hand);

        foreach (Transform t in UI.basicsPanel.transform)
            if (t.gameObject.activeSelf)
                cards.Add (t.GetComponentInChildren<SC_BaseCard> ());

        foreach (SC_BaseCard c in cards) {

            if (!show)
                c.UICard.SetHighlight (false);
            else if (c.CanUse (localPlayer, true))
                c.UICard.SetHighlight (true);

        }

    }

    public static void BecomeActive () {

        UI.ShowBasics ();

        UpdateHighlightedCards (true);

    } 

    public delegate bool CardOfType (SC_BaseCard c);

    public bool HasOnePlayableCardInHand (bool ignorePrio, CardOfType ca = null) {

        foreach (SC_BaseCard c in Hand)
            if ((ca == null || ca (c)) && c.CanUse (this, ignorePrio))
                return true;

        return false;

    }

    public void StartChoosingCard (ChoosingCard t) {

        ChoosingCard = t;

        UI.ShowMessage (t.ToString ());

    }
    #endregion

    #region Wait for players
    bool waiting = true;

    public static IEnumerator WaitForPlayers () {

        localPlayer.FinishWaitingServerRpc ();

        while (localPlayer.waiting || otherPlayer.waiting)
            yield return new WaitForEndOfFrame ();

        localPlayer.waiting = otherPlayer.waiting = true;

    }

    [ServerRpc]
    void FinishWaitingServerRpc () {

        FinishWaitingClientRpc ();

    }

    [ClientRpc]
    void FinishWaitingClientRpc () {

        waiting = false;

    }
    #endregion

}
