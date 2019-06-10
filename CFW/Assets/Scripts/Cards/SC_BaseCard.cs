using UnityEngine;
using static SC_Global;

namespace Card {

    public abstract class SC_BaseCard : MonoBehaviour {

        static SC_Player Player { get { return SC_Player.localPlayer; } }

        protected static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

        protected static SC_GameManager GM { get { return SC_GameManager.Instance; } }

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

                return s + "/" + name;

            }
        }

        string TypeToString (int i) {

            return types[i].ToString() + (types.Length == i + 1 ? "s" : "");

        }

        public virtual void Use (GameObject caller) {

            Player.Turn ^= true;

            if(!Player.Turn) {

                Player.Busy = false;

                Player.CanPlay = false;

                UI.bodyPartDamageChoice.panel.SetActive(false);

            } else {

                Player.CmdDraw(1);

            }

        }

    }

}
