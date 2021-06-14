using System.Collections;
using UnityEngine;

namespace Card {

    public class SC_DestroyTheBackstory : SC_BaseCard {

        OnRingClickedDelegate onRingClicked;

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            foreach (SC_BaseCard c in (user.IsLocalPlayer ? UI.otherRingSlots : UI.localRingSlots)[0].slot.parent.GetComponentsInChildren<SC_BaseCard> ())
                if (c.Is (SC_Global.CardType.Mytho))
                    goto HasTarget;

            return false;
            
            HasTarget:

            return base.CanUse (user, ignorePriority, ignoreLocks);

        }

        protected override IEnumerator ApplyEffects () {

            yield return StartCoroutine (base.ApplyEffects ());

            ApplyingEffects = true;

            if (Caller.IsLocalPlayer)
                UI.ShowMessage ("DestroyBackstory");

            onRingClicked = (c) => {

                if (c.Caller != Caller && c.Is (SC_Global.CardType.Mytho))
                    Caller.SetIntChoiceServerRpc ("DestroyBackstory", c.UICard.transform.GetSiblingIndex ());

            };

            OnRingClickedEvent += onRingClicked;

            Caller.IntChoices["DestroyBackstory"] = -1;

            while (Caller.IntChoices["DestroyBackstory"] == -1)
                yield return new WaitForEndOfFrame ();

            OnRingClickedEvent -= onRingClicked;

            UI.messagePanel.SetActive (false);

            (Caller.IsLocalPlayer ? UI.otherRingSlots : UI.localRingSlots)[0].slot.parent.GetChild (Caller.IntChoices["DestroyBackstory"]).GetComponent<SC_UI_Card> ().ToGraveyard (1, AppliedEffects, false);

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

        }

    }

}
