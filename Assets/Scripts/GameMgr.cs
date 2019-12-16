using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMgr : MonoBehaviour
{
    public GameObject TileObj;    // 用來產生(複製)全部 tile 的參考物件
    public Tile DragingTile;      // 紀錄正在拖曳的 tile
    public List<Tile> Tiles;      // 記錄全部的 tile

    // 用於讓拖曳 tile 置頂的順序數字 (每次拖曳就增加)
    private int TopSortingOrder = 100; 

    // tile 圖片的像素尺寸
    private readonly float TileSpriteSize = 200;


    void Start()
    {
        // 設定攝影機 Size 使 1 單位 = 1 pixel
        Camera.main.orthographicSize = Screen.height / 2;

        // 產生所有 Tile
        SpawnAllTiles();
    }


    void Update()
    {
        // 滑鼠
        if (Input.GetMouseButtonDown(0) && !DragingTile)
            InputPointDown(Input.mousePosition);

        if (DragingTile)
            InputPointMove(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
            InputPointUp();


        // 觸控
        /*
        if (Input.touchCount > 0)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
                InputPointDown(Input.mousePosition);

            else if (Input.touches[0].phase == TouchPhase.Moved)
                InputPointMove(Input.mousePosition);

            else if (Input.touches[0].phase == TouchPhase.Ended)
                InputPointUp();
        }
        */
    }


    // 按下
    void InputPointDown(Vector3 point)
    {

        // 用點擊座標取得 Tile
        DragingTile = GetTileByScreenPoint(point);

        // 將拖曳中 Tile 放到最上層
        PutTileOnTop(DragingTile);
    }


    // 放開
    void InputPointUp()
    {
        if (DragingTile)
        {
            // 放回位置
            DragingTile.transform.position = RowColToScreenPoint(DragingTile.Row, DragingTile.Col);

            // 清空紀錄
            DragingTile = null;

            // 計算連線
            List<List<Tile>> conns = GetTileConnections();

            // 消除連線
            RemoveTileConnections(conns);

            // 產生新 tile 填補空缺
            List<Tile> tiles = GetUniqueTileFromConnections(conns); // 從連線清單中取得不重複的 tile 列表
            CreateTilesAfterRemoved(tiles);
        }
    }


    // 移動
    void InputPointMove(Vector3 point)
    {
        // 如果正在拖曳
        if (DragingTile)
        {
            // 讓選取的 Tile 跟著手指
            Vector3 inputPoint = Camera.main.ScreenToWorldPoint(point);
            DragingTile.transform.position = new Vector3(inputPoint.x, inputPoint.y, 0);

            // 手指與其他 Tile 的碰撞偵測
            RaycastHit2D[] hitInfos = Physics2D.RaycastAll(inputPoint, Vector2.zero);
            foreach (RaycastHit2D hitInfo in hitInfos)
            {
                if (hitInfo.collider.tag == "Tile")
                {
                    // 判斷碰撞的 tile 並不在移動中，也不是拖曳中
                    Tile hoveredTile = hitInfo.collider.gameObject.GetComponent<Tile>();
                    if (!hoveredTile.IsMoving && hoveredTile != DragingTile)
                    {
                        // 與拖曳中的 tile 交換位置
                        SwitchTileWithDraging(hoveredTile);
                    }
                }
            }
        }
    }


    // 用座標取得 Tile
    Tile GetTileByScreenPoint(Vector3 screenPoint)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(screenPoint);
        RaycastHit2D hitInfo = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hitInfo)
        {
            if (hitInfo.collider.tag == "Tile")
                return hitInfo.collider.gameObject.GetComponent<Tile>();
        }

        return null;
    }


    // 將 Tile 顯示在最上層
    void PutTileOnTop(Tile tile)
    {
        TopSortingOrder++;
        DragingTile.GetComponent<SpriteRenderer>().sortingOrder = TopSortingOrder;
    }


    // 與拖曳中的 tile 交換位置
    void SwitchTileWithDraging(Tile tile)
    {
        // 座標資料交換
        int tempRow = DragingTile.Row;
        int tempCol = DragingTile.Col;

        DragingTile.Row = tile.Row;
        DragingTile.Col = tile.Col;

        tile.Row = tempRow;
        tile.Col = tempCol;

        // 移動動畫
        Vector3 newPos = RowColToScreenPoint(tile.Row, tile.Col);
        tile.MoveToPosition(newPos);
    }


    // 一次產生所有磚
    public void SpawnAllTiles()
    {
        Tiles = new List<Tile>();

        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 6; c++)
            {
                CreateNewTile(r, c);
            }
        }
    }


    // 從連線清單中取得不重複的 tile 列表
    private List<Tile> GetUniqueTileFromConnections(List<List<Tile>> conns)
    {
        List<Tile> tiles = new List<Tile>();

        foreach (List<Tile> line in conns)
        {
            foreach (Tile tile in line)
            {
                if (!tiles.Contains(tile))
                    tiles.Add(tile);
            }
        }

        return tiles;
    }


    // 消除連線
    private void RemoveTileConnections(List<List<Tile>> conns)
    {
        foreach (List<Tile> line in conns)
        {
            foreach (Tile tile in line)
            {
                // 視覺消除
                tile.transform.localScale = new Vector3(0, 0, 0);

                // 資料消除
                Tiles.Remove(tile);
            }
        }
    }


    // 產生新 tile 填補空缺
    public void CreateTilesAfterRemoved(List<Tile> removedTiles)
    {
        foreach (Tile removedTile in removedTiles)
        {
            CreateNewTile(removedTile.Row, removedTile.Col);
        }
    }


    // 產生新 tile 到指定坐標
    public Tile CreateNewTile(int row, int col)
    {
        // 產生新的 Tile
        GameObject newTile = Instantiate(TileObj);

        // 放到指定位置
        newTile.transform.position = RowColToScreenPoint(row, col);

        // 縮放 Tile 至合適大小 
        float tileSize = Screen.width / 6;
        float tileScale = tileSize / TileSpriteSize;
        newTile.transform.localScale = new Vector3(tileScale, tileScale, 0);

        // 放大動畫
        Hashtable hash = new Hashtable();
        hash.Add("scale", new Vector3(0, 0, 0));
        hash.Add("easetype", iTween.EaseType.easeOutBack);
        hash.Add("time", 1);
        hash.Add("delay", 0.05 * row);
        iTween.ScaleFrom(newTile, hash);

        // 紀錄位置資訊
        Tile tile = newTile.GetComponent<Tile>();
        tile.Row = row;
        tile.Col = col;

        // 登錄到總表
        Tiles.Add(tile);

        return tile;
    }


    // 由 Row, Column 轉換為場景座標
    public Vector3 RowColToScreenPoint(int row, int col)
    {
        float tileSize = Screen.width / 6;
        float tileScale = tileSize / TileSpriteSize;

        float startX = -(Screen.width / 2) + (tileSize / 2);
        float startY = 0;

        float x = startX + col * tileSize;
        float y = startY - row * tileSize;

        return new Vector3(x, y, 0);
    }


    // 取得 tile 參照陣列
    public Tile[,] GetTileMap()
    {
        Tile[,] tileMap = new Tile[5, 6];
        foreach (Tile tile in Tiles)
        {
            tileMap[tile.Row, tile.Col] = tile;
        }

        return tileMap;
    }


    // 結算連線
    public List<List<Tile>> GetTileConnections()
    {
        // 產生 tile 陣列參照
        Tile[,] tileMap = new Tile[5, 6];
        foreach (Tile tile in Tiles)
        {
            tileMap[tile.Row, tile.Col] = tile;
        }

        List<List<Tile>> connects = new List<List<Tile>>();

        // 遍歷每一個 tile
        for (int r = 0; r < 5; r++)
        {
            for (int c = 0; c < 6; c++)
            {
                // 目前計算的起點 tile
                Tile tileA = tileMap[r,c];

                List<Tile> horzTileList = new List<Tile>();

                // 水平連線
                for (int c2 = c; c2 < 6; c2++)
                {
                    Tile tileB = tileMap[r, c2];

                    if (tileB.Type == tileA.Type && !tileB.IsHorzLined)
                    {
                        horzTileList.Add(tileB);
                    }
                    else
                        break;
                }

                // 水平超過三個記錄到連線列表
                if (horzTileList.Count >= 3)
                {
                    connects.Add(horzTileList);

                    // 把有通過的作不重複計算的標記
                    foreach (Tile tile in horzTileList)
                        tile.IsHorzLined = true;
                }


                // 垂直連線
                List<Tile> vertTileList = new List<Tile>();
                for (int r2 = r; r2 < 5; r2++)
                {
                    Tile tileB = tileMap[r2, c];

                    if (tileB.Type == tileA.Type && !tileB.IsVertLined)
                    {
                        vertTileList.Add(tileB);
                    }
                    else
                        break;
                }

                // 垂直超過三個記錄到連線列表
                if (vertTileList.Count >= 3)
                {
                    connects.Add(vertTileList);

                    // 把有通過的作不重複計算的標記
                    foreach (Tile tile in vertTileList)
                        tile.IsVertLined = true;
                }
            }
        }

        return connects;
    }
}
