using System;
using UnityEngine;
using static SC_Global;

namespace Card {

    public class SC_OffensiveMove : SC_AttackCard {
        
        [Header("Offensive Move Variables")]      
        [Tooltip("Effects of this offensive move on your opponent")]
        public Effect effect;        

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

        [HideInInspector]
        public Effect effectModifiers;

        public Effect GetEffect {

            get {

                Effect e = effect;

                e.stamina = Mathf.Max (Mathf.Min (e.stamina, 1), e.stamina + effectModifiers.stamina);
                e.health = Mathf.Max (Mathf.Min (e.health, 1), e.health + effectModifiers.health);
                if (e.bodyPartDamage.bodyPart != BodyPart.None)
                    e.bodyPartDamage.damage = Mathf.Max (Mathf.Min (e.bodyPartDamage.damage, 1), e.bodyPartDamage.damage + effectModifiers.bodyPartDamage.damage);

                return e;

            }

        }

        public int moveOfDoom;

        public override void BasicEffects () {

            base.BasicEffects ();

            Receiver.ApplySingleEffect ("Stamina", -GetEffect.stamina);

            Receiver.ApplySingleEffect ("Health", -GetEffect.health, true);

            if (GetEffect.health > 0)
                OnOffensiveMoveDamage?.Invoke ();

            ApplyBodyPartDamage ();

        }

        public override void UpdateValuesUI (bool first = false, Transform[][] lines = null) {

            base.UpdateValuesUI (first, new Transform[][] { UICard.offensiveMoveValues.lines, UICard.offensiveMoveBigValues.lines });

            UICard.SetAttackValue ("healthCost", GetCost.health, true);            

            UICard.SetAttackValue ("staminaDamage", GetEffect.stamina, false);
            UICard.SetAttackValue ("healthDamage", GetEffect.health, true);

        }        

        public static event Action OnOffensiveMoveDamage;        

    }

}
