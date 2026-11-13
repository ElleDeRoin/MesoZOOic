using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class DinoStateSave
{
    public string instanceId;
    public float happiness;
    public float hunger;
}

[Serializable]
public class DecorationSave
{
    public string slotKey;
    public string decorationId;
}

[Serializable]
public class SaveFile
{
    public int version = 1;
    public bool hasMoney;
    public float money;
    public int amber;

    public List<OwnedDino> dinos = new List<OwnedDino>();
    public List<DinoStateSave> dinoStates = new List<DinoStateSave>();
    public List<DecorationSave> decorations = new List<DecorationSave>();
}

public static class SaveSystem
{
    private const string LegacyDinoKey = "OwnedDinos";
    private static SaveFile data;
    private static bool moneyReady;

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    public static SaveFile Data
    {
        get
        {
            if (data == null)
                data = ReadFromDisk();

            return data;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        data = null;
        moneyReady = false;

        SaveFile save = Data;

        if (save.hasMoney)
        {
            PlayerMoney.EnsureInitialized(save.money, save.amber);
            moneyReady = true;
        }

        ApplyDecorations(save);

        GameObject runner = new GameObject("SaveRunner");
        runner.AddComponent<SaveRunner>();
        UnityEngine.Object.DontDestroyOnLoad(runner);
    }

    public static void Save()
    {

        DinoCollection collection = DinoCollection.Instance;

        if (Wallet.Instance != null)
            moneyReady = true;

        SaveFile file = new SaveFile
        {
            hasMoney = moneyReady,
            money = moneyReady ? PlayerMoney.Balance : 0f,
            amber = moneyReady ? PlayerMoney.Amber : 0
        };

        foreach (OwnedDino owned in collection.Owned)
        {
            file.dinos.Add(owned);

            if (DinoRegistry.Instance.TryGet(owned.instanceId, out DinoState state))
            {
                file.dinoStates.Add(new DinoStateSave
                {
                    instanceId = owned.instanceId,
                    happiness = state.happiness,
                    hunger = state.hunger
                });
            }
        }


        foreach (KeyValuePair<string, DecorationData> pair in DecorationStore.GetAll())
        {
            if (pair.Value == null)
                continue;

            file.decorations.Add(new DecorationSave
            {
                slotKey = pair.Key,
                decorationId = pair.Value.name
            });
        }

        data = file;
        WriteToDisk(file);
    }

    public static void ResetAll()
    {
        DecorationStore.ClearAll();
        DinoCollection.Instance.ClearAll();
        DinoRegistry.Instance.Clear();

        PlayerMoney.Reset();
        moneyReady = false;

        if (Wallet.Instance != null)
        {
            PlayerMoney.EnsureInitialized(
                Wallet.Instance.StartingMoney,
                Wallet.Instance.StartingAmber
            );
            moneyReady = true;
        }

        GameSession.SelectedDino = null;

        PlayerPrefs.DeleteKey(LegacyDinoKey);

        Save();
    }

    private static void ApplyDecorations(SaveFile save)
    {
        if (save.decorations.Count == 0)
            return;

        var catalog = new Dictionary<string, DecorationData>();

        foreach (DecorationData deco in Resources.LoadAll<DecorationData>("Decorations"))
            catalog[deco.name] = deco;

        foreach (DecorationSave entry in save.decorations)
        {
            if (catalog.TryGetValue(entry.decorationId, out DecorationData deco))
            {
                DecorationStore.Set(entry.slotKey, deco);
            }
            else
            {
                Debug.LogWarning(
                    $"Saved decoration '{entry.decorationId}' not found dawg."
                );
            }
        }
    }

    private static SaveFile ReadFromDisk()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                SaveFile loaded = JsonUtility.FromJson<SaveFile>(File.ReadAllText(FilePath));

                if (loaded != null)
                    return loaded;
            }
            catch (Exception e)
            {
                Debug.LogError("Save file got corrupted dawg: " + e.Message);

                try { File.Copy(FilePath, FilePath + ".corrupt", true); }
                catch { }
            }
        }

        SaveFile fresh = new SaveFile();
        MigrateLegacyDinos(fresh);
        return fresh;
    }

    [Serializable]
    private class LegacyDinoSave
    {
        public List<OwnedDino> dinos = new List<OwnedDino>();
    }

    private static void MigrateLegacyDinos(SaveFile target)
    {
        if (!PlayerPrefs.HasKey(LegacyDinoKey))
            return;

        LegacyDinoSave legacy =
            JsonUtility.FromJson<LegacyDinoSave>(PlayerPrefs.GetString(LegacyDinoKey));

        if (legacy != null && legacy.dinos != null)
            target.dinos.AddRange(legacy.dinos);
    }

    private static void WriteToDisk(SaveFile file)
    {
        try
        {
            string json = JsonUtility.ToJson(file, true);
            string temp = FilePath + ".tmp";
            File.WriteAllText(temp, json);

            if (File.Exists(FilePath))
                File.Delete(FilePath);

            File.Move(temp, FilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game dawg: " + e.Message);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Delete Save File")]
    private static void DeleteSaveFile()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);

        PlayerPrefs.DeleteKey(LegacyDinoKey);
        Debug.Log("Save file deleted dawg: " + FilePath);
    }

    [UnityEditor.MenuItem("Show Save Folder")]
    private static void ShowSaveFolder()
    {
        UnityEditor.EditorUtility.RevealInFinder(FilePath);
    }
#endif
}