using System.Collections.Generic;
using UnityEngine;

public class MinesweepSolver
{
    // ============================================================
    // Solver Result
    // ============================================================

    public class Result
    {
        // 确定是雷
        public List<Vector2Int> mines = new();

        // 确定是安全
        public List<Vector2Int> safe = new();

        // 所有当前可观察到的未知格
        public List<Vector2Int> unknown = new();

        // 是否发现矛盾
        public bool contradiction = false;

        // 是否至少进行了一次推理   
        public bool solved = false;
    }


    // ============================================================
    // 内部约束
    // ============================================================

    private class Constraint
    {
        // 这个数字周围还没有确定的格子
        public HashSet<Vector2Int> cells = new();

        // 这些格子里面还需要多少个雷
        public int mines;
    }


    // ============================================================
    // 主入口
    // ============================================================

    public Result Solve(MinesweepManager manager)
    {
        Result result = new Result();

        if (manager == null)
            return result;


        // ========================================================
        // 已知状态
        // ========================================================

        HashSet<Vector2Int> known_mines = new();
        HashSet<Vector2Int> known_safe = new();


        // ========================================================
        // 读取已经确定是雷的格子
        // ========================================================

        for (int x = 0; x < manager.Width; x++)
        {
            for (int y = 0; y < manager.Height; y++)
            {
                Vector2Int cell =
                    new Vector2Int(x, y);

                // ------------------------------------------------
                // 已经插旗
                // ------------------------------------------------

                if (manager.IsFlagged(x, y))
                {
                    known_mines.Add(cell);
                    continue;
                }

                // ------------------------------------------------
                // 已经打开，但实际上是雷
                //
                // 这是本 Solver 的重要修复：
                // 已打开的雷也必须直接视为已知雷。
                // ------------------------------------------------

                if (
                    manager.IsOpened(x, y) &&
                    manager.IsMine(x, y)
                )
                {
                    known_mines.Add(cell);
                }
            }
        }


        // ========================================================
        // 反复推理
        // ========================================================

        bool changed = true;

        while (changed)
        {
            changed = false;


            // ====================================================
            // 构建当前所有数字约束
            // ====================================================

            List<Constraint> constraints =
                BuildConstraints(
                    manager,
                    known_mines,
                    known_safe
                );


            // ====================================================
            // 检查约束是否合法
            // ====================================================

            foreach (Constraint constraint in constraints)
            {
                // ------------------------------------------------
                // 雷数量不能小于 0
                // ------------------------------------------------

                if (constraint.mines < 0)
                {
                    result.contradiction = true;
                    continue;
                }


                // ------------------------------------------------
                // 雷数量不能超过未知格数量
                // ------------------------------------------------

                if (constraint.mines > constraint.cells.Count)
                {
                    result.contradiction = true;
                    continue;
                }


                // =================================================
                // 0 个雷
                // =================================================

                if (constraint.mines == 0)
                {
                    foreach (Vector2Int cell in constraint.cells)
                    {
                        if (known_mines.Contains(cell))
                            continue;

                        if (known_safe.Add(cell))
                            changed = true;
                    }
                }


                // =================================================
                // 数量等于格子数量
                // =================================================

                else if (
                    constraint.mines ==
                    constraint.cells.Count
                )
                {
                    foreach (Vector2Int cell in constraint.cells)
                    {
                        if (known_safe.Contains(cell))
                            continue;

                        if (known_mines.Add(cell))
                            changed = true;
                    }
                }
            }


            // ====================================================
            // 集合差集推理
            // ====================================================

            constraints =
                BuildConstraints(
                    manager,
                    known_mines,
                    known_safe
                );

            if (ApplySubsetInference(
                constraints,
                known_mines,
                known_safe
            ))
            {
                changed = true;
            }
        }


        // ========================================================
        // 最终整理
        // ========================================================

        foreach (Vector2Int cell in known_mines)
        {
            if (!result.mines.Contains(cell))
                result.mines.Add(cell);
        }


        foreach (Vector2Int cell in known_safe)
        {
            if (!result.safe.Contains(cell))
                result.safe.Add(cell);
        }


        // ========================================================
        // 获取未知格
        // ========================================================

        for (int x = 0; x < manager.Width; x++)
        {
            for (int y = 0; y < manager.Height; y++)
            {
                Vector2Int cell =
                    new Vector2Int(x, y);

                // 已打开
                if (manager.IsOpened(x, y))
                    continue;

                // 已知雷
                if (known_mines.Contains(cell))
                    continue;

                // 已知安全
                if (known_safe.Contains(cell))
                    continue;

                // 未知
                result.unknown.Add(cell);
            }
        }


        // ========================================================
        // 是否成功得到结果
        // ========================================================

        result.solved =
            result.mines.Count > 0 ||
            result.safe.Count > 0;

        return result;
    }


    // ============================================================
    // 构建所有数字约束
    // ============================================================

