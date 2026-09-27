using System;
using UnityEngine;

public class MinesweepManager
{
    private int width;
    private int height;

    private bool[,] mines;
    private bool[,] opened;
    private bool[,] flagged;

    private int mine_count;
    private bool mines_generated;


    // ============================================================
    // Constructor
    // ============================================================

    public MinesweepManager(int width, int height)
    {
        this.width = width;
        this.height = height;

        mines = new bool[width, height];
        opened = new bool[width, height];
        flagged = new bool[width, height];
    }


    public MinesweepManager(
        int width,
        int height,
        int mine_count
    )
    {
        this.width = width;
        this.height = height;

        this.mine_count = Mathf.Clamp(
            mine_count,
            0,
            width * height
        );

        mines = new bool[width, height];
        opened = new bool[width, height];
        flagged = new bool[width, height];
    }


    // ============================================================
    // 设置雷数量
    // ============================================================

    public void SetMineCount(int count)
    {
        mine_count = Mathf.Clamp(
            count,
            0,
            width * height
        );
    }


    // ============================================================
    // 生成雷
    // ============================================================

    public void GenerateMines(int mine_count)
    {
        mine_count = Mathf.Clamp(
            mine_count,
            0,
            width * height
        );

        Array.Clear(
            mines,
            0,
            mines.Length
        );

        Array.Clear(
            opened,
            0,
            opened.Length
        );

        Array.Clear(
            flagged,
            0,
            flagged.Length
        );

        int count = 0;

        while (count < mine_count)
        {
            int x = UnityEngine.Random.Range(
                0,
                width
            );

            int y = UnityEngine.Random.Range(
                0,
                height
            );

            if (mines[x, y])
                continue;

            mines[x, y] = true;
            count++;
        }

        mines_generated = true;
    }


    // ============================================================
    // 第一次 Open 专用生成雷
    //
    // 第一次点击位置周围 3x3 不生成雷
    // ============================================================

