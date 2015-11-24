using UnityEngine;
using System.Collections.Generic;

public class RegularFluidMecahnicsController : MonoBehaviour {

    #region 描画用の変数
    // 1ルーム当たりのスプライトの大きさ.
    const float ROOM_SPRITE_LENGTH = 2.935f;

    [Header("流体力学ルームの部屋のX数"), Range(0, 100)]
    public int ROOM_MAX_X;

    [Header("流体力学ルームの部屋のY数"), Range(0, 100)]
    public int ROOM_MAX_Y;

    [Header("部屋のPrfab")]
    public GameObject roomPrefab;

    [Header("壁のprefab")]
    public GameObject wallPrefab;

    // Roomの2次元リスト.
    private List<List<GameObject>> rooms;
    #endregion

    #region 数値計算用変数

    [Header("リラクゼーション数")]
    public int iteration = 10;

    [Header("デルタX")]
    public float DX = 10.0f;

    [Header("デルタY")]
    public float DY = 10.0f;

    [Header("レイノルズ数")]
    public float Re = 500.0f;

    [Header("デルタT")]
    public float deltaT = 0.01f;

    // 圧力.
    private float[,] prs;

    // 流れ関数.
    private float[,] psi;

    private float[,] velX;
    private float[,] velY;

    // x方向微分.
    private float[,] velXgx;

    // y方向微分.
    private float[,] velXgy;

    // x方向微分.
    private float[,] velYgx;

    // y方向微分.
    private float[,] velYgy;

    // 過度.
    private float[,] omg;



    private int NX;
    private int NY;

    public float maxPsi0 = 0.02f;
    public float minPsi0 = -0.1f;
    public float maxOmg0 = 20.0f;
    public float minOmg0 = -21.0f;
    public float maxPrs0 = 1.0f;
    public float minPrs0 = -0.4f;

    #endregion

    #region Unityライフサイクル.

    void Start()
    {

        PhysicsRoomLevelImportor physicsRoomLevelImportor = GetComponent<PhysicsRoomLevelImportor>();
        RoomInformation roomInformation = physicsRoomLevelImportor.GetRegularRoomInformation("roomLevel");

        InitPhysicsRooms(roomInformation);

        InitData();

    }



    void Update()
    {

        Calculate();

        // 描画更新.
        DrawVelocity();

    }

    #endregion

    #region 流体力学用の関数.

    private void InitPhysicsRooms(RoomInformation roomInformation)
    {

        rooms = roomInformation.physicsRooms;

        ROOM_MAX_X = roomInformation.ROOM_MAX_X;
        ROOM_MAX_Y = roomInformation.ROOM_MAX_Y;

        // 速度点は(部屋の数+1)個存在する.
        NX = ROOM_MAX_X;
        NY = ROOM_MAX_Y;

    }

    /// <summary>
    /// 変数などを初期化する.
    /// </summary>
    private void InitData()
    {

        prs = new float[NY, NX];
        psi = new float[NY, NX];
        omg = new float[NY, NX];
        velX = new float[NY, NX];
        velY = new float[NY, NX];
        velXgx = new float[NY, NX];
        velXgy = new float[NY, NX];
        velYgx = new float[NY, NX];
        velYgy = new float[NY, NX];

        for (int Y = 0; Y < ROOM_MAX_Y; Y++)
        {
            for (int X = 0; X < ROOM_MAX_X; X++)
            {

                prs[Y, X] = 0.0f;



                velY[Y, X] = 0.0f;
                velXgx[Y, X] = 0.0f;
                velXgy[Y, X] = 0.0f;
                velYgx[Y, X] = 0.0f;
                velYgy[Y, X] = 0.0f;
                omg[Y, X] = 0.0f;

            }
        }

        maxPrs0 = -1000.0f;
        minPrs0 = 1000.0f;
        maxOmg0 = -1000.0f;
        minOmg0 = 1000.0f;

    }

    #endregion

    #region 計算用関数

    private void Calculate()
    {

        methodCIP(velX, velXgx, velXgy, velX, velY);
        methodCIP(velY, velYgx, velYgy, velX, velY);

        Calculate_Poisson();

        Update_Variables();
    }

