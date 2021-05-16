namespace Card {

    public class SC_PaperCut : SC_BaseCard {

        public override bool CanUse (SC_Player user) {            

            return base.CanUse (user) && GM.localGraveyard.Cards.FindAll (new System.Predicate<SC_BaseCard> ((t) => { return t.Is (SC_Global.CardType.Hardcore); })).Count >= 4;

        }        

    }

}

