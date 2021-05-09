using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_EyeRake : SC_BaseCard {

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            if (Other.Hand.Count > 0) {

                ApplyingEffects = true;

                yield return StartCoroutine (ApplyEffect (() => {

                    if (Other.IsLocalPlayer)
                        SC_Player.localPlayer.DiscardServerRpc (Other.Hand[Random.Range (0, Other.Hand.Count)].Path);

                }));                

            }

        }

    }

}
