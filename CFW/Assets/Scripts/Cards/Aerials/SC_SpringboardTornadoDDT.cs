using System.Collections;
using UnityEngine;
using static SC_Player;

namespace Card {

    public class SC_SpringboardTornadoDDT : SC_OffensiveMove {

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            yield return StartCoroutine (MakeChoice (() => {

                if (localPlayer.Stamina > GetCost.stamina)
                    UI.ShowNumberChoiceUI (Mathf.Min (3, localPlayer.Stamina - GetCost.stamina));
                else {

                    localPlayer.SetIntChoiceServerRpc ("NumberChoice", 0);

                    MakingChoices = false;

                }

            }));

        }

        protected override IEnumerator ApplyEffects () {

            if (Caller.GetIntChoice ("NumberChoice") != 0) {

                cost.stamina += Caller.GetIntChoice ("NumberChoice");

                effect.health += Caller.GetIntChoice ("NumberChoice");

                UpdateValuesUI ();

            }

            yield return StartCoroutine (base.ApplyEffects ());            

        }

    }

}
