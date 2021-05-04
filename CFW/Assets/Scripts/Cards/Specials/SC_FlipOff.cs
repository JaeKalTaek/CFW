namespace Card {

    public class SC_FlipOff : SC_BaseCard {

        public override void ApplyEffect () {            

            base.ApplyEffect ();

            if (Other.Hand.Count > 0) {

                ApplyingEffects = true;

                if (Other.IsLocalPlayer) {

                    SC_Player.localPlayer.Discarding = true;

                    UI.ShowMessage ("Discard");

                }

            }

        }

    }

}
