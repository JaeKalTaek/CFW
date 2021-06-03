using System.Collections;
using UnityEngine;
using static SC_UI_Manager;

namespace Card {

    public class SC_DestroyTheBackstory : SC_BaseCard {

        OnRingClickedDelegate onRingClicked;

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            foreach (RingSlot r in user.IsLocalPlayer ? UI.otherRingSlots : UI.localRingSlots)
                if (r.occupied)
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

            SC_BaseCard target = (Caller.IsLocalPlayer ? UI.otherRingSlots : UI.localRingSlots)[0].slot.parent.GetChild (Caller.IntChoices["DestroyBackstory"]).GetComponentInChildren<SC_BaseCard> ();

            (Caller.IsLocalPlayer ? UI.otherRingSlots : UI.localRingSlots)[target.RingSlot].occupied = false;

            target.DiscardedFromRing ();

            target.UICard.ToGraveyard (1, AppliedEffects, false);

            while (ApplyingEffects)
                yield return new WaitForEndOfFrame ();

        }

    }

}
