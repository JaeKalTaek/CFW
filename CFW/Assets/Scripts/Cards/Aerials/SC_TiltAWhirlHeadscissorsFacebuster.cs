using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_TiltAWhirlHeadscissorsFacebuster : SC_OffensiveMove {

        protected override void FinishedUsing () {

            CurrentEffect = new CommonEffect (CommonEffectType.Grab, true);

            effectTarget = Caller;

            Grab ();

            StartCoroutine (EffectCoroutine ());

        }

        IEnumerator EffectCoroutine () {

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            BaseFinishedUsing ();

        }

    }

}
