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

        public override bool CanUse () {

            if (base.CanUse () && localPlayer.Health > cost.health && localPlayer.Stamina >= cost.stamina) {

                foreach (BodyPart b in localPlayer.BodyPartsHealth.Keys)
                    if (b == cost.bodyPartDamage.bodyPart && localPlayer.BodyPartsHealth[b] < cost.bodyPartDamage.damage)
                        return false;

                return true;

            } else         
                return false;

        }

        public override IEnumerator StartUsing () {

            MakingChoices = true;

            StartCoroutine (base.StartUsing ());            

            OffensiveBodyPartDamage bodyPartDamage = (this as SC_OffensiveMove)?.effectOnOpponent.bodyPartDamage ?? (this as SC_Submission).effect.bodyPartDamage;

            if (bodyPartDamage.otherBodyPart != BodyPart.None && !bodyPartDamage.both) {

                activeCard = this;

                foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                    t.gameObject.SetActive (t.name == bodyPartDamage.bodyPart.ToString () || t.name == bodyPartDamage.otherBodyPart.ToString ());

                UI.ShowBodyPartPanel ();

            } else
                MakingChoices = false;

            yield return null;

        }

        public override void ApplyEffect () {

            base.ApplyEffect ();

            GM.AddMatchHeat (finisher ? GM.maxMatchHeat : matchHeatGain, true);

            Caller.ApplySingleEffect ("Stamina", null, cost);

            Caller.ApplySingleEffect ("Health", null, cost);

            if (cost.bodyPartDamage.bodyPart != BodyPart.None)
                Caller.ApplySingleBodyEffect (cost.bodyPartDamage.bodyPart, cost.bodyPartDamage.damage);

        }

        protected void ApplyBodyPartDamage () {

            OffensiveBodyPartDamage bodyPartDamage = (this as SC_OffensiveMove)?.effectOnOpponent.bodyPartDamage ?? (this as SC_Submission).effect.bodyPartDamage;

            if (bodyPartDamage.bodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == BodyPart.None || bodyPartDamage.bodyPart == (BodyPart) Caller.GetChoice ("BodyPart")))
                Other.ApplySingleBodyEffect (bodyPartDamage.bodyPart, bodyPartDamage.damage);

            if (bodyPartDamage.otherBodyPart != BodyPart.None && (bodyPartDamage.both || bodyPartDamage.otherBodyPart == (BodyPart) Caller.GetChoice ("BodyPart")))
                Other.ApplySingleBodyEffect (bodyPartDamage.otherBodyPart, bodyPartDamage.damage);

        }

    }

}
