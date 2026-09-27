#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class LevelJsonEditorWindow : EditorWindow
{
    // ============================================================
    // 文件路径
    // ============================================================

    private const string LEVEL_PATH =
        "Assets/Resources/Levels/Level.json";

    // ============================================================
    // JSON
    // ============================================================

    private JObject rootJson;
    private JArray levels;

    // ============================================================
    // 当前选择
    // ============================================================

    private int currentLevel = 0;

    private string selectedEvent = null;
    private string selectedSource = null;

    // ============================================================
    // Scroll
    // ============================================================

    private Vector2 levelScroll;
    private Vector2 setupScroll;
    private Vector2 sourceScroll;

    // ============================================================
    // UI
    // ============================================================

    private float leftWidth = 180f;

    private bool showSetup = true;
    private bool showSource = true;
    private bool showConfig = true;

    // ============================================================
    // Dirty
    // ============================================================

    private bool dirty = false;

    // ============================================================
    // Event Type Names
    // ============================================================

    private readonly List<string> eventTypeNames =
        new List<string>
        {
            "事件",
            "计时器",
            "与",
            "或",
            "非"
        };

    // ============================================================
    // Window
    // ============================================================

    [MenuItem("Tools/Level JSON Editor")]
    public static void Open()
    {
        LevelJsonEditorWindow window =
            GetWindow<LevelJsonEditorWindow>();

        window.titleContent =
            new GUIContent("Level JSON Editor");

        window.minSize =
            new Vector2(1200, 750);

        window.LoadJson();

        window.Show();
    }

    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        DrawToolbar();

        if (levels == null)
        {
            EditorGUILayout.HelpBox(
                "无法读取 Level.json。",
                MessageType.Error
            );

            if (GUILayout.Button("重新加载"))
            {
                LoadJson();
            }

            return;
        }

        DrawMain();
    }

    // ============================================================
    // Toolbar
    // ============================================================

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(
            EditorStyles.toolbar
        );

        if (GUILayout.Button(
            "Reload",
            EditorStyles.toolbarButton,
            GUILayout.Width(70)))
        {
            ReloadWithCheck();
        }

        if (GUILayout.Button(
            "Save",
            EditorStyles.toolbarButton,
            GUILayout.Width(70)))
        {
            SaveJson();
        }

        GUILayout.Space(10);

        GUILayout.Label(
            dirty
                ? "● Modified"
                : "Saved",
            dirty
                ? EditorStyles.boldLabel
                : EditorStyles.label
        );

        GUILayout.FlexibleSpace();

        GUILayout.Label(
            LEVEL_PATH,
            EditorStyles.miniLabel
        );

        EditorGUILayout.EndHorizontal();
    }

    // ============================================================
    // Reload
    // ============================================================

    private void ReloadWithCheck()
    {
        if (dirty)
        {
            bool save =
                EditorUtility.DisplayDialog(
                    "未保存修改",
                    "Level.json 有未保存修改，是否保存？",
                    "保存",
                    "放弃"
                );

            if (save)
            {
                SaveJson();
            }
        }

        LoadJson();
    }

    // ============================================================
    // Main
    // ============================================================

    private void DrawMain()
    {
        EditorGUILayout.BeginHorizontal();

        DrawLevelList();

        DrawEditor();

        EditorGUILayout.EndHorizontal();
    }

    // ============================================================
    // Level List
    // ============================================================

    private void DrawLevelList()
    {
        EditorGUILayout.BeginVertical(
            GUILayout.Width(leftWidth)
        );

        GUILayout.Label(
            "Levels",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("+"))
        {
            AddLevel();
        }

        if (GUILayout.Button("-"))
        {
            DeleteLevel();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        levelScroll =
            EditorGUILayout.BeginScrollView(
                levelScroll
            );

        for (int i = 0; i < levels.Count; i++)
        {
            bool selected =
                currentLevel == i;

            GUIStyle style =
                selected
                    ? EditorStyles.toolbarButton
                    : EditorStyles.miniButton;

            if (GUILayout.Button(
                $"Level {i}",
                style))
            {
                currentLevel = i;

                selectedEvent = null;
                selectedSource = null;
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // Editor
    // ============================================================

    private void DrawEditor()
    {
        JObject level =
            GetCurrentLevel();

        if (level == null)
        {
            EditorGUILayout.HelpBox(
                "没有选择关卡。",
                MessageType.Info
            );

            return;
        }

        EditorGUILayout.BeginVertical();

        GUILayout.Label(
            $"Level {currentLevel}",
            EditorStyles.largeLabel
        );

        EditorGUILayout.Space(5);

        DrawSetup(level);

        EditorGUILayout.Space(10);

        DrawSource(level);

        EditorGUILayout.Space(10);

        DrawLevelConfig(level);

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // Setup
    // ============================================================

    private void DrawSetup(JObject level)
    {
        JObject setup =
            GetOrCreateObject(
                level,
                "Setup"
            );

        showSetup =
            EditorGUILayout.Foldout(
                showSetup,
                "Setup",
                true,
                EditorStyles.foldoutHeader
            );

        if (!showSetup)
            return;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "Add Event",
            GUILayout.Width(100)))
        {
            AddEvent(setup);
        }

        if (GUILayout.Button(
            "Delete Event",
            GUILayout.Width(110)))
        {
            DeleteSelectedEvent(setup);
        }

        GUILayout.Space(10);

        GUI.enabled =
            !string.IsNullOrEmpty(selectedEvent) &&
            CanMoveEventUp(setup, selectedEvent);

        if (GUILayout.Button(
            "▲ Up",
            GUILayout.Width(70)))
        {
            MoveEvent(setup, selectedEvent, -1);
        }

        GUI.enabled =
            !string.IsNullOrEmpty(selectedEvent) &&
            CanMoveEventDown(setup, selectedEvent);

        if (GUILayout.Button(
            "▼ Down",
            GUILayout.Width(70)))
        {
            MoveEvent(setup, selectedEvent, 1);
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        setupScroll =
            EditorGUILayout.BeginScrollView(
                setupScroll,
                GUILayout.Height(250)
            );

        List<string> names =
            setup.Properties()
                .Select(x => x.Name)
                .ToList();

        foreach (string name in names)
        {
            JObject ev =
                setup[name] as JObject;

            if (ev == null)
                continue;

            DrawEventRow(
                name,
                ev
            );
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(5);

        if (!string.IsNullOrEmpty(
            selectedEvent))
        {
            JObject ev =
                setup[selectedEvent] as JObject;

            if (ev != null)
            {
                DrawEventInspector(
                    setup,
                    selectedEvent,
                    ev
                );
            }
        }
    }

    // ============================================================
    // Event Row
    // ============================================================

    private void DrawEventRow(
        string name,
        JObject ev)
    {
        bool selected =
            selectedEvent == name;

        Rect rect =
            EditorGUILayout.BeginHorizontal(
                selected
                    ? EditorStyles.helpBox
                    : GUIStyle.none
            );

        if (GUILayout.Button(
            selected ? "●" : "○",
            GUILayout.Width(25)))
        {
            selectedEvent = name;
            Repaint();
        }

        GUILayout.Label(
            name,
            GUILayout.Width(120)
        );

        GUILayout.Label(
            $"Source: {GetString(ev, "source")}",
            GUILayout.Width(180)
        );

        GUILayout.Label(
            $"Target: {GetInt(ev, "target")}",
            GUILayout.Width(80)
        );

        GUILayout.Label(
            $"Time: {GetFloat(ev, "time"):0.###}",
            GUILayout.Width(100)
        );

        string trigger =
            GetTriggerText(
                ev["trigger"] as JObject
            );

        GUILayout.Label(trigger);

        EditorGUILayout.EndHorizontal();

        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            rect.Contains(Event.current.mousePosition))
        {
            selectedEvent = name;
            Repaint();
        }
    }

    // ============================================================
    // Event Inspector
    // ============================================================

    private void DrawEventInspector(
        JObject setup,
        string name,
        JObject ev)
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        GUILayout.Label(
            "Event Inspector",
            EditorStyles.boldLabel
        );

        // ========================================================
        // Event Name
        // ========================================================

        string newName =
            EditorGUILayout.TextField(
                "Event Name",
                name
            );

        newName =
            newName.Trim();

        if (newName != name)
        {
            RenameEvent(
                setup,
                name,
                newName
            );

            EditorGUILayout.EndVertical();

            return;
        }

        // ========================================================
        // Type
        // ========================================================

        int type =
            GetInt(
                ev,
                "type"
            );

        int newType =
            EditorGUILayout.Popup(
                "Type",
                Mathf.Clamp(
                    type,
                    0,
                    eventTypeNames.Count - 1
                ),
                eventTypeNames.ToArray()
            );

        if (newType != type)
        {
            ev["type"] =
                newType;

            MarkDirty();
        }

        // ========================================================
        // Source
        // ========================================================

        DrawEventSourceField(ev);

        // ========================================================
        // Target
        // ========================================================

        int target =
            GetInt(
                ev,
                "target"
            );

        int newTarget =
            EditorGUILayout.IntField(
                "Target",
                target
            );

        if (newTarget != target)
        {
            ev["target"] =
                newTarget;

            MarkDirty();
        }

        // ========================================================
        // Time
        // ========================================================

        float time =
            GetFloat(
                ev,
                "time"
            );

        float newTime =
            EditorGUILayout.FloatField(
                "Time",
                time
            );

        if (!Mathf.Approximately(
            newTime,
            time))
        {
            ev["time"] =
                newTime;

            MarkDirty();
        }

        // ========================================================
        // Trigger
        // ========================================================

        DrawTrigger(ev);

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // Event Source
    // ============================================================

    private void DrawEventSourceField(
        JObject ev)
    {
        string source =
            GetString(
                ev,
                "source"
            );

        List<string> sourceNames =
            GetSourceNames();

        if (sourceNames.Count > 0)
        {
            int index =
                sourceNames.IndexOf(
                    source
                );

            if (index < 0)
                index = 0;

            int newIndex =
                EditorGUILayout.Popup(
                    "Source",
                    index,
                    sourceNames.ToArray()
                );

            if (newIndex >= 0 &&
                newIndex < sourceNames.Count)
            {
                string newSource =
                    sourceNames[newIndex];

                if (newSource != source)
                {
                    ev["source"] =
                        newSource;

                    MarkDirty();
                }
            }
        }
        else
        {
            string newSource =
                EditorGUILayout.TextField(
                    "Source",
                    source
                );

            if (newSource != source)
            {
                ev["source"] =
                    newSource;

                MarkDirty();
            }
        }
    }

    // ============================================================
    // Rename Event
    // ============================================================

    private void RenameEvent(
        JObject setup,
        string oldName,
        string newName)
    {
        if (string.IsNullOrEmpty(newName))
            return;

        if (newName == oldName)
            return;

        if (setup.ContainsKey(newName))
        {
            EditorUtility.DisplayDialog(
                "无法重命名",
                $"Event \"{newName}\" 已经存在。",
                "OK"
            );

            return;
        }

        JObject newSetup =
            new JObject();

        foreach (JProperty property
            in setup.Properties().ToList())
        {
            if (property.Name == oldName)
            {
                newSetup.Add(
                    newName,
                    property.Value
                );
            }
            else
            {
                newSetup.Add(
                    property.Name,
                    property.Value
                );
            }
        }

        JObject level =
            GetCurrentLevel();

        if (level == null)
            return;

        level["Setup"] =
            newSetup;

        foreach (JProperty property
            in newSetup.Properties())
        {
            JObject otherEvent =
                property.Value as JObject;

            if (otherEvent == null)
                continue;

            JObject trigger =
                otherEvent["trigger"] as JObject;

            if (trigger == null)
                continue;

            string triggerSource =
                GetString(
                    trigger,
                    "source"
                );

            if (triggerSource == oldName)
            {
                trigger["source"] =
                    newName;
            }
        }

        selectedEvent =
            newName;

        MarkDirty();
    }

    // ============================================================
    // Trigger
    // ============================================================

    private void DrawTrigger(
        JObject ev)
    {
        JObject trigger =
            GetOrCreateObject(
                ev,
                "trigger"
            );

        GUILayout.Space(8);

        GUILayout.Label(
            "Trigger",
            EditorStyles.boldLabel
        );

        string source =
            GetString(
                trigger,
                "source"
            );

        List<string> triggerSources =
            new List<string>();

        // --------------------------------------------------------
        // Global Timmer
        // --------------------------------------------------------

        List<string> timmers = new List<string>()
        {
            "LevelTimer",
            "RoundCounter",
            "TimeLeft"
        };

        foreach (String timer in timmers)
        {
            triggerSources.Add(
                timer
            );
        }

        // --------------------------------------------------------
        // Events
        // --------------------------------------------------------

        JObject setup =
            GetCurrentSetup();

        if (setup != null)
        {
            foreach (string name
                in setup.Properties()
                    .Select(x => x.Name))
            {
                if (!triggerSources.Contains(name))
                {
                    triggerSources.Add(name);
                }
            }
        }

        int index =
            triggerSources.IndexOf(
                source
            );

        if (index < 0)
            index = 0;

        int newIndex =
            EditorGUILayout.Popup(
                "Source",
                index,
                triggerSources.ToArray()
            );

        if (newIndex >= 0 &&
            newIndex < triggerSources.Count)
        {
            string newSource =
                triggerSources[newIndex];

            if (newSource != source)
            {
                trigger["source"] =
                    newSource;

                MarkDirty();
            }
        }

        // ========================================================
        // prm FLOAT
        // ========================================================

        float prm =
            GetFloat(
                trigger,
                "prm"
            );

        float newPrm =
            EditorGUILayout.FloatField(
                "Prm",
                prm
            );

        if (!Mathf.Approximately(
            newPrm,
            prm))
        {
            trigger["prm"] =
                newPrm;

            MarkDirty();
        }

        // ========================================================
        // Trigger 说明
        // ========================================================

        if (source == "LevelTimer")
        {
            EditorGUILayout.HelpBox(
                $"LevelTimer 经过 {newPrm:0.###} 秒后触发。",
                MessageType.Info
            );
        }
        else if (source == "RoundCounter")
        {
            EditorGUILayout.HelpBox(
                $"RoundCounter 达到 {newPrm:0.###} 时触发。",
                MessageType.Info
            );
        }
        else if (!string.IsNullOrEmpty(source))
        {
            JObject setupEvent =
                setup != null
                    ? setup[source] as JObject
                    : null;

            if (setupEvent != null)
            {
                int type =
                    GetInt(
                        setupEvent,
                        "type"
                    );

                if (type == 1)
                {
                    EditorGUILayout.HelpBox(
                        $"{source} 是 Timer，等待 {newPrm:0.###} 秒。",
                        MessageType.Info
                    );
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"{source} 的结果序号 [{newPrm:0.###}]。",
                        MessageType.Info
                    );
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"{source} 的 Trigger 参数：{newPrm:0.###}",
                    MessageType.Info
                );
            }
        }
    }

    // ============================================================
    // Source
    // ============================================================

    private void DrawSource(
        JObject level)
    {
        JObject source =
            GetOrCreateObject(
                level,
                "Source"
            );

        showSource =
            EditorGUILayout.Foldout(
                showSource,
                "Source",
                true,
                EditorStyles.foldoutHeader
            );

        if (!showSource)
            return;

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "Add Source",
            GUILayout.Width(100)))
        {
            AddSource(source);
        }

        if (GUILayout.Button(
            "Delete Source",
            GUILayout.Width(110)))
        {
            DeleteSelectedSource(source);
        }

        GUILayout.Space(10);

        GUI.enabled =
            !string.IsNullOrEmpty(selectedSource) &&
            CanMoveSourceUp(source, selectedSource);

        if (GUILayout.Button(
            "▲ Up",
            GUILayout.Width(70)))
        {
            MoveSource(source, selectedSource, -1);
        }

        GUI.enabled =
            !string.IsNullOrEmpty(selectedSource) &&
            CanMoveSourceDown(source, selectedSource);

        if (GUILayout.Button(
            "▼ Down",
            GUILayout.Width(70)))
        {
            MoveSource(source, selectedSource, 1);
        }

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        sourceScroll =
            EditorGUILayout.BeginScrollView(
                sourceScroll,
                GUILayout.Height(250)
            );

        List<string> names =
            source.Properties()
                .Select(x => x.Name)
                .ToList();

        foreach (string name in names)
        {
            JObject data =
                source[name] as JObject;

            if (data == null)
                continue;

            DrawSourceRow(
                name,
                data
            );
        }

        EditorGUILayout.EndScrollView();

        GUILayout.Space(5);

        if (!string.IsNullOrEmpty(selectedSource))
        {
            JObject data =
                source[selectedSource] as JObject;

            if (data != null)
            {
                DrawSourceInspector(
                    source,
                    selectedSource,
                    data
                );
            }
        }
    }

    // ============================================================
    // Source Row
    // ============================================================

    private void DrawSourceRow(
        string name,
        JObject data)
    {
        bool selected =
            selectedSource == name;

        Rect rect =
            EditorGUILayout.BeginHorizontal(
                selected
                    ? EditorStyles.helpBox
                    : GUIStyle.none
            );

        if (GUILayout.Button(
            selected ? "●" : "○",
            GUILayout.Width(25)))
        {
            selectedSource =
                name;

            Repaint();
        }

        GUILayout.Label(
            name,
            GUILayout.Width(120)
        );

        GUILayout.Label(
            $"Type: {GetInt(data, "EventType")}",
            GUILayout.Width(100)
        );

        GUILayout.Label(
            $"ID: {GetDifficultyArrayText(data["ID"])}",
            GUILayout.Width(180)
        );

        GUILayout.Label(
            $"Args: {GetArgsText(data["args"])}"
        );

        EditorGUILayout.EndHorizontal();

        if (Event.current.type == EventType.MouseDown &&
            Event.current.button == 0 &&
            rect.Contains(Event.current.mousePosition))
        {
            selectedSource =
                name;

            Repaint();
        }
    }

    // ============================================================
    // Source Inspector
    // ============================================================

    private void DrawSourceInspector(
        JObject source,
        string name,
        JObject data)
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        GUILayout.Label(
            "Source Inspector",
            EditorStyles.boldLabel
        );

        // ========================================================
        // Source Name
        // ========================================================

        string newName =
            EditorGUILayout.TextField(
                "Source Name",
                name
            );

        newName =
            newName.Trim();

        if (newName != name)
        {
            RenameSource(
                source,
                name,
                newName
            );

            EditorGUILayout.EndVertical();

            return;
        }

        // ========================================================
        // Event Type
        // ========================================================

        int eventType =
            GetInt(
                data,
                "EventType"
            );

        string[] eventTypes =
        {
            "Fixed (0)",
            "Select (1)",
            "Handle (2)"
        };

        int newType =
            EditorGUILayout.Popup(
                "Event Type",
                Mathf.Clamp(
                    eventType,
                    0,
                    eventTypes.Length - 1
                ),
                eventTypes
            );

        if (newType != eventType)
        {
            data["EventType"] =
                newType;

            MarkDirty();
        }

        // ========================================================
        // ID Difficulty Array
        // ========================================================

        DrawDifficultyArrayField(
            data,
            "ID"
        );

        // ========================================================
        // Args
        // ========================================================

        DrawArgs(data);

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // Rename Source
    // ============================================================

    private void RenameSource(
        JObject source,
        string oldName,
        string newName)
    {
        if (string.IsNullOrEmpty(newName))
            return;

        if (newName == oldName)
            return;

        if (source.ContainsKey(newName))
        {
            EditorUtility.DisplayDialog(
                "无法重命名",
                $"Source \"{newName}\" 已经存在。",
                "OK"
            );

            return;
        }

        JObject newSource =
            new JObject();

        foreach (JProperty property
            in source.Properties().ToList())
        {
            if (property.Name == oldName)
            {
                newSource.Add(
                    newName,
                    property.Value
                );
            }
            else
            {
                newSource.Add(
                    property.Name,
                    property.Value
                );
            }
        }

        JObject level =
            GetCurrentLevel();

        if (level == null)
            return;

        level["Source"] =
            newSource;

        JObject setup =
            level["Setup"] as JObject;

        if (setup != null)
        {
            foreach (JProperty property
                in setup.Properties())
            {
                JObject ev =
                    property.Value as JObject;

                if (ev == null)
                    continue;

                string eventSource =
                    GetString(
                        ev,
                        "source"
                    );

                if (eventSource == oldName)
                {
                    ev["source"] =
                        newName;
                }
            }
        }

        selectedSource =
            newName;

        MarkDirty();
    }

    // ============================================================
    // Difficulty Array Field
    // ============================================================

    private void DrawDifficultyArrayField(
        JObject data,
        string key)
    {
        GUILayout.Space(8);

        GUILayout.Label(
            key + " Difficulty Array",
            EditorStyles.boldLabel
        );

        JArray array =
            data[key] as JArray;

        // --------------------------------------------------------
        // 兼容旧数据
        //
        // 如果 ID 原本就是 int：
        // ID: 1006
        //
        // 自动转换成：
        // ID: [1006]
        //
        // 只有确实是旧格式时才转换。
        // --------------------------------------------------------

        if (array == null)
        {
            JToken oldToken =
                data[key];

            array =
                new JArray();

            if (oldToken != null &&
                oldToken.Type != JTokenType.Null)
            {
                int oldValue =
                    GetTokenInt(oldToken);

                array.Add(oldValue);
            }
            else
            {
                // 新建 DifficultyArray 默认只有一项
                array.Add(0);
            }

            data[key] =
                array;

            MarkDirty();
        }

        DrawDifficultyArray(
            array
        );
    }

    // ============================================================
    // Difficulty Array
    // ============================================================

    private void DrawDifficultyArray(
        JArray array)
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        for (int i = 0; i < array.Count; i++)
        {
            int value =
                GetTokenInt(
                    array[i]
                );

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                $"Diff {i}",
                GUILayout.Width(60)
            );

            int newValue =
                EditorGUILayout.IntField(
                    value
                );

            if (newValue != value)
            {
                array[i] =
                    newValue;

                MarkDirty();
            }

            if (GUILayout.Button(
                "-",
                GUILayout.Width(25)))
            {
                array.RemoveAt(i);

                // DifficultyArray 不允许变成 0 项
                if (array.Count == 0)
                {
                    array.Add(0);
                }

                MarkDirty();

                EditorGUILayout.EndHorizontal();

                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "+ Difficulty"))
        {
            // 每次新增只增加一项
            array.Add(0);

            MarkDirty();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // Args
    // ============================================================

    private void DrawArgs(
        JObject data)
    {
        GUILayout.Space(8);

        GUILayout.Label(
            "Args",
            EditorStyles.boldLabel
        );

        JArray args =
            data["args"] as JArray;

        if (args == null)
        {
            args =
                new JArray();

            data["args"] =
                args;

            MarkDirty();
        }

        for (int i = 0; i < args.Count; i++)
        {
            JToken token =
                args[i];

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                $"[{i}]",
                GUILayout.Width(30)
            );

            // ========================================================
            // Difficulty Array
            // 数组中的值全部按照 float 处理
            // ========================================================

            if (token.Type == JTokenType.Array)
            {
                DrawDifficultyArray(
                    token as JArray
                );
            }

            // ========================================================
            // 普通参数
            // 普通参数强制按照 int 处理
            // ========================================================

            else
            {
                int value =
                    GetTokenInt(token);

                int newValue =
                    EditorGUILayout.IntField(
                        value
                    );

                if (newValue != value)
                {
                    args[i] =
                        newValue;

                    MarkDirty();
                }
            }

            if (GUILayout.Button(
                "-",
                GUILayout.Width(25)))
            {
                args.RemoveAt(i);

                EditorGUILayout.EndHorizontal();

                MarkDirty();

                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        // ============================================================
        // 添加参数
        // ============================================================

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(
            "+ Add Arg"))
        {
            // 普通参数默认 int
            args.Add(0);

            MarkDirty();
        }

        if (GUILayout.Button(
            "+ Difficulty Array"))
        {
            // ========================================================
            // 默认只有一项
            // 类型为 float
            // ========================================================

            JArray difficulty =
                new JArray
                {
                    0.0f
                };

            args.Add(
                difficulty
            );

            MarkDirty();
        }

        EditorGUILayout.EndHorizontal();
    }

    // ============================================================
    // Generic Difficulty Array
    // ============================================================

    private void DrawGenericDifficultyArray(
        JArray array)
    {
        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        for (int i = 0; i < array.Count; i++)
        {
            int value =
                GetTokenInt(
                    array[i]
                );

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                $"Diff {i}",
                GUILayout.Width(60)
            );

            int newValue =
                EditorGUILayout.IntField(
                    value
                );

            if (newValue != value)
            {
                array[i] =
                    newValue;

                MarkDirty();
            }

            if (GUILayout.Button(
                "-",
                GUILayout.Width(25)))
            {
                array.RemoveAt(i);

                if (array.Count == 0)
                {
                    array.Add(0);
                }

                MarkDirty();

                EditorGUILayout.EndHorizontal();

                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button(
            "+ Difficulty"))
        {
            array.Add(0);

            MarkDirty();
        }

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // Level Config
    // ============================================================

    private void DrawLevelConfig(
        JObject level)
    {
        JObject config =
            GetOrCreateObject(
                level,
                "LevelConfig"
            );

        showConfig =
            EditorGUILayout.Foldout(
                showConfig,
                "LevelConfig",
                true,
                EditorStyles.foldoutHeader
            );

        if (!showConfig)
            return;

        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        DrawIntField(
            config,
            "width"
        );

        DrawIntField(
            config,
            "height"
        );

        DrawIntField(
            config,
            "time_max"
        );

        DrawIntField(
            config,
            "times"
        );

        // ========================================================
        // mine Difficulty Array
        // ========================================================

        DrawDifficultyArrayField(
            config,
            "mine"
        );

        EditorGUILayout.EndVertical();
    }

    // ============================================================
    // Add Event
    // ============================================================

    private void AddEvent(
        JObject setup)
    {
        List<string> names =
            setup.Properties()
                .Select(x => x.Name)
                .ToList();

        string newName =
            GetNextName(
                names,
                "Event"
            );

        string firstSource =
            GetFirstSourceName();

        JObject newEvent =
            new JObject
            {
                ["type"] = 0,

                ["source"] =
                    firstSource,

                ["trigger"] =
                    new JObject
                    {
                        ["source"] =
                            "LevelTimer",

                        ["prm"] =
                            0f
                    },

                ["target"] =
                    0,

                ["time"] =
                    0f
            };

        // ========================================================
        // 无选择
        // ========================================================

        if (string.IsNullOrEmpty(
            selectedEvent))
        {
            setup.Add(
                newName,
                newEvent
            );
        }
        else
        {
            // ====================================================
            // 插入选中 Event 后面
            // ====================================================

            JObject newSetup =
                new JObject();

            bool inserted =
                false;

            foreach (JProperty property
                in setup.Properties().ToList())
            {
                newSetup.Add(
                    property.Name,
                    property.Value
                );

                if (property.Name ==
                    selectedEvent)
                {
                    newSetup.Add(
                        newName,
                        newEvent
                    );

                    inserted = true;
                }
            }

            if (!inserted)
            {
                newSetup.Add(
                    newName,
                    newEvent
                );
            }

            JObject level =
                GetCurrentLevel();

            if (level != null)
            {
                level["Setup"] =
                    newSetup;
            }
        }

        selectedEvent =
            newName;

        MarkDirty();
    }

    // ============================================================
    // Delete Event
    // ============================================================

    private void DeleteSelectedEvent(
        JObject setup)
    {
        if (string.IsNullOrEmpty(
            selectedEvent))
        {
            return;
        }

        if (!setup.ContainsKey(
            selectedEvent))
        {
            return;
        }

        bool confirm =
            EditorUtility.DisplayDialog(
                "Delete Event",
                $"确定删除 Event \"{selectedEvent}\"？",
                "Delete",
                "Cancel"
            );

        if (!confirm)
            return;

        string deletedName =
            selectedEvent;

        setup.Remove(
            deletedName
        );

        // ========================================================
        // 清理 Trigger
        // ========================================================

        foreach (JProperty property
            in setup.Properties())
        {
            JObject ev =
                property.Value as JObject;

            if (ev == null)
                continue;

            JObject trigger =
                ev["trigger"] as JObject;

            if (trigger == null)
                continue;

            string triggerSource =
                GetString(
                    trigger,
                    "source"
                );

            if (triggerSource ==
                deletedName)
            {
                trigger["source"] =
                    "LevelTimer";

                trigger["prm"] =
                    0f;
            }
        }

        selectedEvent =
            null;

        MarkDirty();
    }

    // ============================================================
    // Move Event
    // ============================================================

    private void MoveEvent(
        JObject setup,
        string name,
        int direction)
    {
        if (string.IsNullOrEmpty(name))
            return;

        List<JProperty> properties =
            setup.Properties()
                .ToList();

        int index =
            properties.FindIndex(
                x => x.Name == name
            );

        if (index < 0)
            return;

        int newIndex =
            index + direction;

        if (newIndex < 0 ||
            newIndex >= properties.Count)
        {
            return;
        }

        JProperty moving =
            properties[index];

        properties.RemoveAt(index);

        properties.Insert(
            newIndex,
            moving
        );

        JObject newSetup =
            new JObject();

        foreach (JProperty property
            in properties)
        {
            newSetup.Add(
                property.Name,
                property.Value
            );
        }

        JObject level =
            GetCurrentLevel();

        if (level != null)
        {
            level["Setup"] =
                newSetup;
        }

        MarkDirty();
    }

    // ============================================================
    // Can Move Event
    // ============================================================

    private bool CanMoveEventUp(
        JObject setup,
        string name)
    {
        List<string> names =
            setup.Properties()
                .Select(x => x.Name)
                .ToList();

        int index =
            names.IndexOf(name);

        return index > 0;
    }

    private bool CanMoveEventDown(
        JObject setup,
        string name)
    {
        List<string> names =
            setup.Properties()
                .Select(x => x.Name)
                .ToList();

        int index =
            names.IndexOf(name);

        return index >= 0 &&
               index < names.Count - 1;
    }

    // ============================================================
    // Add Source
    // ============================================================

    private void AddSource(
        JObject source)
    {
        List<string> names =
            source.Properties()
                .Select(x => x.Name)
                .ToList();

        string name =
            GetNextName(
                names,
                "T"
            );

        JObject data =
            new JObject
            {
                ["EventType"] =
                    0,

                // =================================================
                // ID 是 DifficultyArray
                // 默认只有一项
                // =================================================

                ["ID"] =
                    new JArray
                    {
                        0
                    },

                ["args"] =
                    new JArray()
            };

        source.Add(
            name,
            data
        );

        selectedSource =
            name;

        MarkDirty();
    }

    // ============================================================
    // Delete Source
    // ============================================================

    private void DeleteSelectedSource(
        JObject source)
    {
        if (string.IsNullOrEmpty(
            selectedSource))
        {
            return;
        }

        if (!source.ContainsKey(
            selectedSource))
        {
            return;
        }

        bool confirm =
            EditorUtility.DisplayDialog(
                "Delete Source",
                $"确定删除 Source \"{selectedSource}\"？",
                "Delete",
                "Cancel"
            );

        if (!confirm)
            return;

        string deletedName =
            selectedSource;

        source.Remove(
            deletedName
        );

        JObject level =
            GetCurrentLevel();

        if (level != null)
        {
            JObject setup =
                level["Setup"] as JObject;

            if (setup != null)
            {
                foreach (JProperty property
                    in setup.Properties())
                {
                    JObject ev =
                        property.Value as JObject;

                    if (ev == null)
                        continue;

                    string eventSource =
                        GetString(
                            ev,
                            "source"
                        );

                    if (eventSource ==
                        deletedName)
                    {
                        ev["source"] =
                            GetFirstSourceName();
                    }
                }
            }
        }

        selectedSource =
            null;

        MarkDirty();
    }

    // ============================================================
    // Move Source
    // ============================================================

    private void MoveSource(
        JObject source,
        string name,
        int direction)
    {
        if (string.IsNullOrEmpty(name))
            return;

        List<JProperty> properties =
            source.Properties()
                .ToList();

        int index =
            properties.FindIndex(
                x => x.Name == name
            );

        if (index < 0)
            return;

        int newIndex =
            index + direction;

        if (newIndex < 0 ||
            newIndex >= properties.Count)
        {
            return;
        }

        JProperty moving =
            properties[index];

        properties.RemoveAt(index);

        properties.Insert(
            newIndex,
            moving
        );

        JObject newSource =
            new JObject();

        foreach (JProperty property
            in properties)
        {
            newSource.Add(
                property.Name,
                property.Value
            );
        }

        JObject level =
            GetCurrentLevel();

        if (level != null)
        {
            level["Source"] =
                newSource;
        }

        MarkDirty();
    }

    // ============================================================
    // Can Move Source
    // ============================================================

    private bool CanMoveSourceUp(
        JObject source,
        string name)
    {
        List<string> names =
            source.Properties()
                .Select(x => x.Name)
                .ToList();

        int index =
            names.IndexOf(name);

        return index > 0;
    }

    private bool CanMoveSourceDown(
        JObject source,
        string name)
    {
        List<string> names =
            source.Properties()
                .Select(x => x.Name)
                .ToList();

        int index =
            names.IndexOf(name);

        return index >= 0 &&
               index < names.Count - 1;
    }

    // ============================================================
    // Add Level
    // ============================================================

    private void AddLevel()
    {
        JObject level =
            new JObject
            {
                ["Setup"] =
                    new JObject(),

                ["Source"] =
                    new JObject(),

                ["LevelConfig"] =
                    new JObject
                    {
                        ["width"] =
                            9,

                        ["height"] =
                            9,

                        ["time_max"] =
                            80,

                        ["times"] =
                            3,

                        // =========================================
                        // mine DifficultyArray
                        // 默认只有一项
                        // =========================================

                        ["mine"] =
                            new JArray
                            {
                                0
                            }
                    }
            };

        levels.Add(
            level
        );

        currentLevel =
            levels.Count - 1;

        selectedEvent =
            null;

        selectedSource =
            null;

        MarkDirty();
    }

    // ============================================================
    // Delete Level
    // ============================================================

    private void DeleteLevel()
    {
        if (levels.Count <= 0)
            return;

        bool confirm =
            EditorUtility.DisplayDialog(
                "Delete Level",
                $"确定删除 Level {currentLevel}？",
                "Delete",
                "Cancel"
            );

        if (!confirm)
            return;

        levels.RemoveAt(
            currentLevel
        );

        currentLevel =
            Mathf.Clamp(
                currentLevel,
                0,
                Mathf.Max(
                    0,
                    levels.Count - 1
                )
            );

        selectedEvent =
            null;

        selectedSource =
            null;

        MarkDirty();
    }

    // ============================================================
    // Level Config Int
    // ============================================================

    private void DrawIntField(
        JObject obj,
        string key)
    {
        int value =
            GetInt(
                obj,
                key
            );

        int newValue =
            EditorGUILayout.IntField(
                key,
                value
            );

        if (newValue != value)
        {
            obj[key] =
                newValue;

            MarkDirty();
        }
    }

    // ============================================================
    // Current Level
    // ============================================================

    private JObject GetCurrentLevel()
    {
        if (levels == null)
            return null;

        if (currentLevel < 0 ||
            currentLevel >= levels.Count)
        {
            return null;
        }

        return levels[currentLevel]
            as JObject;
    }

    // ============================================================
    // Current Setup
    // ============================================================

    private JObject GetCurrentSetup()
    {
        JObject level =
            GetCurrentLevel();

        if (level == null)
            return null;

        return level["Setup"]
            as JObject;
    }

    // ============================================================
    // Source Names
    // ============================================================

    private List<string> GetSourceNames()
    {
        JObject level =
            GetCurrentLevel();

        if (level == null)
        {
            return new List<string>();
        }

        JObject source =
            level["Source"]
                as JObject;

        if (source == null)
        {
            return new List<string>();
        }

        return source.Properties()
            .Select(x => x.Name)
            .ToList();
    }

    // ============================================================
    // First Source
    // ============================================================

    private string GetFirstSourceName()
    {
        List<string> names =
            GetSourceNames();

        if (names.Count == 0)
            return "";

        return names[0];
    }

    // ============================================================
    // Trigger Text
    // ============================================================

    private string GetTriggerText(
        JObject trigger)
    {
        if (trigger == null)
            return "Trigger: None";

        return
            $"Trigger: " +
            $"{GetString(trigger, "source")} " +
            $"[{GetFloat(trigger, "prm"):0.###}]";
    }

    // ============================================================
    // Get / Create Object
    // ============================================================

    private JObject GetOrCreateObject(
        JObject parent,
        string key)
    {
        JObject obj =
            parent[key] as JObject;

        if (obj == null)
        {
            obj =
                new JObject();

            parent[key] =
                obj;

            MarkDirty();
        }

        return obj;
    }

    // ============================================================
    // Get String
    // ============================================================

    private string GetString(
        JObject obj,
        string key)
    {
        if (obj == null)
            return "";

        JToken token =
            obj[key];

        if (token == null)
            return "";

        return token.ToString();
    }

    // ============================================================
    // Get Int
    // ============================================================

    private int GetInt(
        JObject obj,
        string key)
    {
        if (obj == null)
            return 0;

        JToken token =
            obj[key];

        if (token == null)
            return 0;

        return GetTokenInt(token);
    }

    // ============================================================
    // Get Token Int
    // ============================================================

    private int GetTokenInt(
        JToken token)
    {
        if (token == null)
            return 0;

        if (token.Type ==
            JTokenType.Integer)
        {
            return token.Value<int>();
        }

        if (token.Type ==
            JTokenType.Float)
        {
            return Mathf.RoundToInt(
                token.Value<float>()
            );
        }

        if (token.Type ==
            JTokenType.Boolean)
        {
            return token.Value<bool>()
                ? 1
                : 0;
        }

        if (int.TryParse(
            token.ToString(),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int result))
        {
            return result;
        }

        return 0;
    }

    // ============================================================
    // Get Float
    // ============================================================

    private float GetFloat(
        JObject obj,
        string key)
    {
        if (obj == null)
            return 0f;

        JToken token =
            obj[key];

        if (token == null)
            return 0f;

        if (float.TryParse(
            token.ToString(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float result))
        {
            return result;
        }

        return 0f;
    }

    // ============================================================
    // Difficulty Array Text
    // ============================================================

    private string GetDifficultyArrayText(
        JToken token)
    {
        if (token == null)
            return "[0]";

        if (token.Type !=
            JTokenType.Array)
        {
            return $"[{GetTokenInt(token)}]";
        }

        return token.ToString(
            Formatting.None
        );
    }

    // ============================================================
    // Args Text
    // ============================================================

    private string GetArgsText(
        JToken token)
    {
        if (token == null)
            return "[]";

        return token.ToString(
            Formatting.None
        );
    }

    // ============================================================
    // Parse Token
    // ============================================================

    private JToken ParseToken(
        string value)
    {
        value =
            value.Trim();

        if (int.TryParse(
            value,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out int intValue))
        {
            return new JValue(
                intValue
            );
        }

        if (float.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float floatValue))
        {
            return new JValue(
                floatValue
            );
        }

        if (bool.TryParse(
            value,
            out bool boolValue))
        {
            return new JValue(
                boolValue
            );
        }

        return new JValue(
            value
        );
    }

    // ============================================================
    // Next Name
    // ============================================================

    private string GetNextName(
        List<string> names,
        string prefix)
    {
        int index = 0;

        while (true)
        {
            string name =
                prefix + index;

            if (!names.Contains(name))
            {
                return name;
            }

            index++;
        }
    }

    // ============================================================
    // Dirty
    // ============================================================

    private void MarkDirty()
    {
        dirty = true;
    }

    // ============================================================
    // Load JSON
    // ============================================================

    private void LoadJson()
    {
        string fullPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                LEVEL_PATH
            );

        if (!File.Exists(fullPath))
        {
            Debug.LogError(
                $"Level.json 不存在：\n{fullPath}"
            );

            rootJson = null;
            levels = null;

            return;
        }

        try
        {
            string json =
                File.ReadAllText(
                    fullPath
                );

            // ====================================================
            // Level.json 根节点是数组
            // ====================================================

            rootJson =
                JObject.Parse(
                    "{\"Levels\":" +
                    json +
                    "}"
                );

            levels =
                rootJson["Levels"]
                    as JArray;

            if (levels == null)
            {
                Debug.LogError(
                    "Level.json 根节点必须是数组。"
                );

                rootJson = null;
                levels = null;

                return;
            }

            currentLevel =
                Mathf.Clamp(
                    currentLevel,
                    0,
                    Mathf.Max(
                        0,
                        levels.Count - 1
                    )
                );

            selectedEvent =
                null;

            selectedSource =
                null;

            dirty =
                false;
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"读取 Level.json 失败：\n{e}"
            );

            rootJson = null;
            levels = null;
        }
    }

    // ============================================================
    // Save JSON
    // ============================================================

    private void SaveJson()
    {
        if (levels == null)
            return;

        string fullPath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                LEVEL_PATH
            );

        try
        {
            string json =
                levels.ToString(
                    Formatting.Indented
                );

            File.WriteAllText(
                fullPath,
                json
            );

            AssetDatabase.Refresh();

            dirty =
                false;

            Debug.Log(
                "Level.json 保存成功。"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"保存 Level.json 失败：\n{e}"
            );
        }
    }

    // ============================================================
    // Destroy
    // ============================================================

    private void OnDestroy()
    {
        if (!dirty)
            return;

        bool save =
            EditorUtility.DisplayDialog(
                "Level JSON Editor",
                "Level.json 有未保存修改，是否保存？",
                "保存",
                "不保存"
            );

        if (save)
        {
            SaveJson();
        }
    }
}

#endif