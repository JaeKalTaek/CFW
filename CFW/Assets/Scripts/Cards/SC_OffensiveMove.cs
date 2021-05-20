using System;
using UnityEngine;
using static SC_Global;

namespace Card {

    public class SC_OffensiveMove : SC_AttackCard {
        
        [Header("Offensive Move Variables")]      
        [Tooltip("Effects of this offensive move on your opponent")]
        public Effect effectOnOpponent;        

        [Serializable]
        public struct Effect {

            public int stamina;

            public int health;

            public OffensiveBodyPartDamage bodyPartDamage;

        }

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return CanUse (user, 1, ignorePriority, ignoreLocks);

        }

        public bool CanUse (SC_Player user, int chain, bool ignorePriority = false, bool ignoreLocks = false) {

            if (base.CanUse (user, ignorePriority, ignoreLocks) && user.Health > cost.health * chain && user.Stamina >= cost.stamina * chain) {

                foreach (BodyPart b in user.BodyPartsHealth.Keys)
                    if (b == cost.bodyPartDamage.bodyPart && user.BodyPartsHealth[b] < cost.bodyPartDamage.damage * chain)
                        return false;

                return true;

            } else
                return false;

        }

        public override void NonMatchHeatEffects () {

            base.NonMatchHeatEffects ();

            Receiver.ApplySingleEffect ("Stamina", null, effectOnOpponent);

            Receiver.ApplySingleEffect ("Health", null, effectOnOpponent);

            ApplyBodyPartDamage ();

        }

    }

}
