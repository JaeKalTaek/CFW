using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public SC_BaseCard Card { get; set; }

    public bool Moving { get; set; }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    RectTransform RecT { get { return transform as RectTransform; } }

    RectTransform BigRec { get { return bigCard.transform as RectTransform; } }

    public GameObject bigCard;

    bool Local { get { return transform.parent.name.Contains("Local"); } }

    int prevSiblingIndex;

    void Awake() {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;

        BigRec.anchoredPosition = Vector2.up * BigRec.sizeDelta.y / 2;

    }

    public void SetImages () {

        Sprite s = Resources.Load<Sprite>(Card.Path);

        GetComponent<Image>().sprite = s;

        bigCard.GetComponent<Image>().sprite = s;

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if (!Moving && Local) {

            bigCard.SetActive(true);

            prevSiblingIndex = RecT.GetSiblingIndex();

            RecT.SetAsLastSibling();           

        }

    }

    public void OnPointerExit (PointerEventData eventData) {

        if (!Moving && Local) {

            bigCard.SetActive(false);

            RecT.SetSiblingIndex(prevSiblingIndex);

        }

    }

}
