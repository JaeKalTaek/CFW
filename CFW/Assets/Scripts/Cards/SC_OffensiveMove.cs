using System;
using UnityEngine;
using static SC_Global;
using static SC_Player;

namespace Card {

    public class SC_OffensiveMove : SC_AttackCard {

        public static bool currentChoice;

        [Header("Offensive Move Variables")]      
        [Tooltip("Effects of this offensive move on your opponent")]
        public Effect effectOnOpponent;        

        [Serializable]
        public struct Effect {

            public int stamina;

            public int health;

            public OffensiveBodyPartDamage bodyPartDamage;

        }

        public override void ApplyEffect (SC_Player caller) {

            base.ApplyEffect (caller);

            // Effect on user
            caller.ApplySingleEffect ("Stamina", cost);

            caller.ApplySingleEffect ("Health", cost);

            if (cost.bodyPartDamage.bodyPart != BodyPart.None)
                caller.ApplySingleBodyEffect (cost.bodyPartDamage.bodyPart, cost.bodyPartDamage.damage);

            // Effect on opponent
            SC_Player other = caller.IsLocalPlayer ? otherPlayer : localPlayer;

            other.ApplySingleEffect ("Stamina", effectOnOpponent);

            other.ApplySingleEffect ("Health", effectOnOpponent);

            if (effectOnOpponent.bodyPartDamage.bodyPart != BodyPart.None && (effectOnOpponent.bodyPartDamage.otherBodyPart == BodyPart.None || currentChoice || effectOnOpponent.bodyPartDamage.both))
                other.ApplySingleBodyEffect (effectOnOpponent.bodyPartDamage.bodyPart, effectOnOpponent.bodyPartDamage.damage);

            if (effectOnOpponent.bodyPartDamage.otherBodyPart != BodyPart.None && (effectOnOpponent.bodyPartDamage.both || !currentChoice))
                other.ApplySingleBodyEffect (effectOnOpponent.bodyPartDamage.otherBodyPart, effectOnOpponent.bodyPartDamage.damage);

        }        

    }

}
