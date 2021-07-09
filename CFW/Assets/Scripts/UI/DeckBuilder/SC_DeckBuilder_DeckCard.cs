using Card;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SC_DeckBuilder_DeckCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    public float bigSize;

    public RectTransform RecT, bigRec;

    public TextMeshProUGUI nameText;

    public Image bigImage;

    void Start () {

        nameText.text = Card.name;

        bigImage.sprite = Resources.Load<Sprite> (Card.Path);

        bigRec.sizeDelta = new Vector2 (bigSize, bigSize * (260f/190));

        if ((bigRec.parent.parent as RectTransform).anchoredPosition.x + ((bigRec.parent.parent as RectTransform).sizeDelta.x / 2) + bigRec.rect.width > 1920) {

            bigRec.anchorMin = bigRec.anchorMax = Vector2.up * .5f;

            bigRec.pivot = new Vector2 (1, .5f);

        }

    }

    public void OnPointerEnter (PointerEventData eventData) {

        bigRec.SetParent (SC_DeckBuilder.Instance.deckCardsPreviewParent);
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
