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
            caller.Stamina = caller.Stamina.ReduceWithMin(cost.stamina);

            GM.SetValue (caller.IsLocalPlayer, "Stamina", caller.Stamina);

            caller.Health = caller.Health.ReduceWithMin(cost.health);

            GM.SetValue (caller.IsLocalPlayer, "Health", caller.Health);

            if (cost.bodyPartDamage.bodyPart != BodyPart.None) {

                caller.BodyPartsHealth[cost.bodyPartDamage.bodyPart] = caller.BodyPartsHealth[cost.bodyPartDamage.bodyPart].ReduceWithMin(cost.bodyPartDamage.damage);

                GM.SetValue (caller.IsLocalPlayer, cost.bodyPartDamage.bodyPart.ToString(), caller.BodyPartsHealth[cost.bodyPartDamage.bodyPart]);

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
