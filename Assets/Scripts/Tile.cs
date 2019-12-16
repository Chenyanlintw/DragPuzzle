using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // 類型
    public int Type = 0;

    // 座標
    public int Row;
    public int Col;

    // 是否在移動中 (交換動畫中)
    public bool IsMoving;

    // 連線標記 (用於連線結算)
    public bool IsHorzLined = false;
    public bool IsVertLined = false;

    // 各種類型的圖形 (由unity 介面中拖入指定)
    public Sprite[] TypeSprites;

    // 交換動畫用的資料
    private Vector3 PrevPosition;
    private Vector3 TargetPosition;
    private int MoveCount = 0;
    private readonly int MoveSteps = 6;


    void Start()
    {
        // 隨機變換類型
        RandomType();
    }


    void Update()
    {
        if (IsMoving)
        {
            // 動畫尚未結束
            if (MoveCount < MoveSteps)
            {
                // 取得目標方向的夾角 (弧度)
                float angle = Mathf.Atan2(TargetPosition.y - PrevPosition.y, TargetPosition.x - PrevPosition.x);

                // 計算每次要移動的角度
                float theta = Mathf.PI / MoveSteps * MoveCount;

                // 依目標方向決定起始角度
                // 向左 
                if ((int)Mathf.Floor(angle) == 0)
                    theta += Mathf.PI;

                // 向右
                if ((int)Mathf.Floor(angle) == 3) { }

                // 向上
                if ((int)Mathf.Floor(angle) == -2)
                    theta += Mathf.PI / 2;

                // 向下
                if ((int)Mathf.Floor(angle) == 1)
                    theta -= Mathf.PI / 2;

                // 取得旋轉半徑
                float distance = (PrevPosition - TargetPosition).magnitude;
                float r = distance / 2;

                // 偏移 (依方向決定圓心的位置)
                float offsetX = Mathf.Cos(angle) * r;
                float offsetY = Mathf.Sin(angle) * r;

                // 計算 Tile 移動這一步後的位置
                float x = PrevPosition.x + Mathf.Cos(theta) * r + offsetX;
                float y = PrevPosition.y + Mathf.Sin(theta) * r + offsetY;
                transform.position = new Vector3(x, y, 0);

                MoveCount++;
            }

            // 步數已達 = 動畫結束，停止重繪，重設相關參數
            else
            {
                transform.position = TargetPosition;
                IsMoving = false;
                MoveCount = 0;
            }
        }
    }


    // 隨機變換類型
    public void RandomType()
    {
        // 紀錄亂數產生 0~4 整數
        Type = (int)Mathf.Floor(Random.Range(0, 5));

        // 切換顯示圖案
        GetComponent<SpriteRenderer>().sprite = TypeSprites[Type];
    }


    // 移動到指定位置
    public void MoveToPosition(Vector3 pos)
    {
        // 紀錄現在位置 & 目標位置
        PrevPosition = transform.position;
        TargetPosition = pos;

        // 啟動動畫
        IsMoving = true;
    }

}
