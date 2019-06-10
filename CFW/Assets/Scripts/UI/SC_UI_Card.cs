using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    public bool Moving { get; set; }

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    RectTransform RecT { get { return transform as RectTransform; } }

    RectTransform BigRec { get { return bigCard.transform as RectTransform; } }

    public GameObject bigCard;

    bool Local { get { return transform.parent.name.Contains("Local"); } }

    int prevSiblingIndex;

    void Awake() {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;

        BigRec.anchoredPosition = Vector2.up * 400.5f;

    }

    public void SetImages () {

        Sprite s = Resources.Load<Sprite>(Card.Path);

        GetComponent<Image>().sprite = s;

        bigCard.GetComponent<Image>().sprite = s;

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if (!Moving && Local && !SC_Player.localPlayer.Busy) {

            bigCard.SetActive(true);

            prevSiblingIndex = RecT.GetSiblingIndex();

            RecT.SetAsLastSibling();           

        }

    }

    public void OnPointerExit (PointerEventData eventData) {

        if (!Moving && Local && !SC_Player.localPlayer.Busy) {

            bigCard.SetActive(false);

            RecT.SetSiblingIndex(prevSiblingIndex);

        }

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (Local && (GM.MatchHeat >= Card.matchHeat) && SC_Player.localPlayer.CanPlay) {

            UI.skipButton.SetActive(false);

            SC_Player.localPlayer.CanPlay = false;

            SC_OffensiveMove om = Card as SC_OffensiveMove;

            if(om && (om.effectOnOpponent.bodyPartDamage.otherBodyPart != SC_Global.BodyPart.None) && !om.effectOnOpponent.bodyPartDamage.both) {

                GM.UsingCardID = name;

                SC_Player.localPlayer.Busy = true;

                UI.bodyPartDamageChoice.firstChoice.text = om.effectOnOpponent.bodyPartDamage.bodyPart.ToString();

                UI.bodyPartDamageChoice.secondChoice.text = om.effectOnOpponent.bodyPartDamage.otherBodyPart.ToString();

                UI.bodyPartDamageChoice.panel.SetActive(true);

            } else
                SC_Player.localPlayer.CmdUseBaseCard(SC_Player.localPlayer.gameObject, name);

        }

    }

}
