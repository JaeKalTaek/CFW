namespace Card {

    public class SC_IrishWhip : SC_BaseCard {

        public override void ApplyModifiers () {

            (activeCard as SC_OffensiveMove).effectOnOpponent.health += 2;

            base.ApplyModifiers ();

        }

    }

}
