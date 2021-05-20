namespace Card {

    public class SC_PaperCut : SC_BaseCard {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            return base.CanUse (user, ignorePriority, ignoreLocks) && GM.localGraveyard.Cards.FindAll (new System.Predicate<SC_BaseCard> ((t) => { return t.Is (SC_Global.CardType.Hardcore); })).Count >= 4;

        }        

    }

}

