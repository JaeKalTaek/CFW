namespace Card {

    public class SC_FlipOff : SC_BaseCard {

        public override void ApplyEffect () {            

            base.ApplyEffect ();

            if (Other.Hand.Count > 0) {

                ApplyingEffects = true;

                if (Other.IsLocalPlayer) {

                    SC_Player.localPlayer.ChoosingCard = SC_Global.ChoosingCard.Discarding;

                    UI.ShowMessage ("Discard");

                }

            }

        }

    }

}
