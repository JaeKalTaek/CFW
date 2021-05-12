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

        public override void NonMatchHeatEffects () {

            base.NonMatchHeatEffects ();

            Receiver.ApplySingleEffect ("Stamina", null, effectOnOpponent);

            Receiver.ApplySingleEffect ("Health", null, effectOnOpponent);

            ApplyBodyPartDamage ();

        }

    }

}
