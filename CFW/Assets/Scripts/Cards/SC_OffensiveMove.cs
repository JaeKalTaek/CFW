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

            public static Effect operator * (Effect e, int x) {

                e.stamina *= x;
                e.health *= x;
                e.bodyPartDamage.damage *= x;
                return e;

            }

            public static Effect operator / (Effect e, int x) {

                e.stamina /= x;
                e.health /= x;
                e.bodyPartDamage.damage /= x;
                return e;

            }

        }

        public int moveOfDoom;

        public override void NonMatchHeatEffects () {

            base.NonMatchHeatEffects ();

            Receiver.ApplySingleEffect ("Stamina", -effectOnOpponent.stamina);

            Receiver.ApplySingleEffect ("Health", -effectOnOpponent.health, true);

            if (effectOnOpponent.health > 0)
                OnOffensiveMoveDamage?.Invoke ();

            ApplyBodyPartDamage ();

        }

        public static event Action OnOffensiveMoveDamage;        

    }

}
