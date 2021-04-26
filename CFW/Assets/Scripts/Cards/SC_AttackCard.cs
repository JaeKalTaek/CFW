using System;
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

        public override void ApplyEffect () {

            base.ApplyEffect ();

            GM.AddMatchHeat (finisher ? GM.maxMatchHeat : matchHeatGain, true);

        }

    }

}
