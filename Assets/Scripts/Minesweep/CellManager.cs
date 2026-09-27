using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellManager : MonoBehaviour
{
    public GameObject cell_prefab;
    public SpriteRenderer board;
    public Button face;
    public Button restart;
    public ToolManager toolManager;

    public EventManager eventManager;

    public Level levelInitor;

    public NumberDisplay turn_count;
    public NumberDisplay timer_left;

    public GameObject lose_panel;
    public GameObject win_panel;

    public float cell_size = 0.64f;
    public float target_height = 3.2f;

    public bool game_over = false;

    private MinesweepManager minesweepManager = null;

    public float time_max;

    public int turn;

    // ============================================================
    // Cell
    // ============================================================

    private Dictionary<Vector2Int, Cell> cells = new();


    // ============================================================
    // Unity
    // ============================================================

    void Start()
    {
        
        levelInitor.InitLevel();
        // if (face != null)
        // {
        //     restart.onClick.AddListener(Restart);
        // }
    }

    void Update()
    {
        timer_left.number =
            (int)(time_max - eventManager.level_timer.time);
        eventManager.time_left.time = time_max - eventManager.level_timer.time;
    }


    // ============================================================
    // 创建棋盘
    // ============================================================

    public void CreateCells(
        int col,
        int row,
        int mine_count,
        float time_limit,
        int turn_num,
        bool generate_mine = false
    )
    {
        turn = turn_num;
        transform.localScale = Vector3.one;

        time_max = time_limit;

        minesweepManager = new MinesweepManager(
            col,
            row,
            mine_count
        );

        // 清空旧的 Cell 记录
        cells.Clear();

        // 如果之前已经创建过 Cell，先删除
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }


        // ========================================================
        // 创建 Cell
        // ========================================================

        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < row; j++)
            {
                Vector3 position = transform.position;

                position.x +=
                    i * cell_size
                    - (col - 1) * cell_size / 2f;

                position.y -=
                    j * cell_size
                    - (row - 1) * cell_size / 2f;


                GameObject cell_object = Instantiate(
                    cell_prefab,
                    position,
                    Quaternion.identity,
                    transform
                );


                Cell cell = cell_object.GetComponent<Cell>();

                if (cell == null)
                {
                    Debug.LogError(
                        $"Cell Prefab 上没有 Cell 组件: {cell_object.name}"
                    );

                    continue;
                }


                // ====================================================
                // 设置 Cell
                // ====================================================

                cell.spot = new Vector2(i, j);
                cell.ingame = true;


                // ====================================================
                // 存储 Cell
                // ====================================================

                Vector2Int spot =
                    new Vector2Int(i, j);

                cells[spot] = cell;
            }
        }


        // ========================================================
        // 缩放
        // ========================================================

        float height =
            row * cell_size;

        float scale =
            target_height / height;

        transform.localScale =
            new Vector3(
                scale,
                scale,
                transform.localScale.z
            );


        board.size =
            new Vector2(
                col * cell_size
                / height
                * target_height
                + 0.2f,
                board.size.y
            );
        if (generate_mine)
        {
            minesweepManager.GenerateMines(10); 
        }
        
    }


    // ============================================================
    // 根据坐标获取 Cell
    // ============================================================

    public Cell GetCell(
        int x,
        int y
    )
    {
        cells.TryGetValue(
            new Vector2Int(x, y),
            out Cell cell
        );

        return cell;
    }


    // ============================================================
    // 根据坐标获取 Cell
    // ============================================================

    public Cell GetCell(
        Vector2Int spot
    )
    {
        cells.TryGetValue(
            spot,
            out Cell cell
        );

        return cell;
    }


    // ============================================================
    // 获取所有 Cell
    // ============================================================

    public Dictionary<Vector2Int, Cell> GetCells()
    {
        return cells;
    }


    // ============================================================
    // 检查胜利
    // ============================================================

    public void CheckWin()
    {
        if (game_over)
            return;

        if (minesweepManager == null)
            return;

        if (minesweepManager.IsWin())
        {
            turn --;
            if (turn == 0) 
                Win();
            else
                StartCoroutine(NextRound());
        }
    }

    private IEnumerator NextRound() 
    {
        print("Next Round!");
        
        yield return new WaitForSeconds(0.5f);
        minesweepManager.Restart();
        eventManager.round_timer.time ++;
        print($"{turn}"); 
    }
   

    // ============================================================
    // 胜利
    // ============================================================

    public void Win()
    {
        print("You won");
        
    }

    private IEnumerator WinCo() 
    {
        print("Next Round!");
        
        yield return new WaitForSeconds(0.5f);
        win_panel.SetActive(true);
    }


    // ============================================================
    // Game Over
    // ============================================================

    public void GameOver()
    {
        if (game_over)
            return;

        game_over = true;

        Debug.Log("Game Over");

        Cell[] cell_array =
            GetComponentsInChildren<Cell>();

        eventManager.level_timer.SetPulse(false);

        foreach (Cell cell in cell_array)
        {
            cell.GameOver();
        }
        eventManager.GameOver();
        StartCoroutine(ShowLose()); 
        

    }

    public IEnumerator ShowLose()
    {
        yield return new WaitForSeconds(2);
        lose_panel.SetActive(true);
    }


    // ============================================================
    // 重开
    // ============================================================

    public void Restart()
    {

        if (minesweepManager == null)
            return;

        game_over = false;

        minesweepManager.Restart();

        toolManager.Restart();

        levelInitor.InitLevel();

        Cell[] cell_array =
            GetComponentsInChildren<Cell>();

        foreach (Cell cell in cell_array)
        {
            cell.Restart();
        }

        eventManager.level_timer.time = 0;

        eventManager.level_timer.SetPulse(true);
    }


    // ============================================================
    // 获取 MinesweepManager
    // ============================================================

    public MinesweepManager GetMinesweepManager()
    {
        return minesweepManager;
    }


    // ============================================================
    // 获取 ToolManager
    // ============================================================

    public ToolManager GetToolManager()
    {
        return toolManager;
    }


    // ============================================================
    // Cell 更新
    // ============================================================

    public void OnUpdateCells()
    {
        CheckWin();
    }
}