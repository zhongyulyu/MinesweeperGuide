using UnityEngine;
using TMPro;

/// <summary>
/// 将挂载物体的 TMP 文本实时同步为 "前缀 + 当前项目版本号"
/// 支持编辑模式与运行模式，前缀可在 Inspector 中配置
/// </summary>
[ExecuteInEditMode]               // 允许在编辑器模式下执行
[RequireComponent(typeof(TMP_Text))] // 自动要求挂载 TMP_Text 组件
public class VersionText : MonoBehaviour
{
    [SerializeField]
    [Tooltip("显示在版本号前面的文字，例如：Version: 或 留空")]
    private string prefix = ""; // 默认无前缀，你可以在 Inspector 里改

    private TMP_Text _tmpText;
    private string _cachedVersion; // 缓存版本号
    private string _cachedPrefix;  // 缓存前缀

    private void OnEnable()
    {
        // 获取组件并初始化缓存
        _tmpText = GetComponent<TMP_Text>();
        _cachedPrefix = prefix;
        UpdateVersion();
    }

    private void Update()
    {
        // 检测版本号或前缀是否发生变化
        if (Application.version != _cachedVersion || prefix != _cachedPrefix)
        {
            UpdateVersion();
        }
    }

    /// <summary>
    /// 更新文本显示
    /// </summary>
    private void UpdateVersion()
    {
        if (_tmpText == null)
            _tmpText = GetComponent<TMP_Text>();

        // 更新缓存
        _cachedVersion = Application.version;
        _cachedPrefix = prefix;

        // 拼接前缀和版本号并赋值
        _tmpText.text = _cachedPrefix + _cachedVersion;
    }

    /// <summary>
    /// 当 Inspector 中的属性被修改时调用（编辑模式下实时生效）
    /// </summary>
    private void OnValidate()
    {
        UpdateVersion();
    }

    /// <summary>
    /// 当组件被重置时调用（右键 -> Reset）
    /// </summary>
    private void Reset()
    {
        prefix = "Version: ";
        UpdateVersion();
    }
}