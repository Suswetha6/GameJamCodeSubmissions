using System.Collections.Generic;
using UnityEngine;
using TMPro; // ✅ IMPORTANT
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private CalculationManager calculationManager;
    [SerializeField] private CacheManager cacheManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private TextMeshProUGUI selectedText;
    [SerializeField] private Card[] cards;   // Card1-4

    [Header("How To Play")]
    [SerializeField] private GameObject howToPlayPanel;

    [Header("End Panel")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private TextMeshProUGUI stepsText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button nextLevelButton;

    [Header("Buttons")]
    [SerializeField] private Button clearButton;

    // -------------------------------------------------------------------------

    [System.Serializable]
    private struct LevelData
    {
        public int target;
        public int[] cards;
        public int par;
    }

    private static readonly LevelData[] levels = new LevelData[]
    {
        new LevelData { target = 36, cards = new[] { 3, 6, 2, 5 }, par = 2 }, // easy:   multiple ×× paths
        new LevelData { target = 25, cards = new[] { 3, 4, 6, 7 }, par = 2 }, // medium: ×+ and ×− paths
        new LevelData { target = 40, cards = new[] { 3, 5, 7, 4 }, par = 2 }, // medium: single +× path
        new LevelData { target = 16, cards = new[] { 3, 4, 5, 7 }, par = 2 }, // hard:   addition chains
        new LevelData { target = 15, cards = new[] { 3, 9, 4, 7 }, par = 2 }, // hardest: single −× path
    };

    private int currentLevelIndex = 0;
    private bool bonusCacheNext = false;
    private bool isLoseState = false;
    private Card selectedCard1 = null;
    private Card selectedCard2 = null;
    private string pendingOp = null;

    // -------------------------------------------------------------------------

    void Start()
    {
        LoadLevel(currentLevelIndex);
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(true);
    }

    // Wire to the "Let's Play!" button inside HowToPlayPanel
    public void OnStartGame()
    {
        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);
    }

    public void LoadLevel(int index)
    {
        currentLevelIndex = index;
        LevelData level = levels[index];

        calculationManager.ResetSteps();
        cacheManager.Clear();
        cacheManager.SetMaxSize(bonusCacheNext ? 3 : 2);
        bonusCacheNext = false;

        targetText.text = "Target: " + level.target;
        selectedText.text = "";

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].SetValue(level.cards[i]);
            cards[i].ResetCard();
        }

        selectedCard1 = null;
        selectedCard2 = null;
        pendingOp = null;
        endPanel.SetActive(false);
        UpdateClearButton();
    }

    // Called by Card.OnClick — new flow: Card1 → Operator → Card2 → auto-compute
    public void OnCardSelected(Card card)
    {
        if (selectedCard1 == null)
        {
            selectedCard1 = card;
            card.SetSelected(true);
            calculationManager.SelectNumber(card.GetValue());
            selectedText.text = card.GetValue().ToString();
        }
        else if (pendingOp == null && card != selectedCard1)
        {
            // Re-select first card before an operator is chosen
            selectedCard1.SetSelected(false);
            calculationManager.ClearSelection();
            selectedCard1 = card;
            card.SetSelected(true);
            calculationManager.SelectNumber(card.GetValue());
            selectedText.text = card.GetValue().ToString();
        }
        else if (pendingOp != null && card != selectedCard1)
        {
            selectedCard2 = card;
            card.SetSelected(true);
            calculationManager.SelectNumber(card.GetValue());
            TriggerCompute();
        }
        UpdateClearButton();
    }

    // Called by operator buttons — stores the operator and waits for second card
    public void OnOperatorPressed(string op)
    {
        if (selectedCard1 == null || pendingOp != null) return;
        Debug.Log($"[GM] Operator pending: {op} | sel1={selectedCard1.name}");
        pendingOp = op;
        selectedText.text = selectedCard1.GetValue() + " " + op + " ?";
        UpdateClearButton();
    }

    private void TriggerCompute()
    {
        Debug.Log($"[GM] Computing: {selectedCard1.GetValue()} {pendingOp} {selectedCard2.GetValue()}");
        string op = pendingOp;
        pendingOp = null;

        int? result = calculationManager.Compute(op);
        Debug.Log($"[GM] Result: {(result.HasValue ? result.Value.ToString() : "null (invalid)")}");

        if (result == null)
        {
            selectedCard1?.SetSelected(false);
            selectedCard2?.SetSelected(false);
            selectedCard1 = null;
            selectedCard2 = null;
            selectedText.text = "";
            UpdateClearButton();
            return;
        }

        // Cache cards are reusable — only consume original cards
        if (selectedCard1 != null && !selectedCard1.IsCache) selectedCard1.SetConsumed();
        else selectedCard1?.SetSelected(false);

        if (selectedCard2 != null && !selectedCard2.IsCache) selectedCard2.SetConsumed();
        else selectedCard2?.SetSelected(false);

        selectedCard1 = null;
        selectedCard2 = null;
        selectedText.text = "";
        UpdateClearButton();
        CheckWin(result.Value);
    }

    private void CheckWin(int latestResult)
    {
        LevelData level = levels[currentLevelIndex];

        bool won = latestResult == level.target;
        if (!won)
            foreach (int cached in cacheManager.GetValues())
                if (cached == level.target) { won = true; break; }

        if (won) { ShowEndPanel(); return; }

        // Give the user at least 2 operations to fill the cache before declaring a loss
        if (calculationManager.GetStepCount() >= 2 && !CanReachTarget())
        {
            Debug.Log("[GM] Lose: no path to target from current state.");
            ShowLosePanel();
        }
    }

    // 3-step lookahead: returns false only when target is unreachable in any 3-move sequence
    private bool CanReachTarget()
    {
        int target = levels[currentLevelIndex].target;
        var originals = new List<int>();
        foreach (Card c in cards)
            if (c.IsAvailable) originals.Add(c.GetValue());
        return SearchReachable(originals, cacheManager.GetValues(), target, 3);
    }

    private bool SearchReachable(List<int> orig, List<int> cache, int target, int depth)
    {
        if (orig.Contains(target) || cache.Contains(target)) return true;
        if (depth == 0 || orig.Count + cache.Count < 2) return false;

        int cacheMax = cacheManager.MaxSize;
        string[] ops = { "+", "-", "*", "/", "%" };

        var all = new List<int>(orig);
        all.AddRange(cache);
        var isOrig = new bool[all.Count];
        for (int k = 0; k < orig.Count; k++) isOrig[k] = true;

        for (int i = 0; i < all.Count; i++)
        for (int j = 0; j < all.Count; j++)
        {
            if (i == j) continue;
            foreach (string op in ops)
            {
                int? r = CalculationManager.TryCompute(all[i], all[j], op);
                if (r == null) continue;
                if (r == target) return true;

                var newOrig = new List<int>(orig);
                if (isOrig[i]) newOrig.Remove(all[i]);
                if (isOrig[j]) newOrig.Remove(all[j]);

                var newCache = new List<int>(cache);
                if (newCache.Count >= cacheMax) newCache.RemoveAt(0);
                newCache.Add(r.Value);

                if (SearchReachable(newOrig, newCache, target, depth - 1)) return true;
            }
        }
        return false;
    }

    private void ShowEndPanel()
    {
        isLoseState = false;
        var btnLabel = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnLabel != null) btnLabel.text = "Next Level";

        LevelData level = levels[currentLevelIndex];
        int steps = calculationManager.GetStepCount();

        string stars;
        if (steps <= level.par)
        {
            stars = "★★★ Perfect!";
            bonusCacheNext = true;
        }
        else if (steps == level.par + 1)
            stars = "★★ Good";
        else
            stars = "★ You can do better";

        List<string> stepList = calculationManager.GetSteps();
        stepsText.text = string.Join("\n", stepList);
        scoreText.text = stars;

        bool hasNext = currentLevelIndex + 1 < levels.Length;
        nextLevelButton.gameObject.SetActive(hasNext);

        endPanel.SetActive(true);
    }

    public void OnNextLevel()
    {
        if (isLoseState) { isLoseState = false; LoadLevel(currentLevelIndex); return; }
        LoadLevel(currentLevelIndex + 1);
    }

    public void OnRestart()
    {
        LoadLevel(currentLevelIndex);
    }

    private void ShowLosePanel()
    {
        isLoseState = true;
        stepsText.text = "No more moves!";
        scoreText.text = "Try again.";
        var btnLabel = nextLevelButton.GetComponentInChildren<TextMeshProUGUI>();
        if (btnLabel != null) btnLabel.text = "Retry";
        nextLevelButton.gameObject.SetActive(true);
        endPanel.SetActive(true);
    }

    private void UpdateClearButton()
    {
        if (clearButton != null)
            clearButton.interactable = (selectedCard1 != null || pendingOp != null);
    }

    // Wire to ClearButton in Inspector — cancels the current selection and pending operator
    public void OnClearSelection()
    {
        Debug.Log($"[GM] Clear selection: deselecting {selectedCard1?.name}, op={pendingOp}");
        calculationManager.ClearSelection();
        selectedCard1?.SetSelected(false);
        selectedCard2?.SetSelected(false);
        selectedCard1 = null;
        selectedCard2 = null;
        pendingOp = null;
        selectedText.text = "";
        UpdateClearButton();
    }
}