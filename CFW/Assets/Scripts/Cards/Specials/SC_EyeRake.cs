using UnityEngine;

namespace Card {

    public class SC_EyeRake : SC_BaseCard {

        public override void ApplyEffect () {            

            base.ApplyEffect ();

            if (Other.Hand.Count > 0) {

                ApplyingEffects = true;

                if (Other.IsLocalPlayer)
                    SC_Player.localPlayer.DiscardServerRpc (Other.Hand[Random.Range (0, Other.Hand.Count)].Path);

            }

        }

    }

}
