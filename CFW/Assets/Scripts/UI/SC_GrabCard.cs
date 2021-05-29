using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SC_GrabCard : MonoBehaviour, IPointerClickHandler {

    int origin;

    public void SetOrigin (int o) {

        origin = o;

        transform.parent.GetComponentInChildren<TextMeshProUGUI> ().text = o == 0 ? "From your deck" : (o == 1 ? "From your discard" : "From opponent's discard");

    }

    public void OnPointerClick (PointerEventData eventData) {

        SC_UI_Manager.Instance.grabUI.panel.SetActive (false);

        SC_Player.localPlayer.SetIntChoiceServerRpc ("Grab", origin);

        SC_Player.localPlayer.SetStringChoiceServerRpc ("Grab", name);

    }

}
