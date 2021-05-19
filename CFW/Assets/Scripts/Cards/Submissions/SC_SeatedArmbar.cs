using System.Collections;
using static SC_Player;

namespace Card {

    public class SC_SeatedArmbar : SC_Submission {

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            if (localPlayer.Health > cost.health + 2) {

                yield return StartCoroutine (MakeChoice (() => {

                    UI.ShowBooleanChoiceUI ("Pay 2 health to reduce stamina by 1", "Skip", (b) => {

                        cost.health += b ? 2 : 0;

                        effect.stamina += b ? 1 : 0;

                        MakingChoices = false;

                    });

                }));

            }

        }

    }

}

