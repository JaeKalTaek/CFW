using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static SC_Global;
using static SC_Player;

namespace Card {

    public class SC_BaseCard : MonoBehaviour {

        protected static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

        protected static SC_GameManager GM { get { return SC_GameManager.Instance; } }

        public SC_UI_Card UICard { get; set; }

        [Header("Base Card Variables")]
        [Tooltip("The minimum Match Heat required to play this card")]
        public int matchHeat;

        [Tooltip("Types of this card")]
        public CardType[] types;

        [Tooltip ("Common requirements of this card")]
        public CommonRequirement[] commonRequirements;

        [Tooltip ("Common effects of this card")]     
        public CommonEffect[] commonEffects;        

        public enum CommonEffectType { Assess, MatchHeatEffect, SingleValueEffect, BodyPartEffect, Tire, Break, Rest, Draw, Count, AlignmentChoice, DoubleTap, Lock, Exchange }

        public enum ValueName { None, Health, Stamina, Alignment }

        public SC_Player Caller { get; set; }

        public SC_Player Receiver { get; set; }

        public static SC_BaseCard activeCard, lockingCard;

        [Serializable]
        public struct CommonEffect {

            public bool effectOnOpponent;

            public CommonEffectType effectType;

            public ValueName valueName;

            public int effectValue;
        
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

        protected virtual void Awake () {

            UICard = transform.parent.GetComponent<SC_UI_Card> ();

        }

        #region Can use
        public virtual bool CanUse (SC_Player user) {

            if (user.Turn && GM.MatchHeat >= matchHeat && (!Is (CardType.Special) || !user.SpecialUsed) && (NoLock || Is (CardType.Basic) || Has (CommonEffectType.Break))) {

                foreach (CommonRequirement c in commonRequirements)
                    if (!Test (c, user))
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

        public IEnumerator StartPlaying () {

            activeCard = this;

            yield return StartCoroutine (MakeChoices ());

            localPlayer.PlayCardServerRpc (UICard.name);

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

            Caller = c;

            Receiver = c.IsLocalPlayer ? otherPlayer : localPlayer;       

            Caller.Hand.Remove (this);

            localPlayer.Busy = true;

            UICard.RecT.SetParent (UICard.RecT.parent.parent);

            SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

            UICard.RecT.pivot = Vector2.one * .5f;

            UICard.RecT.anchoredPosition3D = Vector3.up * (Caller.IsLocalPlayer ? UICard.RecT.sizeDelta.y / 2 : (GM.transform as RectTransform).sizeDelta.y - UICard.RecT.sizeDelta.y / 2);

            UICard.RecT.DOLocalMove (Vector3.zero, 1);

            DOTween.Sequence ().Append (UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * GM.playedSizeMultiplicator, 1)).OnComplete (() => { StartCoroutine (Use ()); });

            UICard.Flip (!Caller.IsLocalPlayer, 1);

        }        

        IEnumerator Use () {

            yield return new WaitForSeconds (GM.responseTime);

            yield return StartCoroutine (ApplyEffects ());

            if (GM.Count == 3) {

                UI.ShowEndPanel (!Caller.IsLocalPlayer);

            } else if (this != lockingCard) {

                UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.playedSizeMultiplicator, 1);

                UICard.ToGraveyard (1, FinishedUsing, false);

            } else
                NextTurn ();

        }

        void FinishedUsing () {

            foreach (CommonEffect c in commonEffects) {

                CurrentEffect = c;

                MethodInfo mi = typeof (SC_BaseCard).GetMethod (c.effectType.ToString () + "Finished");

                if (mi != null) {

                    mi.Invoke (this, null);

                    return;

                }                

            }

            BaseFinishedUsing ();

        }

