using System;
using UnityEngine;
using static SC_Global;
using System.Collections;

namespace Card {

    public class SC_Submission : SC_AttackCard {

        [Header ("Submission Variables")]
        [Tooltip ("Effects of this submission on your opponent")]
        public Effect effect;

        [Serializable]
        public struct Effect {

            public int stamina;

            public OffensiveBodyPartDamage bodyPartDamage;

            public int breakCost;            

        }

        protected override IEnumerator ApplyEffects () {            

            Receiver.ApplySingleEffect ("Stamina", -effect.stamina);

            ApplyBodyPartDamage ();

            yield return StartCoroutine (base.ApplyEffects ());

            yield return StartCoroutine (ApplyEffect (Lock));

        }

    }

}
