using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SC_Global;

public class SC_ShiFuMiChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public ShiFuMi choice;

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    Image image;

    static SC_ShiFuMiChoice[] choices;

    bool on, locked;

    void Start() {

        if(choices == null)
            choices = transform.parent.GetComponentsInChildren<SC_ShiFuMiChoice>();

        image = GetComponent<Image>();

        image.color = GM.baseShiFuMiChoiceColor;

    }

    public void OnPointerEnter (PointerEventData eventData) {

        on = true;

        if(!locked)
            image.color = Color.white;

    }

    public void OnPointerExit (PointerEventData eventData) {

        on = false;

        if(!locked)
            image.color = GM.baseShiFuMiChoiceColor;

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (!locked) {

            foreach (SC_ShiFuMiChoice c in choices) {

                c.locked = true;

                if(c != this)
                    c.image.color = GM.lockedChoiceColor;

            }

            SC_Player.localPlayer.CmdShiFuMiChoice((int)choice);

        }

    }

    public static void Draw () {

        SC_GameManager.Instance.shifumiText.text = "Draw ! Choose again !";

        foreach (SC_ShiFuMiChoice c in choices) {

            c.locked = false;

            c.image.color = c.on ? Color.white : SC_GameManager.Instance.baseShiFuMiChoiceColor;

        }

    }

}