    void methodCIP(float[,] f, float[,] gx, float[,] gy, float[,] vx, float[,] vy)
    {
        float[,] newF = new float[NY, NX];//関数
        float[,] newGx = new float[NY, NX];//x方向微分
        float[,] newGy = new float[NY, NX];//y方向微分

        float c11, c12, c21, c02, c30, c20, c03, a, b, sx, sy, x, y, dx, dy, dx2, dy2, dx3, dy3;

        int ip, jp;
        for (int j = 1; j < NX; j++)
            for (int i = 1; i < NY; i++)
            {
                if (vx[i, j] >= 0.0) sx = 1.0f; else sx = -1.0f;
                if (vy[i, j] >= 0.0) sy = 1.0f; else sy = -1.0f;

                x = -vx[i, j] * deltaT;
                y = -vy[i, j] * deltaT;
                ip = (int)(i - sx);//上流点
                jp = (int)(j - sy);
                dx = sx * DX;
                dy = sy * DY;
                dx2 = dx * dx;
                dy2 = dy * dy;
                dx3 = dx2 * dx;
                dy3 = dy2 * dy;

                c30 = ((gx[ip, j] + gx[i, j]) * dx - 2.0f * (f[i, j] - f[ip, j])) / dx3;
                c20 = (3.0f * (f[ip, j] - f[i, j]) + (gx[i, j] + 2.0f * gx[i, j]) * dx) / dx2;
                c03 = ((gy[i, jp] + gy[i, j]) * dy - 2.0f * (f[i, j] - f[i, jp])) / dy3;
                c02 = (3.0f * (f[i, jp] - f[i, j]) + (gy[i, jp] + 2.0f * gy[i, j]) * dy) / dy2;
                a = f[i, j] - f[i, jp] - f[ip, j] + f[ip, jp];
                b = gy[ip, j] - gy[i, j];
                c12 = (-a - b * dy) / (dx * dy2);
                c21 = (-a - (gx[i, jp] - gx[i, j]) * dx) / (dx2 * dy);
                c11 = -b / dx + c21 * dx;

                newF[i, j] = f[i, j] + ((c30 * x + c21 * y + c20) * x + c11 * y + gx[i, j]) * x
                           + ((c03 * y + c12 * x + c02) * y + gy[i, j]) * y;

                newGx[i, j] = gx[i, j] + (3.0f * c30 * x + 2.0f * (c21 * y + c20)) * x + (c12 * y + c11) * y;
                newGy[i, j] = gy[i, j] + (3.0f * c03 * y + 2.0f * (c12 * x + c02)) * y + (c21 * x + c11) * x;

                //粘性項に中央差分
                newF[i, j] += deltaT * ((f[i - 1, j] + f[i + 1, j] - 2.0f * f[i, j]) / dx2
                           + (f[i, j - 1] + f[i, j + 1] - 2.0f * f[i, j]) / dy2) / Re;
            }

        //更新
        for (int j = 1; j < NY; j++)
            for (int i = 1; i < NX; i++)
            {
                // ここに何か入れる！
                f[i, j] = newF[i, j];
                gx[i, j] = newGx[i, j];
                gy[i, j] = newGy[i, j];
            }
    }

    /// <summary>
    /// ポアソン方程式の計算.
    /// </summary>
    private void Calculate_Poisson()
    {

        float tolerance = 0.00001f;
        float maxError = 0.0f;
        float DX2 = DX * DX;
        float DY2 = DY * DY;
        float A1 = 0.5f * DY2 / (DX2 + DY2);
        float A2 = 0.5f * DX2 / (DX2 + DY2);
        float A3 = 0.25f * DX2 * DY2 / (DX2 + DY2);

        // ポアソン方程式の右辺.
        float[,] D = new float[NY, NX];
        for (int j = 1; j < NY - 1; j++)
            for (int i = 1; i < NX - 1; i++)
            {
                float a = (velX[i + 1, j] - velX[i, j]) / DX;
                float b = (velY[i, j + 1] - velY[i, j]) / DY;
                D[i, j] = A3 * (a + b) / deltaT;
            }

        //反復法
        int cnt = 0;
        while (cnt < iteration)
        {
            maxError = 0.0f;

            //圧力境界値
            for (int j = 1; j < NY; j++)
            {
                prs[0, j] = prs[1, j] - 2.0f * velX[0, j] / (DX * Re);//左端
                prs[NX - 1, j] = prs[NX - 2, j] + 2.0f * velX[NX, j] / (DX * Re);//右端
            }
            for (int i = 1; i < NX; i++)
            {
                prs[i, 0] = prs[i, 1] - 2.0f * velY[i, 0] / (DY * Re);//下端
                prs[i, NY - 1] = prs[i, NY - 2] + 2.0f * velY[i, NY] / (DY * Re);//上端
            }

            for (int j = 1; j < NY - 1; j++)
                for (int i = 1; i < NX - 1; i++)
                {
                    float pp = A1 * (prs[i + 1, j] + prs[i - 1, j]) + A2 * (prs[i, j + 1] + prs[i, j - 1]) - D[i, j];
                    float error = Mathf.Abs(pp - prs[i, j]);
                    if (error > maxError) maxError = error;
                    prs[i, j] = pp;//更新 
                }
            if (maxError < tolerance) break;

            cnt++;
        }


    }

    /// <summary>
    /// 変数更新.
    /// </summary>
    private void Update_Variables()
    {

        //速度ベクトルの更新
        for (int j = 1; j < NY; j++)
            for (int i = 1; i < NX; i++)
            {
                //if (type[i][j] != "INSIDE") continue;
                velX[i,j] += -0.5f * deltaT * (prs[i + 1,j] - prs[i - 1,j]) / DX;
                velY[i,j] += -0.5f * deltaT * (prs[i,j + 1] - prs[i,j - 1]) / DY;

            }

        //Omega
        for (int i = 1; i <= NX - 1; i++)
            for (int j = 1; j <= NY - 1; j++)
            {
                omg[i, j] = 0.5f * ((velY[i + 1, j] - velY[i - 1, j]) / DX - (velX[i, j + 1] - velX[i, j - 1]) / DY);
            }

        //流れ関数，圧力、渦度の最小値，最大値
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
            {
                if (prs[i, j] > maxPrs0) maxPrs0 = prs[i, j];
                if (prs[i, j] < minPrs0) minPrs0 = prs[i, j];
                if (omg[i, j] > maxOmg0) maxOmg0 = omg[i, j];
                if (omg[i, j] < minOmg0) minOmg0 = omg[i, j];
            }
    }


    #endregion

    #region 描画用の関数

    private void DrawVelocity()
    {

        for (int Y = 0; Y < ROOM_MAX_Y; Y++)
        {
            for (int X = 0; X < ROOM_MAX_X; X++)
            {
                RegularPhysicsRoom currentRoom = rooms[Y][X].GetComponent<RegularPhysicsRoom>();

                
            }
        }



    }
    #endregion

}
