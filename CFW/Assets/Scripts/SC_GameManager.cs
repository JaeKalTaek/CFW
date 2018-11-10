using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static SC_Global;

public class SC_GameManager : NetworkBehaviour {

    [Header("Match Heat")]
    public int startMatchHeat;

    public int MatchHeat { get; set; }

    public TextMeshProUGUI matchHeatText;

    [Header("Start hand size")]
    public int startHandSize;

    [Header("UI")]
    public GameObject connectingPanel;
        
    public GameObject shifumiPanel, chooseTurnPanel, waitPanel;

    public TextMeshProUGUI localDeckSize, otherDeckSize;

    public RectTransform localHand, otherHand;

    [Header("Cards")]
    public float cardWidth;    

    public float enlargeCardFactor;

    [Header("Values")]
    public int baseHealth;

    public int baseStamina, baseBodyPartHealth;

    public Transform localValues, otherValues;

    [Header("ShiFuMi")]
    public TextMeshProUGUI shifumiText;

    public Color baseShiFuMiChoiceColor, lockedChoiceColor;    

    public static SC_GameManager Instance;

    public bool GameOn { get; set; }

    public bool AlreadyDrew { get; set; }

    void Awake () {

        connectingPanel.SetActive(true);

        Instance = this;

        SetMatchHeat(startMatchHeat);

        SetAllValues(true);
        SetAllValues(false);

    }

    void Update () {

        if (Input.GetButtonDown("Draw"))
            TryDraw();

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

        foreach (BodyPart bP in Enum.GetValues(typeof(BodyPart)))
            if (bP != BodyPart.None)
                SetValue(local, bP.ToString(), baseBodyPartHealth);

    }

    public void ShowTurnPanel(bool choose) {

        (choose ? chooseTurnPanel : waitPanel).SetActive(true);

    }

    public void ChooseStartTurn(bool yes) {

        SC_Player.localPlayer.CmdSetStartTurn(yes);

    }

    public void StartGame() {

        GameOn = true;
        chooseTurnPanel.SetActive(false);
        waitPanel.SetActive(false);

    }

    public void TryDraw() {

        if(SC_Player.localPlayer.Turn && !AlreadyDrew) {

            AlreadyDrew = true;

            SC_Player.localPlayer.CmdDraw(1);

        }

    }

}
