using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Handshake : SC_BaseCard {

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            ApplyingEffects = true;

            Receiver.IntChoices["Handshake"] = -1;

            if (Receiver.IsLocalPlayer) {

                UI.handshakeUI.heelChoice.SetActive (Receiver.Alignment <= 0);

                UI.handshakeUI.faceChoice.SetActive (Receiver.Alignment >= 0);

                UI.handshakeUI.panel.SetActive (true);

            }

            while (Receiver.GetIntChoice ("Handshake") == -1)
                yield return new WaitForEndOfFrame ();

            int choice = Receiver.GetIntChoice ("Handshake");

            if (choice != 1) {

                if (choice == 0)
                    Caller.ApplySingleEffect ("Health", -2);
                else if (choice == 2)
                    yield return Draw (Receiver);

                yield return Draw (Caller);

            }

            ApplyingEffects = false;

        }

    }

}