    private List<Constraint> BuildConstraints(
        MinesweepManager manager,
        HashSet<Vector2Int> known_mines,
        HashSet<Vector2Int> known_safe
    )
    {
        List<Constraint> constraints = new();


        // ========================================================
        // 遍历所有格子
        // ========================================================

        for (int x = 0; x < manager.Width; x++)
        {
            for (int y = 0; y < manager.Height; y++)
            {
                // 只有已经打开的数字格才能产生约束
                if (!manager.IsOpened(x, y))
                    continue;


                int number =
                    manager.GetMineCount(x, y);


                HashSet<Vector2Int> cells =
                    new();

                int known_mine_count = 0;


                // =================================================
                // 获取周围 8 格
                // =================================================

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;


                        int nx = x + dx;
                        int ny = y + dy;


                        if (!manager.IsInsidePublic(nx, ny))
                            continue;


                        Vector2Int cell =
                            new Vector2Int(nx, ny);


                        // =========================================
                        // 已知雷
                        // =========================================

                        if (known_mines.Contains(cell))
                        {
                            known_mine_count++;
                            continue;
                        }


                        // =========================================
                        // 已知安全
                        // =========================================

                        if (known_safe.Contains(cell))
                            continue;


                        // =========================================
                        // 已经打开
                        //
                        // 正常情况下这里应该是安全格。
                        // 已打开的雷已经在 known_mines 中，
                        // 所以不会错误地在这里被跳过。
                        // =========================================

                        if (manager.IsOpened(nx, ny))
                            continue;


                        // =========================================
                        // 真正的未知格
                        // =========================================

                        cells.Add(cell);
                    }
                }


                // =================================================
                // 计算剩余需要的雷数
                // =================================================

                int remaining_mines =
                    number - known_mine_count;


                // =================================================
                // 没有未知格
                // =================================================

                if (cells.Count == 0)
                {
                    // 如果周围没有未知格，
                    // 但数字仍然要求存在雷，
                    // 说明出现矛盾。
                    if (remaining_mines != 0)
                    {
                        Constraint contradiction =
                            new Constraint();

                        contradiction.mines =
                            remaining_mines;

                        constraints.Add(
                            contradiction
                        );
                    }

                    continue;
                }


                // =================================================
                // 创建约束
                // =================================================

                Constraint constraint =
                    new Constraint();

                constraint.cells = cells;
                constraint.mines =
                    remaining_mines;

                constraints.Add(constraint);
            }
        }


        return constraints;
    }


    // ============================================================
    // 集合差集推理
    //
    // A = {a,b,c}, mines = 2
    // B = {a,b},   mines = 1
    //
    // A - B = {c}
    // 需要 1 个雷
    //
    // => c 一定是雷
    // ============================================================

    private bool ApplySubsetInference(
        List<Constraint> constraints,
        HashSet<Vector2Int> known_mines,
        HashSet<Vector2Int> known_safe
    )
    {
        bool changed = false;


        for (int i = 0; i < constraints.Count; i++)
        {
            Constraint a =
                constraints[i];

            if (a.cells.Count == 0)
                continue;


            for (int j = 0; j < constraints.Count; j++)
            {
                if (i == j)
                    continue;


                Constraint b =
                    constraints[j];

                if (b.cells.Count == 0)
                    continue;


                // =================================================
                // 判断 A 是否是 B 的子集
                // =================================================

                if (!IsSubset(
                    a.cells,
                    b.cells
                ))
                {
                    continue;
                }


                // =================================================
                // 差集
                //
                // B - A
                // =================================================

                HashSet<Vector2Int> difference =
                    new HashSet<Vector2Int>(
                        b.cells
                    );

                difference.ExceptWith(
                    a.cells
                );


                if (difference.Count == 0)
                    continue;


                // =================================================
                // 差集需要的雷数量
                // =================================================

                int remaining =
                    b.mines -
                    a.mines;


                // =================================================
                // 差集需要 0 个雷
                // =================================================

                if (remaining == 0)
                {
                    foreach (
                        Vector2Int cell
                        in difference
                    )
                    {
                        if (known_mines.Contains(cell))
                            continue;

                        if (known_safe.Add(cell))
                            changed = true;
                    }
                }


                // =================================================
                // 差集全部是雷
                // =================================================

                else if (
                    remaining ==
                    difference.Count
                )
                {
                    foreach (
                        Vector2Int cell
                        in difference
                    )
                    {
                        if (known_safe.Contains(cell))
                            continue;

                        if (known_mines.Add(cell))
                            changed = true;
                    }
                }
            }
        }


        return changed;
    }


    // ============================================================
    // 判断 A 是否是 B 的子集
    // ============================================================

    private bool IsSubset(
        HashSet<Vector2Int> a,
        HashSet<Vector2Int> b
    )
    {
        foreach (Vector2Int cell in a)
        {
            if (!b.Contains(cell))
                return false;
        }

        return true;
    }


    // ============================================================
    // 获取确定是雷
    // ============================================================

    public List<Vector2Int> GetMines(
        MinesweepManager manager
    )
    {
        return Solve(manager).mines;
    }


    // ============================================================
    // 获取确定安全
    // ============================================================

    public List<Vector2Int> GetSafe(
        MinesweepManager manager
    )
    {
        return Solve(manager).safe;
    }


    // ============================================================
    // 获取所有未知
    // ============================================================

    public List<Vector2Int> GetUnknown(
        MinesweepManager manager
    )
    {
        return Solve(manager).unknown;
    }


    // ============================================================
    // 是否存在确定安全格
    // ============================================================

    public bool HasSafeMove(
        MinesweepManager manager
    )
    {
        return Solve(manager).safe.Count > 0;
    }


    // ============================================================
    // 是否存在确定雷
    // ============================================================

    public bool HasCertainMine(
        MinesweepManager manager
    )
    {
        return Solve(manager).mines.Count > 0;
    }


    // ============================================================
    // 是否可以确定下一步
    // ============================================================

    public bool CanSolve(
        MinesweepManager manager
    )
    {
        Result result =
            Solve(manager);

        return
            result.safe.Count > 0 ||
            result.mines.Count > 0;
    }
}