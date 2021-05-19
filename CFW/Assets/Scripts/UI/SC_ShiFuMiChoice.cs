using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static SC_Global;

public class SC_ShiFuMiChoice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public ShiFuMi choice;

    Image image;

    static SC_ShiFuMiChoice[] choices;

    bool on, locked;

    static Color baseColor = new Color (1, 1, 1, .5f);
    static Color lockedColor = new Color (.25f, .25f, .25f, 1);

    public static TextMeshProUGUI text;

    void Start() {

        if (choices == null) {

            choices = transform.parent.GetComponentsInChildren<SC_ShiFuMiChoice> ();

            text = choices[0].transform.parent.GetComponentInChildren<TextMeshProUGUI> ();

        }

        image = GetComponent<Image>();

        image.color = baseColor;

    }

    public static void Show () {

        Reset ();

        text.text = "Choose!";

        SC_GameManager.Instance.shifumiPanel.transform.SetAsLastSibling ();

        SC_GameManager.Instance.shifumiPanel.SetActive (true);

    }

    public void OnPointerEnter (PointerEventData eventData) {

        on = true;

        if(!locked)
            image.color = Color.white;

    }

    public void OnPointerExit (PointerEventData eventData) {

        on = false;

        if(!locked)
            image.color = baseColor;

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (!locked) {

            foreach (SC_ShiFuMiChoice c in choices) {

                c.locked = true;

                if(c != this)
                    c.image.color = lockedColor;

            }

            SC_Player.localPlayer.ShiFuMiChoiceServerRpc ((int) choice);

        }

    }

    static void Reset () {

        SC_Player.localPlayer.ShifumiChoice = ShiFuMi.None;

        SC_Player.otherPlayer.ShifumiChoice = ShiFuMi.None;

        foreach (SC_ShiFuMiChoice c in choices) {

            c.locked = false;

            c.image.color = c.on ? Color.white : baseColor;

        }

    }

    public static void Draw () {

        text.text = "Draw ! Choose again !";

        Reset ();

    }

}
