using UnityEngine;
using UnityEngine.InputSystem;

public class LevelSelectCamera : MonoBehaviour
{
    public float left_limit = -10f;
    public float right_limit = 10f;

    public float bottom_limit = -5f;
    public float top_limit = 5f;

    public float move_speed = 0.01f;
    public float smooth_speed = 8f;

    private bool dragging;
    private Vector2 last_mouse_position;

    private Vector2 target_position;


    void Start()
    {
        target_position = transform.position;
    }


    void Update()
    {
        Mouse mouse = Mouse.current;

        if (mouse == null)
            return;


        // =========================
        // 左键开始拖动
        // =========================

        if (mouse.leftButton.wasPressedThisFrame)
        {
            dragging = true;
            last_mouse_position = mouse.position.ReadValue();
        }


        // =========================
        // 左键结束拖动
        // =========================

        if (mouse.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }


        // =========================
        // 拖动
        // =========================

        if (dragging)
        {
            Vector2 current_mouse_position =
                mouse.position.ReadValue();

            Vector2 delta =
                current_mouse_position -
                last_mouse_position;

            // 鼠标向右 → 摄像机向左
            // 鼠标向上 → 摄像机向下
            target_position -= delta * move_speed;

            last_mouse_position = current_mouse_position;
        }


        // =========================
        // 限制范围
        // =========================

        target_position.x = Mathf.Clamp(
            target_position.x,
            left_limit,
            right_limit
        );

        target_position.y = Mathf.Clamp(
            target_position.y,
            bottom_limit,
            top_limit
        );


        // =========================
        // 平滑移动
        // =========================

        float smooth =
            1f - Mathf.Exp(
                -smooth_speed * Time.deltaTime
            );

        Vector2 new_position = Vector2.Lerp(
            transform.position,
            target_position,
            smooth
        );

        transform.position = new Vector3(
            new_position.x,
            new_position.y,
            transform.position.z
        );
    }
}