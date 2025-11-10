// EconomyManager.cs
using System.IO;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Economy Settings")]
    public int startingCoins = 500;
    public int entryFee = 100;         // cost to play
    public int winReward = 200;        // reward on winning
    public int attemptsDefault = 3;    // default tries

    private int coins;

    private string saveFileName = "SaveData.json";

    // SESSION FLAG: marks whether the entry fee has already been charged for the current play session.
    // Intentionally not persisted across application restarts.
    private bool entryFeePaidThisSession = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadEconomy(); // load saved data on startup
            entryFeePaidThisSession = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Coins API
    public int GetCoins() => coins;

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveEconomy();
        Debug.Log($"[Economy] AddCoins: +{amount} => {coins}");
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveEconomy();
            Debug.Log($"[Economy] SpendCoins: -{amount} => {coins}");
            return true;
        }
        Debug.LogWarning($"[Economy] SpendCoins failed - not enough coins. Have={coins} Need={amount}");
        return false;
    }

    // Cheat: instantly add a large amount
    public void CheatAddCoins(int amount)
    {
        coins += amount;
        SaveEconomy();
        Debug.Log($"[Economy] CheatAddCoins: +{amount} => {coins}");
    }
    #endregion

    #region Entry fee session helpers (used by MenuManager)
    /// <summary>
    /// Returns true if entry fee has already been charged for this session.
    /// </summary>
    public bool IsEntryFeePaid() => entryFeePaidThisSession;

    /// <summary>
    /// Mark that entry fee has been paid this session (so you don't charge it again when reloading scene).
    /// </summary>
    public void MarkEntryFeePaid()
    {
        entryFeePaidThisSession = true;
        Debug.Log("[Economy] Entry fee marked PAID for this session.");
    }

    /// <summary>
    /// Clear the "entry paid" flag (useful for testing or when resetting menu state).
    /// </summary>
    public void ClearEntryFeePaid()
    {
        entryFeePaidThisSession = false;
        Debug.Log("[Economy] Entry fee flag CLEARED for this session.");
    }
    #endregion

    #region PlayerPrefs + File Saving
    // Quick PlayerPrefs sync (for compatibility with older systems)
    private void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.Save();
    }

    private void LoadFromPlayerPrefs()
    {
        coins = PlayerPrefs.GetInt("Coins", startingCoins);
    }

    // JSON file saving (learn file saving)
    [System.Serializable]
    private class SaveData
    {
        public int coins;
    }

    public void SaveEconomy()
    {
        // Save to PlayerPrefs for quick access
        SaveToPlayerPrefs();

        // Also save to JSON file (persistent path)
        SaveData data = new SaveData { coins = coins };
        string json = JsonUtility.ToJson(data);
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        try
        {
            File.WriteAllText(path, json);
            // Debug.Log($"Saved economy to {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Economy] Failed saving economy file: " + e.Message);
        }
    }

    public void LoadEconomy()
    {
        // Try JSON file first
        string path = Path.Combine(Application.persistentDataPath, saveFileName);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                coins = data.coins;
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Economy] Failed to load JSON save; falling back to PlayerPrefs. " + e.Message);
            }
        }

        // fallback to PlayerPrefs
        LoadFromPlayerPrefs();
    }

    // Useful debug helper: reset economy (dev only)
    public void ResetEconomyToDefaults()
    {
        coins = startingCoins;
        SaveEconomy();
        Debug.Log("[Economy] Reset to defaults: " + coins);
    }
    #endregion
}
