namespace Card {

    public class SC_NoEscape : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Submission))
                (activeCard as SC_Submission).effect.stamina += 1;

        }

    }

}
