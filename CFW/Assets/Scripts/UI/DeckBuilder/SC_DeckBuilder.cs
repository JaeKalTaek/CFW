using Card;
using static Card.SC_BaseCard;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static SC_Global;
using System;

public class SC_DeckBuilder : MonoBehaviour {

    public static SC_DeckBuilder Instance;

    [Header("General deck settings")]
    public int deckSize;

    [Header ("Cards filtering")]
    public RectTransform searchCardsParent;

    public float searchCardSize;
    Vector2 size;    

    public GameObject moreFiltersPanel;
    public TextMeshProUGUI moreFiltersButtonText;

    public TextMeshProUGUI resultsCount;

    public static Dictionary<SC_BaseCard, SC_DeckBuilder_SearchCard> filteredCards;

    [Header ("Deck")]
    public RectTransform[] deckCardsColumns;

    public ScrollRect deckScrollRect;

    public RectTransform deckContent;

    public float deckCardHeight;

    public TextMeshProUGUI[] deckMatchHeatRepartitions;

    public TextMeshProUGUI deckCardsCount;    

    public SC_DecksManager deckManager;

    public Transform deckCardsPreviewParent;

    public static Dictionary<SC_BaseCard, SC_DeckBuilder_DeckCard> deckCards;

    #region Filters
    [Serializable]
    public class MinMaxFilter {

        public int min, max;

        public TMP_InputField minInput, maxInput;

        public int Min { get { int.TryParse (minInput.text, out int min); return min; } }

        public int Max { get { int.TryParse (maxInput.text, out int max); return max; } }

    }

    [Header ("Filters")]
    public MinMaxFilter matchHeatFilter;

    public TMP_Dropdown mainType, firstSubType, secondSubType;
    public Toggle mainTypeToggle, firstSubTypeToggle, secondSubTypeToggle;
    Toggle[] typeToggles;

    public TMP_InputField oracle;

    public TMP_Dropdown alignment;

    public Transform commonEffectsParent;
    Toggle[] commonEffects;

    public Toggle matchHeat;

    #region Attack card filters
    public MinMaxFilter attackCardStaminaCost;
    public TMP_Dropdown attackCardBodyPartsCost;
    public MinMaxFilter attackCardBodyPartsCostValue;
    public TMP_Dropdown attackCardBodyPartsDamage;
    public MinMaxFilter attackCardBodyPartsDamageValue;

    public MinMaxFilter offensiveMoveHealthCost, offensiveMoveStaminaDamage, offensiveMoveHealthDamage;
    public MinMaxFilter submissionStaminaReduction, submissionBreakCost;
    #endregion

    TMP_Dropdown[] dropdownFilters;

    MinMaxFilter[] minMaxFilters;
    #endregion    

