using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_FootballKick : SC_OffensiveMove {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, ignoreLocks) && !user.Neutral;

        }

        protected override IEnumerator ApplyEffects () {

            commonEffects.Add (new CommonEffect (CommonEffectType.Chain, v: Mathf.Abs (Caller.Alignment)));

            return base.ApplyEffects ();

        }

    }

}
