using UnityEngine;

namespace Card {

    public class SC_Resilience : SC_BaseCard {

        public override void ApplyModifiers () {

            SC_OffensiveMove c = activeCard as SC_OffensiveMove;

            c.effectOnOpponent.bodyPartDamage.damage = Mathf.Max (1, c.effectOnOpponent.bodyPartDamage.damage - 2);
            c.effectOnOpponent.health = Mathf.Max (1, c.effectOnOpponent.health - 2);

            base.ApplyModifiers ();

        }

    }

}
