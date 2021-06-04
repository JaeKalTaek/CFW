using System.Collections;
using UnityEngine;
using static SC_Player;

namespace Card {

    public class SC_LegendaryRivalry : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard as SC_AttackCard)
                Counters++;

        }

        public override void OnRingClicked () {

            base.OnRingClicked ();

            if (Counters >= 2 && localPlayer == Caller && Caller.Turn && !activeCard) {

                UI.showBasicsButton.SetActive (false);

                localPlayer.PlayCardServerRpc (UICard.transform.GetSiblingIndex (), true);

            }

        }

        public override void Play (SC_Player c) {

            if (OnTheRing) {             

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
