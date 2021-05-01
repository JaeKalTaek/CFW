namespace Card {

    public class SC_BreakFree : SC_BaseCard {

        public override void ApplyEffect () {

            base.ApplyEffect ();

            GM.AddMatchHeat (SC_GameManager.count);

        }

    }

}
