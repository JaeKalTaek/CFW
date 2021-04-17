using System;
using UnityEngine;
using static SC_Global;
using static SC_Player;

namespace Card {

    public class SC_OffensiveMove : SC_AttackCard {

        public static bool currentChoice;

        [Header("Offensive Move Variables")]
        [Tooltip("Effects of this offensive move on you")]
        public SelfEffect effectOnYou;

        [Tooltip("Effects of this offensive move on your opponent")]
        public Effect effectOnOpponent;

        [Serializable]
        public struct SelfEffect {

            public int stamina;

            public int health;

            public SelfBodyPartDamage bodyPartDamage;

        }

        [Serializable]
        public struct Effect {

            public int stamina;

            public int health;

            public BodyPartDamage bodyPartDamage;

        }

        public override void ApplyEffect (SC_Player caller) {

            base.ApplyEffect (caller);

            // Effect on user
            caller.Stamina = caller.Stamina.ReduceWithMin(effectOnYou.stamina);

            GM.SetValue (caller.IsLocalPlayer, "Stamina", caller.Stamina);

            caller.Health = caller.Health.ReduceWithMin(effectOnYou.health);

            GM.SetValue (caller.IsLocalPlayer, "Health", caller.Health);

            if (effectOnYou.bodyPartDamage.bodyPart != BodyPart.None) {

                caller.BodyPartsHealth[effectOnYou.bodyPartDamage.bodyPart] = caller.BodyPartsHealth[effectOnYou.bodyPartDamage.bodyPart].ReduceWithMin(effectOnYou.bodyPartDamage.damage);

                GM.SetValue (caller.IsLocalPlayer, effectOnYou.bodyPartDamage.bodyPart.ToString(), caller.BodyPartsHealth[effectOnYou.bodyPartDamage.bodyPart]);

            }

            // Effect on opponent
            SC_Player other = caller.IsLocalPlayer ? otherPlayer : localPlayer;

            other.Stamina = other.Stamina.ReduceWithMin(effectOnOpponent.stamina);

            GM.SetValue (other.IsLocalPlayer, "Stamina", other.Stamina);

            other.Health = other.Health.ReduceWithMin(effectOnOpponent.health);

            GM.SetValue (other.IsLocalPlayer, "Health", other.Health);

            if (effectOnOpponent.bodyPartDamage.bodyPart != BodyPart.None && (effectOnOpponent.bodyPartDamage.otherBodyPart == BodyPart.None || currentChoice || effectOnOpponent.bodyPartDamage.both)) {

                other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart] = other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart].ReduceWithMin (effectOnOpponent.bodyPartDamage.damage);

                GM.SetValue (other.IsLocalPlayer, effectOnOpponent.bodyPartDamage.bodyPart.ToString (), other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart]);

            }

            if (effectOnOpponent.bodyPartDamage.otherBodyPart != BodyPart.None && (effectOnOpponent.bodyPartDamage.both || !currentChoice)) {

                other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.otherBodyPart] = other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.otherBodyPart].ReduceWithMin (effectOnOpponent.bodyPartDamage.damage);

                GM.SetValue (other.IsLocalPlayer, effectOnOpponent.bodyPartDamage.otherBodyPart.ToString (), other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart]);

            }

        }

        /*void ApplySingleEffect (SC_Player target, string field) {

            target.Stamina = target.Stamina.ReduceWithMin (effectOnOpponent.stamina);

            GM.SetValue (target == localPlayer, "Stamina", target.Stamina);

        }*/

        void DamageBodyPart () {



        }

    }

}
