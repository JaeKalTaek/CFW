using System;
using UnityEngine;
using static SC_Global;

namespace Card {

    public class SC_OffensiveMove : SC_AttackCard {

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

        public void Use (GameObject caller, bool choice) {

            Use(caller);

            SC_Player other = caller == SC_Player.localPlayer.gameObject ? SC_Player.otherPlayer : SC_Player.localPlayer;

            BodyPart bp = choice ? effectOnOpponent.bodyPartDamage.bodyPart : effectOnOpponent.bodyPartDamage.otherBodyPart;

            other.BodyPartsHealth[bp] = other.BodyPartsHealth[bp].ReduceWithMin(effectOnOpponent.bodyPartDamage.damage);

            GM.SetValue(other == SC_Player.localPlayer, bp.ToString(), other.BodyPartsHealth[bp]);

        }

        public override void Use (GameObject caller) {

            SC_Player user = caller.GetComponent<SC_Player>();

            base.Use(caller);

            // Effect on user
            user.Stamina = user.Stamina.ReduceWithMin(effectOnYou.stamina);

            GM.SetValue(user == SC_Player.localPlayer, "Stamina", user.Stamina);

            user.Health = user.Health.ReduceWithMin(effectOnYou.health);

            GM.SetValue(user == SC_Player.localPlayer, "Health", user.Health);

            if (effectOnYou.bodyPartDamage.bodyPart != BodyPart.None) {

                user.BodyPartsHealth[effectOnYou.bodyPartDamage.bodyPart] = user.BodyPartsHealth[effectOnYou.bodyPartDamage.bodyPart].ReduceWithMin(effectOnYou.bodyPartDamage.damage);

                GM.SetValue(user == SC_Player.localPlayer, effectOnYou.bodyPartDamage.bodyPart.ToString(), user.BodyPartsHealth[effectOnYou.bodyPartDamage.bodyPart]);

            }

            // Effect on opponent
            SC_Player other = user == SC_Player.localPlayer ? SC_Player.otherPlayer : SC_Player.localPlayer;

            other.Stamina = other.Stamina.ReduceWithMin(effectOnOpponent.stamina);

            GM.SetValue(other == SC_Player.localPlayer, "Stamina", other.Stamina);

            other.Health = other.Health.ReduceWithMin(effectOnOpponent.health);

            GM.SetValue(other == SC_Player.localPlayer, "Health", other.Health);

            if (effectOnOpponent.bodyPartDamage.bodyPart != BodyPart.None && (effectOnOpponent.bodyPartDamage.otherBodyPart == BodyPart.None || effectOnOpponent.bodyPartDamage.both)) {

                other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart] = other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart].ReduceWithMin(effectOnOpponent.bodyPartDamage.damage);

                GM.SetValue(other == SC_Player.localPlayer, effectOnOpponent.bodyPartDamage.bodyPart.ToString(), other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart]);

                if (effectOnOpponent.bodyPartDamage.otherBodyPart != BodyPart.None) {

                    other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart] = other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart].ReduceWithMin(effectOnOpponent.bodyPartDamage.damage);

                    GM.SetValue(other == SC_Player.localPlayer, effectOnOpponent.bodyPartDamage.bodyPart.ToString(), other.BodyPartsHealth[effectOnOpponent.bodyPartDamage.bodyPart]);

                }

            }

        }

    }

}
