using TMPro;
using UnityEngine;
using UnityEngine.Networking;

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

    }

    public void SetMatchHeat(int nMH) {

        MatchHeat = nMH;

        matchHeatText.text = MatchHeat.ToString();

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
