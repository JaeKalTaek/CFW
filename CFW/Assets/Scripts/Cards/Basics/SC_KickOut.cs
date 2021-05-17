using System.Collections;

namespace Card {

    public class SC_KickOut : SC_BaseCard {

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            GM.AddMatchHeat (GM.Count);

        }

    }

}
