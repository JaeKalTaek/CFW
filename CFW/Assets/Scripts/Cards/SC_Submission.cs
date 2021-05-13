using System;
using UnityEngine;
using static SC_Global;
using DG.Tweening;
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

            public int breakCost, earlyBreakDamage;            

        }

        public override IEnumerator ApplyEffects () {            

            Receiver.ApplySingleEffect ("Stamina", -effect.stamina);

            ApplyBodyPartDamage ();

            yield return StartCoroutine (base.ApplyEffects ());

            yield return StartCoroutine (ApplyEffect (Lock));

        }

        public void Maintain () {

            ZoomEffect (ApplyBodyPartDamage, NextTurn, GM.enlargeCardFactor);    

        }

    }

}
