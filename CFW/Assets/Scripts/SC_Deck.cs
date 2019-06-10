using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using Card;

public class SC_Deck : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public List<SC_BaseCard> cards;

    public int Size { get { return cards.Count; } }

    public TextMeshProUGUI TSize;

    static SC_GameManager GM { get { return SC_GameManager.Instance; } }

    static SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    bool Local { get; set; }

    RectTransform RectT { get { return transform as RectTransform; } }

    public void Setup (bool local) {

        Local = local;

        Draw(GM.startHandSize);

        if (!local) {

            RectT.anchorMin = Vector2.up;
            RectT.anchorMax = Vector2.up;
            RectT.anchoredPosition = new Vector2(-RectT.anchoredPosition.x, RectT.anchoredPosition.y);
            RectT.localRotation = Quaternion.Euler(0, 0, 180);
            TSize.rectTransform.localRotation = Quaternion.Euler(0, 0, -180);

        }

        TSize.gameObject.SetActive(false);

    }

    public void Draw (int nbr, bool tween = false) {

        for (int i = 0; i < nbr; i++)
            Draw(tween);

    }

    public static void OrganizeHand (RectTransform rT) {

        for (int i = 0; i < rT.childCount; i++)
            rT.GetChild(i).transform.localPosition = new Vector3((((rT.childCount - 1) / 2f) - i) * (GM.cardWidth / 2).F(rT.childCount % 2 == 0), 108.I(rT == GM.otherHand), 0);

    }

    void Draw (bool tween = false) {

        RectTransform rT = Local ? GM.localHand : GM.otherHand;

        SC_UI_Card c = Instantiate(Resources.Load<SC_UI_Card>("Prefabs/Card"), Vector3.zero, Local ? Quaternion.identity : Quaternion.Euler(0, 0, 180), rT);

        c.name = cards[0].Path;

        c.Card = Resources.Load<SC_BaseCard>(cards[0].Path);

        if (Local && !tween)
            c.SetImages();

        cards.RemoveAt(0);

        OrganizeHand(rT);

        if (tween) {

            c.Moving = true;

            Vector3 target = c.transform.localPosition;

            c.transform.position = transform.position;

            c.transform.DOLocalMove(target, GM.drawSpeed, true).OnComplete(() => { FinishDrawing(c); });
            c.transform.DORotate(Vector3.up * 90.I(Local), GM.drawSpeed / 2).OnComplete(() => { if(Local) c.SetImages(); });
            c.transform.DORotate(Local ? Vector3.zero : Vector3.forward * 180, GM.drawSpeed / 2).SetDelay(GM.drawSpeed / 2);

        }

        TSize.text = Size.ToString();

    }

    void FinishDrawing(SC_UI_Card c) {

        if (SC_Player.localPlayer.Turn) {

            SC_Player.localPlayer.CanPlay = true;

            UI.skipButton.SetActive(true);

        }

        c.Moving = false;        

    }

    public void Shuffle() {

        int[] newOrder = new int[cards.Count];

        for (int i = 0; i < newOrder.Length; i++)
            newOrder[i] = i;

        newOrder.Shuffle();

        SC_Player.localPlayer.CmdShuffleDeck(newOrder);

    }

    public void Shuffle(int[] newOrder) {

        List<SC_BaseCard> oldCards = new List<SC_BaseCard>(cards);

        for (int i = 0; i < newOrder.Length; i++)
            cards[i] = oldCards[newOrder[i]];

    }

    public void OnPointerEnter (PointerEventData eventData) {

        TSize.gameObject.SetActive(true);

    }

    public void OnPointerExit (PointerEventData eventData) {

        TSize.gameObject.SetActive(false);

    }

}
