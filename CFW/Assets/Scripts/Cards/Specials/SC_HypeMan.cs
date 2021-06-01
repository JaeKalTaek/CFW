using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_HypeMan : SC_BaseCard {

        public override IEnumerator FirstTurnEffect () {

            SetCurrentEffect (new CommonEffect (CommonEffectType.Grab, true));

            GrabPerform ();

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            if (Caller.GetIntChoice ("May") != -1 && Caller.GetIntChoice ("Grab") != -1) {

                ApplyingEffects = true;

                Discard (Caller, () => { ApplyingEffects = false; });

                while (ApplyingEffects)
                    yield return new WaitForEndOfFrame ();

            }

            activeCard = null;

        }

    }

}

