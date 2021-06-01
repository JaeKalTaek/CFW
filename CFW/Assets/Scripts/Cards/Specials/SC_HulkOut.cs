namespace Card {

    public class SC_HulkOut : SC_BaseCard {

        public override void ApplyModifiers () {

            (activeCard as SC_OffensiveMove).effectOnOpponent /= 2;

            base.ApplyModifiers ();

        }

    }

}
