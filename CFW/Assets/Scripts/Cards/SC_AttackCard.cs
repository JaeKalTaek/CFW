using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public delegate int HealthCostModifier (SC_BaseCard c, SC_Player p);

        public static List<HealthCostModifier> healthCostModifiers;

        public int GetHealthCost (SC_Player user) {

            int h = cost.health;

            foreach (HealthCostModifier m in healthCostModifiers)
                h += m (this, user);

            return h;

        }

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return CanUse (user, 1, ignorePriority, ignoreLocks);

        }

        public bool CanUse (SC_Player user, int chain, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, ignoreLocks) && CanPayCost (user, chain);

        }

        public bool CanPayCost (SC_Player user, int chain = 1) {

            if ((user.Health == 0 || user.Health > GetHealthCost (user) * chain) && user.Stamina >= cost.stamina * chain) {

                foreach (BodyPart b in user.BodyPartsHealth.Keys)
                    if (b == cost.bodyPartDamage.bodyPart && (user.BodyPartsHealth[b] == 0 || user.BodyPartsHealth[b] < cost.bodyPartDamage.damage * chain))
                        return false;

                return true;

            } else
                return false;

        }

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            OffensiveBodyPartDamage bodyPartDamage = (this as SC_OffensiveMove)?.effectOnOpponent.bodyPartDamage ?? (this as SC_Submission).effect.bodyPartDamage;

            if (bodyPartDamage.otherBodyPart != BodyPart.None && !bodyPartDamage.both) {

                yield return StartCoroutine (MakeChoice (() => {

                    foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                        t.gameObject.SetActive (t.name == bodyPartDamage.bodyPart.ToString () || t.name == bodyPartDamage.otherBodyPart.ToString ());

                    UI.ShowBodyPartPanel ();

                }));

            }

        }

        public virtual void PayCost () {

            Caller.ApplySingleEffect ("Stamina", -cost.stamina);

            Caller.ApplySingleEffect ("Health", -GetHealthCost (Caller));

            if (cost.bodyPartDamage.bodyPart != BodyPart.None)
                Caller.ApplySingleBodyEffect (cost.bodyPartDamage.bodyPart, cost.bodyPartDamage.damage);

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

            OffensiveBodyPartDamage bodyPartDamage = (this as SC_OffensiveMove)?.effectOnOpponent.bodyPartDamage ?? (this as SC_Submission).effect.bodyPartDamage;

            if (bodyPartDamage.bodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == BodyPart.None || bodyPartDamage.bodyPart == (BodyPart) Caller.GetIntChoice ("BodyPart")))
                Receiver.ApplySingleBodyEffect (bodyPartDamage.bodyPart, bodyPartDamage.damage, true);

            if (bodyPartDamage.otherBodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == (BodyPart) Caller.GetIntChoice ("BodyPart")))
                Receiver.ApplySingleBodyEffect (bodyPartDamage.otherBodyPart, bodyPartDamage.damage, true);

        }

    }

}
