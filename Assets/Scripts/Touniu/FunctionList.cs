using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FunctionList : MonoBehaviour
{
    #region Inspector

    public CellManager cellManager;
    public EventParticle eventParticle;
    public ToolDrop toolDrop;

    #endregion

    private Coroutine current_coroutine;

    #region Types

    public enum CellTarget
    {
        KnownEmpty,
        KnownMine,
        AllEmpty,
        AllMine,
        Flagged,
        Opened
    }

    #endregion


    #region Functions

    static private Dictionary<
        int,
        Func<List<object>, int>
    > Functions;

    #endregion


    #region Regist

    void Awake()
    {
        Functions = new Dictionary<
            int,
            Func<List<object>, int>  
        >()
        {
            { 101, AUV },
            { 102, Move },

            { 1001, RemoveMine },
            { 1002, AddTime },
            { 1003, FlagMine },
            { 1004, OpenEmpty },
            { 1005, AddFlag },
            { 1006, AddHammer },

            { 2001, AddMine },
            { 2002, ReduceTime },
            { 2003, OpenMine },
            { 2004, CloseEmpty },
            // { 2005, AddFlag },  
            // { 2006, AddHammer },

            { 10001, OpenOneMine },
            { 10002, OpenOneEmpty },
            { 10003, OpenMiddle }
        };
    } 

    #endregion


    #region Target

    private List<Vector2Int> GetSet(
        CellTarget target_type,
        bool exclude_flagged = false,
        bool exclude_opened = false
    )
    {
        MinesweepManager manager =
            cellManager.GetMinesweepManager();

        if (manager == null)
            return null;


        #region Solver

        HashSet<Vector2Int> known_targets = null;

        if (target_type == CellTarget.KnownMine ||
            target_type == CellTarget.KnownEmpty)
        {
            MinesweepSolver solver = new();

            List<Vector2Int> result;

            if (target_type == CellTarget.KnownMine)
            {
                result = solver.GetMines(manager);
            }
            else
            {
                result = solver.GetSafe(manager);
            }

            if (result == null)
                return null;

            known_targets =
                new HashSet<Vector2Int>(result);
        }

        #endregion


        #region Create Set

        List<Vector2Int> set =
            new List<Vector2Int>();

        #endregion


        #region Board

        for (int x = 0; x < manager.Width; x++)
        {
            for (int y = 0; y < manager.Height; y++)
            {
                #region Exclude Flagged

                if (exclude_flagged &&
                    manager.IsFlagged(x, y))
                {
                    continue;
                }

                #endregion


                #region Exclude Opened

                if (exclude_opened &&
                    manager.IsOpened(x, y))
                {
                    continue;
                }

                #endregion


                Vector2Int spot =
                    new Vector2Int(x, y);

                bool match = false;


                #region Check Target

                switch (target_type)
                {
                    case CellTarget.KnownMine:

                        match =
                            known_targets.Contains(spot);

                        break;


                    case CellTarget.KnownEmpty:

                        match =
                            known_targets.Contains(spot);

                        break;


                    case CellTarget.AllEmpty:

                        match =
                            !manager.IsMine(x, y);

                        break;


                    case CellTarget.AllMine:

                        match =
                            manager.IsMine(x, y);

                        break;


                    case CellTarget.Flagged:

                        match =
                            manager.IsFlagged(x, y);

                        break;


                    case CellTarget.Opened:

                        match =
                            manager.IsOpened(x, y);

                        break;
                }

                #endregion


                if (match)
                {
                    set.Add(spot);
                }
            }
        }

        #endregion


        return set;
    }


    private List<Vector2Int> GetRandom(
        List<Vector2Int> set,
        int amount
    )
    {
        if (set == null)
            return null;

        if (set.Count == 0)
            return new List<Vector2Int>();


        amount =
            Mathf.Clamp(
                amount,
                0,
                set.Count
            );

        if (amount <= 0)
            return new List<Vector2Int>();


        #region Copy

        List<Vector2Int> result =
            new List<Vector2Int>(set);

        #endregion


        #region Fisher-Yates

        for (int i = 0; i < amount; i++)
        {
            int random_index =
                UnityEngine.Random.Range(
                    i,
                    result.Count
                );

            Vector2Int temp =
                result[i];

            result[i] =
                result[random_index];

            result[random_index] =
                temp;
        }

        #endregion


        return result.GetRange(
            0,
            amount
        );
    }

    #endregion


    #region Function Call

    public int CallFunction(
        int id,
        List<List<object>> args
    )
    {
        print($"头牛管理器：调用了{id}");
        if (!Functions.TryGetValue(
            id,
            out Func<List<object>, int> function
        ))
        {
            Debug.LogWarning(
                $"Function ID 不存在: {id}"
            );

            return 0;
        }


        // ========================================================
        // 解析参数
        // ========================================================

        List<object> new_args = new();


        foreach (List<object> list in args)
        {
            if (list == null ||
                list.Count == 0)
            {
                new_args.Add(null);
                continue;
            }


            int index =
                LevelInput.level_touniu_diff;


            // 超出范围时 Clamp
            index = Mathf.Clamp(
                index,
                0,
                list.Count - 1
            );


            new_args.Add(
                list[index]
            );
        }


        // ========================================================
        // 调用函数
        // ========================================================
        if (current_coroutine != null)
        {
            StopCoroutine(current_coroutine);
        }
        int ret =  function.Invoke(
            new_args
        );
        return ret;
    }

    #endregion

    #region Interrupt
    public void Interrupt()
    {
        if (current_coroutine != null)
            StopCoroutine(current_coroutine);
    }
    #endregion
    
    #region 101 - AUV
    private int AUV(
        List<object> args
    )
    {
        StartCoroutine(GetComponent<TouniuHandler>().OnAUVNoHammer());
        return 0;
    }
    #endregion

    #region 102 - Move

    private int Move( 
        List<object> args
    )
    {

        float x =
            Convert.ToSingle(args[0]);

        float y =
            Convert.ToSingle(args[1]);

        float move_time =
            Convert.ToSingle(args[2]);

        GetComponent<TouniuHandler>().StartMovement(
            new MovementEvent(
                target: new Vector2(x, y),
                move_time: move_time
            )
        );

        return 0;
    }

    #endregion

    #region 1001 - Remove Mine

    private int RemoveMine(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 1;
        }

        int amount = Convert.ToInt32(args[0]);

        MinesweepManager manager =
            cellManager.GetMinesweepManager();

        List<Vector2Int> known_mines =
            GetSet(
                CellTarget.KnownMine,
                false,
                true
            );

        List<Vector2Int> mines =
            GetRandom(
                known_mines,
                amount
            );


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _RemoveMineCoroutine(
                manager,
                mines
            )
        );

        #endregion


        return mines.Count;
    }


    private IEnumerator _RemoveMineCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            manager.SetMine(
                mine.x,
                mine.y,
                false
            );
            manager.Open(
                mine.x,
                mine.y
            );

            Debug.Log(
                $"Remove Mine: {mine}"
            );
        }
    }

    #endregion

    #region 1002 - Add Time

    private int AddTime(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 0;
        }

        float amount = Convert.ToSingle(args[0]);
        
        current_coroutine = StartCoroutine(_AddTimeCoroutine(amount));
        return 0;
    }

    private IEnumerator _AddTimeCoroutine(
        float amount
    )
    {
        Instantiate(eventParticle, transform).Play(transform.position, cellManager.timer_left.transform.position);
        yield return new WaitForSeconds(0.5f);
        cellManager.time_max += amount;
    }

    #endregion

    #region 1003 - Flag Mine

    private int FlagMine(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 1;
        }

        MinesweepManager manager =
            cellManager.GetMinesweepManager();

        int amount = Convert.ToInt32(args[0]);

        if (manager == null)
            return 0;

        List<Vector2Int> known_mines =
            GetSet(
                CellTarget.KnownMine,
                true,
                true
            );

        if (known_mines == null)
            return 0;

        List<Vector2Int> mines =
            GetRandom(
                known_mines,
                amount
            );

        if (mines == null)
            return 0;


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _FlagMineCoroutine(
                manager,
                mines
            )
        );

        #endregion


        return mines.Count;
    }


    private IEnumerator _FlagMineCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            manager.SetFlagged(
                mine.x,
                mine.y,
                true
            );

            Debug.Log(
                $"Flag Mine: {mine}"
            );
        }
    }

    #endregion

    #region 1004 - Open Empty
    private int OpenEmpty(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 0;
        }

        int amount = Convert.ToInt32(args[0]);

        MinesweepManager manager =
            cellManager.GetMinesweepManager();

        if (manager == null)
            return 0;

        List<Vector2Int> known_empty =
            GetSet(
                CellTarget.KnownEmpty,
                true,
                true
            );

        if (known_empty == null)
            return 0;

        List<Vector2Int> empty =
            GetRandom(
                known_empty,
                amount
            );

        if (empty == null)
            return 0;


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _OpenEmptyCoroutine(
                manager,
                empty
            )
        );

        #endregion


        return empty.Count;
    }


    private IEnumerator _OpenEmptyCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            cellManager.GetCell(mine).Open();

            Debug.Log(
                $"Opened: {mine}"
            );
        }
    }
    #endregion

    #region 1005 - Add Flag
    private int AddFlag(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 0;
        }

        float amount = Convert.ToSingle(args[0]);
        
        current_coroutine = StartCoroutine(_AddFlagCoroutine(amount));
        return 0;
    }

    private IEnumerator _AddFlagCoroutine(
        float amount
    )
    {
        for (int i = 0; i < amount; i++)
        {
            ToolDrop drop = Instantiate(toolDrop);
            drop.toolManager = cellManager.GetToolManager();
            drop.transform.position = transform.position;
            yield return new WaitForSeconds(.05f);
        }
        

    }
    #endregion

    #region 1006 - Add Hammer
    private int AddHammer(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 0;
        }

        float amount = Convert.ToSingle(args[0]);
        
        current_coroutine = StartCoroutine(_AddHammerCoroutine(amount));
        return 0;
    }

    private IEnumerator _AddHammerCoroutine(
        float amount
    )
    {
        for (int i = 0; i < amount; i++)
        {
            ToolDrop drop = Instantiate(toolDrop);
            drop.toolManager = cellManager.GetToolManager();
            drop.transform.position = transform.position;
            drop.index = 1;
            yield return new WaitForSeconds(.05f);
        }
        

    }
    #endregion

    #region 2001 - Add Mine

    private int AddMine(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 1;
        }

        int amount = Convert.ToInt32(args[0]);

        MinesweepManager manager =
            cellManager.GetMinesweepManager();

        List<Vector2Int> known_mines =
            GetSet(
                CellTarget.AllEmpty,
                false,
                true
            );

        List<Vector2Int> mines =
            GetRandom(
                known_mines,
                amount
            );


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _AddMineCoroutine(
                manager,
                mines
            )
        );

        #endregion


        return mines.Count;
    }


    private IEnumerator _AddMineCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            cellManager.GetCell(mine).SetMine(
                true
            );

            Debug.Log(
                $"Add Mine: {mine}"
            );
        }
    }

    #endregion

    #region 2002 - Reduce Time

    private int ReduceTime(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 0;
        }

        float amount = Convert.ToSingle(args[0]);
        
        current_coroutine = StartCoroutine(_ReduceTimeCoroutine(amount));
        return 0;
    }

    private IEnumerator _ReduceTimeCoroutine(
        float amount
    )
    {
        Instantiate(eventParticle, transform).Play(transform.position, cellManager.timer_left.transform.position);
        yield return new WaitForSeconds(0.5f);
        cellManager.time_max -= amount;
    }

    #endregion

    #region 2003 - Open Mine

    private int OpenMine(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 1;
        }

        MinesweepManager manager =
            cellManager.GetMinesweepManager();

        int amount = Convert.ToInt32(args[0]);

        if (manager == null)
            return 0;

        List<Vector2Int> known_mines =
            GetSet(
                CellTarget.KnownMine,
                true,
                true
            );

        if (known_mines == null)
            return 0;

        List<Vector2Int> mines =
            GetRandom(
                known_mines,
                amount
            );

        if (mines == null)
            return 0;


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _OpenMineCoroutine(
                manager,
                mines
            )
        );

        #endregion


        return 0;
    }


    private IEnumerator _OpenMineCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            cellManager.GetCell(mine).Open(false);

            Debug.Log(
                $"Opened Mine: {mine}"
            );
        }
    }

    #endregion

    #region 2004 - Close Empty
    private int CloseEmpty(
        List<object> args
    )
    {
        if (args == null ||
            args.Count == 0)
        {
            return 1;
        }

        int amount = Convert.ToInt32(args[0]);

        MinesweepManager manager =
            cellManager.GetMinesweepManager();

        if (manager == null)
            return 1;

        List<Vector2Int> known_empty =
            GetSet(
                CellTarget.Opened,
                true,
                false
            );
        if (known_empty == null)
            return 1;

        List<Vector2Int> empty =
            GetRandom(
                known_empty,
                amount
            );

        if (empty == null)
            return 1;


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _CloseEmptyCoroutine(
                manager,
                empty
            )
        );

        #endregion


        return empty.Count;
    }


    private IEnumerator _CloseEmptyCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            manager.SetOpened(
                mine.x,
                mine.y,
                false
            );

            Debug.Log(
                $"Flag Mine: {mine}"
            );
        }
    }
    #endregion

    #region 2005 - Add Flag
    // private int AddFlag(
    //     List<object> args
    // )
    // {
    //     if (args == null ||
    //         args.Count == 0)
    //     {
    //         return 0;
    //     }

    //     float amount = Convert.ToSingle(args[0]);
        
    //     current_coroutine = StartCoroutine(_AddFlagCoroutine(amount));
    //     return 0;
    // }

    // private IEnumerator _AddFlagCoroutine(
    //     float amount
    // )
    // {
    //     for (int i = 0; i < amount; i++)
    //     {
    //         ToolDrop drop = Instantiate(toolDrop);
    //         drop.toolManager = cellManager.GetToolManager();
    //         drop.transform.position = transform.position;
    //         yield return new WaitForSeconds(.05f);
    //     }
        

    // }
    #endregion

    #region 10001 - Open One Mine
    private int OpenOneMine(
        List<object> args
    )
    {

        MinesweepManager manager =
            cellManager.GetMinesweepManager();


        if (manager == null)
            return 0;

        List<Vector2Int> known_mines =
            GetSet(
                CellTarget.AllMine,
                true,
                true
            );

        if (known_mines == null)
            return 0;

        List<Vector2Int> mines =
            GetRandom(
                known_mines,
                1
            );

        if (mines == null)
            return 0;


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _OpenOneMineCoroutine(
                manager,
                mines
            )
        );

        #endregion


        return 0;
    }


    private IEnumerator _OpenOneMineCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            cellManager.GetCell(mine).Open(false);

            Debug.Log(
                $"Opened Mine: {mine}"
            );
        }
    }
    #endregion

    #region 10002 - Open One Empty
    private int OpenOneEmpty(
        List<object> args
    )
    {

        MinesweepManager manager =
            cellManager.GetMinesweepManager();


        if (manager == null)
            return 0;

        List<Vector2Int> known_mines =
            GetSet(
                CellTarget.AllEmpty,
                true,
                true
            );

        if (known_mines == null)
            return 0;

        List<Vector2Int> mines =
            GetRandom(
                known_mines,
                1
            );

        if (mines == null)
            return 0;


        #region Start Coroutine

        current_coroutine = StartCoroutine(
            _OpenOneEmptyCoroutine(
                manager,
                mines
            )
        );

        #endregion


        return 0;
    }


    private IEnumerator _OpenOneEmptyCoroutine(
        MinesweepManager manager,
        List<Vector2Int> mines
    )
    {
        foreach (Vector2Int mine in mines)
        {
            Instantiate(
                eventParticle,
                transform
            ).Play(
                transform.position,
                cellManager.GetCells()[mine].transform.position
            );

            yield return new WaitForSeconds(
                0.5f
            );

            manager.Open(
                mine.x,
                mine.y
            );

            Debug.Log(
                $"Opened Mine: {mine}"
            );
        }
    }
    #endregion

    #region 10003 - Open Middle

    private int OpenMiddle(List<object> args)
    {
        MinesweepManager manager =
            cellManager.GetMinesweepManager();
        StartCoroutine(_OpenMiddleCoroutine(manager));
        return 0;
    }

    private IEnumerator _OpenMiddleCoroutine(
        MinesweepManager manager
    )
    {
        Instantiate(
                eventParticle,
                transform
        ).Play(
            transform.position,
            cellManager.GetCells()[new Vector2Int(5,5)].transform.position
        );

        yield return new WaitForSeconds(
            0.5f
        );

        manager.Open(
            5,
            5
        );
    }
    #endregion
}