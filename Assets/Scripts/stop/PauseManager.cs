using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("拖入你需要控制显示/隐藏的根物体（例如你的UI面板或模型组）")]
    public GameObject targetRoot; // 需要控制所有子物体的根节点

    [Tooltip("暂停时的时间流速，0为完全静止")]
    public float pauseTimeScale = 0f;

    void Update()
    {
        // 检测空格键按下
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 如果当前时间在正常流动，则暂停并显示
            if (Time.timeScale > 0f && Time.timeScale != pauseTimeScale)
            {
                SetTimeScale(pauseTimeScale);
                SetAllChildrenActive(targetRoot, true);
            }
            else // 否则恢复时间并隐藏
            {
                SetTimeScale(1f);
                SetAllChildrenActive(targetRoot, false);
            }
        }
    }

    /// <summary>
    /// 递归设置目标物体及其所有层级子物体的激活状态
    /// </summary>
    void SetAllChildrenActive(GameObject root, bool isActive)
    {
        if (root == null) return;

        // 设置当前物体的激活状态
        root.SetActive(isActive);

        // 遍历并递归调用所有子物体（自动检测，支持任意深度）
        foreach (Transform child in root.transform)
        {
            SetAllChildrenActive(child.gameObject, isActive);
        }
    }

    /// <summary>
    /// 安全地修改时间流速
    /// </summary>
    void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Debug.Log($"时间流速已更改为: {scale}");
    }
}