    void Start () {        

        moreFiltersPanel.SetActive (false);

        Instance = this;

        deckCards = new Dictionary<SC_BaseCard, SC_DeckBuilder_DeckCard> ();

        size = Resources.Load<RectTransform> ("Prefabs/Cards/P_UI_Card").sizeDelta * searchCardSize;

        dropdownFilters = transform.GetComponentsInChildren<TMP_Dropdown> ();

        minMaxFilters = new MinMaxFilter[] { matchHeatFilter, attackCardStaminaCost, attackCardBodyPartsCostValue,
            attackCardBodyPartsDamageValue, offensiveMoveHealthCost, offensiveMoveStaminaDamage, offensiveMoveHealthDamage,
                submissionStaminaReduction, submissionBreakCost};

        #region Setup Min Max Filters
        for (int i = 1; i < minMaxFilters.Length; i++) {

            minMaxFilters[i].min = 50;
            minMaxFilters[i].max = 0;

        }

        #region Set minimum and maximum attack filter values
        foreach (SC_BaseCard c in allCards) {

            SC_AttackCard a = c as SC_AttackCard;

            if (a) {

                attackCardStaminaCost.min = Mathf.Min (attackCardStaminaCost.min, a.cost.stamina);
                attackCardStaminaCost.max = Mathf.Max (attackCardStaminaCost.max, a.cost.stamina);

                attackCardBodyPartsCostValue.min = Mathf.Min (attackCardBodyPartsCostValue.min, a.cost.bodyPartDamage.damage);
                attackCardBodyPartsCostValue.max = Mathf.Max (attackCardBodyPartsCostValue.max, a.cost.bodyPartDamage.damage);

                int bpDamage = ((a as SC_OffensiveMove)?.effect.bodyPartDamage ?? (a as SC_Submission).effect.bodyPartDamage).damage;

                attackCardBodyPartsDamageValue.min = Mathf.Min (attackCardBodyPartsDamageValue.min, bpDamage);
                attackCardBodyPartsDamageValue.max = Mathf.Max (attackCardBodyPartsDamageValue.max, bpDamage);

                SC_OffensiveMove o = c as SC_OffensiveMove;

                if (o) {

                    offensiveMoveHealthCost.min = Mathf.Min (offensiveMoveHealthCost.min, o.cost.health);
                    offensiveMoveHealthCost.max = Mathf.Max (offensiveMoveHealthCost.max, o.cost.health);

                    offensiveMoveStaminaDamage.min = Mathf.Min (offensiveMoveStaminaDamage.min, o.effect.stamina);
                    offensiveMoveStaminaDamage.max = Mathf.Max (offensiveMoveStaminaDamage.max, o.effect.stamina);

                    offensiveMoveHealthDamage.min = Mathf.Min (offensiveMoveHealthDamage.min, o.effect.health);
                    offensiveMoveHealthDamage.max = Mathf.Max (offensiveMoveHealthDamage.max, o.effect.health);

                } else {

                    SC_Submission s = c as SC_Submission;

                    submissionStaminaReduction.min = Mathf.Min (submissionStaminaReduction.min, s.effect.stamina);
                    submissionStaminaReduction.max = Mathf.Max (submissionStaminaReduction.max, s.effect.stamina);

                    submissionBreakCost.min = Mathf.Min (submissionBreakCost.min, s.effect.breakCost);
                    submissionBreakCost.max = Mathf.Max (submissionBreakCost.max, s.effect.breakCost);

                }

            }

        }
        #endregion

        foreach (MinMaxFilter m in minMaxFilters) {   

            m.minInput.text = m.min.ToString ();
            m.maxInput.text = m.max.ToString ();

            m.minInput.onEndEdit.AddListener ((s) => {

                int clamped = Mathf.Clamp (m.Min, m.min, m.Max);

                if (m.Min != clamped)
                    m.minInput.text = clamped.ToString ();

            });

            m.maxInput.onEndEdit.AddListener ((s) => {

                int clamped = Mathf.Clamp (m.Max, m.Min, m.max);

                if (m.Max != clamped)
                    m.maxInput.text = clamped.ToString ();

            });

        }
        #endregion

        commonEffects = commonEffectsParent.GetComponentsInChildren<Toggle> ();

        typeToggles = new Toggle[] { mainTypeToggle, firstSubTypeToggle, secondSubTypeToggle };

    }

    void Update () {

        deckScrollRect.horizontalNormalizedPosition = Mathf.Clamp01 (deckScrollRect.horizontalNormalizedPosition + Input.GetAxis ("Mouse ScrollWheel") * Time.deltaTime * deckScrollRect.scrollSensitivity);

    }

    #region Filtering
    #region Types filter
    bool CheckType (SC_BaseCard c, TMP_Dropdown d) {

        if (d.value == 1 && !(c as SC_OffensiveMove))
            return false;
        else if (d.value == 2 && !(c as SC_Submission))
            return false;
        else if (d.value == 3 && !c.Is (CardType.Aerial))
            return false;
        else if (d.value == 4 && !c.Is (CardType.Classic))
            return false;
        else if (d.value == 5 && !c.Is (CardType.Strike))
            return false;
        else if (d.value == 6 && !c.Is (CardType.Mytho))
            return false;
        else if (d.value == 7 && !c.Is (CardType.Hardcore))
            return false;

        return true;

    }
    #endregion

    public void ToggleMoreFilters () {

        moreFiltersPanel.SetActive (!moreFiltersPanel.activeSelf);

        moreFiltersButtonText.text = (moreFiltersPanel.activeSelf ? "Less" : "More") + " filters";

    }

    public void ResetFilters () {

        foreach (Toggle t in typeToggles)
            t.isOn = true;

        foreach (TMP_Dropdown d in dropdownFilters) {            

            d.value = 0;

            d.RefreshShownValue ();

        }

        foreach (MinMaxFilter m in minMaxFilters) {

            m.minInput.text = m.min.ToString ();
            m.maxInput.text = m.max.ToString ();

        }

        foreach (Toggle t in commonEffects)
            t.isOn = false;

        matchHeat.isOn = false;

        oracle.text = "";

    }

