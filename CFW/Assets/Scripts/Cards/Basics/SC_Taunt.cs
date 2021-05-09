using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_Taunt : SC_BaseCard {

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            Caller.ApplySingleEffect ("Alignment", Mathf.Clamp (Caller.Alignment, -1, 1));

        }

    }

}
