using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class Card : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI valueText;
    [SerializeField] private Image background;
    [SerializeField] private CalculationManager calculationManager;

    [SerializeField] private bool isCache;
    [SerializeField] private GameManager gameManager;

    [SerializeField] private Color normalColor   = Color.white;
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f); // yellow
    [SerializeField] private Color consumedColor = new Color(0.35f, 0.35f, 0.35f); // dark grey

    private Button button;
    private int value;
    private bool consumed = false;

    public bool IsAvailable => !consumed;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        if (background == null)
            Debug.LogError($"[Card] background is null on '{gameObject.name}' — assign it in the Inspector.", this);
    }

    public void SetValue(int v)
    {
        value = v;
        valueText.text = v.ToString();
    }

    public int GetValue() => value;

    // Called by GameManager after a successful Compute() to permanently remove card
    public void SetConsumed()
    {
        consumed = true;
        Debug.Log($"[Card] Consumed: {gameObject.name}");
        button.interactable = false;
        background.color = consumedColor;
    }

    // Called by GameManager to highlight this card as selected
    public void SetSelected(bool on)
    {
        background.color = on ? selectedColor : normalColor;
    }

    // Called by GameManager on level load or invalid op restore
    public void ResetCard()
    {
        consumed = false;
        button.interactable = true;
        background.color = normalColor;
    }

    public void ClearSlot()
    {
        button.interactable = false;
        valueText.text = "";
        background.color = normalColor;
    }

    public bool IsCache => isCache;

    public void OnClick()
    {
        Debug.Log($"[Card] Clicked: {gameObject.name} value={value} isCache={isCache}");
        gameManager.OnCardSelected(this);
    }
}
