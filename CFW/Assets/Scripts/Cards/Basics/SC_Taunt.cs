using UnityEngine;

namespace Card {

    public class SC_Taunt : SC_BaseCard {

        public override void ApplyEffect () {

            base.ApplyEffect ();

            caller.ApplySingleEffect ("Alignment", Mathf.Clamp (caller.Alignment, -1, 1));

        }

    }

}
