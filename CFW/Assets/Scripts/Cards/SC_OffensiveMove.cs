using System;
using UnityEngine;
using static SC_Global;
using static SC_Player;

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

        public override void StartUsing () {

            if (effectOnOpponent.bodyPartDamage.otherBodyPart != BodyPart.None && !effectOnOpponent.bodyPartDamage.both) {

                activeCard = this;

                foreach (Transform t in UI.bodyPartDamageChoicePanel.transform)
                    if (t.GetSiblingIndex () > 0)
                        t.gameObject.SetActive (t.name == effectOnOpponent.bodyPartDamage.bodyPart.ToString () || t.name == effectOnOpponent.bodyPartDamage.otherBodyPart.ToString ());

                UI.bodyPartDamageChoicePanel.SetActive (true);

            } else
                base.StartUsing ();

        }

        public override void ApplyEffect () {

            base.ApplyEffect ();

            // Effect on user
            caller.ApplySingleEffect ("Stamina", null, cost);

            caller.ApplySingleEffect ("Health", null, cost);

            if (cost.bodyPartDamage.bodyPart != BodyPart.None)
                caller.ApplySingleBodyEffect (cost.bodyPartDamage.bodyPart, cost.bodyPartDamage.damage);

            // Effect on opponent
            other.ApplySingleEffect ("Stamina", null, effectOnOpponent);

            other.ApplySingleEffect ("Health", null, effectOnOpponent);

            if (effectOnOpponent.bodyPartDamage.bodyPart != BodyPart.None && (effectOnOpponent.bodyPartDamage.both || effectOnOpponent.bodyPartDamage.otherBodyPart == BodyPart.None || effectOnOpponent.bodyPartDamage.bodyPart == (BodyPart) localPlayer.CurrentChoice))
                other.ApplySingleBodyEffect (effectOnOpponent.bodyPartDamage.bodyPart, effectOnOpponent.bodyPartDamage.damage);

            if (effectOnOpponent.bodyPartDamage.otherBodyPart != BodyPart.None && (effectOnOpponent.bodyPartDamage.both || effectOnOpponent.bodyPartDamage.otherBodyPart == (BodyPart) localPlayer.CurrentChoice))
                other.ApplySingleBodyEffect (effectOnOpponent.bodyPartDamage.otherBodyPart, effectOnOpponent.bodyPartDamage.damage);

        }        

    }

}
