using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card;
using static SC_Player;
using DG.Tweening;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public RectTransform RecT { get { return transform as RectTransform; } }

    RectTransform BigRec { get { return bigCard.transform as RectTransform; } }

    public GameObject bigCard;

    bool Local { get { return transform.parent.name.Contains("Local"); } }

    bool IsBasic { get { return Card.Is (SC_Global.CardType.Basic); } }

    void Awake() {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;        

        Card = GetComponentInChildren<SC_BaseCard> ();

        if (!Card)
            BigRec.anchoredPosition += Vector2.up * GM.yOffset;

    }

    public void SetImages () {

        GetComponent<Image>().sprite = bigCard.GetComponent<Image>().sprite = Resources.Load<Sprite> (Card.Path);

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if ((IsBasic || localPlayer.Hand.Contains (Card)) && !localPlayer.Busy) {

            bigCard.transform.SetParent (transform.parent);

            bigCard.transform.SetAsLastSibling ();

            bigCard.SetActive(true);      

        }

    }

    public void OnPointerExit (PointerEventData eventData) {

        if ((IsBasic || localPlayer.Hand.Contains (Card)) && !localPlayer.Busy) {

            bigCard.transform.SetParent (transform);

            bigCard.SetActive (false);

        }

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (localPlayer.Assessing && SC_BaseCard.activeCard != Card) {

            localPlayer.Assessing = false;

            UI.assessPanel.SetActive (false);

            if (SC_BaseCard.activeCard.Is (SC_Global.CardType.Basic))
                localPlayer.UseBasicCardServerRpc (SC_BaseCard.activeCard.UICard.transform.GetSiblingIndex (), localPlayer.Hand.IndexOf (Card));
            else
                localPlayer.UseCardServerRpc (SC_BaseCard.activeCard.UICard.name, localPlayer.Hand.IndexOf (Card));

        } else if (IsBasic || (Local && Card.CanUse () && localPlayer.CanPlay)) {

            if (IsBasic)
                UI.basicsPanel.SetActive (false);                

            OnPointerExit (new PointerEventData (EventSystem.current));

            UI.skipButton.SetActive (false);            

            localPlayer.CanPlay = false;

            localPlayer.Busy = true;

            Card.StartUsing ();

        }

    }

    public void Flip (bool flip, float speed) {

        if (flip)
            DOTween.Sequence ().Append (transform.DORotate (Vector3.up * 90, speed)
                .OnComplete (() => { SetImages (); }))
                .Append (transform.DORotate (Vector3.zero, speed));

    }

}
