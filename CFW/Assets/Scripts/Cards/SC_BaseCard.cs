using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SC_Global;
using static SC_Player;
using static SC_UI_Manager;

namespace Card {

    public class SC_BaseCard : MonoBehaviour {

        #region Variables, Properties, Structures
        protected static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

        protected static SC_GameManager GM { get { return SC_GameManager.Instance; } }

        public SC_UI_Card UICard { get; set; }

        [Header("Base Card Variables")]
        public int matchHeat;

        public CardType[] types;

        public CommonRequirement[] commonRequirements;

        public bool unblockable, stayOnRing;

        public int RingSlot { get; set; }
        public bool OnTheRing { get { return RingSlot != -1; } }

        int counters;
        public int Counters {

            get { return counters; }

            set {

                counters = value;

                UICard.counters.nbr.text = UICard.bigCounters.nbr.text = counters.ToString ();

                UICard.counters.root.SetActive (counters > 0);

                UICard.bigCounters.root.SetActive (counters > 0);

            }

        }

        public List<CommonEffect> commonEffects;        

        public enum CommonEffectType {

            Assess, MatchHeatEffect, SingleValueEffect, BodyPartEffect, Tire, Break, Rest,
            Draw, Count, AlignmentChoice, DoubleTap, Lock, Exchange, Chain, DiscardRandom,
            DiscardChosen, Refill, StartPin, Response, Counter, Boost, Grab, Turn, Modifier,
            OnPlayTrigger, OnNewTurnTrigger, OnNoAttackTurnTrigger, OnOffensiveMoveDamageTrigger,
            OnFinishedPlayingTrigger

        }

        public enum ValueName { None, Health, Stamina, Alignment }

        public SC_Player Caller { get; set; }

        public SC_Player Receiver { get; set; }

        protected static bool responding, boosting;

        public bool Stolen { get; set; }

        public static bool interceptNext;

        public static SC_BaseCard activeCard, lockingCard, originalCard;

        protected SC_BaseCard interceptFinishCard;

        public static List<SC_BaseCard> modifierCards;

        public static Stack<SC_BaseCard> respondedCards;

        [Serializable]
        public struct CommonEffect {

            public bool effectOnOpponent, may;

            public CommonEffectType effectType;

            public ValueName valueName;

            public int effectValue;

            public CommonEffect (CommonEffectType t, bool m = false, int v = 0, bool o = false) {

                effectOnOpponent = o;

                may = m;

                effectType = t;

                valueName = ValueName.None;

                effectValue = v;

            }

        }

        public enum RequirementType { Minimum, Maximum }

        [Serializable]
        public struct CommonRequirement {

            public bool opponent;

            public ValueName valueType;

            public RequirementType requirementType;

            public int requirementValue;

        }

        public string Path {

            get {

                string s = types[0].ToString () + (types.Length == 1 ? "s" : "");

                for (int i = 1; i < types.Length; i++)
                    s += " " + types[i].ToString () + (types.Length == i + 1 ? "s" : "");

                return s + "/" + name.Replace("(Clone)", "");                

            }

        }
        #endregion

        void OnValidate () {

            if (commonEffects != null) {

                if ((IsResponse || Has (CommonEffectType.Boost)) && !GetComponent<SC_CardMatcher> ())
                    gameObject.AddComponent<SC_CardMatcher> ();

                if (Has (CommonEffectType.Grab) && !GetComponent<SC_CardGrabber> ())
                    gameObject.AddComponent<SC_CardGrabber> ();

            }

            if (types != null && Is (CardType.Mytho))
                stayOnRing = true;

        }

        protected virtual void Awake () {

            RingSlot = -1;

            UICard = transform.parent.GetComponent<SC_UI_Card> ();

        }        

        #region Can use      
        public virtual bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            //DebugWithTime (Path);

            bool prio = ignorePriority || (user.Turn && !activeCard) || responding || boosting;

            bool locked = ignoreLocks || NoLock || Is (CardType.Basic) || Has (CommonEffectType.Break);

            //DebugWithTime ((GM.MatchHeat >= matchHeat) + ", " + (!Is (CardType.Special) || !user.SpecialUsed) + ", " + prio + ", " + locked);

