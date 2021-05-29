using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_619 : SC_OffensiveMove {

        protected override void FinishedUsing () {

            originalCard = null;

            CurrentEffect = new CommonEffect (CommonEffectType.Grab);

            Grab ();

            StartCoroutine (EffectCoroutine ());

        }

        IEnumerator EffectCoroutine () {

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            if (Caller.GetIntChoice ("May") == -1 || Caller.GetIntChoice ("Grab") == -1)
                BaseFinishedUsing ();
            else if (Caller.IsLocalPlayer)
                Caller.Hand[Caller.Hand.Count - 1].StartCoroutine (Caller.Hand[Caller.Hand.Count - 1].StartPlaying ());

        }

    }

}
