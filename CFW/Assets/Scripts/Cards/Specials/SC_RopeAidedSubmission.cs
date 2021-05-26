namespace Card {

    public class SC_RopeAidedSubmission : SC_BaseCard {

        public override void Boost () {

            (respondedCards.Peek () as SC_Submission).effect.bodyPartDamage.damage += 2;

        }

    }

}
