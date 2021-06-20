using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SC_Global;

namespace Card {

    public abstract class SC_AttackCard : SC_BaseCard {

        [Header("Attack Card Variables")]
        [Tooltip("Match Heat gained when using this card (irrelevent if Finisher)")]
        public int matchHeatGain;

        [Tooltip ("Effects of this offensive move on you")]
        public Cost cost;

        [Tooltip("Is this card a Finisher ?")]
        public bool finisher;

        [Serializable]
        public struct Cost {

            public int stamina;

            public int health;

            public BodyPartDamage bodyPartDamage;

            public static Cost operator * (Cost c, int x) {

                c.stamina *= x;
                c.health *= x;
                c.bodyPartDamage.damage *= x;
                return c;

            }

            public static Cost operator / (Cost c, int x) {

                c.stamina /= x;
                c.health /= x;
                c.bodyPartDamage.damage /= x;
                return c;

            }

        }

        [HideInInspector]
        public Cost costModifiers;

        public Cost GetCost {

            get {

                Cost c = cost;

                c.stamina = Mathf.Max (Mathf.Min (c.stamina, 1), c.stamina + costModifiers.stamina);
                c.health = Mathf.Max (Mathf.Min (c.health, 1), c.health + costModifiers.health);
                if (c.bodyPartDamage.bodyPart != BodyPart.None)
                    c.bodyPartDamage.damage = Mathf.Max (Mathf.Min (c.bodyPartDamage.damage, 1), c.bodyPartDamage.damage + costModifiers.bodyPartDamage.damage);

                return c;

            }

        }

        public override void UpdateValuesUI (bool first = false, Transform[][] lines = null) {

            if (first) {

                BodyPart bp = ((this as SC_OffensiveMove)?.effect.bodyPartDamage ?? (this as SC_Submission).effect.bodyPartDamage).bodyPart;

                foreach (Transform[] t in lines) {

                    for (int i = 0; i < t.Length; i++) {

                        foreach (Transform bg in t[i]) {

                            if (i == t.Length - 1 && GetCost.bodyPartDamage.bodyPart == BodyPart.None && bp == BodyPart.None)
                                bg.gameObject.SetActive (false);
                            else
                                bg.GetComponent<Image> ().color = (i % 2 != (t.Length % 2)) == (GetCost.bodyPartDamage.bodyPart != BodyPart.None || bp != BodyPart.None) ? UI.darkGrey : UI.lightGrey;

                        }

                    }

                }

            }

            UICard.SetAttackValue ("staminaCost", GetCost.stamina, false);

            UICard.SetAttackValue ("bodyPartsCost", GetCost.bodyPartDamage.bodyPart == BodyPart.None ? "" : GetCost.bodyPartDamage.bodyPart + " - " + GetCost.bodyPartDamage.damage, true);

            OffensiveBodyPartDamage b = (this as SC_OffensiveMove)?.GetEffect.bodyPartDamage ?? (this as SC_Submission).GetEffect.bodyPartDamage;

            string s = "";

            if (b.bodyPart != BodyPart.None) {

                s += b.bodyPart;

                if (b.otherBodyPart != BodyPart.None)
                    s += " " + (b.both ? "&" : "or") + " " + b.otherBodyPart;

                s += " - " + b.damage;

            }

            UICard.SetAttackValue ("bodyPartsDamage", s, true);

        }

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return CanUse (user, 1, ignorePriority, ignoreLocks);

        }

        public bool CanUse (SC_Player user, int chain, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, ignoreLocks) && CanPayCost (user, chain);

        }

        public bool CanPayCost (SC_Player user, int chain = 1) {

            if ((user.Health == 0 || user.Health > GetCost.health * chain) && user.Stamina >= GetCost.stamina * chain) {

                foreach (BodyPart b in user.BodyPartsHealth.Keys)
                    if (b == GetCost.bodyPartDamage.bodyPart && (user.BodyPartsHealth[b] == 0 || user.BodyPartsHealth[b] < GetCost.bodyPartDamage.damage * chain))
                        return false;

                return true;

            } else
                return false;

        }

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            OffensiveBodyPartDamage bodyPartDamage = (this as SC_OffensiveMove)?.effect.bodyPartDamage ?? (this as SC_Submission).effect.bodyPartDamage;

            if (bodyPartDamage.otherBodyPart != BodyPart.None && !bodyPartDamage.both) {

                yield return StartCoroutine (MakeChoice (() => {

                    foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                        t.gameObject.SetActive (t.name == bodyPartDamage.bodyPart.ToString () || t.name == bodyPartDamage.otherBodyPart.ToString ());

                    UI.ShowBodyPartPanel ();

                }));

            }

        }

        public virtual void PayCost () {

            Caller.ApplySingleEffect ("Stamina", -GetCost.stamina);

            Caller.ApplySingleEffect ("Health", -GetCost.health);

            if (GetCost.bodyPartDamage.bodyPart != BodyPart.None)
                Caller.ApplySingleBodyEffect (GetCost.bodyPartDamage.bodyPart, GetCost.bodyPartDamage.damage);

        }

        protected override IEnumerator ApplyEffects () {

            BasicEffects ();

            if (!interceptFinishCard)
                yield return StartCoroutine (base.ApplyEffects ());

        }        

        public virtual void BasicEffects () {

            if (interceptFinishCard || !Ephemeral)
                GM.AddMatchHeat (finisher ? GM.maxMatchHeat : matchHeatGain, !finisher);

        }

        public void ApplyBodyPartDamage () {

            OffensiveBodyPartDamage bodyPartDamage = (this as SC_OffensiveMove)?.GetEffect.bodyPartDamage ?? (this as SC_Submission).GetEffect.bodyPartDamage;

            if (bodyPartDamage.bodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == BodyPart.None || bodyPartDamage.bodyPart == (BodyPart) Caller.GetIntChoice ("BodyPart")))
                Receiver.ApplySingleBodyEffect (bodyPartDamage.bodyPart, bodyPartDamage.damage, true);

            if (bodyPartDamage.otherBodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == (BodyPart) Caller.GetIntChoice ("BodyPart")))
                Receiver.ApplySingleBodyEffect (bodyPartDamage.otherBodyPart, bodyPartDamage.damage, true);

        }

    }

}
