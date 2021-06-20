using System.Collections;
using static SC_Player;

namespace Card {

    public class SC_RunningSingleLegHighKnee : SC_OffensiveMove {

        protected override IEnumerator MakeChoices () {

            yield return StartCoroutine (base.MakeChoices ());

            if (localPlayer.Stamina > GetCost.stamina + 2) {

                yield return StartCoroutine (MakeChoice (() => {

                    UI.ShowBooleanChoiceUI ("Pay 2 stamina for 2 bonus damage", "Skip", (b) => {

                        cost.stamina += b ? 2 : 0;

                        effect.health += b ? 2 : 0;

                        UpdateValuesUI ();

                        MakingChoices = false;

                    });

                }));

            }

        }

    }

}
