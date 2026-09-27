using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public TMP_Text content;

    public Button yes_button;
    public Button no_button;

    public TMP_Text yes_text;
    public TMP_Text no_text;

    public CanvasGroup canvas_group;
 
    public float FadeCoroutine_time = 0.25f;

    private Coroutine show_coroutine;

    // 当前事件结束 Trigger
    private Trigger finish_trigger;

    private Trigger yes_trigger;
    private Trigger no_trigger;



    void Awake()
    {
        if (canvas_group == null)
            canvas_group = GetComponent<CanvasGroup>();

        HideImmediate();
        yes_button.onClick.AddListener(OnYes);
        no_button.onClick.AddListener(OnNo);
    }


    // ============================================================
    // Show
    // ============================================================

    public void Show(Dialogue dialogue, Trigger[] triggers)
    {
        if (dialogue == null)
            return;

        if (canvas_group == null)
            return;

        // 如果之前正在播放
        if (show_coroutine != null)
        {
            StopCoroutine(show_coroutine);
            show_coroutine = null;
        }

        // ========================================================
        // 创建 Trigger
        // ========================================================
        

        if (dialogue.show_choise)
        {
            yes_trigger = triggers[0];
            no_trigger = triggers[1];

            show_coroutine = StartCoroutine(
                ShowCoroutine(dialogue)
            );

        }
        else
        {
            finish_trigger = triggers[0];
        }

        show_coroutine = StartCoroutine(
            ShowCoroutine(dialogue)
        );

    }

    public void Fade()
    {
        if (canvas_group == null)
            return;

        // 如果之前正在播放
        if (show_coroutine != null)
        {
            StopCoroutine(show_coroutine);
            show_coroutine = null;
        }

        show_coroutine = StartCoroutine(
            FadeCoroutine(canvas_group.alpha,0,FadeCoroutine_time*canvas_group.alpha)
        );

    }


    // ============================================================
    // 显示
    // ============================================================

    private IEnumerator ShowCoroutine(Dialogue dialogue)
    {
        content.text = dialogue.content;
        content.color = dialogue.content_color;

        if (dialogue.show_choise)
        {
            yes_button.gameObject.SetActive(true);
            no_button.gameObject.SetActive(true);

            yes_text.text =
                string.IsNullOrEmpty(dialogue.yes_text)
                    ? "是"
                    : dialogue.yes_text;

            no_text.text =
                string.IsNullOrEmpty(dialogue.no_text)
                    ? "否"
                    : dialogue.no_text;

            yes_button.interactable = true;
            no_button.interactable = true;
        }
        else
        {
            yes_button.gameObject.SetActive(false);
            no_button.gameObject.SetActive(false);
        }

        canvas_group.interactable = true;
        canvas_group.blocksRaycasts = true;

        yield return FadeCoroutine(
            0f,
            1f,
            FadeCoroutine_time
        );

        yield return new WaitForSeconds(
            dialogue.exist_time
        );

        // 如果是普通 Dialogue
        // 到这里直接结束
        if (!dialogue.show_choise)
        {
            canvas_group.interactable = false;
            canvas_group.blocksRaycasts = false;

            yield return FadeCoroutine(
                1f,
                0f,
                FadeCoroutine_time
            );

            finish_trigger.SetTrigger(() => true);

            show_coroutine = null;
            yield break;
        }

        // ========================================================
        // Select 不自动结束
        // 等待玩家点击
        // ========================================================

        yield return null;
    }


    // ============================================================
    // Yes
    // ============================================================

    private void OnYes()
    {
        if (yes_trigger == null)
            return;

        if (show_coroutine != null)
        {
            StopCoroutine(show_coroutine);
            show_coroutine = null;
        }

        yes_button.interactable = false;
        no_button.interactable = false;

        print("YES");
        StartCoroutine(
            FinishSelect(
                yes_trigger
            )
        );
    }


    // ============================================================
    // No
    // ============================================================

    private void OnNo()
    {
        if (no_trigger == null)
            return;

        if (show_coroutine != null)
        {
            StopCoroutine(show_coroutine);
            show_coroutine = null;
        }

        yes_button.interactable = false;
        no_button.interactable = false;

        StartCoroutine(
            FinishSelect(
                no_trigger
            )
        );
    }


    // ============================================================
    // Select 结束
    // ============================================================

    private IEnumerator FinishSelect(
        Trigger selected_trigger
    )
    {
        canvas_group.interactable = false;
        canvas_group.blocksRaycasts = false;

        yield return FadeCoroutine(
            1f,
            0f,
            FadeCoroutine_time
        );

        selected_trigger.SetTrigger(
            () => true
        );

        yes_trigger = null;
        no_trigger = null;

        show_coroutine = null;
    }


    // ============================================================
    // FadeCoroutine
    // ============================================================

    private IEnumerator FadeCoroutine(
        float from,
        float to,
        float time
    )
    {
        canvas_group.alpha = from;

        if (time <= 0f)
        {
            canvas_group.alpha = to;
            yield break;
        }

        float timer = 0f;

        while (timer < time)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(
                timer / time
            );

            t = t * t * (3f - 2f * t);

            canvas_group.alpha = Mathf.Lerp(
                from,
                to,
                t
            );

            yield return null;
        }

        canvas_group.alpha = to;
    }


    // ============================================================
    // 立即隐藏
    // ============================================================

    private void HideImmediate()
    {
        if (canvas_group == null)
            return;

        canvas_group.alpha = 0f;
        canvas_group.interactable = false;
        canvas_group.blocksRaycasts = false;
    }
}