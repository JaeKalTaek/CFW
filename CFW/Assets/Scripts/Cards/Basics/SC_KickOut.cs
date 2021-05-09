using System.Collections;

namespace Card {

    public class SC_KickOut : SC_BaseCard {

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            GM.AddMatchHeat (GM.count);

        }

    }

}
