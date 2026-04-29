using System.Collections.Generic;
using UnityEngine;

public class CalculationManager : MonoBehaviour
{
    [SerializeField] private CacheManager cacheManager;

    private int? selected1 = null;
    private int? selected2 = null;
    private List<string> steps = new List<string>();

    public void SelectNumber(int value)
    {
        if (selected1 == null)
            selected1 = value;
        else if (selected2 == null)
            selected2 = value;
    }

    public bool HasTwoSelected()
    {
        return selected1 != null && selected2 != null;
    }

    // Pure operator logic — no side effects, used for lookahead checks
    public static int? TryCompute(int a, int b, string op)
    {
        switch (op)
        {
            case "+": return a + b;
            case "-": return a - b > 0 ? (int?)(a - b) : null;
            case "*": return a * b;
            case "%":
                int lg = Mathf.Max(a, b), sm = Mathf.Min(a, b);
                int rem = lg % sm;
                return rem != 0 ? (int?)rem : null;
            case "/":
                int n = Mathf.Max(a, b), d = Mathf.Min(a, b);
                return d != 0 && n % d == 0 ? (int?)(n / d) : null;
        }
        return null;
    }

    // Computes, records step, and adds to cache
    public int? Compute(int a, int b, string op)
    {
        int? result = TryCompute(a, b, op);
        if (result != null)
        {
            steps.Add($"{a} {op} {b} = {result}");
            cacheManager.AddValue(result.Value);
        }
        return result;
    }

    // Overload using internally selected numbers
    public int? Compute(string op)
    {
        if (!HasTwoSelected()) return null;
        int a = selected1.Value;
        int b = selected2.Value;
        ClearSelection();
        return Compute(a, b, op);
    }

    public void ClearSelection()
    {
        selected1 = null;
        selected2 = null;
    }

    public List<string> GetSteps() => new List<string>(steps);

    public int GetStepCount() => steps.Count;

    public void ResetSteps()
    {
        steps.Clear();
        ClearSelection();
    }

}
