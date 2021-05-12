using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_EyeRake : SC_BaseCard {

        public override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            if (Receiver.Hand.Count > 0) {

                ApplyingEffects = true;

                yield return StartCoroutine (ApplyEffect (() => {

                    if (Receiver.IsLocalPlayer)
                        SC_Player.localPlayer.DiscardServerRpc (Receiver.Hand[Random.Range (0, Receiver.Hand.Count)].Path);

                }));                

            }

        }

    }

}
