using UnityEngine;

/// <summary>
/// 挂载后，物体（或组件）在激活指定秒数后自动变为未激活。
/// 支持多次使用，并可在 Inspector 中选择预设时间或自定义时间。
/// </summary>
[DisallowMultipleComponent]
public class AutoDeactivate : MonoBehaviour
{
    /// <summary>
    /// 禁用目标类型
    /// </summary>
    public enum DeactivateTarget
    {
        GameObject,
        Component
    }

    /// <summary>
    /// 预设的时间选项（下拉菜单）
    /// </summary>
    public enum DelayOption
    {
        HalfSecond = 0,   // 0.5 秒
        OneSecond = 1,    // 1 秒
        TwoSeconds = 2,   // 2 秒
        ThreeSeconds = 3, // 3 秒
        FiveSeconds = 4,  // 5 秒
        Custom = 5        // 自定义（使用下方输入框的值）
    }

    [Header("时间设置")]
    [SerializeField]
    [Tooltip("从下拉菜单中选择一个预设时间，或选 Custom 手动输入")]
    private DelayOption delayOption = DelayOption.TwoSeconds;

    [SerializeField]
    [Tooltip("当上方选择 Custom 时，此值生效（单位：秒）")]
    private float customDelay = 3.0f;

    [Header("目标设置")]
    [SerializeField]
    [Tooltip("选择禁用目标：是整个游戏物体，还是仅此组件")]
    private DeactivateTarget target = DeactivateTarget.GameObject;

    private Coroutine _deactivateCoroutine;

    /// <summary>
    /// 根据选项计算实际要等待的秒数
    /// </summary>
    private float ActualDelay
    {
        get
        {
            switch (delayOption)
            {
                case DelayOption.HalfSecond: return 0.5f;
                case DelayOption.OneSecond: return 1f;
                case DelayOption.TwoSeconds: return 2f;
                case DelayOption.ThreeSeconds: return 3f;
                case DelayOption.FiveSeconds: return 5f;
                case DelayOption.Custom:
                default:
                    return Mathf.Max(0, customDelay); // 防止负数
            }
        }
    }

    private void OnEnable()
    {
        StartTimer();
    }

    private void OnDisable()
    {
        StopTimer();
    }

    /// <summary>
    /// 启动倒计时
    /// </summary>
    private void StartTimer()
    {
        StopTimer(); // 确保没有重复的协程
        _deactivateCoroutine = StartCoroutine(DeactivateAfterDelay());
    }

    /// <summary>
    /// 停止并清理倒计时
    /// </summary>
    private void StopTimer()
    {
        if (_deactivateCoroutine != null)
        {
            StopCoroutine(_deactivateCoroutine);
            _deactivateCoroutine = null;
        }
    }

    /// <summary>
    /// 协程：等待后执行禁用
    /// </summary>
    private System.Collections.IEnumerator DeactivateAfterDelay()
    {
        yield return new WaitForSeconds(ActualDelay);

        switch (target)
        {
            case DeactivateTarget.GameObject:
                gameObject.SetActive(false);
                break;
            case DeactivateTarget.Component:
                this.enabled = false;
                break;
        }

        _deactivateCoroutine = null;
    }

    /// <summary>
    /// [公共接口] 外部脚本调用：重置/刷新倒计时
    /// 例如：玩家再次点击该物体，重新计时 3 秒后隐藏
    /// </summary>
    public void ResetTimer()
    {
        if (gameObject.activeInHierarchy && this.enabled)
        {
            StartTimer();
        }
    }

    /// <summary>
    /// [公共接口] 外部脚本调用：动态设置自定义秒数并立即开始计时
    /// 这会强制将模式切换为 Custom
    /// </summary>
    /// <param name="seconds">要延迟的秒数</param>
    public void SetDelay(float seconds)
    {
        customDelay = seconds;
        delayOption = DelayOption.Custom;
        if (gameObject.activeInHierarchy && this.enabled)
        {
            StartTimer();
        }
    }

    private void Reset()
    {
        delayOption = DelayOption.TwoSeconds;
        customDelay = 3f;
        target = DeactivateTarget.GameObject;
    }
}