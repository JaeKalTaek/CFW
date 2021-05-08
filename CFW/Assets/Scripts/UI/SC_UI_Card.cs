using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Card;
using static SC_Player;
using DG.Tweening;
using System;
using static Card.SC_BaseCard;
using static SC_Global;

public class SC_UI_Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    public SC_BaseCard Card { get; set; }

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public RectTransform RecT { get { return transform as RectTransform; } }

    public RectTransform BigRec { get { return bigCard.transform as RectTransform; } }

    public GameObject bigCard;

    bool Local { get { return transform.parent.name.Contains ("Local"); } }

    bool IsBasic { get { return Card.Is (SC_Global.CardType.Basic); } }

    void Awake () {

        BigRec.sizeDelta = RecT.sizeDelta * GM.enlargeCardFactor;

        Card = GetComponentInChildren<SC_BaseCard> ();

        if (!Card)
            BigRec.anchoredPosition += Vector2.up * GM.yOffset;

    }

    public void SetImages () {

        GetComponent<Image> ().sprite = bigCard.GetComponent<Image> ().sprite = Resources.Load<Sprite> (Card.Path);

    }

    public void OnPointerEnter (PointerEventData eventData) {

        if ((IsBasic || localPlayer.Hand.Contains (Card) || lockingCard == Card) && (!localPlayer.Busy || localPlayer.ChoosingCard != ChoosingCard.None)) {

            bigCard.transform.SetParent (transform.parent);

            bigCard.transform.SetAsLastSibling ();

            bigCard.SetActive (true);

        }

    }

    public void OnPointerExit (PointerEventData eventData) {

        if (bigCard.activeSelf) {

            bigCard.transform.SetParent (transform);

            bigCard.SetActive (false);

        }

    }

    public void OnPointerClick (PointerEventData eventData) {

        if (activeCard != Card) {

            if (localPlayer.ChoosingCard == ChoosingCard.DoubleTapping && localPlayer.GetStringChoice ("DoubleTapping") == "") {

                localPlayer.SetStringChoiceServerRpc ("DoubleTapping", name);

            } else if (localPlayer.ChoosingCard != ChoosingCard.None && (localPlayer.ChoosingCard != ChoosingCard.DoubleTapping || name != localPlayer.GetStringChoice ("DoubleTapping"))) {

                OnPointerExit (new PointerEventData (EventSystem.current));

                UI.messagePanel.SetActive (false);

                if (localPlayer.ChoosingCard == ChoosingCard.Discarding)
                    localPlayer.DiscardServerRpc (name);
                else if (localPlayer.ChoosingCard == ChoosingCard.Assessing) {

                    localPlayer.SetStringChoiceServerRpc ("Assess", name);

                    activeCard.MakingChoices = false;

                } else {

                    localPlayer.SetStringChoiceServerRpc ("DoubleTapping2", name);

                    activeCard.MakingChoices = false;

                }

                localPlayer.ChoosingCard = ChoosingCard.None;

            } else if (bigCard.activeSelf && Card.CanUse ()) {

                OnPointerExit (new PointerEventData (EventSystem.current));

                foreach (GameObject g in new GameObject[] { UI.basicsPanel, UI.showBasicsButton, UI.showLockedBasicsButton, UI.hideBasicsButton, UI.hideLockedBasicsButton })
                    g.SetActive (false);

                localPlayer.Busy = true;

                if (IsBasic) {

                    GM.localHand.gameObject.SetActive (true);

                    localPlayer.StartUsingBasicServerRpc (transform.GetSiblingIndex ());

                } else
                    StartCoroutine (Card.StartUsing ());

            }

        }

    }

    public void Flip (bool flip, float speed) {

        if (flip)
            DOTween.Sequence ().Append (transform.DORotate (Vector3.up * 90, speed)
                .OnComplete (() => { SetImages (); }))
                .Append (transform.DORotate (Vector3.zero, speed));

    }

    public void ToGraveyard (float speed, Action action, bool? flip = null) {

        transform.SetParent ((Card.Caller.IsLocalPlayer ? GM.localGraveyard : GM.otherGraveyard).transform);

        RecT.anchorMin = RecT.anchorMax = RecT.pivot = Vector2.one * .5f;

        Flip (flip ?? !Card.Caller.IsLocalPlayer, speed / 2);

        RecT.DOAnchorPos (Vector2.zero, speed).OnComplete (() => { action (); });

    }

}
