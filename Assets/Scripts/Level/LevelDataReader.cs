using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public static class LevelDataLoader
{
    // ============================================================
    // Types
    // ============================================================




    // ============================================================
    // Private
    // ============================================================

    private static List<LevelData> levels;

    private static bool loaded = false;


    // ============================================================
    // Load
    // ============================================================

    private static void LoadLevels()
    {
        if (loaded)
            return;

        loaded = true;

        levels = new List<LevelData>();


        // ========================================================
        // 读取 TextAsset
        // ========================================================

        TextAsset asset =
            Resources.Load<TextAsset>(
                "Levels/Level"
            );

        if (asset == null)
        {
            Debug.LogError(
                "[LevelDataLoader] 找不到关卡文件：\n" +
                "Resources/Levels/Level.json"
            );

            return;
        }


        // ========================================================
        // 解析 JSON
        // ========================================================

        try
        {
            List<LevelData> data =
                JsonConvert.DeserializeObject<
                    List<LevelData>
                >(asset.text);


            if (data == null)
            {
                Debug.LogError(
                    "[LevelDataLoader] Level.json 解析结果为空"
                );

                return;
            }


            levels.AddRange(data);


            Debug.Log(
                $"[LevelDataLoader] 成功读取 " +
                $"{levels.Count} 个关卡"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[LevelDataLoader] Level.json 解析失败：\n" +
                exception
            );
        }
    }


    // ============================================================
    // Public API
    // ============================================================

    public static LevelData GetLevelData(
        int index
    )
    {
        LoadLevels();


        if (
            index < 0 ||
            index >= levels.Count
        )
        {
            Debug.LogWarning(
                $"[LevelDataLoader] 关卡索引不存在：{index}"
            );

            return null;
        }


        return levels[index];
    }


    // ============================================================
    // 获取关卡数量
    // ============================================================

    public static int GetLevelCount()
    {
        LoadLevels();

        return levels.Count;
    }
}


public class LevelData
{
    public Dictionary<string, SetupData> Setup;
    public Dictionary<string, SourceData> Source;
    public LevelConfig levelConfig;
}

public class SetupData
{
    public int type;
    public string source;
    public TriggerData trigger;

    public int target;
    public float time;
}

public class TriggerData
{
    public string source;
    public float prm;
}

public class SourceData
{
    public int EventType;
    public List<int> ID;
    public List<List<object>> args;
}

public class LevelConfig
{
    public int width;
    public int height;
    public List<int> mine;
    public float time_max;
    public int times;
}