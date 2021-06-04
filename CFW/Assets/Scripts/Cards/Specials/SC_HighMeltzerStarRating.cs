using System.Collections;
using UnityEngine;
using static SC_Player;

namespace Card {

    public class SC_HighMeltzerStarRating : SC_BaseCard {

        protected override void OnPlayTriggered () {

            base.OnPlayTriggered ();

            if (activeCard.IsResponse || ((activeCard as SC_AttackCard)?.finisher ?? false))
                Counters += 3;

        }

        public override void OnRingClicked () {

            base.OnRingClicked ();

            if (Counters > 0 && localPlayer == Caller && Caller.Turn && !activeCard) {

                UI.showBasicsButton.SetActive (false);

                localPlayer.PlayCardServerRpc (UICard.transform.GetSiblingIndex (), true);

            }

        }

        public override void Play (SC_Player c) {

            if (OnTheRing) {

                GetComponent<SC_CardGrabber> ().maxMatchHeat = Counters;                

                UICard.ToGraveyard (1, () => {                 

                    SetCurrentEffect (new CommonEffect (CommonEffectType.Grab));

                    GrabPerform ();

                    StartCoroutine (Effect ());

                }, false);

            } else
                base.Play (c);

        }

        IEnumerator Effect () {

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

            activeCard = null;

            if (Caller.IsLocalPlayer)
                UI.showBasicsButton.SetActive (true);

        }

    }

}