            if (GM.MatchHeat >= matchHeat && (!IsSpecial || !user.SpecialUsed) && !OnTheRing && prio && locked) {

                foreach (CommonRequirement c in commonRequirements)
                    if (!Test (c, user))
                        return false;

                //DebugWithTime ("COMMON REQUIREMENTS OK");

                if (stayOnRing) {

                    foreach (RingSlot r in user.IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots)
                        if (!r.occupied)
                            goto OpenSlot;

                    return false;

                }
                OpenSlot:

                if (responding) {

                    if (IsResponse) {

                        if (!GetComponent<SC_CardMatcher> ().Matching (activeCard))
                            return false;

                    } else
                        return false;

                } else if (Is (CardType.Special) && IsResponse) {

                    return false;

                } else if (boosting) {

                    if (Has (CommonEffectType.Boost)) {

                        //DebugWithTime ("BOOST MATCHING: " + GetComponent<SC_CardMatcher> ().Matching (activeCard));

                        if (!GetComponent<SC_CardMatcher> ().Matching (activeCard))
                            return false;

                    } else
                        return false;

                } else if (Has (CommonEffectType.Boost))
                    return false;

                return true;

            } else
                return false;

        }

        bool Test (CommonRequirement c, SC_Player user) {

            int value = (int) typeof (SC_Player).GetProperty (c.valueType.ToString ()).GetValue ((c.opponent && user.IsLocalPlayer) || (!c.opponent && !user.IsLocalPlayer) ? otherPlayer : localPlayer);

            return c.requirementType == RequirementType.Minimum ? value > c.requirementValue : value < c.requirementValue;

        }
        #endregion     

        #region Start using & making choices
        public bool MakingChoices { get; set; }

        public List<Action> ChoicesActions { get; set; }

        public IEnumerator StartPlaying (bool resume = false) {

            if (responding || boosting) {

                UI.messagePanel.SetActive (false);                

                respondedCards.Push (activeCard);

                if (responding)
                    localPlayer.StartResponseServerRpc ();
                else {

                    respondedCards.Peek ().StopAllCoroutines ();

                    localPlayer.StartBoostServerRpc (activeCard.UICard.transform.GetSiblingIndex ());

                }

                responding = boosting = false;

            } else if (IsSpecial)
                localPlayer.SpecialUsed = true;

            TryInterceptFinish ();

            activeCard = this;

            boosting = true;

            if (localPlayer.Turn && !localPlayer.FirstAttackPlayed && !resume && !IsSpecial && !interceptFinishCard && localPlayer.HasOnePlayableCardInHand (false)) {

                UI.ShowMessage ("Boost");

                yield return new WaitForSeconds (GM.responseTime);

                UI.messagePanel.SetActive (false);

                boosting = false;

            } else
                boosting = false;

            if (this as SC_AttackCard)
                localPlayer.FirstAttackPlayed = true;

            yield return StartCoroutine (MakeChoices ());

            localPlayer.PlayCardServerRpc (UICard.transform.GetSiblingIndex (), OnTheRing);

        }

        protected virtual IEnumerator MakeChoices () {

            foreach (CommonEffect c in commonEffects) {

                CurrentEffect = c;

                MethodInfo mi = typeof (SC_BaseCard).GetMethod (c.effectType.ToString () + "Choice");

                if (mi != null)
                    yield return StartCoroutine (MakeChoice (() => { mi.Invoke (this, null); }));                

            }

        }

        protected IEnumerator MakeChoice (Action a) {

            MakingChoices = true;

            a ();

            while (MakingChoices)
                yield return new WaitForEndOfFrame ();

        }
        #endregion

        #region Usage
        public virtual void Play (SC_Player c) {

            if (OnTheRing && RingPlay ())
                return;

            UI.messagePanel.SetActive (false);

            TryInterceptFinish ();

            activeCard = this;

            if (!originalCard)
                originalCard = this;

            Caller = c;

            Receiver = Caller.IsLocalPlayer ? otherPlayer : localPlayer;

            UICard.RecT.SetParent (UICard.RecT.parent.parent);

            if (!OnTheRing) {

                Caller.Hand.Remove (this);

                SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

                UICard.RecT.pivot = Vector2.one * .5f;

                UICard.RecT.anchoredPosition3D = Vector3.up * (Caller.IsLocalPlayer ? UICard.RecT.sizeDelta.y / 2 : (GM.transform as RectTransform).sizeDelta.y - UICard.RecT.sizeDelta.y / 2);

            } else {

                (Caller.IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots)[RingSlot].occupied = false;

                RingSlot = -1;

            }

            UICard.RecT.DOLocalMove (Vector3.zero, 1);

            DOTween.Sequence ().Append (UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * GM.playedSizeMultiplicator, 1)).OnComplete (() => { StartCoroutine (Use ()); });

