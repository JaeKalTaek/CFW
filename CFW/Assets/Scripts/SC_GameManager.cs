using System;
using TMPro;
using UnityEngine;
using static SC_Global;
using DG.Tweening;

public class SC_GameManager : MonoBehaviour {

    [Header("Match Heat")]
    public int startMatchHeat, maxMatchHeat, resetMatchHeat;

    public int MatchHeat { get; set; }

    public TextMeshProUGUI matchHeatText;

    [Header("Hand sizes")]
    public int startHandSize;
    public int maxHandSize;

    [Header ("UI")]
    public RectTransform background;
    public GameObject waitingPanel, deckChoicePanel, shifumiPanel, chooseTurnPanel, waitPanel;
    public TMP_Dropdown deckChoice;

    public RectTransform localHand, otherHand;

    public SC_Graveyard localGraveyard, otherGraveyard;

    [Header("Cards")]
    public float cardWidth;    

    public float enlargeCardFactor;

    public float drawSpeed;

    [Header("Values")]
    public int baseHealth;

    public int baseStamina, baseBodyPartHealth;

    public Transform localValues, otherValues;

    [Header("ShiFuMi")]
    public TextMeshProUGUI shifumiText;

    public Color baseShiFuMiChoiceColor, lockedChoiceColor;    

    public static SC_GameManager Instance;

    public string UsingCardID { get; set; }

    void Awake () {

        Instance = this;

        AddMatchHeat(startMatchHeat);

        SetAllValues(true);
        SetAllValues(false);

        DOTween.Init(false, true, LogBehaviour.Verbose);

    }

    public void AddMatchHeat(int gain) {

        MatchHeat = MatchHeat == maxMatchHeat ? resetMatchHeat : Mathf.Min(maxMatchHeat, MatchHeat + gain);

        matchHeatText.text = MatchHeat.ToString();

    }

    public void SetValue(bool local, string id, int value) {

        (local ? localValues : otherValues).Find(id).GetChild(1).GetComponent<TextMeshProUGUI>().text = value.ToString();

    }

    void SetAllValues(bool local) {

        SetValue(local, "Health", baseHealth);

        SetValue(local, "Stamina", baseStamina);

        SetValue(local, "Alignment", 0);

        foreach (BodyPart bP in Enum.GetValues(typeof(BodyPart)))
            if (bP != BodyPart.None)
                SetValue(local, bP.ToString(), baseBodyPartHealth);

    }

    public void ShowTurnPanel(bool choose) {

        (choose ? chooseTurnPanel : waitPanel).SetActive(true);

    }

    public void ChooseStartTurn(bool yes) {

        SC_Player.localPlayer.StartGameServerRpc (yes);

    }

    public void StartGame() {        

        chooseTurnPanel.SetActive(false);
        waitPanel.SetActive(false);

        if (SC_Player.localPlayer.Turn)
            SC_Player.localPlayer.DrawServerRpc (1, true);

    }

}
