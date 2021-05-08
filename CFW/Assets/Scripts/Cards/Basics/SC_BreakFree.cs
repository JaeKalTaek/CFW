namespace Card {

    public class SC_BreakFree : SC_BaseCard {

        public override bool CanUse () {

            return SC_Player.localPlayer.Stamina > (lockingCard as SC_Submission).effect.breakCost && base.CanUse ();

        }

        public override void ApplyEffect () {

            base.ApplyEffect ();

            GM.AddMatchHeat (GM.count);

        }

    }

}