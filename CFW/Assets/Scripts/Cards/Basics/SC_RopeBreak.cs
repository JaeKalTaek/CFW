namespace Card {

    public class SC_RopeBreak : SC_BaseCard {

        public override bool CanUse (SC_Player user) {

            return user.Stamina > (lockingCard as SC_Submission).effect.breakCost && base.CanUse (user);

        }

    }

}
