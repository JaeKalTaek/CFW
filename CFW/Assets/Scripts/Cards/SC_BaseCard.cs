using DG.Tweening;
using UnityEngine;
using static SC_Global;
using static SC_Player;

namespace Card {

    public abstract class SC_BaseCard : MonoBehaviour {

        protected static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

        protected static SC_GameManager GM { get { return SC_GameManager.Instance; } }

        public SC_UI_Card UICard { get; set; }

        [Header("Base Card Variables")]
        [Tooltip("The minimum Match Heat required to play this card")]
        public int matchHeat;

        [Tooltip("Types of this card")]
        public CardType[] types;

        public string Path {

            get {

                string s = TypeToString(0);

                for (int i = 1; i < types.Length; i++)
                    s += " " + TypeToString(i);

                return s + "/" + name.Replace("(Clone)", "");                

            }

        }

        string TypeToString (int i) {

            return types[i].ToString() + (types.Length == i + 1 ? "s" : "");

        }

        public virtual bool CanUse () {

            return GM.MatchHeat >= matchHeat;

        }

        public virtual void Use (SC_Player caller) {

            caller.Hand.Remove (this);

            UICard.Moving = true;

            localPlayer.Busy = true;

            UICard.transform.SetParent (UICard.transform.parent.parent);

            SC_Deck.OrganizeHand (caller.IsLocalPlayer ? GM.localHand : GM.otherHand);

            UICard.RecT.anchoredPosition3D = Vector3.up * (caller.IsLocalPlayer ? UICard.RecT.sizeDelta.y / 2 : (GM.transform as RectTransform).sizeDelta.y - UICard.RecT.sizeDelta.y / 2);

            UICard.RecT.DOLocalMove (Vector3.zero, 1);

            DOTween.Sequence ().Append (UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta * 1.5f, 1))
                .OnComplete (() => {
                    ApplyEffect (caller);
                    FinishUse (caller);
                });

            if (!caller.IsLocalPlayer) {

                DOTween.Sequence ().Append (UICard.transform.DORotate (Vector3.up * 90, .5f)
                    .OnComplete (() => {
                        UICard.SetImages ();
                        UICard.RecT.rotation = Quaternion.Euler (Vector3.up * 90);
                    }))
                    .Append (UICard.transform.DORotate (Vector3.zero, .5f));

            }            

        }

        void FinishUse (SC_Player caller) {

            UICard.RecT.transform.SetParent ((caller.IsLocalPlayer ? GM.localGraveyard : GM.otherGraveyard).transform);

            UICard.RecT.anchorMin = UICard.RecT.anchorMax = Vector2.one * .5f;

            UICard.RecT.DOSizeDelta (UICard.RecT.sizeDelta / 1.5f, 1);

            UICard.RecT.DOAnchorPos (Vector2.zero, 1).OnComplete (() => {

                localPlayer.Busy = false;

                if (caller.IsLocalPlayer)
                    GM.SkipTurn ();

            });

        }

        public abstract void ApplyEffect (SC_Player caller);

    }

}
