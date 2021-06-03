using System.Collections;

namespace Card {

    public class SC_StrikeExchanges : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            forceFirstExchange = Caller;

        }

    }

}

