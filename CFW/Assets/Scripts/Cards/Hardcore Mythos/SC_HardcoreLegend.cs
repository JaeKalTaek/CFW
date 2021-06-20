namespace Card {

    public class SC_HardcoreLegend : SC_BaseCard {

        public override void ApplyModifiersToCard (SC_BaseCard c, bool add) {

            if ((c as SC_OffensiveMove) && c.Is (SC_Global.CardType.Hardcore))
                (c as SC_OffensiveMove).effectModifiers.health -= add ? 1 : -1;

        }
        
    }

}
