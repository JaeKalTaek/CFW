using System.Collections;
using Card;

public class SC_RollOutOfDanger : SC_BaseCard {

    protected override IEnumerator ApplyEffects () {

        yield return StartCoroutine (base.ApplyEffects ());             

        SC_OffensiveMove c = respondedCards.Peek () as SC_OffensiveMove;

        c.PayCost ();

        Receiver.ApplySingleEffect ("Health", -c.effectOnOpponent.health);

    }

}
