namespace Card {

    public class SC_FlippyShit : SC_BaseCard {

        protected override void OnPlayTriggered () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Aerial)) {

                activeCard.SetCurrentEffect (new CommonEffect (CommonEffectType.Assess, true));

                activeCard.Assess ();

            }

        }

    }

}
