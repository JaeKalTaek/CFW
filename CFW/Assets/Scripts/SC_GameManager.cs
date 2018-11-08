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
    public float cardWidth;

    public TextMeshProUGUI localDeckSize, otherDeckSize;

    public RectTransform localHand, otherHand;

    public static SC_GameManager Instance;

    void Awake () {

        Instance = this;

        SetMatchHeat(startMatchHeat);

    }

    public void SetMatchHeat(int nMH) {

        MatchHeat = nMH;

        matchHeatText.text = MatchHeat.ToString();

    }

}
