using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class ToolManager : MonoBehaviour
{
    // ============================================================
    // Inspector
    // ============================================================

    // 道具
    public List<GameObject> tools = new();

    // 对应按钮
    public List<Button> buttons = new();

    // 对应快捷键
    public List<KeyCode> key_bindings = new();

    // 当前使用的道具
    // 0 = 没有道具
    // 1 = tools[0]
    // 2 = tools[1]
    // 3 = tools[2]
    public int current_tool_id = 0;

    // ------------------------------------------------------------
    // 数量系统
    // ------------------------------------------------------------

    // 每个道具的数量限制
    // -1 = 不限制
    public List<int> tool_limits = new();

    // 每个道具当前剩余数量
    private List<int> tool_counts = new();

    // 数量显示
    public List<TMP_Text> count_labels = new();

    // 是否自动消耗
    // true  = 使用一次自动消耗 1 个
    // false = 不自动消耗
    public List<bool> auto_consume = new();


    // ============================================================
    // Private
    // ============================================================

    // 道具原始位置
    private List<Vector3> original_positions = new();

    // List 的实际 index
    // -1 = 没有道具
    private int current_tool = -1;


    // ============================================================
    // Unity
    // ============================================================

    void Start()
    {
        // --------------------------------------------------------
        // 保存初始位置
        // --------------------------------------------------------

        foreach (GameObject tool in tools)
        {
            if (tool != null)
            {
                original_positions.Add(
                    tool.transform.position
                );
            }
            else
            {
                original_positions.Add(
                    Vector3.zero
                );
            }
        }


        // --------------------------------------------------------
        // 初始化数量
        // --------------------------------------------------------

        tool_counts.Clear();

        for (int i = 0; i < tools.Count; i++)
        {
            int limit = GetToolLimit(i);

            // -1 = 无限
            // 有限制的道具初始直接补满
            if (limit >= 0)
            {
                tool_counts.Add(limit);
            }
            else
            {
                // 无限道具这里用 -1 表示无限
                tool_counts.Add(-1);
            }
        }


        // --------------------------------------------------------
        // 绑定按钮
        // --------------------------------------------------------

        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;

            if (buttons[i] != null)
            {
                buttons[i].onClick.AddListener(
                    () => SelectTool(index)
                );
            }
        }


        UpdateCountLabels();
    }


    void Update()
    {
        // ========================================================
        // 右键取消当前道具
        // ========================================================

        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            ReturnCurrentTool();
        }


        // ========================================================
        // 快捷键
        // ========================================================

        if (Keyboard.current != null)
        {
            for (int i = 0; i < key_bindings.Count; i++)
            {
                if (GetKeyDown(key_bindings[i]))
                {
                    SelectTool(i);
                }
            }
        }


        // ========================================================
        // 跟随鼠标
        // ========================================================

        if (current_tool >= 0 &&
            current_tool < tools.Count)
        {
            FollowMouse(
                tools[current_tool]
            );
        }
    }


    // ============================================================
    // 快捷键检测
    // ============================================================

    bool GetKeyDown(KeyCode key)
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
            return false;

        switch (key)
        {
            case KeyCode.A:
                return keyboard.aKey.wasPressedThisFrame;

            case KeyCode.B:
                return keyboard.bKey.wasPressedThisFrame;

            case KeyCode.C:
                return keyboard.cKey.wasPressedThisFrame;

            case KeyCode.D:
                return keyboard.dKey.wasPressedThisFrame;

            case KeyCode.E:
                return keyboard.eKey.wasPressedThisFrame;

            case KeyCode.F:
                return keyboard.fKey.wasPressedThisFrame;

            case KeyCode.G:
                return keyboard.gKey.wasPressedThisFrame;

            case KeyCode.H:
                return keyboard.hKey.wasPressedThisFrame;

            case KeyCode.I:
                return keyboard.iKey.wasPressedThisFrame;

            case KeyCode.J:
                return keyboard.jKey.wasPressedThisFrame;

            case KeyCode.K:
                return keyboard.kKey.wasPressedThisFrame;

            case KeyCode.L:
                return keyboard.lKey.wasPressedThisFrame;

            case KeyCode.M:
                return keyboard.mKey.wasPressedThisFrame;

            case KeyCode.N:
                return keyboard.nKey.wasPressedThisFrame;

            case KeyCode.O:
                return keyboard.oKey.wasPressedThisFrame;

            case KeyCode.P:
                return keyboard.pKey.wasPressedThisFrame;

            case KeyCode.Q:
                return keyboard.qKey.wasPressedThisFrame;

            case KeyCode.R:
                return keyboard.rKey.wasPressedThisFrame;

            case KeyCode.S:
                return keyboard.sKey.wasPressedThisFrame;

            case KeyCode.T:
                return keyboard.tKey.wasPressedThisFrame;

            case KeyCode.U:
                return keyboard.uKey.wasPressedThisFrame;

            case KeyCode.V:
                return keyboard.vKey.wasPressedThisFrame;

            case KeyCode.W:
                return keyboard.wKey.wasPressedThisFrame;

            case KeyCode.X:
                return keyboard.xKey.wasPressedThisFrame;

            case KeyCode.Y:
                return keyboard.yKey.wasPressedThisFrame;

            case KeyCode.Z:
                return keyboard.zKey.wasPressedThisFrame;

            case KeyCode.Space:
                return keyboard.spaceKey.wasPressedThisFrame;

            case KeyCode.LeftShift:
                return keyboard.leftShiftKey.wasPressedThisFrame;

            case KeyCode.RightShift:
                return keyboard.rightShiftKey.wasPressedThisFrame;

            case KeyCode.LeftControl:
                return keyboard.leftCtrlKey.wasPressedThisFrame;

            case KeyCode.RightControl:
                return keyboard.rightCtrlKey.wasPressedThisFrame;

            case KeyCode.LeftAlt:
                return keyboard.leftAltKey.wasPressedThisFrame;

            case KeyCode.RightAlt:
                return keyboard.rightAltKey.wasPressedThisFrame;

            case KeyCode.Alpha0:
                return keyboard.digit0Key.wasPressedThisFrame;

            case KeyCode.Alpha1:
                return keyboard.digit1Key.wasPressedThisFrame;

            case KeyCode.Alpha2:
                return keyboard.digit2Key.wasPressedThisFrame;

            case KeyCode.Alpha3:
                return keyboard.digit3Key.wasPressedThisFrame;

            case KeyCode.Alpha4:
                return keyboard.digit4Key.wasPressedThisFrame;

            case KeyCode.Alpha5:
                return keyboard.digit5Key.wasPressedThisFrame;

            case KeyCode.Alpha6:
                return keyboard.digit6Key.wasPressedThisFrame;

            case KeyCode.Alpha7:
                return keyboard.digit7Key.wasPressedThisFrame;

            case KeyCode.Alpha8:
                return keyboard.digit8Key.wasPressedThisFrame;

            case KeyCode.Alpha9:
                return keyboard.digit9Key.wasPressedThisFrame;
        }

        return false;
    }


    // ============================================================
    // 选择道具
    // ============================================================

    void SelectTool(int index)
    {
        if (index < 0 ||
            index >= tools.Count)
        {
            return;
        }

        if (tools[index] == null)
            return;


        // --------------------------------------------------------
        // 数量为 0 时不能使用
        // --------------------------------------------------------

        if (!HasTool(index))
            return;


        // --------------------------------------------------------
        // 再次选择当前道具
        // → 放回原位
        // --------------------------------------------------------

        if (current_tool == index)
        {
            ReturnCurrentTool();
            return;
        }


        // --------------------------------------------------------
        // 如果已经拿着其他道具
        // → 先放回
        // --------------------------------------------------------

        if (current_tool >= 0)
        {
            ReturnCurrentTool();
        }


        // --------------------------------------------------------
        // 选择新的道具
        // --------------------------------------------------------

        current_tool = index;

        // 0 = 没有道具
        // 1 = tools[0]
        current_tool_id = index + 1;
    }


    // ============================================================
    // 使用道具
    // ============================================================

    // 外部调用
    // amount 可以是正数，也可以是负数
    //
    // 例如：
    // ConsumeTool(0, 1);   // tools[0] -1
    // ConsumeTool(0, 5);   // tools[0] -5
    // ConsumeTool(0, -1);  // tools[0] +1
    public void ConsumeTool(int index, int amount)
    {
        if (index < 0 ||
            index >= tools.Count)
        {
            return;
        }

        // 无限道具不需要消耗
        if (GetToolLimit(index) < 0)
            return;

        tool_counts[index] -= amount;

        // 不允许小于 0
        tool_counts[index] =
            Mathf.Max(0, tool_counts[index]);


        UpdateCountLabels();

        // 当前道具被消耗光
        if (current_tool == index &&
            tool_counts[index] <= 0)
        {
            ReturnCurrentTool();
        }
    }


    // ============================================================
    // 自动消耗当前道具
    // ============================================================

    public void ConsumeCurrentTool()
    {
        if (current_tool < 0)
            return;

        if (!GetAutoConsume(current_tool))
            return;
        
        ConsumeTool(
            current_tool,
            1
        );
    }


    // ============================================================
    // 判断是否拥有道具
    // ============================================================

    public bool HasTool(int index)
    {
        if (index < 0 ||
            index >= tools.Count)
        {
            return false;
        }

        // -1 = 无限
        if (GetToolLimit(index) < 0)
            return true;

        return tool_counts[index] > 0;
    }


    // ============================================================
    // 获取当前数量
    // ============================================================

    public int GetToolCount(int index)
    {
        if (index < 0 ||
            index >= tool_counts.Count)
        {
            return 0;
        }

        return tool_counts[index];
    }


    // ============================================================
    // 获取数量限制
    // ============================================================

    int GetToolLimit(int index)
    {
        if (index < 0 ||
            index >= tool_limits.Count)
        {
            return -1;
        }

        return tool_limits[index];
    }


    // ============================================================
    // 获取自动消耗设置
    // ============================================================

    bool GetAutoConsume(int index)
    {
        if (index < 0 ||
            index >= auto_consume.Count)
        {
            return false;
        }

        return auto_consume[index];
    }


    // ============================================================
    // 更新数量 Label
    // ============================================================

    void UpdateCountLabels()
    {
        for (int i = 0; i < count_labels.Count; i++)
        {
            if (count_labels[i] == null)
                continue;

            // 没有对应道具
            if (i >= tools.Count)
            {
                count_labels[i].text = "";
                continue;
            }

            // 无限道具
            if (GetToolLimit(i) < 0)
            {
                count_labels[i].text = "∞";
                continue;
            }

            count_labels[i].text =
                tool_counts[i].ToString();
        }
    }


    // ============================================================
    // 跟随鼠标
    // ============================================================

    void FollowMouse(GameObject tool)
    {
        if (Mouse.current == null)
            return;

        if (Camera.main == null)
            return;

        Vector2 mouse_position =
            Mouse.current.position.ReadValue();

        Vector3 world_position =
            Camera.main.ScreenToWorldPoint(
                mouse_position
            );

        // 保持道具原来的 Z
        world_position.z =
            tool.transform.position.z;

        tool.transform.position =
            world_position;
    }


    // ============================================================
    // 放回当前道具
    // ============================================================

    public void ReturnCurrentTool()
    {
        if (current_tool < 0 ||
            current_tool >= tools.Count)
        {
            current_tool = -1;
            current_tool_id = 0;
            return;
        }


        // 恢复原来的位置
        if (tools[current_tool] != null &&
            current_tool < original_positions.Count)
        {
            tools[current_tool].transform.position =
                original_positions[current_tool];
        }


        // 清除当前道具
        current_tool = -1;

        // 0 = 没有道具
        current_tool_id = 0;
    }


    // ============================================================
    // 重开
    // ============================================================

    public void Restart()
    {
        // 先取消当前道具
        ReturnCurrentTool();

        // 所有限制数量的道具补满
        for (int i = 0; i < tools.Count; i++)
        {
            int limit = GetToolLimit(i);

            if (limit >= 0)
            {
                if (i >= tool_counts.Count)
                {
                    tool_counts.Add(limit);
                }
                else
                {
                    tool_counts[i] = limit;
                }
            }
            else
            {
                if (i >= tool_counts.Count)
                {
                    tool_counts.Add(-1);
                }
                else
                {
                    tool_counts[i] = -1;
                }
            }
        }

        UpdateCountLabels();
    }
}