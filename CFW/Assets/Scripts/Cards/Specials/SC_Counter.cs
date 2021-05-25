using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Counter : SC_BaseCard {

        public override void CounterFinished () {

            if (Caller.HasOnePlayableCardInHand ((c) => { return c as SC_OffensiveMove; })) {

                Caller.IntChoices["CounterCard"] = -1;

                StartCoroutine (CounterCardCoroutine ());

                if (Caller.IsLocalPlayer) {

                    UI.ShowBooleanChoiceUI ("Play an offensive move?", "Skip", (b) => {

                        Caller.SetIntChoiceServerRpc ("CounterCard", b ? 0 : 1);

                    });

                }

            } else
                base.CounterFinished ();

        }

        protected override void InterceptFinish () {

            base.InterceptFinish ();

            base.CounterFinished ();

        }

        IEnumerator CounterCardCoroutine () {

            while (Caller.GetIntChoice ("CounterCard") == -1)
                yield return new WaitForEndOfFrame ();

            if (Caller.GetIntChoice ("CounterCard") == 1)
                base.CounterFinished ();
            else if (Caller.IsLocalPlayer) {

                Caller.InterceptCardFinishServerRpc ();

                Caller.StartChoosingCard (SC_Global.ChoosingCard.Play);

            }

        }

    }

}