    private void GenerateMines(
        int mine_count,
        int safe_x,
        int safe_y
    )
    {
        mine_count = Mathf.Clamp(
            mine_count,
            0,
            width * height
        );

        Array.Clear(
            mines,
            0,
            mines.Length
        );

        int available_count = 0;

        // ========================================================
        // 计算安全区以外可以放雷的位置
        // ========================================================

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!IsInSafeArea(
                    x,
                    y,
                    safe_x,
                    safe_y
                ))
                {
                    available_count++;
                }
            }
        }


        // ========================================================
        // 如果雷太多
        // 只能把多出来的雷放到安全区
        // ========================================================

        int safe_area_mines = 0;

        if (mine_count > available_count)
        {
            safe_area_mines =
                mine_count -
                available_count;

            mine_count =
                available_count;
        }


        // ========================================================
        // 第一阶段
        // 只在安全区外生成雷
        // ========================================================

        int count = 0;

        while (count < mine_count)
        {
            int x = UnityEngine.Random.Range(
                0,
                width
            );

            int y = UnityEngine.Random.Range(
                0,
                height
            );

            if (mines[x, y])
                continue;

            if (IsInSafeArea(
                x,
                y,
                safe_x,
                safe_y
            ))
                continue;

            mines[x, y] = true;

            count++;
        }


        // ========================================================
        // 第二阶段
        // 安全区内放剩余的雷
        // ========================================================

        count = 0;

        while (count < safe_area_mines)
        {
            int x = UnityEngine.Random.Range(
                Mathf.Max(0, safe_x - 1),
                Mathf.Min(width, safe_x + 2)
            );

            int y = UnityEngine.Random.Range(
                Mathf.Max(0, safe_y - 1),
                Mathf.Min(height, safe_y + 2)
            );

            if (mines[x, y])
                continue;

            mines[x, y] = true;

            count++;
        }

        mines_generated = true;
    }


    // ============================================================
    // 判断是否属于安全区域
    // ============================================================

    private bool IsInSafeArea(
        int x,
        int y,
        int safe_x,
        int safe_y
    )
    {
        return
            Mathf.Abs(x - safe_x) <= 1 &&
            Mathf.Abs(y - safe_y) <= 1;
    }


    // ============================================================
    // Open
    // ============================================================

    public void Open(int x, int y)
    {
        if (!IsInside(x, y))
            return;


        // ========================================================
        // 第一次打开
        // ========================================================

        if (!mines_generated)
        {
            GenerateMines(
                mine_count,
                x,
                y
            );
        }


        // ========================================================
        // 已经打开
        // ========================================================

        if (opened[x, y])
            return;


        // ========================================================
        // 插旗后不能打开
        // ========================================================

        if (flagged[x, y])
            return;


        // ========================================================
        // 打开
        // ========================================================

        opened[x, y] = true;


        // ========================================================
        // 是雷
        // ========================================================

        if (mines[x, y])
            return;


        // ========================================================
        // 周围有雷
        // ========================================================

        if (GetMineCount(x, y) > 0)
            return;


        // ========================================================
        // 周围没有雷
        // 自动扩散
        // ========================================================

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int check_x =
                    x + dx;

                int check_y =
                    y + dy;

                if (!IsInside(
                    check_x,
                    check_y
                ))
                    continue;

                Open(
                    check_x,
                    check_y
                );
            }
        }
    
    }


    // ============================================================
    // 插旗 / 取消旗子
    // ============================================================

    public void ToggleFlag(int x, int y)
    {
        if (!IsInside(x, y))
            return;

        // 已经打开的格子不能插旗
        if (opened[x, y])
            return;

        flagged[x, y] =
            !flagged[x, y];
    }


    // ============================================================
    // 判断是否插旗
    // ============================================================

    public bool IsFlagged(int x, int y)
    {
        if (!IsInside(x, y))
            return false;

        return flagged[x, y];
    }


    // ============================================================
    // 判断是否已经揭开
    // ============================================================

    public bool IsOpened(int x, int y)
    {
        if (!IsInside(x, y))
            return false;

        return opened[x, y];
    }


    // ============================================================
    // 判断是不是雷
    // ============================================================

    public bool IsMine(int x, int y)
    {
        if (!IsInside(x, y))
            return false;

        return mines[x, y];
    }


    // ============================================================
    // 获取周围雷数量
    // ============================================================

    public int GetMineCount(int x, int y)
    {
        if (!IsInside(x, y))
            return 0;

        int count = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int check_x =
                    x + dx;

                int check_y =
                    y + dy;

                if (!IsInside(
                    check_x,
                    check_y
                ))
                    continue;

                if (mines[check_x, check_y])
                    count++;
            }
        }

        return count;
    }


    // ============================================================
    // 判断是否胜利
    //
    // 所有非雷格子都已经打开
    // ============================================================

    public bool IsWin()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!mines[x, y] &&
                    !opened[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }


    // ============================================================
    // 判断坐标是否在棋盘内
    // ============================================================

    private bool IsInside(int x, int y)
    {
        return
            x >= 0 &&
            x < width &&
            y >= 0 &&
            y < height;
    }


    // ============================================================
    // 重开
    // ============================================================

    public void Restart()
    {
        Array.Clear(
            mines,
            0,
            mines.Length
        );

        Array.Clear(
            opened,
            0,
            opened.Length
        );

        Array.Clear(
            flagged,
            0,
            flagged.Length
        );

        mines_generated = false;
    }

    public int Width
    {
        get { return width; }
    }

    public int Height
    {
        get { return height; }
    }

    public bool IsInsidePublic(int x, int y)
    {
        return IsInside(x, y);
    }

    // ============================================================
// 编辑雷
// ============================================================

    public void SetMine(int x, int y, bool value)
    {
        if (!IsInside(x, y))
            return;

        mines[x, y] = value;

        // 手动编辑雷后，视为雷盘已经生成
        mines_generated = true;
    }


    // ============================================================
    // 编辑旗子
    // ============================================================

    public void SetFlagged(int x, int y, bool value)
    {
        if (!IsInside(x, y))
            return;

        // 已经揭开的格子不能插旗
        if (opened[x, y])
            return;

        flagged[x, y] = value;
    }


    // ============================================================
    // 编辑揭开状态
    // ============================================================

    public void SetOpened(int x, int y, bool value)
    {
        if (!IsInside(x, y))
            return;

        opened[x, y] = value;

        // 揭开以后不能保持旗子
        if (value)
        {
            flagged[x, y] = false;
        }
    }
}