            UICard.Flip (!UICard.FaceUp, 1);

        }        

        protected virtual bool RingPlay () {

            return false;

        }

        IEnumerator Use (bool resumed = false) {

            OnPlay?.Invoke ();

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            if (!resumed) {

                foreach (SC_BaseCard c in new List<SC_BaseCard> (modifierCards))
                    c.ApplyModifiers ();

                yield return new WaitForSeconds (GM.playedDelay);

                if (!unblockable && Receiver.Unlocked && (!Is (CardType.Basic) || Has (CommonEffectType.Lock))) {

                    if (Receiver.IsLocalPlayer) {                        

                        responding = true;

                        UI.ShowMessage (Receiver.HasOnePlayableCardInHand (false) ? "ResponseCan" : "ResponseCant");

                    }

                    yield return new WaitForSeconds (GM.responseTime);

                    responding = false;

                    UI.messagePanel.SetActive (false);

                }

            }            

            yield return StartCoroutine (ApplyEffects ());

            if (GM.Count == 3) {

                UI.ShowEndPanel (!Caller.IsLocalPlayer);

            } else if (this != lockingCard) {

                UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.playedSizeMultiplicator, 1);

                if (stayOnRing)
                    UICard.ToRingSlot ();
                else
                    UICard.ToGraveyard (1, () => { FinishedUsing (); }, false);

            } else {

                activeCard = null;

                NextTurn ();

            }

        }

        List<CommonEffect> finishedEffects;

        public virtual void FinishedUsing (bool countered = false) {

            if (!countered) {

                if (finishedEffects == null)
                    finishedEffects = new List<CommonEffect> ();

                if (interceptFinishCard) {

                    interceptFinishCard.InterceptFinish ();

                    return;

                }

                foreach (CommonEffect c in commonEffects) {

                    if (!finishedEffects.Contains (c)) {

                        CurrentEffect = c;

                        MethodInfo mi = typeof (SC_BaseCard).GetMethod (c.effectType.ToString () + "Finished");

                        if (mi != null) {

                            finishedEffects.Add (c);

                            mi.Invoke (this, null);

                            return;

                        }

                    }

                }

            }

            if (OnFinishedPlaying != null && OnFinishedPlaying.GetInvocationList ().Length > 0) {

                OnFinishedPlaying.Invoke ();

                return;

            }

            BaseFinishedUsing ();

        }

        public virtual void BaseFinishedUsing (bool countered = false) {

            activeCard = originalCard = null;

            if (Is (CardType.Basic)) {

                if (!Has (CommonEffectType.Break))
                    NextTurn (true);
                else if (localPlayer.Turn)
                    UI.showBasicsButton.SetActive (true);

            } else if (Caller.IsLocalPlayer) {

                if (!countered && Receiver.Stamina < 3 && this as SC_OffensiveMove)
                    StartPinfallChoice ();
                else if (IsSpecial)
                    UI.showBasicsButton.SetActive (true);
                else
                    NextTurn ();

            }

        }

        protected virtual void InterceptFinish () { }
        #endregion

        protected void NextTurn (bool noAttack = false) {

            (Caller.IsLocalPlayer ? Caller : null)?.NextTurnServerRpc (noAttack);

        }

        #region Applying Effects
        public CommonEffect CurrentEffect { get; set; }

        public bool ApplyingEffects { get; set; }

        protected SC_Player effectTarget;

        protected virtual IEnumerator ApplyEffects () {

            foreach (CommonEffect e in commonEffects) {

                CurrentEffect = e;

                effectTarget = CurrentEffect.effectOnOpponent ? Receiver : Caller;

                MethodInfo mi = typeof (SC_BaseCard).GetMethod (e.effectType.ToString ());

                if (mi != null)
                    yield return StartCoroutine (ApplyEffect (() => { mi.Invoke (this, null); }));

            }            

        }

        protected IEnumerator ApplyEffect (Action a) {

            a ();

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

        }

        public void AppliedEffects () {

            activeCard.ApplyingEffects = false;

        }

        public void SetCurrentEffect (CommonEffect e) {

            CurrentEffect = e;

            effectTarget = e.effectOnOpponent ? Receiver : Caller;

        }

        #region May
        protected void May (Action a) {

            ApplyingEffects = true;            

            if (CurrentEffect.may) {

                effectTarget.IntChoices["May"] = -1;

                StartCoroutine (MayCoroutine (a));

                if (effectTarget.IsLocalPlayer) {

                    UI.ShowBooleanChoiceUI (CurrentEffect.effectType.ToString (), "Skip", (b) => {

                        if (b)
                            effectTarget.SetIntChoiceServerRpc ("May", 0);
                        else
                            effectTarget.FinishedApplyingEffectsServerRpc ();

                    });

                }

            } else
                a ();

        }

        IEnumerator MayCoroutine (Action a) {

            while (ApplyingEffects && effectTarget.GetIntChoice ("May") == -1)
                yield return new WaitForEndOfFrame ();

            if (ApplyingEffects)
                a ();

        }
        #endregion
        #endregion

        #region Common Effects
        #region Assess
        public void Assess () {

            if (effectTarget.Deck.cards.Count > 0 || effectTarget.Graveyard.Cards.Count > 0) {

                May (() => {

                    StartCoroutine (AssessCoroutine ());

                    if (effectTarget.IsLocalPlayer)
                        effectTarget.StartChoosingCard (ChoosingCard.Assess);

                });

            }

        }

        IEnumerator AssessCoroutine () {

            effectTarget.IntChoices["Assess"] = -1;

            while (effectTarget.GetIntChoice ("Assess") == -1)
                yield return new WaitForEndOfFrame ();

            if (effectTarget.Deck.cards.Count <= 0)
                yield return effectTarget.Deck.Refill ();            

            effectTarget.ActionOnCard (effectTarget.GetIntChoice ("Assess"), (c) => {

                effectTarget.Hand.Remove (c);

                c.Caller = effectTarget;

                c.UICard.ToGraveyard (GM.drawSpeed, AppliedEffects);

            });

            StartCoroutine (effectTarget.Deck.Draw ());

        }
        #endregion

        #region Value effects
        public void MatchHeatEffect () {

            GM.AddMatchHeat (CurrentEffect.effectValue);

        }

        public void SingleValueEffect () {

            effectTarget.ApplySingleEffect (CurrentEffect.valueName.ToString (), CurrentEffect.effectValue);

        }

        public void BodyPartEffect () {

            effectTarget.ApplySingleBodyEffect ((BodyPart) Caller.GetIntChoice ("BodyPart"), CurrentEffect.effectValue);

        } 

        public void BodyPartEffectChoice () {

            foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                t.gameObject.SetActive (true);

            UI.ShowBodyPartPanel (CurrentEffect.effectValue > 0);

        }
        #endregion

        #region Simple Effects
        public void Tire () {

            effectTarget.ApplySingleEffect ("Stamina", -GM.baseStamina);

        }

        public void Break () {

            if (lockingCard) {

                ApplyingEffects = true;

                lockingCard.UICard.ToGraveyard (1, () => {

                    lockingCard.Broken ();

                    if (!localPlayer.Unlocked)
                        localPlayer.locked.Value = Locked.Unlocked;

                    lockingCard = null;

                    ApplyingEffects = false;

                }, false);

            }

        }

        public virtual void Broken () {

            UI.count.gameObject.SetActive (false);

        }

        public void Lock () {

            ApplyingEffects = true;

            UICard.RecT.SetAsFirstSibling ();

            lockingCard = this;

            GM.Count = Receiver.Health <= 0 ? 1 : 0;

            UI.count.gameObject.SetActive (true);

            if (Receiver.IsLocalPlayer)
                Receiver.locked.Value = Is (CardType.Submission) ? Locked.Submission : Locked.Pinned;

            UICard.BigRec.anchorMin = UICard.BigRec.anchorMax = UICard.BigRec.pivot = Vector2.one * .5f;

            UICard.BigRec.anchoredPosition = Vector2.zero;

            UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.playedSizeMultiplicator, 1).onComplete = AppliedEffects;

        }

        public void Rest () {

            Caller.ApplySingleEffect ("Stamina", 1);

            Caller.ApplySingleEffect ("Health", 1);

        }

        public void Count () {

            GM.Count++;

        }        

        public void StartPinFinished () {

            if (Caller.IsLocalPlayer && Receiver.Stamina < 3)
                Caller.StartUsingBasicServerRpc (3);

        }

        public void Turn () {

            effectTarget.Alignment *= -1;

        }

        public void Modifier () {

            modifierCards.Add (this);

        }
        #endregion

        #region Triggered effects
        //On Play
        public static event Action OnPlay;

        public void OnPlayTrigger () {

            OnPlay += OnPlayTriggered;

        }

        protected virtual void OnPlayTriggered () { }

        //On Finished Playing
        public static event Action OnFinishedPlaying;

        public void OnFinishedPlayingTrigger () {

            OnFinishedPlaying += OnFinishedPlayingTriggered;

        }

        protected virtual void OnFinishedPlayingTriggered () { }

        //On New Turn
        public void OnNewTurnTrigger () {

            OnNewTurn += OnNewTurnTriggered;

        }

        protected virtual void OnNewTurnTriggered () { }

        //On No Attack Turn   
        public void OnNoAttackTurnTrigger () {

            OnNoAttackTurn += OnNoAttackTurnTriggered;

        }

        protected virtual void OnNoAttackTurnTriggered () { }

        //On Offensive Move Damage
        public void OnOffensiveMoveDamageTrigger () {

            SC_OffensiveMove.OnOffensiveMoveDamage += OnOffensiveMoveDamageTriggered;

        }

        protected virtual void OnOffensiveMoveDamageTriggered () { }
        #endregion

        #region Draw
        public void Draw () {

            May (() => {

                StartCoroutine (Draw (effectTarget, CurrentEffect.effectValue));

            });

        }

        protected IEnumerator Draw (SC_Player p, int d = 1) {

            yield return StartCoroutine (p.Deck.Draw (Mathf.Max (1, d)));

            ApplyingEffects = false;

        }
        #endregion        

        #region Alignment Choice
        public void AlignmentChoice () {

            Caller.ApplySingleEffect ("Alignment", Caller.GetIntChoice ("AlignmentChoice"));

        }

        public void AlignmentChoiceChoice () {

            UI.ShowAlignmentChoice (CurrentEffect.effectValue);

        }
        #endregion

        #region Double Tap
        public void DoubleTapFinished () {

            if (Ephemeral) {

                originalCard.FinishedUsing ();

            } else {

                if (Caller.Hand.Count >= 2 && CanUse (Caller, true)) {

                    if (Caller.IsLocalPlayer) {                  

                        UI.ShowBooleanChoiceUI ("Double Tap", "Skip", (b) => {

                            if (b)
                                Caller.StartDoubleDiscard (Caller.CopyAndStartUsingServerRpc);
                            else
                                NextTurn ();

                        });

                    }

                } else
                    FinishedUsing ();

            }

        }
        #endregion

        #region Exchange
        public static SC_Player forceFirstExchange;

        public void Exchange () {

            Receiver.IntChoices["Exchange"] = -1;            

            if (CanUse (Receiver, true)) {

                if (!Ephemeral && forceFirstExchange == Caller)
                    localPlayer.SetIntChoiceServerRpc ("Exchange", 0);
                else {

                    ApplyingEffects = true;

                    if (Receiver.IsLocalPlayer)
                        UI.ShowBooleanChoiceUI ("Accept Exchange", "Refuse Exchange", (b) => { localPlayer.SetIntChoiceServerRpc ("Exchange", b ? 0 : 1); });

                    StartCoroutine (ExchangeCoroutine ());

                }

            }

        }

        IEnumerator ExchangeCoroutine () {

            while (Receiver.GetIntChoice ("Exchange") == -1)
                yield return new WaitForEndOfFrame ();

            ApplyingEffects = false;

        }

        public void ExchangeFinished () {

            if (Receiver.GetIntChoice ("Exchange") == 0) {

                if (Receiver.IsLocalPlayer)
                    Receiver.CopyAndStartUsingServerRpc ();

            } else
                originalCard.FinishedUsing ();

        }
        #endregion

        #region Chain
        public int MaxChain { get; set; }

        public virtual void Chain () {

            if (!Ephemeral && CanUse (Caller, true)) {

                ApplyingEffects = true;

                Caller.IntChoices["NumberChoice"] = -1;

                if (Caller.IsLocalPlayer) {

                    MaxChain = 1;

                    while (MaxChain < CurrentEffect.effectValue && (this as SC_OffensiveMove).CanPayCost (Caller, MaxChain + 1))
                        MaxChain++;

                    UI.ShowNumberChoiceUI (MaxChain);

                }

                StartCoroutine (WaitChainChoice ());

            } else if (!Ephemeral)
                Caller.IntChoices["NumberChoice"] = 0;

        }

        IEnumerator WaitChainChoice () {

            while (Caller.GetIntChoice ("NumberChoice") == -1)
                yield return new WaitForEndOfFrame ();

            ApplyingEffects = false;

        }

        public void ChainFinished () {

            if (Caller.GetIntChoice ("NumberChoice") == 0) {

                originalCard.FinishedUsing ();

            } else {                 

                Caller.IntChoices["NumberChoice"]--;

                if (Caller.IsLocalPlayer)
                    Caller.CopyAndStartUsingServerRpc ();

            }

        }
        #endregion

        #region Discard
        public static int discardNbr = -1;

        void Discard (Action a) {

            if (discardNbr == 0 || effectTarget.Hand.Count == 0) {

                discardNbr = -1;

                ApplyingEffects = false;

            } else {

                discardNbr = discardNbr == -1 ? Mathf.Max (0, CurrentEffect.effectValue - 1) : discardNbr - 1;

                ApplyingEffects = true;

                if (effectTarget.IsLocalPlayer)
                    a ();

            }

        }

        public void DiscardRandom () {            

            Discard (() => { effectTarget.DiscardServerRpc (UnityEngine.Random.Range (0, effectTarget.Hand.Count)); });

        }

        public void DiscardChosen () {

            Discard (() => { effectTarget.StartChoosingCard (ChoosingCard.Discard); });

        }

        public void Discard (SC_Player owner, Action a = null) {

            Caller = owner;

            Caller.Hand.Remove (this);

            SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

            if (discardNbr != -1) {

                a = () => {

                    if (CurrentEffect.effectType == CommonEffectType.DiscardChosen)
                        activeCard.DiscardChosen ();
                    else
                        activeCard.DiscardRandom ();

                };

            }

            UICard.ToGraveyard (GM.drawSpeed, a ?? AppliedEffects);

        }
        #endregion

        #region Refill
        public void Refill () {

            if (effectTarget.Graveyard.Cards.Count > 0) {

                ApplyingEffects = true;

                StartCoroutine (Refilling ());

            }

        }

        IEnumerator Refilling () {

            yield return StartCoroutine (effectTarget.Deck.Refill ());

            ApplyingEffects = false;

        }
        #endregion

        #region Responses
        #region Response
        public void ResponseFinished () {

            if (respondedCards.Count > 0) {

                activeCard = respondedCards.Pop ();

                activeCard.StartCoroutine (activeCard.Use (true));

            }

        }
        #endregion

        #region Counter
        public void Counter () {

            if (respondedCards.Count > 0) {

                ApplyingEffects = true;

                respondedCards.Peek ().UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.playedSizeMultiplicator, 1);

                respondedCards.Peek ().UICard.ToGraveyard (1, AppliedEffects, false);                

            }

        }

        public virtual void CounterFinished () {

            if (respondedCards.Count == 1) {

                if (respondedCards.Peek ())
                    respondedCards.Peek ().FinishedUsing (true);
                else if (originalCard)
                    originalCard.FinishedUsing (true);
                else if (Receiver.IsLocalPlayer)
                    Receiver.NextTurnServerRpc ();

                respondedCards.Pop ();

            } else if (respondedCards.Count > 1) {

                if (!respondedCards.Peek ().Has (CommonEffectType.Boost)) {

                    respondedCards.Pop ();

                    activeCard = respondedCards.Pop ();

                    activeCard.StartCoroutine (activeCard.Use (true));

                } else
                    respondedCards.Pop ().BoostFinished ();

            } else
                FinishedUsing ();

        }
        #endregion
        #endregion

        #region Boost     
        public virtual void Boost () {

            originalCard = null;

        }

        public void BoostFinished () {

            if (respondedCards.Count > 0) {

                SC_BaseCard c = respondedCards.Pop ();

                if (Caller.IsLocalPlayer)
                    c.StartCoroutine (c.StartPlaying (true));

            } else
                FinishedUsing ();

        }
        #endregion

        #region Grab
        public int GrabsRemaining { get; set; }

        public void Grab () {

            if (!(this as SC_AttackCard))
                GrabPerform ();

        }

        public void GrabPerform () {

            activeCard = this;

            GrabsRemaining = Mathf.Max (CurrentEffect.effectValue, 1);

            ApplyingEffects = true;

            SC_CardGrabber grabber = GetComponent<SC_CardGrabber> ();

            bool[] where = new bool[] { grabber.deck, grabber.discard, grabber.otherDiscard };

            List<SC_BaseCard>[] lists = new List<SC_BaseCard>[] { effectTarget.Deck.cards, effectTarget.Graveyard.Cards, (effectTarget == Caller ? Receiver : Caller).Graveyard.Cards };

            Vector2 size = Resources.Load<RectTransform> ("Prefabs/P_Grab_Card").sizeDelta;

            foreach (Transform t in UI.grabUI.container)
                Destroy (t.gameObject);

            float xMargin = (UI.grabUI.container.rect.width - (size.x * 5)) / 6f;

            float yMargin = xMargin;

            int x = 0;
            int y = 0;

            for (int i = 0; i < 3; i++) {

                 if (where [i]) {

                    foreach (SC_BaseCard c in lists[i]) {

                        if (grabber.Matching (c)) {

                            if (effectTarget.IsLocalPlayer) {

                                SC_GrabCard g = Instantiate (Resources.Load<GameObject> ("Prefabs/P_Grab_Card"), UI.grabUI.container).GetComponentInChildren<SC_GrabCard> ();

                                g.Setup (c, i);

                                (g.transform.parent as RectTransform).anchoredPosition = new Vector2 (x * size.x + (x + 1) * xMargin, -y * size.y - (y + 1) * yMargin);

                            }

                            x = x == 4 ? 0 : x + 1;

                            y = x == 0 ? y + 1 : y;

                        }

                    }

                }

            }

            effectTarget.IntChoices["Grab"] = -1;

            GrabsRemaining = Mathf.Min (GrabsRemaining, y * 5 + x);

            if (GrabsRemaining == 0)
                StartCoroutine (NoGrabbing ());
            else {

                May (() => {

                    StartCoroutine (Grabbing ());

                    if (effectTarget.IsLocalPlayer) {

                        y = Mathf.Max (0, y - 1 - (x == 0 ? 1 : 0));

                        UI.grabUI.container.sizeDelta = new Vector2 (UI.grabUI.container.sizeDelta.x, size.y * (y + 2) + yMargin * (y + 3));

                        SC_GrabCard.selectedCards = new List<SC_BaseCard> ();

                        UI.ShowGrabPanel ();

                    }

                });

            }

        }

        IEnumerator NoGrabbing () {

            SC_GrabCard.canGrab = null;

            if (effectTarget.IsLocalPlayer)
                UI.ShowMessage ("NoGrabbing");

            yield return new WaitForSeconds (.5f);

            UI.messagePanel.SetActive (false);

            ApplyingEffects = false;

        }

        IEnumerator Grabbing () {

            while (effectTarget.GetIntChoice ("Grab") == -1)
                yield return new WaitForEndOfFrame ();

            SC_GrabCard.canGrab = null;

            for (int i = 0; i < Mathf.Max (CurrentEffect.effectValue, 1); i++) {

                SC_CardZone zone = effectTarget.GetIntChoice ("Grab" + i) == 0 ? effectTarget.Deck : ((effectTarget.GetIntChoice ("Grab" + i) == 1 ? effectTarget.Graveyard : (effectTarget == Caller ? Receiver : Caller).Graveyard) as SC_CardZone);

                SC_BaseCard grabbed = zone.GetCards ().Find ((c) => { return c.Path == effectTarget.GetStringChoice ("Grab" + i); });

                yield return StartCoroutine (zone.Grab (effectTarget.IsLocalPlayer, grabbed));

            }            

            ApplyingEffects = false;

        }

        public virtual void GrabFinished () {

            if (this as SC_AttackCard) {

                CommonEffect effect = new CommonEffect ();

                foreach (CommonEffect c in commonEffects)
                    if (c.effectType == CommonEffectType.Grab)
                        effect = c;

                SetCurrentEffect (effect);

                GrabPerform ();

                StartCoroutine (GrabFinishedCoroutine ());

            } else
                FinishedUsing ();

        }

        protected virtual IEnumerator GrabFinishedCoroutine () {

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            FinishedUsing ();

        }
        #endregion
        #endregion

        public void StartPinfallChoice () {

            UI.ShowBooleanChoiceUI ("Start Pinfall", "Skip", (b) => {

                if (b)
                    localPlayer.StartUsingBasicServerRpc (3);
                else
                    NextTurn ();

            });

        }

        public virtual void ApplyModifiers () {

            modifierCards.Remove (this);

        }

        public virtual IEnumerator FirstTurnEffect () {

            yield return null;

        }

        #region Getters
        public bool Is (CardType t) {

            foreach (CardType c in types)
                if (c == t)
                    return true;

            return false;

        }

        public bool IsSpecial { get { return Is (CardType.Special) || Is (CardType.Mytho); } }

        public bool Has (CommonEffectType ce) {

            foreach (CommonEffect c in commonEffects)
                if (c.effectType == ce)
                    return true;

            return false;

        }

        public bool IsResponse { get { return Has (CommonEffectType.Response) || Has (CommonEffectType.Counter); } }

        public bool Ephemeral { get; set; }

        public bool IsAlignmentCard (bool face) {

            foreach (CommonRequirement c in commonRequirements)
                if (c.valueType == ValueName.Alignment && !c.opponent && ((!face && c.requirementType == RequirementType.Maximum && c.requirementValue <= 0) || (face && c.requirementType == RequirementType.Minimum && c.requirementValue >= 0)))
                    return true;

            return false;

        }
        #endregion

        public static SC_UI_Card Create (SC_BaseCard original, Transform parent) {

            SC_UI_Card c = Instantiate (Resources.Load<SC_UI_Card> ("Prefabs/P_UI_Card"), parent);

            c.name = original.GetComponentInChildren<SC_BaseCard> ().Path;

            c.Card = Instantiate (Resources.Load<SC_BaseCard> (c.name), c.transform);

            return c;

        }

        #region Cards hovering
        public delegate void OnCardHovered (SC_BaseCard c, bool on);
        protected OnCardHovered cardHovered;

        public static event OnCardHovered OnCardHoveredEvent;

        public void CardHovered (bool on) {

            OnCardHoveredEvent?.Invoke (this, on);

        }
        #endregion

        #region On Ring Clicked
        public delegate void OnRingClickedDelegate (SC_BaseCard c);

        public static event OnRingClickedDelegate OnRingClickedEvent;

        public virtual void OnRingClicked () {

            OnRingClickedEvent?.Invoke (this);

            OnRingUseCounters ();

        }

        public virtual void OnRingUseCounters (int counters = -1) {

            if (counters != -1 && Counters >= counters && localPlayer == Caller && Caller.Turn && !activeCard) {

                UI.showBasicsButton.SetActive (false);

                localPlayer.PlayCardServerRpc (UICard.transform.GetSiblingIndex (), true);

            }

        }

        protected IEnumerator ClickedEffect () {

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            activeCard = null;

            if (Caller.IsLocalPlayer)
                UI.showBasicsButton.SetActive (true);

        }
        #endregion

        public virtual void DiscardedFromRing () {

            (Caller.IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots)[RingSlot].occupied = false;

            RingSlot = -1;

            Counters = 0;

            if (Has (CommonEffectType.OnPlayTrigger))
                OnPlay -= OnPlayTriggered;

            modifierCards.Remove (this);

            if (Has (CommonEffectType.OnNewTurnTrigger))
                OnNewTurn -= OnNewTurnTriggered;

            if (Has (CommonEffectType.OnNoAttackTurnTrigger))
                OnNoAttackTurn -= OnNoAttackTurnTriggered;

            if (Has (CommonEffectType.OnOffensiveMoveDamageTrigger))
                SC_OffensiveMove.OnOffensiveMoveDamage -= OnOffensiveMoveDamageTriggered;

            if (Has (CommonEffectType.OnFinishedPlayingTrigger))
                OnFinishedPlaying -= OnFinishedPlayingTriggered;

        }

        void TryInterceptFinish () {

            if (interceptNext) {

                interceptNext = false;

                interceptFinishCard = activeCard;

            }

        }

    }

}
