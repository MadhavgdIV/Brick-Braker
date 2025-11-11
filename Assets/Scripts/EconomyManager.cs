using System;
using System.IO;
using UnityEngine;

[Serializable]
public class EconomySaveData
{
    public int coins = 0;
    public int winStreak = 0;   // persist streak across runs
}

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Save")]
    public string saveFileName = "economy.json";

    [Header("Debug")]
    public int startingCoins = 500; // used only if no save exists

    private EconomySaveData data;
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<EconomySaveData>(json) ?? new EconomySaveData();
            }
            else
            {
                data = new EconomySaveData { coins = startingCoins, winStreak = 0 };
                Save(); // create the file
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("EconomyManager Load failed: " + ex);
            data = new EconomySaveData { coins = startingCoins, winStreak = 0 };
        }
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError("EconomyManager Save failed: " + ex);
        }
    }

    // Public API
    public int GetCoins() => data.coins;

    // Tries to deduct. Returns true if successful.
    public bool TryDeduct(int amount)
    {
        if (amount <= 0) return true;
        if (data.coins >= amount)
        {
            data.coins -= amount;
            Save();
            return true;
        }
        return false;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        data.coins += amount;
        Save();
    }

    public void SetCoins(int amount)
    {
        data.coins = Mathf.Max(0, amount);
        Save();
    }

    // -----------------------
    // Win streak API
    // -----------------------
    public int GetWinStreak() => data.winStreak;

    // Call when the player wins: increments streak, returns the bonus amount to award
    public int OnPlayerWinAndGetBonus()
    {
        data.winStreak = Mathf.Max(0, data.winStreak) + 1;
        int bonus = CalculateWinStreakBonus(data.winStreak);
        data.coins += bonus;
        Save();
        return bonus;
    }

    // Call when player loses: resets streak to 0 (no bonus next time)
    public void OnPlayerLose_ResetStreak()
    {
        if (data.winStreak != 0)
        {
            data.winStreak = 0;
            Save();
        }
    }

    // separate pure function for clarity/testing
    public static int CalculateWinStreakBonus(int streakCount)
    {
        if (streakCount <= 0) return 0;
        if (streakCount == 1) return 100;
        if (streakCount == 2) return 200;
        return 300;
    }
}
