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

        [HideInInspector]
        public Effect effectModifiers;

        public Effect GetEffect {

            get {

                Effect e = effect;

                e.stamina = Mathf.Max (Mathf.Min (e.stamina, 1), e.stamina + effectModifiers.stamina);
                e.breakCost = Mathf.Max (Mathf.Min (e.breakCost, 1), e.breakCost + effectModifiers.breakCost);
                if (e.bodyPartDamage.bodyPart != BodyPart.None)
                    e.bodyPartDamage.damage = Mathf.Max (Mathf.Min (e.bodyPartDamage.damage, 1), e.bodyPartDamage.damage + effectModifiers.bodyPartDamage.damage);

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

            Receiver.ApplySingleEffect ("Stamina", -GetEffect.stamina);

            ApplyBodyPartDamage ();

        }

        public override void UpdateValuesUI (bool first = false, Transform[][] lines = null) {

            base.UpdateValuesUI (first, new Transform[][] { UICard.submissionValues.lines, UICard.submissionBigValues.lines });

            UICard.SetAttackValue ("staminaReduction", GetEffect.stamina, false);
            UICard.SetAttackValue ("breakCost", GetEffect.breakCost, false);

        }

    }

}
