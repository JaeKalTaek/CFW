namespace Card {

    public class SC_RopeBreak : SC_BaseCard {

        public override bool CanUse () {

            return SC_Player.localPlayer.Stamina > (lockingCard as SC_Submission).effect.breakCost && base.CanUse ();

        }

    }

}
