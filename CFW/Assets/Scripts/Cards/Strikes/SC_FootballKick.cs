using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_FootballKick : SC_OffensiveMove {

        public override bool CanUse (SC_Player user) {

            return base.CanUse (user) && user.Alignment != 0;

        }

        protected override IEnumerator ApplyEffects () {

            commonEffects.Add (new CommonEffect (CommonEffectType.Chain, false, Mathf.Abs (Caller.Alignment)));

            return base.ApplyEffects ();

        }

    }

}
