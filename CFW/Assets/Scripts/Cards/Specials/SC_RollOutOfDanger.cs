using System.Collections;

namespace Card {

    public class SC_RollOutOfDanger : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            Receiver.ApplySingleEffect ("Health", -(respondedCards.Peek () as SC_OffensiveMove).effectOnOpponent.health);

        }

    }

}
