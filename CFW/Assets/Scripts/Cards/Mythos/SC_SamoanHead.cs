namespace Card {

    public class SC_SamoanHead : SC_BaseCard {

        protected override void OnOffensiveMoveDamageTriggered () {

            if (activeCard.Receiver == Caller)
                activeCard.Caller.ApplySingleEffect ("Health", -1);

        }

    }

}
