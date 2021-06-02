using System.Collections;
using static SC_Player;

namespace Card {

    public class SC_TournamentOfDeath : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Hardcore))
                Counters++;

        }

        public override void OnRingClicked () {

            if (Counters >= 3 && localPlayer == Caller && Caller.Turn && !activeCard) {

                UI.showBasicsButton.SetActive (false);

                localPlayer.PlayCardServerRpc (UICard.transform.GetSiblingIndex (), OnTheRing);

            }                

        }

        public override void Play (SC_Player c) {

            if (OnTheRing) {

                Counters -= 3;

                activeCard = this;

                SetCurrentEffect (new CommonEffect (CommonEffectType.Draw));

                StartCoroutine (Effect ());

            } else
                base.Play (c);

        }

        IEnumerator Effect () {

            yield return StartCoroutine (ApplyEffect (Draw));

            activeCard = null;

        }

    }

}
