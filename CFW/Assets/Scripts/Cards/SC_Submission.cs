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

            public static Effect operator * (Effect e, int x) {

                e.stamina *= x;
                e.bodyPartDamage.damage *= x;
                return e;

            }

            public static Effect operator / (Effect e, int x) {

                e.stamina /= x;
                e.bodyPartDamage.damage /= x;
                return e;

            }

        }

        protected override IEnumerator ApplyEffects () {            

            yield return StartCoroutine (base.ApplyEffects ());

            if (!interceptFinishCard)
                yield return StartCoroutine (ApplyEffect (Lock));

        }

        public override void BasicEffects () {

            base.BasicEffects ();

            Receiver.ApplySingleEffect ("Stamina", -effect.stamina);

            ApplyBodyPartDamage ();

        }

        public override void UpdateValuesUI (bool first = false, Transform[][] lines = null) {

            base.UpdateValuesUI (first, new Transform[][] { UICard.submissionValues.lines, UICard.submissionBigValues.lines });

            UICard.SetAttackValue ("staminaReduction", effect.stamina, false);
            UICard.SetAttackValue ("breakCost", effect.breakCost, false);

        }

    }

}