        void BaseFinishedUsing () {

            localPlayer.Busy = false;

            if (Is (CardType.Basic)) {

                if (!Has (CommonEffectType.Break))
                    NextTurn ();
                else if (localPlayer.Turn)
                    UI.showBasicsButton.SetActive (true);

            } else if (Caller.IsLocalPlayer) {

                if (otherPlayer.Stamina < 3 && this as SC_OffensiveMove)
                    UI.pinfallPanel.SetActive (true);
                else if (Is (CardType.Special)) {

                    Caller.SpecialUsed = true;

                    UI.showBasicsButton.SetActive (true);

                } else
                    NextTurn ();

            }

        }
        #endregion

        protected void NextTurn () {

            (Caller.IsLocalPlayer ? Caller : null)?.NextTurnServerRpc ();

        }

        #region Applying Effects
        public CommonEffect CurrentEffect { get; set; }

        public bool ApplyingEffects { get; set; }

        public virtual IEnumerator ApplyEffects () {

            foreach (CommonEffect e in commonEffects) {

                CurrentEffect = e;

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
        #endregion

        #region Common Effects
        #region Assess
        public void Assess () {

            ApplyingEffects = true;

            StartCoroutine (AssessCoroutine ());

        }

        IEnumerator AssessCoroutine () {

            if (Caller.Deck.cards.Count <= 0)
                yield return Caller.Deck.Refill ();

            Caller.ActionOnCard (Caller.GetStringChoice ("Assess"), (c) => {

                Caller.Hand.Remove (c);

                c.Caller = Caller;

                c.UICard.ToGraveyard (GM.drawSpeed, AppliedEffects);

            });

            yield return StartCoroutine (Caller.Deck.Draw (false));

        }

        public void AssessChoice () {

            localPlayer.ChoosingCard = ChoosingCard.Assessing;

            UI.ShowMessage ("Assess");

        }
        #endregion

        #region Value effects
        public void MatchHeatEffect () {

            GM.AddMatchHeat (CurrentEffect.effectValue);

        }

        public void SingleValueEffect () {

            (CurrentEffect.effectOnOpponent ? Receiver : Caller).ApplySingleEffect (CurrentEffect.valueName.ToString (), CurrentEffect.effectValue);

        }

        public void BodyPartEffect () {

            (CurrentEffect.effectOnOpponent ? Receiver : Caller).ApplySingleBodyEffect ((BodyPart) Caller.GetIntChoice ("BodyPart"), CurrentEffect.effectValue);

        } 

        public void BodyPartEffectChoice () {

            foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                t.gameObject.SetActive (true);

            UI.ShowBodyPartPanel (CurrentEffect.effectValue > 0);

        }
        #endregion

        #region Simple Effects
        public void Tire () {

            (CurrentEffect.effectOnOpponent ? Receiver : Caller).ApplySingleEffect ("Stamina", -GM.baseStamina);

        }

        public void Break () {

            ApplyingEffects = true;

            lockingCard.UICard.ToGraveyard (1, () => {

                if (lockingCard.Is (CardType.Basic))
                    Destroy (lockingCard.UICard.gameObject);

                if (!localPlayer.Unlocked)
                    localPlayer.locked.Value = Locked.Unlocked;

                ApplyingEffects = false;

            }, false);          

        }

        public void Lock () {

            ApplyingEffects = true;

            UICard.RecT.SetAsFirstSibling ();

            lockingCard = this;

            GM.Count = 0;

            if (localPlayer != Caller)
                localPlayer.locked.Value = Is (CardType.Submission) ? Locked.Submission : Locked.Pinned;

            Vector3 oldPos = UICard.RecT.position;

            UICard.RecT.anchorMin = UICard.RecT.anchorMax = UICard.BigRec.anchorMin = UICard.BigRec.anchorMax = UICard.BigRec.pivot = Vector2.one * .5f;

            UICard.RecT.position = oldPos;

            UICard.BigRec.anchoredPosition = Vector2.up * -UICard.RecT.sizeDelta.y / (2 * GM.playedSizeMultiplicator);

            UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.playedSizeMultiplicator, 1);

            UICard.RecT.DOAnchorPosY (UICard.RecT.sizeDelta.y * .75f / GM.playedSizeMultiplicator, 1).onComplete = AppliedEffects;

        }

        public void Rest () {

            Caller.ApplySingleEffect ("Stamina", 1);

            Caller.ApplySingleEffect ("Health", 1);

        }

        public void Count () {

            GM.Count++;

        }
        #endregion

        #region Draw
        public void Draw () {

            StartCoroutine (Draw (CurrentEffect.effectOnOpponent ? Receiver : Caller));

        }

        protected IEnumerator Draw (SC_Player p) {

            ApplyingEffects = true;

            yield return StartCoroutine (p.Deck.Draw (false));

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
        public void DoubleTap () {

            activeCard = this;

        }

        public void DoubleTapEffect () {

            localPlayer.Busy = true;

            Caller.ActionOnCard (Caller.GetStringChoice ("DoubleTapping"), (c) => {

                c.Discard (Caller, () => {

                    Caller.ActionOnCard (Caller.GetStringChoice ("DoubleTapping2"), (ca) => {

                        ca.Discard (Caller, () => {

                            Caller.CopyAndStartUsing (UICard);
                                

                        });

                    });

                });

            });
            
        }

        public void DoubleTapFinished () {

            if (Ephemeral) {

                BaseFinishedUsing ();

            } else {

                if (Caller.Hand.Count >= 2 && CanUse (Caller)) {

                    localPlayer.Busy = false;

                    if (Caller.IsLocalPlayer) {

                        localPlayer.Turn = false;

                        Caller.StringChoices["DoubleTapping"] = "";

                        UI.DoubleTapUI.SetActive (true);

                    }

                } else
                    BaseFinishedUsing ();

            }

        }        
        #endregion

        #region Exchange
        public static SC_BaseCard exchangedCard;

        public void Exchange () {

            Receiver.StringChoices["Exchange"] = "";            

            Receiver.Turn = true;

            if (CanUse (Receiver)) {

                ApplyingEffects = true;

                if (Receiver.IsLocalPlayer)
                    UI.ExchangeUI.SetActive (true);                

                StartCoroutine (ExchangeCoroutine ());

            }

            Receiver.Turn = false;

        }

        IEnumerator ExchangeCoroutine () {

            while (Receiver.GetStringChoice ("Exchange") == "")
                yield return new WaitForEndOfFrame ();

            ApplyingEffects = false;

        }

        public void ExchangeFinished () {

            if (Receiver.GetStringChoice ("Exchange") == "Accept") {

                exchangedCard = exchangedCard ?? this;

                if (Receiver.IsLocalPlayer)
                    Receiver.ExchangeServerRpc ();

            } else if (exchangedCard) {                

                exchangedCard.BaseFinishedUsing ();

                exchangedCard = null;

            } else
                BaseFinishedUsing ();

        }        
        #endregion

        public void AppliedEffects () {

            ApplyingEffects = false;

        }
        #endregion

        public void Discard (SC_Player owner, Action a = null) {

            Caller = owner;

            Caller.Hand.Remove (this);

            SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);            

            UICard.ToGraveyard (GM.drawSpeed, a ?? AppliedEffects);

        }

        public void ZoomEffect (Action effect, Action afterEffect, float zoom) {

            UICard.RecT.SetAsLastSibling ();

            UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * zoom, .5f).OnComplete (() => {

                effect ();

                UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / zoom, .5f).OnComplete (() => {

                    UICard.RecT.SetAsFirstSibling ();

                    afterEffect ();

                });

            });

        }

        #region Getters
        public bool Is (CardType t) {

            foreach (CardType c in types)
                if (c == t)
                    return true;

            return false;

        }

        public bool Has (CommonEffectType ce) {

            foreach (CommonEffect c in commonEffects)
                if (c.effectType == ce)
                    return true;

            return false;

        }

        public bool Ephemeral { get; set; }

        public bool IsEphemeral () {

            return Ephemeral || Is (CardType.Basic);

        }
        #endregion

    }

}
