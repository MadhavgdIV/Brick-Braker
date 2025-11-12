using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GameData
{
    public int finalScore = 0;
    public int highScore = 0;
    public int lastStreakBonus = 0;
    public bool didWin = false;
}

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("Save")]
    public string saveFileName = "gamedata.json";

    // Expose path for debug/testing if needed
    public string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private GameData data;

    private void Awake()
    {
        // Typical singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    private void OnApplicationQuit()
    {
        // Persist any last changes
        SaveNow();
    }

 
    public static GameDataManager EnsureExists()
    {
        if (Instance != null) return Instance;

        GameObject go = new GameObject("GameDataManager");
        // AddComponent will synchronously run Awake, so Instance should be set when this returns.
        return go.AddComponent<GameDataManager>();
    }

    private void Load()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<GameData>(json) ?? new GameData();
                Debug.Log($"GameDataManager: Loaded data from {SavePath}");
            }
            else
            {
                data = new GameData();
                SaveNow(); // create file with defaults
                Debug.Log($"GameDataManager: No save found. Created default save at {SavePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"GameDataManager Load failed: {ex}");
            data = new GameData();
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
            Debug.LogError($"GameDataManager Save failed: {ex}");
        }
    }

    /// <summary>
    /// Public wrapper to save immediately.
    /// </summary>
    public void SaveNow()
    {
        if (data == null) data = new GameData();
        Save();
    }

    /// <summary>
    /// Public API to get the current data object. Caller should not assume it can't be null;
    /// method ensures data is loaded.
    /// </summary>
    public GameData GetData()
    {
        if (data == null) Load();
        return data;
    }

    /// <summary>
    /// Set final results and update high score if needed, then persist.
    /// </summary>
    public void SetFinalResults(int finalScore, bool didWin, int lastStreakBonus)
    {
        if (data == null) Load();

        data.finalScore = finalScore;
        data.didWin = didWin;
        data.lastStreakBonus = lastStreakBonus;

        if (finalScore > data.highScore)
        {
            data.highScore = finalScore;
            Debug.Log($"GameDataManager: New high score saved: {data.highScore}");
        }

        SaveNow();
    }

    /// <summary>
    /// Optional helper to set high score directly (if ever needed).
    /// </summary>
    public void SetHighScore(int highScore)
    {
        if (data == null) Load();
        data.highScore = highScore;
        SaveNow();
    }

    /// <summary>
    /// Reset the in-memory data to defaults and persist the file (overwrites save file).
    /// </summary>
    public void ResetToDefaults()
    {
        data = new GameData();
        SaveNow();
        Debug.Log("GameDataManager: Reset to defaults and saved.");
    }

    /// <summary>
    /// Deletes the save file from disk. Useful for testing.
    /// Does not destroy the in-memory data instance (call ResetToDefaults if you want that).
    /// </summary>
    public bool DeleteSaveFile()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log($"GameDataManager: Deleted save file at {SavePath}");
                return true;
            }

            Debug.LogWarning($"GameDataManager: No save file to delete at {SavePath}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.LogError($"GameDataManager: Failed to delete save file: {ex}");
            return false;
        }
    }
}