    public void Filter () {

        filteredCards = new Dictionary<SC_BaseCard, SC_DeckBuilder_SearchCard> ();

        if (moreFiltersPanel.activeSelf)
            ToggleMoreFilters ();

        foreach (Transform t in searchCardsParent)
            Destroy (t.gameObject);

        float marginsNbr = (((int) searchCardsParent.rect.width) / ((int) size.x)) - 1;

        float margin = (searchCardsParent.rect.width % size.x) / marginsNbr;

        int i, x, y = x = i = 0;               

        foreach (SC_BaseCard c in allCards) {

            #region Oracle filter
            if (oracle.text != "" && !c.HasText (oracle.text) && !c.name.ToLower ().Contains (oracle.text.ToLower ()))
                continue;
            #endregion

            #region Types filters
            if (mainType.value == 1 && (c.GetType ().IsSubclassOf (typeof (SC_AttackCard)) != mainTypeToggle.isOn))
                continue;
            else if ((mainType.value == 2 && (c.IsSpecial != mainTypeToggle.isOn)) || (mainType.value == 3 && (c.Is (CardType.Basic) != mainTypeToggle.isOn)))
                continue;

            if ((CheckType (c, firstSubType) != firstSubTypeToggle.isOn) || (CheckType (c, secondSubType) != secondSubTypeToggle.isOn))
                continue;
            #endregion

            #region Aligment filter
            if (alignment.value > 0) {

                if (alignment.value == 1) {

                    CommonRequirement? r = null;

                    foreach (CommonRequirement cr in c.commonRequirements)
                        if (cr.valueType == ValueName.Alignment && !cr.opponent)
                            r = cr;

                    if (r == null || c.IsAlignmentCard (false) || c.IsAlignmentCard (true))
                        continue;

                } else if ((alignment.value == 2 && !c.IsAlignmentCard (true)) || (alignment.value == 3 && !c.IsAlignmentCard (false)))
                    continue;

            }
            #endregion          
            
            #region Common effects filter
            bool skip = false;

            foreach (Toggle t in commonEffects) {

                if (t.isOn) {

                    string s = t.transform.parent.GetComponentInChildren<TextMeshProUGUI> ().text.Replace (" ", "");

                    if (Enum.TryParse (s, out CommonEffectType ty)) {

                        if (!c.Has (ty) && !c.additionalKeywords.Contains (s)) {

                            skip = true;

                            continue;

                        }

                    } else if (s == "Discard") {

                        if (!c.Has (CommonEffectType.DiscardChosen) && !c.Has (CommonEffectType.DiscardRandom) && !c.additionalKeywords.Contains (s)) {

                            skip = true;

                            continue;

                        }

                    } else
                        Debug.LogError ("WRONG COMMON EFFECT FILTER");

                }

            }

            if (skip)
                continue;

            #endregion

            #region Match Heat effect filter
            if (matchHeat.isOn) {

                if (!c.Has (CommonEffectType.MatchHeatEffect) && ((c as SC_AttackCard)?.matchHeatGain ?? 0) <= 0)
                    continue;

            }
            #endregion

            #region Attack values filters
            SC_AttackCard a = c as SC_AttackCard;

            if (a) {

                if (a.cost.stamina < attackCardStaminaCost.Min || a.cost.stamina > attackCardStaminaCost.Max)
                    continue;

                if (attackCardBodyPartsCost.value == 0 || a.cost.bodyPartDamage.bodyPart == (BodyPart) attackCardBodyPartsCost.value || (attackCardBodyPartsCost.value == 6 && a.cost.bodyPartDamage.bodyPart == BodyPart.None)) {

                    if (a.cost.bodyPartDamage.damage < attackCardBodyPartsCostValue.Min || a.cost.bodyPartDamage.damage > attackCardStaminaCost.Max)
                        continue;

                } else
                    continue;

                OffensiveBodyPartDamage bpDamage = (a as SC_OffensiveMove)?.effect.bodyPartDamage ?? (a as SC_Submission).effect.bodyPartDamage;

                if (attackCardBodyPartsDamage.value == 0 || bpDamage.bodyPart == (BodyPart) attackCardBodyPartsDamage.value || bpDamage.otherBodyPart == (BodyPart) attackCardBodyPartsDamage.value || (attackCardBodyPartsDamage.value == 6 && bpDamage.bodyPart == BodyPart.None && bpDamage.otherBodyPart == BodyPart.None)) {

                    if (bpDamage.damage < attackCardBodyPartsDamageValue.Min || bpDamage.damage > attackCardBodyPartsDamageValue.Max)
                        continue;

                } else
                    continue;

                SC_OffensiveMove o = c as SC_OffensiveMove;

                if (o) {

                    if (o.cost.health < offensiveMoveHealthCost.Min || o.cost.health > offensiveMoveHealthCost.Max)
                        continue;

                    if (o.effect.stamina < offensiveMoveStaminaDamage.Min || o.effect.stamina > offensiveMoveStaminaDamage.Max)
                        continue;

                    if (o.effect.health < offensiveMoveHealthDamage.Min || o.effect.health > offensiveMoveHealthDamage.Max)
                        continue;

                } else {

                    SC_Submission s = c as SC_Submission;

                    if (s.effect.stamina < submissionStaminaReduction.Min || s.effect.stamina > submissionStaminaReduction.Max)
                        continue;

                    if (s.effect.breakCost < submissionBreakCost.Min || s.effect.breakCost > submissionBreakCost.Max)
                        continue;

                }

            }
            #endregion

            if (c.matchHeat >= matchHeatFilter.Min && c.matchHeat <= matchHeatFilter.Max) {

                RectTransform r = Instantiate (Resources.Load<RectTransform> ("Prefabs/DeckBuilder/P_DeckBuilder_SearchCard"), searchCardsParent);

                r.sizeDelta = size;

                r.GetComponent<SC_DeckBuilder_SearchCard> ().Card = c;

                r.GetComponent<Image> ().sprite = Resources.Load<Sprite> (c.Path);

                r.anchoredPosition = new Vector2 (x * (size.x + margin), -y * (size.y + margin));                

                x = x == ((int) searchCardsParent.rect.width) / ((int) size.x) - 1 ? 0 : x + 1;

                y = x == 0 ? y + 1 : y;

                i++;

            }

        }

        resultsCount.text = i.ToString ();
        resultsCount.transform.parent.gameObject.SetActive (true);

        y += (x == 0 ? -1 : 0) + 1;        

        searchCardsParent.sizeDelta = new Vector2 (searchCardsParent.sizeDelta.x, Mathf.Max (300, size.y * y + margin * (y - 1)));

    }
    #endregion

