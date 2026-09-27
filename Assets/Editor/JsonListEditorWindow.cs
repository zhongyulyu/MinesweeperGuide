using UnityEngine;
using UnityEditor;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public class JsonListEditorWindow : EditorWindow
{
    // ============================================================
    // Constants
    // ============================================================

    private const float RowHeight = 24f;
    private const float IndexWidth = 45f;
    private const float ButtonWidth = 28f;
    private const float MinColumnWidth = 160f;

    private const float HeaderHeight = 45f;
    private const float TableHeight = 300f;


    // ============================================================
    // JSON Data
    // ============================================================

    private JArray root;

    private string currentFilePath = "";


    // ============================================================
    // Scroll
    // ============================================================

    private Vector2 rowScroll;


    // ============================================================
    // Selection
    // ============================================================

    private int selectedRow = -1;


    // ============================================================
    // Add Field
    // ============================================================

    private string newFieldName = "";
    private string newFieldValue = "";


    // ============================================================
    // Window
    // ============================================================

    [MenuItem("Tools/JSON List Editor")]
    public static void Open()
    {
        GetWindow<JsonListEditorWindow>(
            "JSON List Editor"
        );
    }


    // ============================================================
    // Enable
    // ============================================================

    private void OnEnable()
    {
        titleContent =
            new GUIContent(
                "JSON List Editor"
            );

        minSize =
            new Vector2(
                800,
                500
            );
    }


    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.Space(5);

        if (root == null)
        {
            DrawEmptyState();
            return;
        }

        DrawInfo();

        EditorGUILayout.Space(5);

        DrawTable();

        EditorGUILayout.Space(5);

        DrawAddField();

        EditorGUILayout.Space(5);

        DrawBottomButtons();
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
            "New",
            EditorStyles.toolbarButton,
            GUILayout.Width(60)
        ))
        {
            NewJson();
        }

        if (GUILayout.Button(
            "Open",
            EditorStyles.toolbarButton,
            GUILayout.Width(60)
        ))
        {
            OpenJson();
        }

        if (GUILayout.Button(
            "Save",
            EditorStyles.toolbarButton,
            GUILayout.Width(60)
        ))
        {
            SaveJson();
        }

        if (GUILayout.Button(
            "Save As",
            EditorStyles.toolbarButton,
            GUILayout.Width(70)
        ))
        {
            SaveJsonAs();
        }

        GUILayout.FlexibleSpace();

        string fileName =
            string.IsNullOrEmpty(
                currentFilePath
            )
                ? "Untitled"
                : Path.GetFileName(
                    currentFilePath
                );

        GUILayout.Label(
            fileName,
            EditorStyles.toolbarButton
        );

        EditorGUILayout.EndHorizontal();
    }


    // ============================================================
    // Empty State
    // ============================================================

    private void DrawEmptyState()
    {
        EditorGUILayout.Space(30);

        EditorGUILayout.HelpBox(
            "Open a JSON file or create a new one.",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        if (GUILayout.Button(
            "Create New JSON",
            GUILayout.Height(35)
        ))
        {
            NewJson();
        }
    }


    // ============================================================
    // Info
    // ============================================================

    private void DrawInfo()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label(
            $"Rows: {root.Count}",
            EditorStyles.boldLabel
        );

        if (selectedRow >= 0 &&
            selectedRow < root.Count)
        {
            GUILayout.Space(20);

            GUILayout.Label(
                $"Selected: {selectedRow}",
                EditorStyles.boldLabel
            );
        }

        if (root.Count > 0)
        {
            GUILayout.Space(20);

            JObject first =
                root[0] as JObject;

            if (first != null)
            {
                int fieldCount =
                    GetObjectPropertyCount(
                        first
                    );

                GUILayout.Label(
                    $"Fields: {fieldCount}",
                    EditorStyles.boldLabel
                );
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(
            "Validate",
            GUILayout.Width(80)
        ))
        {
            ValidateStructure();
        }

        EditorGUILayout.EndHorizontal();
    }


    // ============================================================
    // Table
    // ============================================================

    private void DrawTable()
    {
        if (root.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "The JSON list is empty.",
                MessageType.Info
            );

            return;
        }

        JObject first =
            root[0] as JObject;

        if (first == null)
        {
            EditorGUILayout.HelpBox(
                "Every item in the root list must be an object.",
                MessageType.Error
            );

            return;
        }

        List<JProperty> properties =
            GetProperties(
                first
            );

        float totalWidth =
            IndexWidth +
            ButtonWidth +
            properties.Count *
            MinColumnWidth;


        // ========================================================
        // Table container
        // ========================================================

        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );


        // ========================================================
        // Header
        // ========================================================

        EditorGUILayout.BeginHorizontal(
            GUILayout.Width(totalWidth),
            GUILayout.Height(HeaderHeight)
        );

        GUILayout.Box(
            "#",
            EditorStyles.toolbarButton,
            GUILayout.Width(IndexWidth),
            GUILayout.Height(HeaderHeight)
        );

        GUILayout.Box(
            "",
            EditorStyles.toolbarButton,
            GUILayout.Width(ButtonWidth),
            GUILayout.Height(HeaderHeight)
        );

        for (
            int i = 0;
            i < properties.Count;
            i++
        )
        {
            DrawHeader(
                properties[i]
            );
        }

        EditorGUILayout.EndHorizontal();


        // ========================================================
        // Only rows scroll
        // ========================================================

        rowScroll =
            EditorGUILayout.BeginScrollView(
                rowScroll,
                GUILayout.Height(TableHeight)
            );


        EditorGUILayout.BeginHorizontal(
            GUILayout.Width(totalWidth)
        );

        EditorGUILayout.BeginVertical();


        for (
            int row = 0;
            row < root.Count;
            row++
        )
        {
            DrawRow(
                row,
                properties,
                totalWidth
            );
        }


        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();


        EditorGUILayout.EndVertical();
    }


    // ============================================================
    // Header
    // ============================================================

    private void DrawHeader(
        JProperty property
    )
    {
        EditorGUILayout.BeginVertical(
            GUILayout.Width(
                MinColumnWidth
            )
        );

        EditorGUILayout.BeginHorizontal();

        string newName =
            EditorGUILayout.TextField(
                property.Name
            );

        if (
            newName != property.Name &&
            !string.IsNullOrWhiteSpace(
                newName
            )
        )
        {
            RenameProperty(
                property.Name,
                newName
            );

            GUIUtility.ExitGUI();
        }

        if (GUILayout.Button(
            "X",
            GUILayout.Width(
                ButtonWidth
            )
        ))
        {
            DeleteProperty(
                property.Name
            );

            GUIUtility.ExitGUI();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Label(
            GetTypeText(
                property.Value
            ),
            EditorStyles.miniLabel
        );

        EditorGUILayout.EndVertical();
    }


    // ============================================================
    // Row
    // ============================================================

    private void DrawRow(
        int row,
        List<JProperty> properties,
        float totalWidth
    )
    {
        JObject obj =
            root[row] as JObject;

        if (obj == null)
            return;


        bool selected =
            selectedRow == row;


        // ========================================================
        // Background
        // ========================================================

        Rect rowRect =
            EditorGUILayout.BeginHorizontal(
                GUILayout.Width(totalWidth),
                GUILayout.Height(RowHeight)
            );


        if (selected)
        {
            Rect backgroundRect =
                new Rect(
                    rowRect.x,
                    rowRect.y,
                    totalWidth,
                    RowHeight
                );

            EditorGUI.DrawRect(
                backgroundRect,
                new Color(
                    0.25f,
                    0.45f,
                    0.75f,
                    0.35f
                )
            );
        }


        // ========================================================
        // Index
        // ========================================================

        if (GUILayout.Button(
            row.ToString(),
            EditorStyles.label,
            GUILayout.Width(IndexWidth)
        ))
        {
            SelectRow(row);
        }


        // ========================================================
        // Delete
        // ========================================================

        if (GUILayout.Button(
            "X",
            GUILayout.Width(ButtonWidth)
        ))
        {
            SelectRow(row);

            if (EditorUtility.DisplayDialog(
                "Delete Row",
                $"Delete row {row}?",
                "Delete",
                "Cancel"
            ))
            {
                root.RemoveAt(row);

                if (root.Count == 0)
                {
                    selectedRow = -1;
                }
                else if (selectedRow >= root.Count)
                {
                    selectedRow =
                        root.Count - 1;
                }
            }

            EditorGUILayout.EndHorizontal();

            GUIUtility.ExitGUI();
        }


        // ========================================================
        // Values
        // ========================================================

        foreach (
            JProperty property
            in properties
        )
        {
            JToken current =
                obj[property.Name];

            if (current == null)
            {
                GUILayout.Label(
                    "Missing",
                    GUILayout.Width(
                        MinColumnWidth
                    )
                );

                continue;
            }


            JToken newValue =
                DrawToken(
                    current,
                    MinColumnWidth
                );


            if (
                newValue != null &&
                !JToken.DeepEquals(
                    current,
                    newValue
                )
            )
            {
                obj[property.Name] =
                    newValue;
            }
        }


        EditorGUILayout.EndHorizontal();


        // ========================================================
        // Click empty area to select
        // ========================================================

        Event e =
            Event.current;

        if (
            e.type == EventType.MouseDown &&
            e.button == 0 &&
            rowRect.Contains(
                e.mousePosition
            )
        )
        {
            SelectRow(row);

            Repaint();

            e.Use();
        }
    }


    // ============================================================
    // Select Row
    // ============================================================

    private void SelectRow(
        int row
    )
    {
        if (
            root == null ||
            row < 0 ||
            row >= root.Count
        )
        {
            selectedRow = -1;
            return;
        }

        selectedRow = row;

        Repaint();
    }


    // ============================================================
    // Draw Token
    // ============================================================

    private JToken DrawToken(
        JToken token,
        float width
    )
    {
        if (token == null)
        {
            return JValue.CreateNull();
        }


        // ========================================================
        // Color / HSV
        // ========================================================

        if (token.Type ==
            JTokenType.Object)
        {
            JObject obj =
                token as JObject;

            if (IsColorObject(obj))
            {
                return DrawColorToken(
                    obj,
                    width
                );
            }

            if (IsHSVObject(obj))
            {
                return DrawHSVToken(
                    obj,
                    width
                );
            }
        }


        // ========================================================
        // Normal type
        // ========================================================

        switch (token.Type)
        {
            // ====================================================
            // String
            // ====================================================

            case JTokenType.String:
            {
                string value =
                    token.Value<string>();

                string newValue =
                    EditorGUILayout.TextField(
                        value,
                        GUILayout.Width(width)
                    );

                return new JValue(
                    newValue
                );
            }


            // ====================================================
            // Integer
            // ====================================================

            case JTokenType.Integer:
            {
                long value =
                    token.Value<long>();

                string text =
                    EditorGUILayout.TextField(
                        value.ToString(
                            CultureInfo.InvariantCulture
                        ),
                        GUILayout.Width(width)
                    );

                long newValue;

                if (
                    long.TryParse(
                        text,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out newValue
                    )
                )
                {
                    return new JValue(
                        newValue
                    );
                }

                return token;
            }


            // ====================================================
            // Float
            // ====================================================

            case JTokenType.Float:
            {
                double value =
                    token.Value<double>();

                string text =
                    EditorGUILayout.TextField(
                        value.ToString(
                            CultureInfo.InvariantCulture
                        ),
                        GUILayout.Width(width)
                    );

                double newValue;

                if (
                    double.TryParse(
                        text,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out newValue
                    )
                )
                {
                    return new JValue(
                        newValue
                    );
                }

                return token;
            }


            // ====================================================
            // Boolean
            // ====================================================

            case JTokenType.Boolean:
            {
                bool value =
                    token.Value<bool>();

                bool newValue =
                    EditorGUILayout.Toggle(
                        value,
                        GUILayout.Width(width)
                    );

                return new JValue(
                    newValue
                );
            }


            // ====================================================
            // Null
            // ====================================================

            case JTokenType.Null:
            {
                GUILayout.Label(
                    "null",
                    GUILayout.Width(width)
                );

                return token;
            }


            // ====================================================
            // Object
            // ====================================================

            case JTokenType.Object:
            {
                JObject obj =
                    token as JObject;

                int count =
                    GetObjectPropertyCount(
                        obj
                    );

                if (GUILayout.Button(
                    $"Object ({count})",
                    EditorStyles.miniButton,
                    GUILayout.Width(width)
                ))
                {
                    JsonObjectPopup.Open(
                        obj,
                        "Object"
                    );
                }

                return obj;
            }


            // ====================================================
            // Array
            // ====================================================

            case JTokenType.Array:
            {
                JArray array =
                    token as JArray;

                int count =
                    array != null
                        ? array.Count
                        : 0;

                if (GUILayout.Button(
                    $"Array [{count}]",
                    EditorStyles.miniButton,
                    GUILayout.Width(width)
                ))
                {
                    JsonArrayPopup.Open(
                        array,
                        "Array"
                    );
                }

                return array;
            }


            // ====================================================
            // Other
            // ====================================================

            default:
            {
                GUILayout.Label(
                    token.Type.ToString(),
                    GUILayout.Width(width)
                );

                return token;
            }
        }
    }


    // ============================================================
    // Is Color Object
    // ============================================================

    private bool IsColorObject(
        JObject obj
    )
    {
        if (obj == null)
            return false;


        bool hasR =
            obj["r"] != null;

        bool hasG =
            obj["g"] != null;

        bool hasB =
            obj["b"] != null;


        if (!hasR ||
            !hasG ||
            !hasB)
        {
            return false;
        }


        foreach (
            JProperty property
            in obj.Properties()
        )
        {
            string name =
                property.Name.ToLowerInvariant();

            if (
                name != "r" &&
                name != "g" &&
                name != "b" &&
                name != "a"
            )
            {
                return false;
            }
        }


        return true;
    }


    // ============================================================
    // Is HSV Object
    // ============================================================

    private bool IsHSVObject(
        JObject obj
    )
    {
        if (obj == null)
            return false;


        bool hasH =
            obj["h"] != null;

        bool hasS =
            obj["s"] != null;

        bool hasV =
            obj["v"] != null;


        if (!hasH ||
            !hasS ||
            !hasV)
        {
            return false;
        }


        foreach (
            JProperty property
            in obj.Properties()
        )
        {
            string name =
                property.Name.ToLowerInvariant();

            if (
                name != "h" &&
                name != "s" &&
                name != "v"
            )
            {
                return false;
            }
        }


        return true;
    }


    // ============================================================
    // Draw Color
    // ============================================================

    private JToken DrawColorToken(
        JObject obj,
        float width
    )
    {
        float r =
            GetFloat(
                obj["r"],
                1f
            );

        float g =
            GetFloat(
                obj["g"],
                1f
            );

        float b =
            GetFloat(
                obj["b"],
                1f
            );

        float a =
            obj["a"] != null
                ? GetFloat(
                    obj["a"],
                    1f
                )
                : 1f;


        Color color =
            new Color(
                r,
                g,
                b,
                a
            );


        Color newColor =
            EditorGUILayout.ColorField(
                color,
                GUILayout.Width(width)
            );


        if (
            !Mathf.Approximately(
                newColor.r,
                r
            ) ||
            !Mathf.Approximately(
                newColor.g,
                g
            ) ||
            !Mathf.Approximately(
                newColor.b,
                b
            ) ||
            !Mathf.Approximately(
                newColor.a,
                a
            )
        )
        {
            obj["r"] =
                new JValue(
                    (double)newColor.r
                );

            obj["g"] =
                new JValue(
                    (double)newColor.g
                );

            obj["b"] =
                new JValue(
                    (double)newColor.b
                );

            if (obj["a"] != null)
            {
                obj["a"] =
                    new JValue(
                        (double)newColor.a
                    );
            }
        }


        return obj;
    }


    // ============================================================
    // Draw HSV
    // ============================================================

    private JToken DrawHSVToken(
        JObject obj,
        float width
    )
    {
        float h =
            GetFloat(
                obj["h"],
                0f
            );

        float s =
            GetFloat(
                obj["s"],
                0f
            );

        float v =
            GetFloat(
                obj["v"],
                0f
            );


        Color color;

        color = Color.HSVToRGB(
            h,
            s,
            v
        );


        Color newColor =
            EditorGUILayout.ColorField(
                color,
                GUILayout.Width(width)
            );


        if (
            !Mathf.Approximately(
                newColor.r,
                color.r
            ) ||
            !Mathf.Approximately(
                newColor.g,
                color.g
            ) ||
            !Mathf.Approximately(
                newColor.b,
                color.b
            )
        )
        {
            float newH;
            float newS;
            float newV;

            Color.RGBToHSV(
                newColor,
                out newH,
                out newS,
                out newV
            );

            obj["h"] =
                new JValue(
                    (double)newH
                );

            obj["s"] =
                new JValue(
                    (double)newS
                );

            obj["v"] =
                new JValue(
                    (double)newV
                );
        }


        return obj;
    }


    // ============================================================
    // Get Float
    // ============================================================

    private float GetFloat(
        JToken token,
        float defaultValue
    )
    {
        if (token == null)
            return defaultValue;


        try
        {
            return token.Value<float>();
        }
        catch
        {
            return defaultValue;
        }
    }


    // ============================================================
    // Add Field
    // ============================================================

    private void DrawAddField()
    {
        if (root.Count == 0)
            return;


        EditorGUILayout.BeginVertical(
            EditorStyles.helpBox
        );

        GUILayout.Label(
            "Add Field",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        newFieldName =
            EditorGUILayout.TextField(
                "Name",
                newFieldName
            );

        newFieldValue =
            EditorGUILayout.TextField(
                "Default Value",
                newFieldValue
            );

        if (GUILayout.Button(
            "Add",
            GUILayout.Width(60)
        ))
        {
            AddField();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Label(
            "The value is automatically detected as JSON.",
            EditorStyles.miniLabel
        );

        EditorGUILayout.EndVertical();
    }


    // ============================================================
    // Bottom Buttons
    // ============================================================

    private void DrawBottomButtons()
    {
        EditorGUILayout.BeginHorizontal();


        if (GUILayout.Button(
            "+ Add Row",
            GUILayout.Height(30)
        ))
        {
            AddRow();
        }


        if (GUILayout.Button(
            "Duplicate Row",
            GUILayout.Height(30)
        ))
        {
            DuplicateRow();
        }


        EditorGUILayout.EndHorizontal();
    }


    // ============================================================
    // New
    // ============================================================

    private void NewJson()
    {
        root =
            new JArray();

        currentFilePath =
            "";

        selectedRow =
            -1;

        rowScroll =
            Vector2.zero;
    }


    // ============================================================
    // Open
    // ============================================================

    private void OpenJson()
    {
        string path =
            EditorUtility.OpenFilePanel(
                "Open JSON",
                Application.dataPath,
                "json"
            );

        if (string.IsNullOrEmpty(path))
            return;


        try
        {
            string json =
                File.ReadAllText(
                    path
                );


            JToken parsed =
                JToken.Parse(
                    json
                );


            if (
                parsed.Type !=
                JTokenType.Array
            )
            {
                throw new Exception(
                    "The root of the JSON must be an array."
                );
            }


            JArray array =
                (JArray)parsed;


            ValidateListStructure(
                array
            );


            root =
                array;

            currentFilePath =
                path;

            selectedRow =
                -1;

            rowScroll =
                Vector2.zero;

            Repaint();
        }
        catch (JsonReaderException e)
        {
            EditorUtility.DisplayDialog(
                "Invalid JSON",
                e.Message,
                "OK"
            );
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog(
                "JSON Error",
                e.Message,
                "OK"
            );
        }
    }


    // ============================================================
    // Validate List Structure
    // ============================================================

    private void ValidateListStructure(
        JArray array
    )
    {
        if (array.Count == 0)
            return;


        // ========================================================
        // Every item must be Object
        // ========================================================

        for (
            int i = 0;
            i < array.Count;
            i++
        )
        {
            if (
                array[i].Type !=
                JTokenType.Object
            )
            {
                throw new Exception(
                    $"Item {i} is not an object."
                );
            }
        }


        JObject first =
            array[0] as JObject;


        if (first == null)
        {
            throw new Exception(
                "First item is not an object."
            );
        }


        List<string> fieldNames =
            GetPropertyNames(
                first
            );


        // ========================================================
        // Compare every object
        // ========================================================

        for (
            int i = 1;
            i < array.Count;
            i++
        )
        {
            JObject obj =
                array[i] as JObject;


            List<string> names =
                GetPropertyNames(
                    obj
                );


            if (
                names.Count !=
                fieldNames.Count
            )
            {
                throw new Exception(
                    $"Item {i} has a different field count."
                );
            }


            foreach (
                string fieldName
                in fieldNames
            )
            {
                JToken value =
                    obj[fieldName];


                if (value == null)
                {
                    throw new Exception(
                        $"Item {i} is missing field \"{fieldName}\"."
                    );
                }


                JToken firstValue =
                    first[fieldName];


                if (
                    !IsSameStructure(
                        firstValue,
                        value
                    )
                )
                {
                    throw new Exception(
                        $"Field \"{fieldName}\" in item {i} has a different structure."
                    );
                }
            }
        }
    }


    // ============================================================
    // Compare Structure
    // ============================================================

    private bool IsSameStructure(
        JToken a,
        JToken b
    )
    {
        if (a == null || b == null)
        {
            return a == null &&
                   b == null;
        }


        // ========================================================
        // Color
        // ========================================================

        if (
            a.Type == JTokenType.Object &&
            b.Type == JTokenType.Object
        )
        {
            JObject ao =
                a as JObject;

            JObject bo =
                b as JObject;


            if (
                IsColorObject(ao) ||
                IsHSVObject(ao)
            )
            {
                return IsSameObjectStructure(
                    ao,
                    bo
                );
            }
        }


        if (a.Type != b.Type)
            return false;


        // ========================================================
        // Object
        // ========================================================

        if (
            a.Type ==
            JTokenType.Object
        )
        {
            return IsSameObjectStructure(
                a as JObject,
                b as JObject
            );
        }


        // ========================================================
        // Array
        // ========================================================

        if (
            a.Type ==
            JTokenType.Array
        )
        {
            JArray aa =
                a as JArray;

            JArray ba =
                b as JArray;


            if (
                aa.Count == 0 ||
                ba.Count == 0
            )
            {
                return true;
            }


            return IsSameStructure(
                aa[0],
                ba[0]
            );
        }


        return true;
    }


    // ============================================================
    // Same Object Structure
    // ============================================================

    private bool IsSameObjectStructure(
        JObject a,
        JObject b
    )
    {
        if (a == null ||
            b == null)
        {
            return false;
        }


        List<string> aNames =
            GetPropertyNames(a);

        List<string> bNames =
            GetPropertyNames(b);


        if (
            aNames.Count !=
            bNames.Count
        )
        {
            return false;
        }


        foreach (
            string name
            in aNames
        )
        {
            if (b[name] == null)
                return false;


            if (
                !IsSameStructure(
                    a[name],
                    b[name]
                )
            )
            {
                return false;
            }
        }


        return true;
    }


    // ============================================================
    // Get Properties
    // ============================================================

    private List<JProperty> GetProperties(
        JObject obj
    )
    {
        List<JProperty> result =
            new List<JProperty>();


        if (obj == null)
            return result;


        foreach (
            JProperty property
            in obj.Properties()
        )
        {
            result.Add(
                property
            );
        }


        return result;
    }


    // ============================================================
    // Get Property Names
    // ============================================================

    private List<string> GetPropertyNames(
        JObject obj
    )
    {
        List<string> result =
            new List<string>();


        if (obj == null)
            return result;


        foreach (
            JProperty property
            in obj.Properties()
        )
        {
            result.Add(
                property.Name
            );
        }


        return result;
    }


    // ============================================================
    // Get Property Count
    // ============================================================

    private int GetObjectPropertyCount(
        JObject obj
    )
    {
        if (obj == null)
            return 0;


        int count = 0;


        foreach (
            JProperty property
            in obj.Properties()
        )
        {
            count++;
        }


        return count;
    }


    // ============================================================
    // Add Row
    // ============================================================

    private void AddRow()
    {
        if (root == null)
        {
            root =
                new JArray();
        }


        // ========================================================
        // Empty list
        // ========================================================

        if (root.Count == 0)
        {
            root.Add(
                new JObject()
            );

            selectedRow = 0;

            return;
        }


        // ========================================================
        // Template
        // ========================================================

        int insertIndex;


        if (
            selectedRow >= 0 &&
            selectedRow < root.Count
        )
        {
            insertIndex =
                selectedRow + 1;
        }
        else
        {
            insertIndex =
                root.Count;
        }


        JObject template =
            root[0] as JObject;


        if (template == null)
            return;


        // ========================================================
        // Clone
        // ========================================================

        JObject newObject =
            template.DeepClone()
                as JObject;


        ResetValues(
            newObject
        );


        root.Insert(
            insertIndex,
            newObject
        );


        selectedRow =
            insertIndex;
    }


    // ============================================================
    // Reset Values
    // ============================================================

    private void ResetValues(
        JToken token
    )
    {
        if (token == null)
            return;


        // ========================================================
        // Object
        // ========================================================

        if (
            token.Type ==
            JTokenType.Object
        )
        {
            JObject obj =
                token as JObject;


            foreach (
                JProperty property
                in obj.Properties()
            )
            {
                ResetValues(
                    property.Value
                );
            }


            return;
        }


        // ========================================================
        // Array
        // ========================================================

        if (
            token.Type ==
            JTokenType.Array
        )
        {
            JArray array =
                token as JArray;


            for (
                int i = 0;
                i < array.Count;
                i++
            )
            {
                ResetValues(
                    array[i]
                );
            }


            return;
        }


        // ========================================================
        // Primitive
        // ========================================================

        JValue value =
            token as JValue;


        if (value == null)
            return;


        switch (token.Type)
        {
            case JTokenType.Integer:

                value.Value =
                    0L;

                break;


            case JTokenType.Float:

                value.Value =
                    0.0;

                break;


            case JTokenType.String:

                value.Value =
                    "";

                break;


            case JTokenType.Boolean:

                value.Value =
                    false;

                break;
        }
    }


    // ============================================================
    // Duplicate Row
    // ============================================================

    private void DuplicateRow()
    {
        if (
            root == null ||
            root.Count == 0
        )
        {
            return;
        }


        if (
            selectedRow < 0 ||
            selectedRow >= root.Count
        )
        {
            selectedRow =
                root.Count - 1;
        }


        JToken clone =
            root[selectedRow]
                .DeepClone();


        int insertIndex =
            selectedRow + 1;


        root.Insert(
            insertIndex,
            clone
        );


        selectedRow =
            insertIndex;
    }


    // ============================================================
    // Add Field
    // ============================================================

    private void AddField()
    {
        if (
            root == null ||
            root.Count == 0
        )
        {
            EditorUtility.DisplayDialog(
                "Error",
                "Add a row first.",
                "OK"
            );

            return;
        }


        string name =
            newFieldName.Trim();


        if (string.IsNullOrEmpty(name))
        {
            EditorUtility.DisplayDialog(
                "Error",
                "Field name cannot be empty.",
                "OK"
            );

            return;
        }


        JObject first =
            root[0] as JObject;


        if (first == null)
            return;


        if (first[name] != null)
        {
            EditorUtility.DisplayDialog(
                "Error",
                $"Field \"{name}\" already exists.",
                "OK"
            );

            return;
        }


        JToken value;


        try
        {
            value =
                JToken.Parse(
                    newFieldValue
                );
        }
        catch
        {
            value =
                new JValue(
                    newFieldValue
                );
        }


        foreach (
            JToken item
            in root
        )
        {
            JObject obj =
                item as JObject;


            if (obj == null)
                continue;


            obj.Add(
                name,
                value.DeepClone()
            );
        }


        newFieldName = "";
        newFieldValue = "";
    }


    // ============================================================
    // Rename Property
    // ============================================================

    private void RenameProperty(
        string oldName,
        string newName
    )
    {
        if (
            string.IsNullOrWhiteSpace(
                newName
            )
        )
        {
            return;
        }


        if (oldName == newName)
            return;


        if (root == null)
            return;


        JObject first =
            root.Count > 0
                ? root[0] as JObject
                : null;


        if (first == null)
            return;


        if (
            first[newName] != null
        )
        {
            EditorUtility.DisplayDialog(
                "Error",
                $"Field \"{newName}\" already exists.",
                "OK"
            );

            return;
        }


        foreach (
            JToken item
            in root
        )
        {
            JObject obj =
                item as JObject;


            if (obj == null)
                continue;


            JProperty property =
                obj.Property(
                    oldName
                );


            if (property == null)
                continue;


            JToken value =
                property.Value;


            property.Replace(
                new JProperty(
                    newName,
                    value
                )
            );
        }
    }


    // ============================================================
    // Delete Property
    // ============================================================

    private void DeleteProperty(
        string name
    )
    {
        if (!EditorUtility.DisplayDialog(
            "Delete Field",
            $"Delete field \"{name}\" from every row?",
            "Delete",
            "Cancel"
        ))
        {
            return;
        }


        foreach (
            JToken item
            in root
        )
        {
            JObject obj =
                item as JObject;


            if (obj == null)
                continue;


            obj.Remove(
                name
            );
        }
    }


    // ============================================================
    // Get Type Text
    // ============================================================

    private string GetTypeText(
        JToken token
    )
    {
        if (token == null)
            return "null";


        if (
            token.Type ==
            JTokenType.Object
        )
        {
            JObject obj =
                token as JObject;


            if (IsColorObject(obj))
                return "color";


            if (IsHSVObject(obj))
                return "hsv";


            int count =
                GetObjectPropertyCount(
                    obj
                );


            return $"object ({count})";
        }


        if (
            token.Type ==
            JTokenType.Array
        )
        {
            JArray array =
                token as JArray;


            int count =
                array != null
                    ? array.Count
                    : 0;


            return $"array [{count}]";
        }


        return token.Type
            .ToString()
            .ToLowerInvariant();
    }


    // ============================================================
    // Validate
    // ============================================================

    private void ValidateStructure()
    {
        if (root == null)
            return;


        try
        {
            ValidateListStructure(
                root
            );


            CheckDuplicateIds();


            EditorUtility.DisplayDialog(
                "Valid",
                "JSON structure is valid.",
                "OK"
            );
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog(
                "Invalid",
                e.Message,
                "OK"
            );
        }
    }


    // ============================================================
    // Duplicate ID Check
    // ============================================================

    private void CheckDuplicateIds()
    {
        HashSet<string> ids =
            new HashSet<string>();


        for (
            int i = 0;
            i < root.Count;
            i++
        )
        {
            JObject obj =
                root[i] as JObject;


            if (obj == null)
                continue;


            JToken id =
                obj["id"];


            if (id == null)
                continue;


            string idText =
                id.ToString(
                    Formatting.None
                );


            if (!ids.Add(idText))
            {
                Debug.LogWarning(
                    $"JSON duplicate id: {idText}, row: {i}"
                );
            }
        }
    }


    // ============================================================
    // Save
    // ============================================================

    private void SaveJson()
    {
        if (
            string.IsNullOrEmpty(
                currentFilePath
            )
        )
        {
            SaveJsonAs();

            return;
        }


        WriteJson(
            currentFilePath
        );
    }


    // ============================================================
    // Save As
    // ============================================================

    private void SaveJsonAs()
    {
        string path =
            EditorUtility.SaveFilePanel(
                "Save JSON",
                Application.dataPath,
                "data",
                "json"
            );


        if (string.IsNullOrEmpty(path))
            return;


        currentFilePath =
            path;


        WriteJson(
            path
        );
    }


    // ============================================================
    // Write JSON
    // ============================================================

    private void WriteJson(
        string path
    )
    {
        try
        {
            string json =
                root.ToString(
                    Formatting.Indented
                );


            File.WriteAllText(
                path,
                json
            );


            AssetDatabase.Refresh();


            Debug.Log(
                $"JSON saved: {path}"
            );
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog(
                "Save Error",
                e.Message,
                "OK"
            );
        }
    }


    // ============================================================
    // Object Popup
    // ============================================================

    private class JsonObjectPopup :
        EditorWindow
    {
        private JObject obj;

        private Vector2 scroll;


        public static void Open(
            JObject obj,
            string title
        )
        {
            JsonObjectPopup window =
                CreateInstance<JsonObjectPopup>();


            window.obj =
                obj;


            window.titleContent =
                new GUIContent(
                    title
                );


            window.minSize =
                new Vector2(
                    500,
                    300
                );


            window.ShowUtility();
        }


        private void OnGUI()
        {
            if (obj == null)
                return;


            scroll =
                EditorGUILayout.BeginScrollView(
                    scroll
                );


            List<JProperty> properties =
                GetProperties(
                    obj
                );


            foreach (
                JProperty property
                in properties
            )
            {
                DrawProperty(
                    property
                );
            }


            EditorGUILayout.EndScrollView();
        }


        private void DrawProperty(
            JProperty property
        )
        {
            EditorGUILayout.BeginHorizontal();


            GUILayout.Label(
                property.Name,
                GUILayout.Width(120)
            );


            JToken token =
                property.Value;


            if (
                token.Type ==
                JTokenType.Object
            )
            {
                JObject objectValue =
                    token as JObject;


                if (IsColorObject(objectValue))
                {
                    DrawColorObject(
                        objectValue
                    );
                }
                else if (IsHSVObject(objectValue))
                {
                    DrawHSVObject(
                        objectValue
                    );
                }
                else if (GUILayout.Button(
                    "Object"
                ))
                {
                    Open(
                        objectValue,
                        property.Name
                    );
                }
            }
            else if (
                token.Type ==
                JTokenType.Array
            )
            {
                JArray array =
                    token as JArray;


                if (GUILayout.Button(
                    $"Array [{array.Count}]"
                ))
                {
                    JsonArrayPopup.Open(
                        array,
                        property.Name
                    );
                }
            }
            else
            {
                property.Value =
                    DrawPrimitive(
                        token
                    );
            }


            EditorGUILayout.EndHorizontal();
        }


        private void DrawColorObject(
            JObject obj
        )
        {
            float r =
                GetFloat(
                    obj["r"],
                    1
                );

            float g =
                GetFloat(
                    obj["g"],
                    1
                );

            float b =
                GetFloat(
                    obj["b"],
                    1
                );

            float a =
                obj["a"] != null
                    ? GetFloat(
                        obj["a"],
                        1
                    )
                    : 1;


            Color color =
                EditorGUILayout.ColorField(
                    new Color(
                        r,
                        g,
                        b,
                        a
                    )
                );


            obj["r"] =
                new JValue(
                    (double)color.r
                );

            obj["g"] =
                new JValue(
                    (double)color.g
                );

            obj["b"] =
                new JValue(
                    (double)color.b
                );

            if (obj["a"] != null)
            {
                obj["a"] =
                    new JValue(
                        (double)color.a
                    );
            }
        }


        private void DrawHSVObject(
            JObject obj
        )
        {
            float h =
                GetFloat(
                    obj["h"],
                    0
                );

            float s =
                GetFloat(
                    obj["s"],
                    0
                );

            float v =
                GetFloat(
                    obj["v"],
                    0
                );


            Color color;

            color = Color.HSVToRGB(
                h,
                s,
                v
            );


            Color newColor =
                EditorGUILayout.ColorField(
                    color
                );


            Color.RGBToHSV(
                newColor,
                out h,
                out s,
                out v
            );


            obj["h"] =
                new JValue(
                    (double)h
                );

            obj["s"] =
                new JValue(
                    (double)s
                );

            obj["v"] =
                new JValue(
                    (double)v
                );
        }


        private JToken DrawPrimitive(
            JToken token
        )
        {
            switch (token.Type)
            {
                case JTokenType.String:
                {
                    return new JValue(
                        EditorGUILayout.TextField(
                            token.Value<string>()
                        )
                    );
                }


                case JTokenType.Integer:
                {
                    long value =
                        token.Value<long>();


                    string text =
                        EditorGUILayout.TextField(
                            value.ToString(
                                CultureInfo.InvariantCulture
                            )
                        );


                    long result;


                    if (
                        long.TryParse(
                            text,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out result
                        )
                    )
                    {
                        return new JValue(
                            result
                        );
                    }


                    return token;
                }


                case JTokenType.Float:
                {
                    double value =
                        token.Value<double>();


                    string text =
                        EditorGUILayout.TextField(
                            value.ToString(
                                CultureInfo.InvariantCulture
                            )
                        );


                    double result;


                    if (
                        double.TryParse(
                            text,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out result
                        )
                    )
                    {
                        return new JValue(
                            result
                        );
                    }


                    return token;
                }


                case JTokenType.Boolean:
                {
                    bool value =
                        token.Value<bool>();


                    return new JValue(
                        EditorGUILayout.Toggle(
                            value
                        )
                    );
                }


                case JTokenType.Null:
                {
                    GUILayout.Label(
                        "null"
                    );


                    return token;
                }


                default:

                    return token;
            }
        }


        private List<JProperty> GetProperties(
            JObject obj
        )
        {
            List<JProperty> result =
                new List<JProperty>();


            if (obj == null)
                return result;


            foreach (
                JProperty property
                in obj.Properties()
            )
            {
                result.Add(
                    property
                );
            }


            return result;
        }


        private bool IsColorObject(
            JObject obj
        )
        {
            if (obj == null)
                return false;


            return obj["r"] != null &&
                   obj["g"] != null &&
                   obj["b"] != null;
        }


        private bool IsHSVObject(
            JObject obj
        )
        {
            if (obj == null)
                return false;


            return obj["h"] != null &&
                   obj["s"] != null &&
                   obj["v"] != null;
        }


        private float GetFloat(
            JToken token,
            float defaultValue
        )
        {
            if (token == null)
                return defaultValue;


            try
            {
                return token.Value<float>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }


    // ============================================================
    // Array Popup
    // ============================================================

    private class JsonArrayPopup :
        EditorWindow
    {
        private JArray array;

        private Vector2 scroll;


        public static void Open(
            JArray array,
            string title
        )
        {
            JsonArrayPopup window =
                CreateInstance<JsonArrayPopup>();


            window.array =
                array;


            window.titleContent =
                new GUIContent(
                    title
                );


            window.minSize =
                new Vector2(
                    500,
                    300
                );


            window.ShowUtility();
        }


        private void OnGUI()
        {
            if (array == null)
                return;


            scroll =
                EditorGUILayout.BeginScrollView(
                    scroll
                );


            for (
                int i = 0;
                i < array.Count;
                i++
            )
            {
                DrawItem(i);
            }


            EditorGUILayout.EndScrollView();


            EditorGUILayout.Space(5);


            if (GUILayout.Button(
                "+ Add",
                GUILayout.Height(30)
            ))
            {
                array.Add(
                    JValue.CreateNull()
                );
            }
        }


        private void DrawItem(
            int index
        )
        {
            if (
                index < 0 ||
                index >= array.Count
            )
            {
                return;
            }


            JToken token =
                array[index];


            EditorGUILayout.BeginHorizontal();


            GUILayout.Label(
                index.ToString(),
                GUILayout.Width(35)
            );


            if (
                token.Type ==
                JTokenType.Object
            )
            {
                JObject obj =
                    token as JObject;


                if (IsColorObject(obj))
                {
                    DrawColor(
                        obj
                    );
                }
                else if (IsHSVObject(obj))
                {
                    DrawHSV(
                        obj
                    );
                }
                else if (GUILayout.Button(
                    "Object"
                ))
                {
                    JsonObjectPopup.Open(
                        obj,
                        $"Object {index}"
                    );
                }
            }
            else if (
                token.Type ==
                JTokenType.Array
            )
            {
                JArray nested =
                    token as JArray;


                if (GUILayout.Button(
                    $"Array [{nested.Count}]"
                ))
                {
                    JsonArrayPopup.Open(
                        nested,
                        $"Array {index}"
                    );
                }
            }
            else
            {
                array[index] =
                    DrawPrimitive(
                        token
                    );
            }


            if (GUILayout.Button(
                "X",
                GUILayout.Width(25)
            ))
            {
                array.RemoveAt(index);

                EditorGUILayout.EndHorizontal();

                GUIUtility.ExitGUI();
            }


            EditorGUILayout.EndHorizontal();
        }


        private JToken DrawPrimitive(
            JToken token
        )
        {
            switch (token.Type)
            {
                case JTokenType.String:

                    return new JValue(
                        EditorGUILayout.TextField(
                            token.Value<string>()
                        )
                    );


                case JTokenType.Integer:
                {
                    long value =
                        token.Value<long>();


                    string text =
                        EditorGUILayout.TextField(
                            value.ToString(
                                CultureInfo.InvariantCulture
                            )
                        );


                    long result;


                    if (
                        long.TryParse(
                            text,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out result
                        )
                    )
                    {
                        return new JValue(
                            result
                        );
                    }


                    return token;
                }


                case JTokenType.Float:
                {
                    double value =
                        token.Value<double>();


                    string text =
                        EditorGUILayout.TextField(
                            value.ToString(
                                CultureInfo.InvariantCulture
                            )
                        );


                    double result;


                    if (
                        double.TryParse(
                            text,
                            NumberStyles.Float,
                            CultureInfo.InvariantCulture,
                            out result
                        )
                    )
                    {
                        return new JValue(
                            result
                        );
                    }


                    return token;
                }


                case JTokenType.Boolean:

                    return new JValue(
                        EditorGUILayout.Toggle(
                            token.Value<bool>()
                        )
                    );


                case JTokenType.Null:

                    GUILayout.Label(
                        "null"
                    );

                    return token;


                default:

                    GUILayout.Label(
                        token.Type.ToString()
                    );

                    return token;
            }
        }


        private void DrawColor(
            JObject obj
        )
        {
            float r =
                GetFloat(
                    obj["r"],
                    1
                );

            float g =
                GetFloat(
                    obj["g"],
                    1
                );

            float b =
                GetFloat(
                    obj["b"],
                    1
                );

            float a =
                obj["a"] != null
                    ? GetFloat(
                        obj["a"],
                        1
                    )
                    : 1;


            Color color =
                EditorGUILayout.ColorField(
                    new Color(
                        r,
                        g,
                        b,
                        a
                    )
                );


            obj["r"] =
                new JValue(
                    (double)color.r
                );

            obj["g"] =
                new JValue(
                    (double)color.g
                );

            obj["b"] =
                new JValue(
                    (double)color.b
                );

            if (obj["a"] != null)
            {
                obj["a"] =
                    new JValue(
                        (double)color.a
                    );
            }
        }


        private void DrawHSV(
            JObject obj
        )
        {
            float h =
                GetFloat(
                    obj["h"],
                    0
                );

            float s =
                GetFloat(
                    obj["s"],
                    0
                );

            float v =
                GetFloat(
                    obj["v"],
                    0
                );


            Color color;

            color = Color.HSVToRGB(
                h,
                s,
                v
            );


            Color newColor =
                EditorGUILayout.ColorField(
                    color
                );


            Color.RGBToHSV(
                newColor,
                out h,
                out s,
                out v
            );


            obj["h"] =
                new JValue(
                    (double)h
                );

            obj["s"] =
                new JValue(
                    (double)s
                );

            obj["v"] =
                new JValue(
                    (double)v
                );
        }


        private bool IsColorObject(
            JObject obj
        )
        {
            if (obj == null)
                return false;


            return obj["r"] != null &&
                   obj["g"] != null &&
                   obj["b"] != null;
        }


        private bool IsHSVObject(
            JObject obj
        )
        {
            if (obj == null)
                return false;


            return obj["h"] != null &&
                   obj["s"] != null &&
                   obj["v"] != null;
        }


        private float GetFloat(
            JToken token,
            float defaultValue
        )
        {
            if (token == null)
                return defaultValue;


            try
            {
                return token.Value<float>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}