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
        public List<CommonEffect> commonEffects;        

        public enum CommonEffectType { Assess, MatchHeatEffect, SingleValueEffect,
            BodyPartEffect, Tire, Break, Rest, Draw, Count, AlignmentChoice, DoubleTap,
            Lock, Exchange, Chain, DiscardRandom, DiscardChosen, Refill, StartPin }

        public enum ValueName { None, Health, Stamina, Alignment }

        public SC_Player Caller { get; set; }

        public SC_Player Receiver { get; set; }

        public static SC_BaseCard activeCard, lockingCard, originalCard, boostingCard;

        [Serializable]
        public struct CommonEffect {

            public bool effectOnOpponent, may;

            public CommonEffectType effectType;

            public ValueName valueName;

            public int effectValue;

            public CommonEffect (CommonEffectType t, bool m = false, int v = 0) {

                effectOnOpponent = true;

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

        protected virtual void Awake () {

            UICard = transform.parent.GetComponent<SC_UI_Card> ();

        }

        public delegate void OnCardHovered (SC_BaseCard c, bool on);
        
        public static event OnCardHovered OnCardHoveredEvent;

        public void CardHovered (bool on) {

            OnCardHoveredEvent?.Invoke (this, on);

        }

        #region Can use
        public virtual bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            bool prio = ignorePriority || (user.Turn && !user.Busy);

            bool locked = ignoreLocks || NoLock || Is (CardType.Basic) || Has (CommonEffectType.Break);

            if (GM.MatchHeat >= matchHeat && (!Is (CardType.Special) || !user.SpecialUsed) && prio && locked) {

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

            localPlayer.PlayCardServerRpc (UICard.transform.GetSiblingIndex () /*UICard.name*/);

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

            activeCard = this;

            originalCard = Ephemeral ? originalCard : this;

            Caller = c;

            Receiver = c.IsLocalPlayer ? otherPlayer : localPlayer;       

            Caller.Hand.Remove (this);

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

            boostingCard?.ApplyBoosts ();

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

                if (Receiver.Stamina < 3 && this as SC_OffensiveMove)
                    UI.pinfallPanel.SetActive (true);
                else if (Is (CardType.Special)) {

                    Caller.SpecialUsed = true;

                    UI.BasicsButton.SetActive (true);

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

        #region May
        void May (Action a) {

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
                        StartAssessChoice ();

                });

            }

        }

        void StartAssessChoice () {

            localPlayer.ChoosingCard = ChoosingCard.Assessing;

            UI.ShowMessage ("Assess");

        }

        IEnumerator AssessCoroutine () {

            effectTarget.IntChoices["Assess"] = -1;

            while (effectTarget.GetIntChoice ("Assess") == -1)
                yield return new WaitForEndOfFrame ();

            if (effectTarget.Deck.cards.Count <= 0)
                yield return effectTarget.Deck.Refill ();

            StartCoroutine (effectTarget.Deck.Draw (false));

            effectTarget.ActionOnCard (effectTarget.GetIntChoice ("Assess"), (c) => {

                effectTarget.Hand.Remove (c);

                c.Caller = effectTarget;

                c.UICard.ToGraveyard (GM.drawSpeed, AppliedEffects);

            });                     

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

                    if (lockingCard.Is (CardType.Basic))
                        Destroy (lockingCard.UICard.gameObject);
                    else
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

            GM.Count = 0;

            UI.count.gameObject.SetActive (true);

            if (Receiver.IsLocalPlayer)
                Receiver.locked.Value = Is (CardType.Submission) ? Locked.Submission : Locked.Pinned;

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

        public void StartPinFinished () {

            if (Caller.IsLocalPlayer && Receiver.Stamina < 3)
                Caller.StartUsingBasicServerRpc (3);

        }
        #endregion

        #region Draw
        public void Draw () {

            May (() => {

                StartCoroutine (Draw (effectTarget, CurrentEffect.effectValue));

            });

        }

        protected IEnumerator Draw (SC_Player p, int d = 1) {

            yield return StartCoroutine (p.Deck.Draw (Mathf.Max (1, d), false));

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

                BaseFinishedUsing ();

            } else {

                if (Caller.Hand.Count >= 2 && CanUse (Caller, ignorePriority: true)) {

                    if (Caller.IsLocalPlayer) {                  

                        UI.ShowBooleanChoiceUI ("Double Tap", "Skip", (b) => {

                            if (b)
                                Caller.StartDoubleDiscard (Caller.CopyAndStartUsingServerRpc);
                            else
                                Caller.NextTurnServerRpc ();

                        });

                    }

                } else
                    BaseFinishedUsing ();

            }

        }        
        #endregion

        #region Exchange
        public void Exchange () {

            Receiver.IntChoices["Exchange"] = -1;            

            if (CanUse (Receiver, ignorePriority: true)) {

                ApplyingEffects = true;

                if (Receiver.IsLocalPlayer)
                    UI.ShowBooleanChoiceUI ("Accept Exchange", "Refuse Exchange", (b) => { localPlayer.SetIntChoiceServerRpc ("Exchange", b ? 0 : 1 /*"Accept" : "Refuse"*/); });               

                StartCoroutine (ExchangeCoroutine ());

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
                originalCard.BaseFinishedUsing ();

        }
        #endregion

        #region Chain
        public int MaxChain { get; set; }

        public virtual void Chain () {

            if (!Ephemeral && CanUse (Caller, ignorePriority: true)) {

                ApplyingEffects = true;

                Caller.IntChoices["NumberChoice"] = -1;

                if (Caller.IsLocalPlayer) {

                    MaxChain = 1;

                    while (MaxChain < CurrentEffect.effectValue && (this as SC_OffensiveMove).CanUse (Caller, MaxChain + 1, true))
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

                BaseFinishedUsing ();

            } else if (Caller.IsLocalPlayer) {

                Caller.IntChoices["NumberChoice"]--;

                Caller.CopyAndStartUsingServerRpc ();

            }

        }
        #endregion

        #region Discard
        void Discard (Action a) {

            if (effectTarget.Hand.Count > 0) {

                ApplyingEffects = true;

                if (effectTarget.IsLocalPlayer)
                    a ();

            }

        }

        public void DiscardRandom () {            

            Discard (() => { effectTarget.DiscardServerRpc (UnityEngine.Random.Range (0, effectTarget.Hand.Count)); });

        }

        public void DiscardChosen () {

            Discard (() => {

                effectTarget.ChoosingCard = ChoosingCard.Discarding;

                UI.ShowMessage ("Discard");

            });

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

        public virtual void ApplyBoosts () {

            boostingCard = null;

        }

        public void AppliedEffects () {

            activeCard.ApplyingEffects = false;

        }
        #endregion

        public void Discard (SC_Player owner, Action a = null) {

            Caller = owner;

            Caller.Hand.Remove (this);

            SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);            

            UICard.ToGraveyard (GM.drawSpeed, a ?? AppliedEffects);

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
        #endregion

    }

}
