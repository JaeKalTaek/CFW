using System.Collections;

namespace Card {

    public class SC_FlipOff : SC_BaseCard {

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            if (Other.Hand.Count > 0) {

                ApplyingEffects = true;

                StartCoroutine (ApplyEffect (() => {

                    if (Other.IsLocalPlayer) {

                        SC_Player.localPlayer.ChoosingCard = SC_Global.ChoosingCard.Discarding;

                        UI.ShowMessage ("Discard");

                    }

                }));                

            }

        }

    }

}
