using System;
using System.Collections;
using UnityEngine;
using static SC_Global;
using static SC_Player;

namespace Card {

    public class SC_AttackCard : SC_BaseCard {

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

        }

        public override bool CanUse (SC_Player user) {

            if (base.CanUse (user) && user.Health > cost.health && user.Stamina >= cost.stamina) {

                foreach (BodyPart b in user.BodyPartsHealth.Keys)
                    if (b == cost.bodyPartDamage.bodyPart && user.BodyPartsHealth[b] < cost.bodyPartDamage.damage)
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

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            GM.AddMatchHeat (finisher ? GM.maxMatchHeat : matchHeatGain, true);

            NonMatchHeatEffects ();

        }

        public virtual void NonMatchHeatEffects () {

            Caller.ApplySingleEffect ("Stamina", null, cost);

            Caller.ApplySingleEffect ("Health", null, cost);

            if (cost.bodyPartDamage.bodyPart != BodyPart.None)
                Caller.ApplySingleBodyEffect (cost.bodyPartDamage.bodyPart, cost.bodyPartDamage.damage);

        }

        protected void ApplyBodyPartDamage () {

            OffensiveBodyPartDamage bodyPartDamage = (this as SC_OffensiveMove)?.effectOnOpponent.bodyPartDamage ?? (this as SC_Submission).effect.bodyPartDamage;

            if (bodyPartDamage.bodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == BodyPart.None || bodyPartDamage.bodyPart == (BodyPart) Caller.GetIntChoice ("BodyPart")))
                Receiver.ApplySingleBodyEffect (bodyPartDamage.bodyPart, bodyPartDamage.damage);

            if (bodyPartDamage.otherBodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == (BodyPart) Caller.GetIntChoice ("BodyPart")))
                Receiver.ApplySingleBodyEffect (bodyPartDamage.otherBodyPart, bodyPartDamage.damage);

        }

    }

}
