using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SC_GameManager : NetworkBehaviour {

    [Header("Match Heat")]
    public int startMatchHeat;

    public int MatchHeat { get; set; }

    public TextMeshProUGUI matchHeatText;

    [Header("Deck Sizes")]
    public TextMeshProUGUI localDeckSize;
    public TextMeshProUGUI otherDeckSize;

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
