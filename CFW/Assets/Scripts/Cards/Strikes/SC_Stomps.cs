using System.Collections;
using Card;
using static SC_Player;

public class SC_Stomps : SC_OffensiveMove {

    protected override IEnumerator MakeChoices () {

        yield return StartCoroutine (base.MakeChoices ());

        if (!Ephemeral) {

            if (localPlayer.Hand.Count > 2 && CanUse (localPlayer, 2)) {

                yield return StartCoroutine (MakeChoice (() => {

                    UI.ShowBooleanChoiceUI ("Discard 2 to Chain 4", "Skip", (b) => {

                        if (b) {

                            localPlayer.StartDoubleDiscard (() => {

                                localPlayer.SetIntChoiceServerRpc ("Stomps", 1);

                                activeCard.MakingChoices = false;

                            });

                        } else {

                            localPlayer.SetIntChoiceServerRpc ("Stomps", 0);

                            MakingChoices = false;

                        }

                    });

                }));

            } else
                Caller.SetIntChoiceServerRpc ("Stomps", 0);

        }

    }

    public override void Chain () {

        if (Ephemeral || Caller.GetIntChoice ("Stomps") != 0)
            base.Chain ();
        else
            Caller.IntChoices["NumberChoice"] = 0;

    }

}
