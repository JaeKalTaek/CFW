using UnityEngine;
using UnityEngine.EventSystems;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public SC_BaseCard Card { get; set; }

    public bool Moving { get; set; }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    RectTransform RecT { get { return transform as RectTransform; } }

    int prevSiblingIndex;

    public void OnPointerEnter (PointerEventData eventData) {

        if (!Moving) {

            prevSiblingIndex = RecT.GetSiblingIndex();

            RecT.SetAsLastSibling();

            RecT.sizeDelta *= GM.enlargeCardFactor;

        }

    }

    public void OnPointerExit (PointerEventData eventData) {

        RecT.SetSiblingIndex(prevSiblingIndex);

        RecT.sizeDelta /= GM.enlargeCardFactor;

    }
}
