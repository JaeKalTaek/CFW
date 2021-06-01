namespace Card {

    public class SC_BodyPartHatred : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && activeCard.Is (SC_Global.CardType.Classic))
                (activeCard as SC_OffensiveMove).effectOnOpponent.bodyPartDamage.damage += 1;

        }

    }

}
