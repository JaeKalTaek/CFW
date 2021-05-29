namespace Card {

    public class SC_RopeAidedSubmission : SC_BaseCard {

        public override void Boost () {

            base.Boost ();

            (respondedCards.Peek () as SC_Submission).effect.bodyPartDamage.damage += 2;

        }

    }

}
