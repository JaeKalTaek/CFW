using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_619 : SC_OffensiveMove {

        public override void GrabFinished () {

            originalCard = null;

            base.GrabFinished ();

        }

        protected override IEnumerator GrabFinishedCoroutine () {

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            if (Caller.GetIntChoice ("May") == -1 || Caller.GetIntChoice ("Grab") == -1)
                FinishedUsing ();
            else if (Caller.Hand[Caller.Hand.Count - 1].CanUse (Caller, true)) {

                if (Caller.IsLocalPlayer)
                    Caller.Hand[Caller.Hand.Count - 1].StartCoroutine (Caller.Hand[Caller.Hand.Count - 1].StartPlaying ());

            } else
                FinishedUsing ();

        }

    }

}
