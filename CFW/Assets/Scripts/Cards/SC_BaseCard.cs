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

        public enum CommonEffectType { Assess, MatchHeatEffect, SingleValueEffect, BodyPartEffect, Tire, Break, Rest, Draw, Count, AlignmentChoice, DoubleTap }

        public enum ValueName { None, Health, Stamina, Alignment }

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

        public SC_Player Caller { get; set; }

        public SC_Player Other { get; set; }

        public virtual bool CanUse () {

            if (GM.MatchHeat >= matchHeat && (!Is (CardType.Special) || !localPlayer.SpecialUsed) && (NoLock || Is (CardType.Basic) || Has (CommonEffectType.Break))) {

                foreach (CommonRequirement c in commonRequirements)
                    if (!Test (c))
                        return false;

                return true;

            } else
                return false;

        }

        bool Test (CommonRequirement c) {

            int value = (int) typeof (SC_Player).GetProperty (c.valueType.ToString ()).GetValue (c.opponent ? otherPlayer : localPlayer);

            return c.requirementType == RequirementType.Minimum ? value > c.requirementValue : value < c.requirementValue;

        }

        public static SC_BaseCard activeCard, lockingCard;

        void Awake () {

            UICard = transform.parent.GetComponent<SC_UI_Card> ();

        }

        #region Start using & making choices
        public bool MakingChoices { get; set; }

        public List<Action> ChoicesActions { get; set; }

        public IEnumerator StartUsing () {

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

        public virtual void Play (SC_Player c) {

            Caller = c;

            Other = c.IsLocalPlayer ? otherPlayer : localPlayer;       

            Caller.Hand.Remove (this);

            localPlayer.Busy = true;

            UICard.RecT.SetParent (UICard.RecT.parent.parent);

            SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

            UICard.RecT.pivot = Vector2.one * .5f;

            UICard.RecT.anchoredPosition3D = Vector3.up * (Caller.IsLocalPlayer ? UICard.RecT.sizeDelta.y / 2 : (GM.transform as RectTransform).sizeDelta.y - UICard.RecT.sizeDelta.y / 2);

            UICard.RecT.DOLocalMove (Vector3.zero, 1);

            DOTween.Sequence ().Append (UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * GM.playedSizeMultiplicator, 1)).OnComplete (() => { StartCoroutine (Use ()); });

            UICard.Flip (!Caller.IsLocalPlayer, .5f);

        }        

        IEnumerator Use () {

            activeCard = this;

            yield return new WaitForSeconds (GM.responseTime);

            ApplyEffect ();

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            if (GM.count == 3) {

                UI.ShowEndPanel (!Caller.IsLocalPlayer);

            } else {

                UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.playedSizeMultiplicator, 1);

                if (Is (CardType.Submission) || UICard.name.EndsWith ("Pinfall")) {

                    UICard.RecT.SetAsFirstSibling ();

                    lockingCard = this;

                    GM.count = 0;

                    if (localPlayer != Caller)
                        localPlayer.locked.Value = Is (CardType.Submission) ? Locked.Submission : Locked.Pinned;


                    Vector3 oldPos = UICard.RecT.position;

                    UICard.RecT.anchorMin = UICard.RecT.anchorMax = UICard.BigRec.anchorMin = UICard.BigRec.anchorMax = UICard.BigRec.pivot = Vector2.one * .5f;

                    UICard.RecT.position = oldPos;

                    UICard.BigRec.anchoredPosition = Vector2.up * -UICard.RecT.sizeDelta.y / (2 * GM.playedSizeMultiplicator);

                    UICard.RecT.DOAnchorPosY (UICard.RecT.sizeDelta.y * .75f / GM.playedSizeMultiplicator, 1).onComplete = NextTurn;

                } else {

                    UICard.ToGraveyard (1, () => {

                        localPlayer.Busy = false;

                        if (Is (CardType.Basic)) {

                            Destroy (transform.parent.gameObject);

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

                        } else
                            localPlayer.Busy = false;

                    }, false);

                }

            }

        }

        protected void NextTurn () {

            localPlayer.Busy = false;

            if (Caller.IsLocalPlayer)
                Caller.NextTurn ();

        }

        public CommonEffect CurrentEffect { get; set; }

        public bool ApplyingEffects { get; set; }

        public virtual void ApplyEffect () {

            foreach (CommonEffect e in commonEffects) {

                CurrentEffect = e;

                typeof (SC_BaseCard).GetMethod (e.effectType.ToString ()).Invoke (this, null);

                /*while (ApplyingEffects)
                    yield return new WaitForEndOfFrame ();*/

            }

        }

        public virtual IEnumerator FinishApplying () {

            yield return null;

            ApplyEffect ();

        }

        #region Common Effects
        #region Assess
        public void Assess () {

            ApplyingEffects = true;

            Caller.ActionOnCard (Caller.GetStringChoice ("Assess"), (c) => {

                Caller.Hand.Remove (c.Card);           

                c.Card.Caller = Caller;

                c.ToGraveyard (GM.drawSpeed, () => { ApplyingEffects = false; });                

            });

            Caller.Deck.Draw (1, false);

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

            (CurrentEffect.effectOnOpponent ? Other : Caller).ApplySingleEffect (CurrentEffect.valueName.ToString (), CurrentEffect.effectValue);

        }

        public void BodyPartEffect () {

            (CurrentEffect.effectOnOpponent ? Other : Caller).ApplySingleBodyEffect ((BodyPart) Caller.GetIntChoice ("BodyPart"), CurrentEffect.effectValue);

        } 

        public void BodyPartEffectChoice () {

            foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                t.gameObject.SetActive (true);

            UI.ShowBodyPartPanel (CurrentEffect.effectValue > 0);

        }
        #endregion

        #region Simple Effects
        public void Tire () {

            (CurrentEffect.effectOnOpponent ? Other : Caller).ApplySingleEffect ("Stamina", -GM.baseStamina);

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

        public void Rest () {

            Caller.ApplySingleEffect ("Stamina", 1);

            Caller.ApplySingleEffect ("Health", 1);

        }
        #endregion

        #region Draw
        public void Draw () {

            StartCoroutine (Draw (CurrentEffect.effectOnOpponent ? Other : Caller));

        }

        protected IEnumerator Draw (SC_Player p) {

            ApplyingEffects = true;

            p.Deck.Draw (1, false);

            yield return new WaitForSeconds (GM.drawSpeed);

            ApplyingEffects = false;

        }
        #endregion

        public void Count () {

            GM.count++;

        }

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

            if (Caller.GetStringChoice ("DoubleTapping") != "") {

                ApplyingEffects = true;

                Caller.ActionOnCard (Caller.GetStringChoice ("DoubleTapping"), (c) => { c.Card.Discard (Caller, DoubleTapNext); });

            }

        }

        void DoubleTapNext () {

            Caller.ActionOnCard (Caller.GetStringChoice ("DoubleTapping2"), (c) => { c.Card.Discard (Caller, DoubleTapEnd); });

        }

        void DoubleTapEnd () {



        }

        public void DoubleTapChoice () {

            localPlayer.SetStringChoiceServerRpc ("DoubleTapping", "");

            if (localPlayer.Hand.Count >= 3) {

                localPlayer.ChoosingCard = ChoosingCard.DoubleTapping;

                localPlayer.StringChoices["DoubleTap2"] = "";

                UI.ShowMessage ("DoubleTap");

            } else
                MakingChoices = false;

        }
        #endregion
        #endregion

        public void Discard (SC_Player owner, Action a = null) {

            Caller = owner;

            Caller.Hand.Remove (this);

            SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);            

            UICard.ToGraveyard (GM.drawSpeed, a ?? (() => { activeCard.ApplyingEffects = false; }));

        }

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

    }

}
