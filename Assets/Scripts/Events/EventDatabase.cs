using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventDatabase
{
    // 结构：[[Fixed], [Select], [Handle]]
    public static List<List<EventBase>> Events = new(3);

    public static void Init()
    {
        Events.Clear();

        List<FixedEvent> fixedList =
            LoadList<FixedEvent>("Events/Fixed");

        List<SelectEvent> selectList =
            LoadList<SelectEvent>("Events/Select");


        Events.Add(new List<EventBase>(fixedList));
        Events.Add(new List<EventBase>(selectList)); 

        Debug.Log(
            "[事件表] 加载完成，共：" +
            Events[0].Count + " 固定 / " +
            Events[1].Count + " 选择 / "
        );
    }

    private static List<T> LoadList<T>(string path)
    {
        TextAsset json = Resources.Load<TextAsset>(path);

        if (json == null)
        {
            Debug.LogWarning(
                "[事件表] 未找到：" + path + ".json"
            );

            return new List<T>();
        }
 
        try
        {
            // 原 JSON 是：
            //
            // [
            //     {...},
            //     {...}
            // ]
            //
            // JsonUtility 不能直接解析数组，
            // 临时变成：
            //
            // {
            //     "items": [
            //         {...},
            //         {...}
            //     ]
            // }

            string wrappedJson =
                "{\"items\":" + json.text + "}";

            Wrapper<T> data =
                JsonUtility.FromJson<Wrapper<T>>(wrappedJson);

            if (data == null || data.items == null)
            {
                Debug.LogWarning(
                    "[事件表] JSON 数据为空：" + path + ".json"
                );

                return new List<T>();
            }

            return new List<T>(data.items);
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[事件表] JSON 解析失败：" +
                path + ".json\n" +
                e
            );

            return new List<T>();
        }
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] items;
    }
}