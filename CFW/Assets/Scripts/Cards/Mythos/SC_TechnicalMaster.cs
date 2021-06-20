namespace Card {

    public class SC_TechnicalMaster : SC_BaseCard {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if (c.Is (SC_Global.CardType.Submission))
                (c as SC_Submission).effectModifiers.bodyPartDamage.damage += add ? 1 : -1;

        }

    }

}
