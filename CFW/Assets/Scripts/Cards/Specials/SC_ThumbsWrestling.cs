using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_ThumbsWrestling : SC_BaseCard {

        bool finished;

        protected override IEnumerator ApplyEffects () {

            SC_Player.shifumiAction = (b) => { StartCoroutine (ShiFuMiEnded (b)); };

            SC_ShiFuMiChoice.Show ();

            while (!finished)
                yield return new WaitForEndOfFrame ();

        }

        IEnumerator ShiFuMiEnded (bool b) {

            GM.shifumiPanel.SetActive (false);

            SetCurrentEffect (new CommonEffect (CommonEffectType.Assess, true), Caller.IsLocalPlayer == b ? Receiver : Caller);

            yield return StartCoroutine (ApplyEffect (Assess));

            ApplyingEffects = true;

            SetCurrentEffect (CurrentEffect = new CommonEffect (CommonEffectType.Draw, v: 2), effectTarget = Caller.IsLocalPlayer != b ? Receiver : Caller);

            yield return StartCoroutine (ApplyEffect (Draw));

            finished = true;

        }

    }

}
