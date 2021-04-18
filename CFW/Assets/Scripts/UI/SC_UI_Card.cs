using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card;
using static SC_Player;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    public bool Moving { get; set; }

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public RectTransform RecT { get { return transform as RectTransform; } }

    RectTransform BigRec { get { return bigCard.transform as RectTransform; } }

    public GameObject bigCard;

    bool Local { get { return transform.parent.name.Contains("Local"); } }

    int prevSiblingIndex;

    void Awake() {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;

        BigRec.anchoredPosition += Vector2.up * GM.yOffset;

    }

    public void SetImages () {

        GetComponent<Image>().sprite = bigCard.GetComponent<Image>().sprite = Resources.Load<Sprite> (Card.Path);

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if (!Moving && Local && !localPlayer.Busy) {

            bigCard.transform.SetParent (transform.parent);

            bigCard.transform.SetAsLastSibling ();

            bigCard.SetActive(true);      

        }

    }

    public void OnPointerExit (PointerEventData eventData) {

        if (!Moving && Local && !localPlayer.Busy) {

            bigCard.transform.SetParent (transform);

            bigCard.SetActive (false);

        }

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (Local && Card.CanUse () && localPlayer.CanPlay) {

            OnPointerExit (new PointerEventData (EventSystem.current));

            UI.skipButton.SetActive (false);            

            localPlayer.CanPlay = false;

            localPlayer.Busy = true;                       

            SC_OffensiveMove om = Card as SC_OffensiveMove;

            if (om && (om.effectOnOpponent.bodyPartDamage.otherBodyPart != SC_Global.BodyPart.None) && !om.effectOnOpponent.bodyPartDamage.both) {

                GM.UsingCardID = name;              

                UI.bodyPartDamageChoice.firstChoice.text = om.effectOnOpponent.bodyPartDamage.bodyPart.ToString ();

                UI.bodyPartDamageChoice.secondChoice.text = om.effectOnOpponent.bodyPartDamage.otherBodyPart.ToString ();

                UI.bodyPartDamageChoice.panel.SetActive (true);

            } else
                localPlayer.UseCardServerRpc (name, false);

        }

    }

}
