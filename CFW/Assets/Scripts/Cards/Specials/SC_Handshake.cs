using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Handshake : SC_BaseCard {

        public override void ApplyEffect () {

            ApplyingEffects = true;

            if (Other.IsLocalPlayer) {

                UI.handshakeUI.heelChoice.SetActive (Other.Alignment <= 0);

                UI.handshakeUI.faceChoice.SetActive (Other.Alignment >= 0);

                UI.handshakeUI.panel.SetActive (true);

            }

        }
        
        public override IEnumerator FinishApplying () {

            int choice = Other.GetChoice ("Handshake");

            if (choice != 1) {

                if (choice == 0)
                    Caller.ApplySingleEffect ("Health", -2);
                else if (choice == 2)
                    yield return Draw (Other);

                yield return Draw (Caller);

            }

            base.ApplyEffect ();

        }

    }

}

