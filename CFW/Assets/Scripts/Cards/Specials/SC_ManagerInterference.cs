using System.Collections;
using UnityEngine;
using static SC_Global;
using static SC_Player;

namespace Card {

    public class SC_ManagerInterference : SC_BaseCard {

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            yield return StartCoroutine (MakeChoice (() => {

                localPlayer.IntChoices["ThreeChoices"] = -1;

                UI.ShowThreeChoices (new string[] { "Reduce stamina", "Hit opponent", "Hit body part" });

            }));

            while (localPlayer.GetIntChoice ("ThreeChoices") == -1)
                yield return new WaitForEndOfFrame ();

            if (localPlayer.GetIntChoice ("ThreeChoices") == 2) {

                yield return StartCoroutine (MakeChoice (() => {

                    BodyPartEffectChoice ();

                }));

            }

        }

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            switch (Caller.GetIntChoice ("ThreeChoices")) {

                case 0:
                    Receiver.ApplySingleEffect ("Stamina", -2);
                    break;
                case 1:
                    Receiver.ApplySingleEffect ("Health", -3);
                    break;
                case 2:
                    Receiver.ApplySingleBodyEffect ((BodyPart) Caller.GetIntChoice ("BodyPart"), 2);
                    break;

            }

        }

    }

}

