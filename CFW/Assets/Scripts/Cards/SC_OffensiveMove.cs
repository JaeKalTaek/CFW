using System;
using System.Collections;
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

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            Other.ApplySingleEffect ("Stamina", null, effectOnOpponent);

            Other.ApplySingleEffect ("Health", null, effectOnOpponent);

            ApplyBodyPartDamage ();

        }        

    }

}
