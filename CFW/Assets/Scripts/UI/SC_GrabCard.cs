using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Card.SC_BaseCard;

public class SC_GrabCard : MonoBehaviour, IPointerClickHandler {

    public Image image;

    public GameObject highlight;

    public TextMeshProUGUI text;

    int origin;

    bool selected;

    public void SetOrigin (int o) {

        origin = o;

        text.text = o == 0 ? "From your deck" : (o == 1 ? "From your discard" : "From opponent's discard");

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (selected || activeCard.GrabsRemaining > 0) {

            selected ^= true;

            activeCard.GrabsRemaining += selected ? -1 : 1;

            highlight.SetActive (selected);

            SC_Player.localPlayer.SetIntChoiceServerRpc ("Grab" + activeCard.GrabsRemaining, origin);

            SC_Player.localPlayer.SetStringChoiceServerRpc ("Grab" + activeCard.GrabsRemaining, name);

            SC_UI_Manager.Instance.GrabSelected ();

        }

    }

}
