using System;
using TMPro;
using UnityEngine;
using static SC_Global;
using static SC_Player;
using DG.Tweening;

public class SC_GameManager : MonoBehaviour {

    [Header ("Match Heat")]
    public int startMatchHeat;
    public int maxMatchHeat, resetMatchHeat;

    public int MatchHeat { get; set; }

    public TextMeshProUGUI matchHeatText;

    [Header("Hand sizes")]
    public int startHandSize;
    public int maxHandSize;

    [Header ("UI")]
    public RectTransform background;
    public GameObject waitingPanel, shifumiPanel, chooseTurnPanel, waitPanel;

    public RectTransform localHand, otherHand;

    public SC_Graveyard localGraveyard, otherGraveyard;

    [Header("Cards")]
    public float cardWidth;

    public float yOffset, enlargeCardFactor, drawSpeed, playedSizeMultiplicator;

    [Header ("Values")]
    [Header ("Base Values")]
    public int baseHealth;

    public int baseStamina, baseBodyPartHealth;

    [Header ("Other Values")]
    public int staminaPerTurn;

    public int maxAlignment;

    public float responseTime;

    [Header("ShiFuMi")]
    public TextMeshProUGUI shifumiText;

    public Color baseShiFuMiChoiceColor, lockedChoiceColor;    

    public static SC_GameManager Instance;

    SC_UI_Manager UI { get { return SC_UI_Manager.Instance; } }

    public int Count { get; set; }

    void Start () {

        waitingPanel.SetActive (true);

        Instance = this;        

        AddMatchHeat (startMatchHeat);

        SetAllValues (true);
        SetAllValues (false);

        DOTween.Init (false, true, LogBehaviour.Verbose);

    }

    public void AddMatchHeat (int gain, bool canReset = false) {

        MatchHeat = canReset && MatchHeat == maxMatchHeat ? resetMatchHeat : Mathf.Min (maxMatchHeat, MatchHeat + gain);

        matchHeatText.text = MatchHeat.ToString();

    }    

    void SetAllValues (bool local) {

        UI.SetValue (local, "Health", baseHealth);

        UI.SetValue (local, "Stamina", baseStamina);

        UI.SetValue (local, "Alignment", 0);

        foreach (BodyPart bP in Enum.GetValues(typeof(BodyPart)))
            if (bP != BodyPart.None)
                UI.SetValue (local, bP.ToString(), baseBodyPartHealth);

    }

    public void ShowTurnPanel(bool choose) {

        (choose ? chooseTurnPanel : waitPanel).SetActive (true);

    }

    public void ChooseStartTurn(bool yes) {

        chooseTurnPanel.SetActive (false);

        localPlayer.StartGameServerRpc (yes);

    }

}
