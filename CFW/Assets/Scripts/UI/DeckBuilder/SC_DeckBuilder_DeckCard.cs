using Card;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SC_DeckBuilder_DeckCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    public float bigSize;

    public RectTransform bigRec;

    RectTransform RecT { get { return transform as RectTransform; } }

    void Start () {

        GetComponent<Image> ().sprite = Resources.Load<Sprite> (Card.Path);

        RecT.sizeDelta = new Vector2 (0, RecT.rect.width * (260f/190f));

        bigRec.GetComponent<Image> ().sprite = GetComponent<Image> ().sprite;

        bigRec.anchorMin = new Vector2 (1, -.5f);

        bigRec.anchorMax = new Vector2 (1 + bigSize, bigSize - .5f);

        bigRec.anchoredPosition = Vector2.zero;

    }

    public void OnPointerEnter (PointerEventData eventData) {

        bigRec.SetParent (transform.parent);
        bigRec.SetAsLastSibling ();
        bigRec.gameObject.SetActive (true);

    }

    public void OnPointerExit (PointerEventData eventData) {

        bigRec.gameObject.SetActive (false);
        bigRec.SetParent (transform);

    }

    public void OnPointerClick (PointerEventData eventData) {

        OnPointerExit (eventData);

        SC_DeckBuilder.TryAddRemove (Card, false);

    }

}
