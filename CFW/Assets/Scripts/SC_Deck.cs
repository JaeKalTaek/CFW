using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class SC_Deck : MonoBehaviour {

	public List<SC_BaseCard> cards;

    public int Size { get { return cards.Count; } }

    SC_GameManager GM { get { return SC_GameManager.Instance; } }

    public void Setup(bool local) {

        Draw(GM.startHandSize, local);

    }

    public void Draw(int nbr, bool local, bool tween = false) {

        for (int i = 0; i < nbr; i++)
            Draw(local, tween);

    }

    void Draw (bool local, bool tween = false) {

        RectTransform rT = local ? GM.localHand : GM.otherHand;

        SC_UI_Card c = Instantiate(Resources.Load<SC_UI_Card>("Prefabs/Card"), Vector3.zero, local ? Quaternion.identity : Quaternion.Euler(0, 0, 180), rT);

        c.name = cards[0].Path;

        c.Card = Resources.Load<SC_BaseCard>(cards[0].Path);

        if (local && !tween)
            c.GetComponent<Image>().sprite = Resources.Load<Sprite>(c.Card.Path);

        cards.RemoveAt(0);

        for (int i = 0; i < rT.childCount; i++)
            rT.GetChild(i).transform.localPosition = new Vector3((((rT.childCount - 1) / 2f) - i) * (rT.childCount % 2 == 0 ? 1 : -1) * (GM.cardWidth / 2), 130 * (local ? -1 : 1), 0);

        if (tween) {

            c.Moving = true;

            Vector3 target = c.transform.localPosition;

            c.transform.position = (local ? GM.localDeck : GM.otherDeck).position;

            c.transform.DOLocalMove(target, GM.drawSpeed, true).OnComplete(() => { FinishDrawing(c); });
            c.transform.DORotate(Vector3.up * 90, GM.drawSpeed / 2).OnComplete(() => { if(local) c.GetComponent<Image>().sprite = Resources.Load<Sprite>(c.Card.Path); });
            c.transform.DORotate(Vector3.zero, GM.drawSpeed / 2).SetDelay(GM.drawSpeed / 2);

        }

        (local ? GM.localDeckSize : GM.otherDeckSize).text = Size.ToString();

    }

    void FinishDrawing(SC_UI_Card c) {

        GM.FinishedDrawing = true;

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

}
