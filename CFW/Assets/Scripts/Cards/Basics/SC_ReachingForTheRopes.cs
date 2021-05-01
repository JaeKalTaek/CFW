using UnityEngine;

namespace Card {

    public class SC_ReachingForTheRopes : SC_BaseCard {

        public override void ApplyEffect () {

            base.ApplyEffect ();

            SC_GameManager.count++;

        }

    }

}
