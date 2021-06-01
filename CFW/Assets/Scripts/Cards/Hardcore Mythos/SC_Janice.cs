namespace Card {

    public class SC_Janice : SC_BaseCard {

        public override void ApplyModifiers () {

            if (activeCard.Caller == Caller && (activeCard as SC_OffensiveMove) && activeCard.Is (SC_Global.CardType.Hardcore)) {

                (activeCard as SC_OffensiveMove).effectOnOpponent.health += 1;

                (activeCard as SC_OffensiveMove).effectOnOpponent.bodyPartDamage.damage += 1;

            }

        }

    }

}
