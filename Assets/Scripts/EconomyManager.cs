using System;
using System.IO;
using UnityEngine;

[Serializable]
public class EconomySaveData
{
    public int coins = 0;
}

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public string saveFileName = "economy.json";
    public int startingCoins = 500; // initial amount

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
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            data = JsonUtility.FromJson<EconomySaveData>(json);
        }
        else
        {
            data = new EconomySaveData { coins = startingCoins };
            Save();
        }
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
    }

    public int GetCoins() => data.coins;

    public bool TryDeduct(int amount)
    {
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
        data.coins += amount;
        Save();
    }

    public void SetCoins(int amount)
    {
        data.coins = Mathf.Max(0, amount);
        Save();
    }
}
