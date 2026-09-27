
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class TouniuHandler : MonoBehaviour
{
    public DialogueUI dialogueUI;
    public GameObject touniu;
    public GameObject AUV;
    public Button hammer_detect;
    public bool can_interupt = true;
    public bool AUVing = false;

    // ============================================================
    // 移动曲线
    // ============================================================

    [Header("移动 Ease 曲线")]
    public AnimationCurve move_curve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


    // ============================================================
    // Audio
    // ============================================================

    [Header("Voice")]
    public AudioSource audio_source;
    public AudioSource auv_source;

    [Header("Voice Animator")]
    public Animator animator;

    private Coroutine talking_coroutine;


    // ============================================================
    // Private
    // ============================================================

    private Coroutine movement_coroutine;
    private FunctionList functionList;


    // ============================================================
    // Unity
    // ============================================================

    public void Awake()
    {
        functionList =
            GetComponent<FunctionList>();

        if (audio_source == null)
        {
            audio_source =
                GetComponent<AudioSource>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        hammer_detect.onClick.AddListener(()=> _OnHammerDetect());
    }

    private void _OnHammerDetect()
    {
        ToolManager toolManager = GetComponentInParent<EventManager>().toolManager;
        if (toolManager.current_tool_id == 2 && can_interupt)
        {
            InterruptEvent();
            

            if (!AUVing)
                toolManager.ConsumeTool(1,1);
            
            StartCoroutine(OnAUV());
        }
    }

    private IEnumerator OnAUV()
    {
        ToolManager toolManager = GetComponentInParent<EventManager>().toolManager;

        GameObject hammer = GenerateHammerAnim();
        toolManager.ReturnCurrentTool();

        yield return new WaitForSeconds(0.08f);

        
        if (AUVing)
            PlayAUV(2);
        else
            PlayAUV(0);
        AUVing = true;
            
        
        
        functionList.Interrupt();

        // 播放 death 动画（与 Talking 动画同一套控制方式）
        if (animator != null)
        {
            animator.SetBool("death", true);
        }

        yield return new WaitForSeconds(2);

        // 结束 death 动画
        if (animator != null)
        {
            animator.SetBool("death", false);
        }

        // AUV 播放流程成功，回到原来的动画：只能是 Talking false
        if (animator != null)
        {
            animator.SetBool("Talking", false);
        }
        AUVing = false;
        Destroy(hammer);
    }

    public IEnumerator OnAUVNoHammer()
    {
        ToolManager toolManager = GetComponentInParent<EventManager>().toolManager;
        toolManager.ReturnCurrentTool();

        yield return new WaitForSeconds(0.08f);

        
        if (AUVing)
            PlayAUV(2);
        else
            PlayAUV(0);
        AUVing = true;
            
        
        
        functionList.Interrupt();

        // 播放 death 动画（与 Talking 动画同一套控制方式）
        if (animator != null)
        {
            animator.SetBool("death", true);
        }

        yield return new WaitForSeconds(2);

        // 结束 death 动画
        if (animator != null)
        {
            animator.SetBool("death", false);
        }

        // AUV 播放流程成功，回到原来的动画：只能是 Talking false
        if (animator != null)
        {
            animator.SetBool("Talking", false);
        }
        AUVing = false;
    }

    private GameObject GenerateHammerAnim()
    {
        ToolManager toolManager = GetComponentInParent<EventManager>().toolManager;
        GameObject hammer_anim = Instantiate(toolManager.tools[1].transform.GetChild(0).gameObject);
        hammer_anim.transform.localScale = new Vector2(3f, 3f);
        hammer_anim.transform.position = toolManager.tools[1].transform.position;
        hammer_anim.SetActive(true);
        hammer_anim.GetComponent<Hammer>().PlaySFX();
        Animator animator = hammer_anim.GetComponent<Animator>();
        animator.SetBool("use", true);
        return hammer_anim;
    }

    public void InterruptEvent()
    {

        // ========================================================
        // 关闭 Dialogue
        // ========================================================

        if (dialogueUI != null)
        {
            dialogueUI.Fade();
        }


        // ========================================================
        // 停止语音
        // ========================================================

        if (audio_source != null)
        {
            audio_source.Stop();
        }
    }
    
    public void PlayAUV(int index)
    {
        string path =
            $"Audio/VO/Other/{index}";

        audio_source.Stop(); 
        AudioClip clip =
            Resources.Load<AudioClip>(
                path
            );

        if (clip == null)
            return;
        auv_source.PlayOneShot(clip);
    }
    // ============================================================
    // 推送事件
    // ============================================================

    public void PushEvent(
        EventBase @event,
        float exist_time,
        Trigger[] triggers
    )
    {
        if (@event == null)
            return;

        if (dialogueUI == null)
            return;
        if (AUVing)
            return;

        print($"收到事件{@event.id}");

        // ========================================================
        // FixedEvent
        // ========================================================

        if (@event is FixedEvent fixedEvent)
        {
            // ----------------------------------------------------
            // 播放语音
            // ----------------------------------------------------

            PlayVoice(
                "Fixed",
                fixedEvent.id
            );


            // ----------------------------------------------------
            // Dialogue
            // ----------------------------------------------------

            Dialogue dialogue =
                new Dialogue(
                    fixedEvent.content
                );

            dialogue.content_color =
                fixedEvent.color;

            dialogue.exist_time =
                exist_time;

            dialogueUI.Show(
                dialogue,
                triggers
            );

            return;
        }


        // ========================================================
        // SelectEvent
        // ========================================================

        if (@event is SelectEvent selectEvent)
        {
            // ----------------------------------------------------
            // 播放语音
            // ----------------------------------------------------

            PlayVoice(
                "Select",
                selectEvent.id
            );


            // ----------------------------------------------------
            // Dialogue
            // ----------------------------------------------------

            Dialogue dialogue =
                new Dialogue(
                    selectEvent.content,
                    selectEvent.opt_a,
                    selectEvent.opt_b,
                    selectEvent.default_choise,
                    selectEvent.color,
                    exist_time,
                    true
                );

            dialogueUI.Show(
                dialogue,
                triggers
            );

            return;
        }


        // ========================================================
        // HandleEvent
        // ============================================================

        if (@event is HandleEvent handleEvent)
        {
            print($"收到Handle {handleEvent.func_id}");
            int code =
                functionList.CallFunction(
                    handleEvent.func_id,
                    handleEvent.args
                );
            

            return;
        }


        // ========================================================
        // MovementEvent
        // ============================================================

        if (@event is MovementEvent movementEvent)
        {
            StartMovement(
                movementEvent
            );

            return;
        }


        // ========================================================
        // 未处理事件
        // ========================================================

        Debug.LogWarning(
            $"[TouniuHandler] 未处理的事件类型：{@event.GetType().Name}"
        );
    }


    // ============================================================
    // 播放 VO
    //
    // Resources/Audio/VO/Fixed/{id}
    // Resources/Audio/VO/Select/{id}
    // ============================================================

    private void PlayVoice(
        string type,
        int id
    )
    {
        if (audio_source == null)
            return;

        // ========================================================
        // 获取 AudioClip
        // ========================================================

        string path =
            $"Audio/VO/{type}/{id}";

        AudioClip clip =
            Resources.Load<AudioClip>(
                path
            );


        if (clip == null)
        {
            Debug.LogWarning(
                $"[TouniuHandler] 找不到语音：Resources/{path}"
            );

            return;
        }


        // ========================================================
        // 播放语音
        // ========================================================
        audio_source.clip = clip;
        audio_source.Play();  


        // ========================================================
        // Talking
        // ========================================================

        if (animator != null)
        {
            animator.SetBool(
                "Talking",
                true
            );
        }
        else
        {
            Debug.LogWarning("No Animator");
        }


        // ========================================================
        // 重置 Talking 计时
        // ========================================================

        if (talking_coroutine != null)
        {
            StopCoroutine(
                talking_coroutine
            );
        }

        talking_coroutine =
            StartCoroutine(
                TalkingCoroutine(
                    clip.length
                )
            );
    }


    // ============================================================
    // Talking 状态
    // ============================================================

    private IEnumerator TalkingCoroutine(
        float duration
    )
    {
        yield return new WaitForSeconds(
            duration
        );


        if (animator != null)
        {
            animator.SetBool(
                "Talking",
                false
            );
        }


        talking_coroutine = null;
    }


    // ============================================================
    // 开始移动
    // ============================================================

    public void StartMovement(
        MovementEvent movementEvent
    )
    {
        if (movementEvent == null)
            return;


        // ========================================================
        // 已经存在移动
        // ========================================================

        if (movement_coroutine != null)
        {
            // 不允许打断
            if (!movementEvent.force)
                return;


            // 强制打断
            StopCoroutine(
                movement_coroutine
            );

            movement_coroutine = null;
        }


        // ========================================================
        // 开始移动
        // ========================================================

        if (movementEvent.random_move)
        {
            movement_coroutine =
                StartCoroutine(
                    RandomMoveCoroutine(
                        movementEvent
                    )
                );
        }
        else
        {
            movement_coroutine =
                StartCoroutine(
                    TargetMoveCoroutine(
                        movementEvent
                    )
                );
        }
    }


    // ============================================================
    // 指定目标移动
    // ============================================================

    private IEnumerator TargetMoveCoroutine(
        MovementEvent movementEvent
    )
    {
        Vector2 start =
            transform.position;

        Vector2 target =
            movementEvent.target;

        if ((start - target).x <= -1f)
            {
                touniu.transform.localScale = new Vector2(1,1)*touniu.transform.localScale.y;
            }
            else if ((start - target).x >= 1f)
            {
                touniu.transform.localScale = new Vector2(-1,1)*touniu.transform.localScale.y;
            }


        // --------------------------------------------------------
        // 瞬移
        // --------------------------------------------------------

        if (movementEvent.instant)
        {
            transform.position =
                target;

            movement_coroutine = null;

            yield break;
        }


        // --------------------------------------------------------
        // 移动时间
        // --------------------------------------------------------

        float time =
            Mathf.Max(
                0f,
                movementEvent.move_time
            );


        if (time <= 0f)
        {
            transform.position =
                target;

            movement_coroutine = null;

            yield break;
        }


        // --------------------------------------------------------
        // Ease 移动
        // --------------------------------------------------------

        float timer = 0f;

        while (timer < time)
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / time
                );

            float eased_t =
                move_curve.Evaluate(
                    t
                );

            transform.position =
                Vector2.Lerp(
                    start,
                    target,
                    eased_t
                );

            yield return null;
        }


        transform.position =
            target;

        movement_coroutine = null;
    }


    // ============================================================
    // 随机漫步
    // ============================================================

    private IEnumerator RandomMoveCoroutine(
        MovementEvent movementEvent
    )
    {
        float timer = 0f;

        float total_time =
            Mathf.Max(
                0f,
                movementEvent.move_time
            );


        while (timer < total_time)
        {
            // ====================================================
            // 当前起点
            // ====================================================

            Vector2 start =
                transform.position;


            // ====================================================
            // 获取目标
            // ====================================================

            Vector2 target =
                GetRandomCameraPosition(
                    start,
                    movementEvent.random_min_distance,
                    movementEvent.random_max_distance
                );


            // ====================================================
            // 朝向
            // ====================================================

            if ((start - target).x <= -1f)
            {
                touniu.transform.localScale = new Vector2(1,1)*touniu.transform.localScale.y;

            }
            else if ((start - target).x >= 1f)
            {
                touniu.transform.localScale = new Vector2(-1,1)*touniu.transform.localScale.y;
                
            }
            


            // ====================================================
            // 单次移动
            // ====================================================

            if (movementEvent.instant)
            {
                transform.position =
                    target;
            }
            else
            {
                float move_speed = 3f;

                float distance =
                    Vector2.Distance(
                        start,
                        target
                    );

                float move_time =
                    distance / move_speed;


                if (move_time > 0f)
                {
                    float move_timer = 0f;

                    while (
                        move_timer < move_time
                    )
                    {
                        float delta =
                            Time.deltaTime;

                        move_timer += delta;
                        timer += delta;


                        float t =
                            Mathf.Clamp01(
                                move_timer /
                                move_time
                            );


                        float eased_t =
                            move_curve.Evaluate(
                                t
                            );


                        transform.position =
                            Vector2.Lerp(
                                start,
                                target,
                                eased_t
                            );


                        if (timer >= total_time)
                            break;


                        yield return null;
                    }
                }


                transform.position =
                    target;
            }


            // ====================================================
            // 总时间结束
            // ====================================================

            if (timer >= total_time)
                break;


            // ====================================================
            // 随机等待
            // ====================================================

            float interval =
                Random.Range(
                    movementEvent.random_min_interval,
                    movementEvent.random_max_interval
                );


            interval =
                Mathf.Max(
                    0f,
                    interval
                );


            float interval_timer = 0f;


            while (
                interval_timer < interval
            )
            {
                float delta =
                    Time.deltaTime;

                interval_timer += delta;
                timer += delta;


                if (timer >= total_time)
                    break;


                yield return null;
            }
        }


        movement_coroutine = null;
    }


    // ============================================================
    // 获取摄像机范围内的随机目标
    // ============================================================

    private Vector2 GetRandomCameraPosition(
        Vector2 start,
        float min_distance,
        float max_distance
    )
    {
        if (Camera.main == null)
            return start;


        Camera camera =
            Camera.main;


        // ========================================================
        // 摄像机范围
        // ========================================================

        float distance_z =
            Mathf.Abs(
                transform.position.z -
                camera.transform.position.z
            );


        Vector3 min =
            camera.ViewportToWorldPoint(
                new Vector3(
                    0f,
                    0f,
                    distance_z
                )
            );


        Vector3 max =
            camera.ViewportToWorldPoint(
                new Vector3(
                    1f,
                    1f,
                    distance_z
                )
            );


        // ========================================================
        // 给角色自身留出边距
        // ========================================================

        float margin_x = 0f;
        float margin_y = 0f;


        SpriteRenderer sprite =
            GetComponent<SpriteRenderer>();


        if (sprite != null)
        {
            margin_x =
                sprite.bounds.extents.x;

            margin_y =
                sprite.bounds.extents.y;
        }


        min.x += margin_x;
        max.x -= margin_x;

        min.y += margin_y;
        max.y -= margin_y;


        // ========================================================
        // 修正距离参数
        // ========================================================

        min_distance =
            Mathf.Max(
                0f,
                min_distance
            );


        max_distance =
            Mathf.Max(
                min_distance,
                max_distance
            );


        // ========================================================
        // 随机寻找目标
        // ========================================================

        for (int i = 0; i < 20; i++)
        {
            Vector2 direction =
                Random.insideUnitCircle;


            if (direction.sqrMagnitude < 0.001f)
                continue;


            direction.Normalize();


            float move_distance =
                Random.Range(
                    min_distance,
                    max_distance
                );


            Vector2 target =
                start +
                direction *
                move_distance;


            if (
                target.x >= min.x &&
                target.x <= max.x &&
                target.y >= min.y &&
                target.y <= max.y
            )
            {
                return target;
            }
        }


        // ========================================================
        // 找不到符合条件的位置
        // ========================================================

        return new Vector2(
            Mathf.Clamp(
                start.x,
                min.x,
                max.x
            ),
            Mathf.Clamp(
                start.y,
                min.y,
                max.y
            )
        );
    }
}