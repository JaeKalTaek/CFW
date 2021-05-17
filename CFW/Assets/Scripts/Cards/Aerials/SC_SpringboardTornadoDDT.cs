using System.Collections;
using UnityEngine;
using static SC_Player;

namespace Card {

    public class SC_SpringboardTornadoDDT : SC_OffensiveMove {

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            yield return StartCoroutine (MakeChoice (() => {

                if (localPlayer.Stamina > cost.stamina)
                    UI.ShowNumberChoiceUI (Mathf.Min (3, localPlayer.Stamina - cost.stamina));
                else {

                    localPlayer.SetIntChoiceServerRpc ("NumberChoice", 0);

                    MakingChoices = false;

                }

            }));

        }

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            if (Caller.GetIntChoice ("NumberChoice") != 0) {

                Caller.ApplySingleEffect ("Stamina", -Caller.GetIntChoice ("NumberChoice"));

                Receiver.ApplySingleEffect ("Health", -Caller.GetIntChoice ("NumberChoice"));

            }

        }

    }

}
