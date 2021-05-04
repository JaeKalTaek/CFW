namespace Card {

    public class SC_KickOut : SC_BaseCard {

        public override void ApplyEffect () {

            base.ApplyEffect ();

            GM.AddMatchHeat (GM.count);

        }

    }

}
