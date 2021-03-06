using static SC_UI_Manager;

namespace Card {

    public class SC_ReboundClothesline : SC_OffensiveMove {

        public override bool CanUse (SC_Player user, bool ignorePriority = false, bool ignoreLocks = false) {

            if (base.CanUse (user, ignorePriority, ignoreLocks))
                return true;
            else {

                stayOnRing = false;

                if (base.CanUse (user, ignorePriority, ignoreLocks))
                    return true;
                else {

                    stayOnRing = true;

                    return false;

                }

            }

        }

        public override void OnRingClicked () {

            base.OnRingClicked ();

            int slot = RingSlot;

            RingSlot = -1;

            if (CanUse (Caller)) {

                UI.basicsPanel.SetActive (false);

                RingSlot = slot;

                StartCoroutine (StartPlaying ());

            } else
                RingSlot = slot;

        }

        public override void Play (SC_Player c) {

            if (OnTheRing) {

                stayOnRing = false;

                additionalCardsToModify.Remove (this);

            } else {

                foreach (RingSlot r in (c.IsLocalPlayer ? UI.localRingSlots : UI.otherRingSlots))
                    if (!r.occupied)
                        goto OpenSlot;

                stayOnRing = false;

            }
        OpenSlot:

            if (stayOnRing)
                additionalCardsToModify.Add (this);

            base.Play (c);

        }

    }

}
