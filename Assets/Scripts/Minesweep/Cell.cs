using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    // ============================================================
    // Inspector
    // ============================================================

    [Range(0, 15)]
    public int state = 0;

    public List<Sprite> textures = new();

    public Vector2 spot;

    public bool ingame = false;

    public bool active = true;

    public Button button;


    // ============================================================
    // Audio
    // ============================================================

    [Header("Audio")]

    public AudioClip mineSound;

    public AudioClip normalSound;

    [Range(0f, 1f)]
    public float soundVolume = 1f;


    // ============================================================
    // Private
    // ============================================================

    private SpriteRenderer spriteRenderer;

    private CellManager parent;
    private MinesweepManager minesweepManager;
    private ToolManager toolManager;


    // ============================================================
    // Unity
    // ============================================================

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateSprite();
    }


    void OnValidate()
    {
        UpdateSprite();
    }


    void Start()
    {
        if (!ingame)
            return;

        parent =
            GetComponentInParent<CellManager>();

        if (parent == null)
            return;

        minesweepManager =
            parent.GetMinesweepManager();

        toolManager =
            parent.GetToolManager();

        if (button != null)
        {
            button.onClick.AddListener(
                OnLeftClick
            );
        }

        UpdateCell();
    }


    void Update()
    {
        if (!ingame)
            return;

        UpdateCell();
        UpdateSprite();
    }


    // ============================================================
    // 左键
    // ============================================================

    void OnLeftClick()
    {
        if (!ingame)
            return;

        if (!active)
            return;

        if (toolManager == null)
        {
            Open(true);
            return;
        }

        switch (toolManager.current_tool_id)
        {
            // ====================================================
            // 没有道具
            // ====================================================

            case 0:

                Open(true);

                break;


            // ====================================================
            // 道具 1：插旗
            // ====================================================

            case 1:

                if (minesweepManager.IsOpened(
                    (int)spot.x,
                    (int)spot.y))
                {
                    return;
                }

                bool was_flagged =
                    minesweepManager.IsFlagged(
                        (int)spot.x,
                        (int)spot.y);

                minesweepManager.ToggleFlag(
                    (int)spot.x,
                    (int)spot.y);

                bool is_flagged =
                    minesweepManager.IsFlagged(
                        (int)spot.x,
                        (int)spot.y);

                if (!was_flagged && is_flagged)
                {
                    toolManager.ConsumeTool(0, 1);
                }

                UpdateCell();

                parent.CheckWin();

                break;


            // ====================================================
            // 其他道具
            // ====================================================

            default:

                break;
        }
    }


    // ============================================================
    // Cell
    // ============================================================

    void UpdateCell()
    {
        if (minesweepManager == null)
            return;

        if (!active)
            return;

        int x = (int)spot.x;
        int y = (int)spot.y;


        // ========================================================
        // 插旗
        // ========================================================

        if (minesweepManager.IsFlagged(x, y))
        {
            state = 14;
            return;
        }


        // ========================================================
        // 未揭开
        // ========================================================

        if (!minesweepManager.IsOpened(x, y))
        {
            state = 15;
            return;
        }


        // ========================================================
        // 是雷
        // ========================================================

        if (minesweepManager.IsMine(x, y))
        {
            state = 12;
            return;
        }


        // ========================================================
        // 普通数字
        // ========================================================

        state =
            minesweepManager.GetMineCount(
                x,
                y
            );
    }


    // ============================================================
    // Open
    // ============================================================

    public void Open(bool can_die = true)
    {
        if (!ingame)
            return;

        if (!active)
            return;

        if (minesweepManager == null)
            return;

        int x = (int)spot.x;
        int y = (int)spot.y;


        // ========================================================
        // 插旗的格子不能打开
        // ========================================================

        if (minesweepManager.IsFlagged(x, y))
            return;


        // ========================================================
        // 已经打开不能重复打开
        // ========================================================

        if (minesweepManager.IsOpened(x, y))
            return;


        // ========================================================
        // 判断是不是雷
        // ========================================================

        bool isMine =
            minesweepManager.IsMine(x, y);


        // ========================================================
        // 播放音效
        // ========================================================

        if (isMine)
        {
            PlaySound(mineSound);
        }
        else
        {
            PlaySound(normalSound);
        }


        // ========================================================
        // 打开
        // ========================================================

        minesweepManager.Open(x, y);


        // ========================================================
        // 踩雷
        // ========================================================

        if (isMine && can_die)
        {
            parent.GameOver();
            return;
        }


        // ========================================================
        // 更新显示
        // ========================================================

        UpdateCell();


        // ========================================================
        // 检查胜利
        // ========================================================

        parent.OnUpdateCells();
    }


    // ============================================================
    // Play Sound
    // ============================================================

    private void PlaySound(
        AudioClip clip)
    {
        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(
            clip,
            Camera.main != null
                ? Camera.main.transform.position
                : transform.position,
            soundVolume
        );
    }


    // ============================================================
    // Flag
    // ============================================================

    public void ToggleFlag()
    {
        if (!ingame)
            return;

        if (!active)
            return;

        if (minesweepManager == null)
            return;

        int x = (int)spot.x;
        int y = (int)spot.y;


        // ========================================================
        // 已经揭开的格子不能插旗
        // ========================================================

        if (minesweepManager.IsOpened(x, y))
            return;


        minesweepManager.ToggleFlag(x, y);

        UpdateCell();

        // ========================================================
        // 插旗后检查胜利
        // ========================================================

        parent.CheckWin();
    }

        // ============================================================
    // 编辑接口
    // ============================================================

    /// <summary>
    /// 设置当前 Cell 的旗子状态
    /// </summary>
    public void SetFlag(bool value)
    {
        if (!ingame)
            return;

        if (!active)
            return;

        if (minesweepManager == null)
            return;

        int x = (int)spot.x;
        int y = (int)spot.y;

        // ========================================================
        // 设置旗子
        // ========================================================

        minesweepManager.SetFlagged(
            x,
            y,
            value
        );

        // ========================================================
        // 更新显示
        // ========================================================

        UpdateCell();
        UpdateSprite();

        // ========================================================
        // 检查胜利
        // ========================================================

        if (parent != null)
        {
            parent.CheckWin();
        }
    }


    /// <summary>
    /// 设置当前 Cell 是否为雷
    /// </summary>
    public void SetMine(bool value)
    {
        if (!ingame)
            return;

        if (!active)
            return;

        if (minesweepManager == null)
            return;

        int x = (int)spot.x;
        int y = (int)spot.y;

        // ========================================================
        // 设置雷
        // ========================================================

        minesweepManager.SetMine(
            x,
            y,
            value
        );

        // ========================================================
        // 更新显示
        // ========================================================

        UpdateCell();
        UpdateSprite();

        // ========================================================
        // 检查胜利
        // ========================================================

        if (parent != null)
        {
            parent.CheckWin();
        }
    }


    // ============================================================
    // Sprite
    // ============================================================

    void UpdateSprite()
    {
        if (spriteRenderer == null)
            spriteRenderer =
                GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            return;

        if (state < 0 ||
            state >= textures.Count)
            return;

        spriteRenderer.sprite =
            textures[state];
    }


    // ============================================================
    // Game Over
    // ============================================================

    public void GameOver()
    {
        active = false;

        int x = (int)spot.x;
        int y = (int)spot.y;

        if (minesweepManager != null &&
            minesweepManager.IsMine(x, y))
        {
            if (minesweepManager.IsOpened(x, y))
            {
                state = 12;
            }
            else
            {
                state = 10;
            }

            UpdateSprite();
        }
    }


    // ============================================================
    // Restart
    // ============================================================

    public void Restart()
    {
        active = true;

        UpdateCell();
        UpdateSprite();
    }
}