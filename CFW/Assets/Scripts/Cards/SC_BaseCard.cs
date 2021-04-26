﻿using DG.Tweening;
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

        [Tooltip ("Common effects of this card")]        
        public CommonEffect[] commonEffects;

        public enum CommonEffectType { Assess, MatchHeatEffect, SingleValueEffect, BodyPartEffect, Tire }

        public enum ValueName { None, Health, Stamina, Alignment }

        [Serializable]
        public struct CommonEffect {

            public bool effectOnOpponent;

            public CommonEffectType effectType;

            public ValueName valueName;

            public int effectValue;
        
        }

        public string Path {

            get {

                string s = types[0].ToString () + (types.Length == 1 ? "s" : "");

                for (int i = 1; i < types.Length; i++)
                    s += " " + types[i].ToString () + (types.Length == i + 1 ? "s" : "");

                return s + "/" + name.Replace("(Clone)", "");                

            }

        }

        protected SC_Player caller, other;

        public virtual bool CanUse () {

            return GM.MatchHeat >= matchHeat;

        }

        public static SC_BaseCard activeCard;

        void Awake () {

            UICard = transform.parent.GetComponent<SC_UI_Card> ();

        }

        public virtual void StartUsing () {

            activeCard = this;

            foreach (CommonEffect c in commonEffects) {

                if (c.effectType == CommonEffectType.Assess) {

                    PrepareAssess ();

                    return;

                }

            }

            if (Is (CardType.Basic))
                localPlayer.UseBasicCardServerRpc (UICard.transform.GetSiblingIndex ());
            else
                localPlayer.UseCardServerRpc (UICard.name);

        }

        void PrepareAssess () {

            localPlayer.Assessing = true;

            UI.assessPanel.SetActive (true);

        }

        public virtual void Play (SC_Player c) {

            caller = c;

            other = c.IsLocalPlayer ? otherPlayer : localPlayer;

            cardToAssess = caller.Hand[caller.CurrentChoice].UICard.name;            

            caller.Hand.Remove (this);

            localPlayer.Busy = true;

            UICard.transform.SetParent (UICard.transform.parent.parent);

            SC_Deck.OrganizeHand (caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

            UICard.RecT.pivot = Vector2.one * .5f;

            UICard.RecT.anchoredPosition3D = Vector3.up * (caller.IsLocalPlayer ? UICard.RecT.sizeDelta.y / 2 : (GM.transform as RectTransform).sizeDelta.y - UICard.RecT.sizeDelta.y / 2);

            UICard.RecT.DOLocalMove (Vector3.zero, 1);

            DOTween.Sequence ().Append (UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * 1.5f, 1)).OnComplete (() => { StartCoroutine (Use ()); });

            UICard.Flip (!caller.IsLocalPlayer, .5f);

        }        

        IEnumerator Use () {

            yield return new WaitForSeconds (1);

            ApplyEffect ();

            while (applyingEffects)
                yield return new WaitForEndOfFrame ();

            if ((caller.IsLocalPlayer ? otherPlayer : localPlayer).Health <= 0) {

                UI.ShowEndPanel (caller.IsLocalPlayer);

            } else {

                UICard.RecT.transform.SetParent ((caller.IsLocalPlayer ? GM.localGraveyard : GM.otherGraveyard).transform);

                UICard.RecT.anchorMin = UICard.RecT.anchorMax = Vector2.one * .5f;

                UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / 1.5f, 1);

                UICard.RecT.DOAnchorPos (Vector2.zero, 1).OnComplete (() => {

                    if (Is (CardType.Basic))
                        Destroy (transform.parent.gameObject);

                    localPlayer.Busy = false;

                    if (caller.IsLocalPlayer)
                        caller.SkipTurn ();

                });

            }

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

            caller.ActionOnCard (cardToAssess, (c) => {

                caller.Hand.Remove (c.Card);

                c.transform.SetParent ((caller.IsLocalPlayer ? GM.localGraveyard : GM.otherGraveyard).transform);

                c.RecT.pivot = c.RecT.anchorMin = c.RecT.anchorMax = Vector2.one * .5f;

                c.Flip (!caller.IsLocalPlayer, .5f);

                c.RecT.DOAnchorPos (Vector2.zero, GM.drawSpeed).OnComplete (() => { applyingEffects = false; });

            });

            caller.Deck.Draw (1, false);

        }

        public void MatchHeatEffect () {

            GM.AddMatchHeat (currentEffect.effectValue);

        }

        public void SingleValueEffect () {

            (currentEffect.effectOnOpponent ? other : caller).ApplySingleEffect (currentEffect.valueName.ToString (), currentEffect.effectValue);

        }

        public void BodyPartEffect () {

            (currentEffect.effectOnOpponent ? other : caller).ApplySingleBodyEffect (BodyPart.Arms, currentEffect.effectValue);

        } 

        public void Tire () {

            (currentEffect.effectOnOpponent ? other : caller).ApplySingleEffect ("Stamina", -GM.baseStamina);

        }

        public bool Is (CardType t) {

            foreach (CardType c in types)
                if (c == t)
                    return true;

            return false;

        }

    }

}
