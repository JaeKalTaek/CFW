using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Handshake : SC_BaseCard {

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            ApplyingEffects = true;

            Other.IntChoices["Handshake"] = -1;

            if (Other.IsLocalPlayer) {

                UI.handshakeUI.heelChoice.SetActive (Other.Alignment <= 0);

                UI.handshakeUI.faceChoice.SetActive (Other.Alignment >= 0);

                UI.handshakeUI.panel.SetActive (true);

            }

            while (Other.GetIntChoice ("Handshake") == -1)
                yield return new WaitForEndOfFrame ();

            int choice = Other.GetIntChoice ("Handshake");

            if (choice != 1) {

                if (choice == 0)
                    Caller.ApplySingleEffect ("Health", -2);
                else if (choice == 2)
                    yield return Draw (Other);

                yield return Draw (Caller);

            }

            ApplyingEffects = false;

        }

    }

}

