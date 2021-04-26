using UnityEngine;

namespace Card {

    public class SC_Taunt : SC_BaseCard {

        public override void ApplyEffect () {

            base.ApplyEffect ();

            Caller.ApplySingleEffect ("Alignment", Mathf.Clamp (Caller.Alignment, -1, 1));

        }

    }

}
