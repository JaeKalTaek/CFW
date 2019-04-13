using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;
using DG.Tweening;

public class SC_GameManager : NetworkBehaviour {

    [Header("Match Heat")]
    public int startMatchHeat;

    public int MatchHeat { get; set; }

    public TextMeshProUGUI matchHeatText;

    [Header("Start hand size")]
    public int startHandSize;

    [Header("UI")]
    public RectTransform background;

    public GameObject connectingPanel, shifumiPanel, chooseTurnPanel, waitPanel;

    public RectTransform localHand, otherHand;

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

    public bool GameOn { get; set; }

    public bool FinishedDrawing { get; set; }

    void Awake () {

        connectingPanel.SetActive(true);

        Instance = this;

        SetMatchHeat(startMatchHeat);

        SetAllValues(true);
        SetAllValues(false);

        DOTween.Init(false, true, LogBehaviour.Verbose);

    }

    public void SetMatchHeat(int nMH) {

        MatchHeat = nMH;

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

        SC_Player.localPlayer.CmdStartGame(yes);

    }

    public void StartGame() {        

        chooseTurnPanel.SetActive(false);
        waitPanel.SetActive(false);

        GameOn = true;

        if (SC_Player.localPlayer.Turn)
            SC_Player.localPlayer.CmdDraw(1);

    }

}