    #region Deck building
    public static void TryAddRemove (SC_BaseCard c, bool add) {

        if (filteredCards != null && filteredCards.ContainsKey (c))
            filteredCards[c].OnPointerClick (null);
        else
            Instance.AddRemoveCard (c, add);

    }

    public void AddRemoveCard (SC_BaseCard c, bool add) {

        if (add)
            AddCard (c);
        else
            RemoveCard (c);

        for (int i = 0; i < 7; i++)
            deckMatchHeatRepartitions[i].text = "(" + Mathf.RoundToInt ((deckCardsColumns[i].childCount - 1) * 100f / deckCards.Count) + "%)";

        float maxSize = 0;

        foreach (RectTransform t in deckCardsColumns)
            maxSize = Mathf.Max (maxSize, (t.childCount - 1) * deckCardHeight + 50);

        deckContent.sizeDelta = new Vector2 (0, Mathf.Max (422, maxSize));

    }

    public static void AddCard (SC_BaseCard c) {

        SC_DeckBuilder_DeckCard d = Instantiate (Resources.Load<SC_DeckBuilder_DeckCard> ("Prefabs/DeckBuilder/P_DeckBuilder_DeckCard"), Instance.deckCardsColumns[(c.matchHeat - 1) / 3]);

        deckCards[c] = d;

        d.Card = c;

        d.RecT.sizeDelta = new Vector2 (d.RecT.sizeDelta.x, Instance.deckCardHeight);

        int index;

        for (index = 1; index < d.RecT.parent.childCount; index++)
            if (d.RecT.parent.GetChild (index).GetComponent<SC_DeckBuilder_DeckCard> ().Card.matchHeat > c.matchHeat)
                break;                

        d.RecT.SetSiblingIndex (index);

        UpdateDeckCardsCount ();

    }

    public static void RemoveCard (SC_BaseCard c) {

        DestroyImmediate (deckCards[c].gameObject);

        deckCards.Remove (c);

        UpdateDeckCardsCount ();

    }

    public static void UpdateDeckCardsCount () {        

        Instance.deckCardsCount.text = deckCards.Count.ToString ();

        Instance.deckManager.UpdateCanSaveDeck ();

    }
    #endregion

}
