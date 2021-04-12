using DG.Tweening;
using UnityEngine;
using static SC_Global;

namespace Card {

    public abstract class SC_BaseCard : MonoBehaviour {

        static SC_Player Player { get { return SC_Player.localPlayer; } }

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

            // UICard.transform.SetParent((caller == Player.gameObject ? GM.localGraveyard : GM.otherGraveyard).RecT);

            UICard.transform.SetParent(UICard.transform.parent.parent);

            float pos = UICard.RecT.sizeDelta.y * 2;

            Vector3 start = UICard.transform.position;

            UICard.RecT.anchoredPosition3D = Vector3.up * (Player.Turn ? pos : (GM.transform as RectTransform).sizeDelta.y - pos);

            Vector3 target = UICard.transform.position;

            UICard.transform.position = start;

            UICard.transform.DOMove(target, 1);

            /*if(!Player.Turn)
                UICard.transform.DORotate()*/

            // UICard.transform.DOMove(Vector3.up * 500, 1);

            // DOTween.Sequence().Append(UICard.transform.DOLocalMove(Vector3.up * 500, 1f)); //.Append(UICard.transform.DOMove(Vector3.up * 130, 1f));

            // UICard.transform.DOMove(Vector3.up * 500, 1f).OnCO

            /*Player.Turn ^= true;

            if(!Player.Turn) {

                Player.Busy = false;

                Player.CanPlay = false;

                UI.bodyPartDamageChoice.panel.SetActive(false);

            } else {

                Player.CmdDraw(1);

            }*/

        }

    }

}
