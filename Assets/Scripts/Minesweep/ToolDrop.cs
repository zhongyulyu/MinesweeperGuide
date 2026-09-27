using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToolDrop : MonoBehaviour
{
    public GameObject sprites_container;
    public ToolManager toolManager;
    public int index;
    public Button button;


    // ============================================================
    // 弹出参数
    // ============================================================

    [Header("Spawn Jump")]
    public float jump_distance = 0.5f;
    public float jump_height = 0.3f;
    public float jump_duration = 0.35f;


    // ============================================================
    // Unity
    // ============================================================

    private void Start()
    {
        if (sprites_container != null &&
            sprites_container.transform.childCount > 0)
        {
            sprites_container.transform
                .GetChild(index)
                .gameObject
                .SetActive(true);
        }

        if (button != null)
        {
            button.onClick.AddListener(Collect);
        }

        // 生成后自动弹出
        StartCoroutine(
            SpawnJumpCoroutine()
        );
    }


    // ============================================================
    // 出生抛物线
    // ============================================================

    private IEnumerator SpawnJumpCoroutine()
    {
        Vector3 start =
            transform.position;


        // ========================================================
        // 随机方向
        // ========================================================

        Vector2 direction =
            Random.insideUnitCircle;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.right;
        }

        direction.Normalize();


        // ========================================================
        // 随机距离
        // ========================================================

        float distance =
            Random.Range(
                jump_distance * 0.7f,
                jump_distance * 1.3f
            );


        Vector3 end =
            start +
            new Vector3(
                direction.x,
                direction.y,
                0f
            ) * distance;


        // ========================================================
        // 抛物线
        // ========================================================

        float timer = 0f;

        while (timer < jump_duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / jump_duration
                );


            // 水平移动
            Vector3 position =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );


            // 抛物线高度
            //
            // 4t(1-t)
            //
            // t = 0    -> 0
            // t = 0.5  -> 1
            // t = 1    -> 0

            float height =
                4f *
                t *
                (1f - t) *
                jump_height;


            position.y += height;

            transform.position =
                position;

            yield return null;
        }


        // ========================================================
        // 确保最终位置
        // ========================================================

        transform.position = end;
    }


    // ============================================================
    // Collect
    // ============================================================

    private void Collect()
    {
        if (toolManager == null)
            return;

        if (index < 0 ||
            index >= toolManager.transform.childCount)
        {
            Debug.LogWarning(
                $"[ToolDrop] 无效的 Tool index：{index}"
            );

            return;
        }

        StartCoroutine(
            CollectCoroutine()
        );
    }


    // ============================================================
    // 移动到 Tool
    // ============================================================

    private IEnumerator CollectCoroutine()
    {
        Transform target =
            toolManager.transform.GetChild(index);

        Vector3 start =
            transform.position;

        Vector3 end =
            target.position;

        const float duration = 0.5f;

        float timer = 0f;


        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            transform.position =
                Vector3.Lerp(
                    start,
                    end,
                    t
                );

            yield return null;
        }


        // ========================================================
        // 确保最终位置
        // ========================================================

        transform.position =
            end;


        // ========================================================
        // 收集 Tool
        // ========================================================

        toolManager.ConsumeTool(
            index,
            -1
        );


        // ========================================================
        // 销毁掉落物
        // ========================================================

        Destroy(gameObject);
    }
}