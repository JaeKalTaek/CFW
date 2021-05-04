using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Handshake : SC_BaseCard {

        public override void ApplyEffect () {

            ApplyingEffects = true;

            if (Other.IsLocalPlayer) {

                UI.handshakeUI.heelChoice.SetActive (Other.Alignment <= 0);

                UI.handshakeUI.faceButton.SetActive (Other.Alignment >= 0);

                UI.handshakeUI.panel.SetActive (true);

            }

        }
        
        public IEnumerator FinishApplying (int choice) {            

            if (choice != 1) {

                if (choice == 0) {

                    Caller.ApplySingleEffect ("Health", -2);

                } else if (choice == 2) {

                    yield return Draw (Other);

                    /*Other.Deck.Draw (1, false);

                    yield return new WaitForSeconds (GM.drawSpeed);*/

                }

                yield return Draw (Caller);

                /*Caller.Deck.Draw (1, false);

                yield return new WaitForSeconds (GM.drawSpeed);*/

            }

            base.ApplyEffect ();

        }

    }

}

