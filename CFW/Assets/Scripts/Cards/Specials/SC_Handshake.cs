using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Handshake : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            ApplyingEffects = true;

            Receiver.IntChoices["ThreeChoices"] = -1;

            if (Receiver.IsLocalPlayer) {

                UI.ShowThreeChoices (new string[] { "Hit opponent", "Do nothing", "Draw a card" });

                UI.threeChoicesUI.leftChoice.SetActive (Receiver.Alignment <= 0);

                UI.threeChoicesUI.rightChoice.SetActive (Receiver.Alignment >= 0);                

            }

            while (Receiver.GetIntChoice ("ThreeChoices") == -1)
                yield return new WaitForEndOfFrame ();

            int choice = Receiver.GetIntChoice ("ThreeChoices");

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

