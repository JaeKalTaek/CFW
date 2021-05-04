using UnityEngine;

namespace Card {

    public class SC_FlipOff : SC_BaseCard {

        public override void ApplyEffect () {

            ApplyingEffects = true;

            base.ApplyEffect ();

            if (!Caller.IsLocalPlayer)
                UI.ShowMessage ("Discard");
                    //SC_Player.localPlayer.RandomDiscardServerRpc (Other.Hand[Random.Range (0, Other.Hand.Count)].Path);

        }

    }

}
