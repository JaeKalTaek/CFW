using DG.Tweening;
using System;
using System.Collections;
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

        public enum CommonEffectType { Assess, MatchHeatEffect, SingleValueEffect, BodyPartEffect, Tire, Break, Rest }

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

            if (GM.MatchHeat >= matchHeat && (NoLock || Is (CardType.Basic) || Has (CommonEffectType.Break))) {

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

        public virtual void StartUsing () {

            activeCard = this;

            if (Has (CommonEffectType.BodyPartEffect)) {

                foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                    t.gameObject.SetActive (true);

                UI.bodyPartDamageChoicePanel.SetActive (true);

            } else if (Has (CommonEffectType.Assess)) {

                localPlayer.Assessing = true;

                UI.assessPanel.SetActive (true);

            } else if (Is (CardType.Basic))
                localPlayer.UseBasicCardServerRpc (UICard.transform.GetSiblingIndex ());
            else
                localPlayer.UseCardServerRpc (UICard.name);

        }

        public virtual void Play (SC_Player c) {

            Caller = c;

            Other = c.IsLocalPlayer ? otherPlayer : localPlayer;

            cardToAssess = Caller.Hand[Caller.CurrentChoice].UICard.name;            

            Caller.Hand.Remove (this);

            localPlayer.Busy = true;

            UICard.transform.SetParent (UICard.transform.parent.parent);

            SC_Deck.OrganizeHand (Caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

            UICard.RecT.pivot = Vector2.one * .5f;

            UICard.RecT.anchoredPosition3D = Vector3.up * (Caller.IsLocalPlayer ? UICard.RecT.sizeDelta.y / 2 : (GM.transform as RectTransform).sizeDelta.y - UICard.RecT.sizeDelta.y / 2);

            UICard.RecT.DOLocalMove (Vector3.zero, 1);

            DOTween.Sequence ().Append (UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * GM.playedSizeMultiplicator, 1)).OnComplete (() => { StartCoroutine (Use ()); });

            UICard.Flip (!Caller.IsLocalPlayer, .5f);

        }        

        IEnumerator Use () {

            yield return new WaitForSeconds (1);

            ApplyEffect ();

            while (applyingEffects)
                yield return new WaitForEndOfFrame ();

            if (SC_GameManager.count == 3) {

                UI.ShowEndPanel (!Caller.IsLocalPlayer);

            } else if ((Caller.IsLocalPlayer ? otherPlayer : localPlayer).Health <= 0) {

                UI.ShowEndPanel (Caller.IsLocalPlayer);

            } else {

                UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / GM.playedSizeMultiplicator, 1);

                if (Is (CardType.Submission) || UICard.name.EndsWith ("Pinfall")) {

                    lockingCard = this;

                    SC_GameManager.count = 0;

                    if (localPlayer != Caller)
                        localPlayer.locked.Value = Is (CardType.Submission) ? Locked.Submission : Locked.Pinned;

                    if (Is (CardType.Submission)) {

                        Vector3 oldPos = UICard.RecT.position;

                        UICard.RecT.anchorMin = UICard.RecT.anchorMax = UICard.BigRec.anchorMin = UICard.BigRec.anchorMax = UICard.BigRec.pivot = Vector2.one * .5f;

                        UICard.RecT.position = oldPos;

                        UICard.BigRec.anchoredPosition = Vector2.zero;

                    }

                    UICard.RecT.DOAnchorPosY (UICard.RecT.sizeDelta.y / 4, 1).onComplete = NextTurn;

                } else {

                    UICard.ToGraveyard (1, () => {

                        localPlayer.Busy = false;

                        if (Is (CardType.Basic)) {

                            Destroy (transform.parent.gameObject);

                            if (!Has (CommonEffectType.Break))
                                NextTurn ();
                            else if (localPlayer.Turn)
                                UI.showBasicsButton.SetActive (true);

                        } else if (Caller.IsLocalPlayer && this as SC_OffensiveMove)
                            UI.pinfallPanel.SetActive (true);

                    });

                }

            }

        }

        protected void NextTurn () {

            localPlayer.Busy = false;

            if (Caller.IsLocalPlayer)
                Caller.NextTurn ();

        }

        CommonEffect currentEffect;

        bool applyingEffects;

        public virtual void ApplyEffect () {

            foreach (CommonEffect e in commonEffects) {

                currentEffect = e;

                typeof (SC_BaseCard).GetMethod (e.effectType.ToString ()).Invoke (this, null);

            }

        }

        string cardToAssess;

        public void Assess () {

            applyingEffects = true;

            Caller.ActionOnCard (cardToAssess, (c) => {

                Caller.Hand.Remove (c.Card);           
                
                c.Flip (!Caller.IsLocalPlayer, .5f);

                c.Card.Caller = Caller;

                c.ToGraveyard (GM.drawSpeed, () => { applyingEffects = false; });                

            });

            Caller.Deck.Draw (1, false);

        }

        public void MatchHeatEffect () {

            GM.AddMatchHeat (currentEffect.effectValue);

        }

        public void SingleValueEffect () {

            (currentEffect.effectOnOpponent ? Other : Caller).ApplySingleEffect (currentEffect.valueName.ToString (), currentEffect.effectValue);

        }

        public void BodyPartEffect () {

            (currentEffect.effectOnOpponent ? Other : Caller).ApplySingleBodyEffect ((BodyPart) Caller.CurrentChoice, currentEffect.effectValue);

        } 

        public void Tire () {

            (currentEffect.effectOnOpponent ? Other : Caller).ApplySingleEffect ("Stamina", -GM.baseStamina);

        }

        public void Break () {

            applyingEffects = true;

            lockingCard.UICard.ToGraveyard (1, () => {

                Destroy (lockingCard.UICard.gameObject);

                if (!localPlayer.Unlocked)
                    localPlayer.locked.Value = Locked.Unlocked;

                applyingEffects = false;

            });          

        }

        public void Rest () {

            Caller.ApplySingleEffect ("Stamina", 1);

            Caller.ApplySingleEffect ("Health", 1);

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
