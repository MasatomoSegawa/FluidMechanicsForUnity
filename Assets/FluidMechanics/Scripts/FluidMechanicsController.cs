using UnityEngine;
using System.Collections.Generic;

public class FluidMechanicsController : MonoBehaviour
{
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

    private float[,] s_velX;
    private float[,] s_velY;

    // x方向速度.
    private float[,] f_VelX;

    // y方向速度.
    private float[,] f_VelY;

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

        InitData();

    }

    void Update()
    {

        // step1
        // 境界条件
        Calculate_Boundarycondition();

        // step2
        // CIP
        Calculate_CIP();

        // step3
        // ポアソン
        Calculate_Poisson();

        // step4
        // 変数アップデート
        Update_Variables();

        // 描画更新.
        DrawVelocity();
    }

    #endregion

    #region 流体力学用の関数.

    /// <summary>
    /// 変数などを初期化する.
    /// </summary>
    private void InitData()
    {
        // 部屋のリストオブジェクトを初期化.
        rooms = new List<List<GameObject>>();

        NX = ROOM_MAX_X + 2;
        NY = ROOM_MAX_Y + 2;

        // 配列の２次元化.
        prs = new float[NY + 1, NX + 1];
        psi = new float[NY + 1, NX + 1];
        omg = new float[NY + 1, NX + 1];
        f_VelX = new float[NY + 1, NX + 1];
        f_VelY = new float[NY + 1, NX + 1];
        s_velX = new float[NY + 1, NX + 1];
        s_velY = new float[NY + 1, NX + 1];
        velXgx = new float[NY + 1, NX + 1];
        velXgy = new float[NY + 1, NX + 1];
        velYgx = new float[NY + 1, NX + 1];
        velYgy = new float[NY + 1, NX + 1];

        // 部屋を生成.
        for (int Y = 0; Y < ROOM_MAX_Y; Y++)
        {

            // Y軸初期化.
            rooms.Add(new List<GameObject>());

            for (int X = 0; X < ROOM_MAX_X; X++)
            {

                GameObject obj = InstanceRoomObject(X, Y);

                Vector3 newPosition = Vector3.zero;
                if (ROOM_MAX_X % 2 == 0)
                {

                    newPosition = new Vector3(
                        (-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x - ROOM_SPRITE_LENGTH / 2,
                        (-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y - ROOM_SPRITE_LENGTH / 2,
                        1.0f
                        );
                }
                else
                {
                    newPosition = new Vector3(
                        (-(ROOM_MAX_X - 1) / 2 + X) * ROOM_SPRITE_LENGTH + transform.position.x,
                        (-(ROOM_MAX_Y - 1) / 2 + Y) * ROOM_SPRITE_LENGTH + transform.position.y,
                        1.0f
                        );
                }

                obj.transform.localPosition = newPosition;
                obj.transform.SetParent(transform);
                obj.name = "Room(" + X.ToString() + " , " + Y.ToString() + ")";
                rooms[Y].Add(obj);

            }
        }
    }

    #endregion

    #region 計算用関数

    /// <summary>
    /// 速度の境界条件
    /// </summary>
    private void Calculate_Boundarycondition()
    {

        // 上下.
        for (int i = 0; i <= NX; i++)
        {

            s_velY[i, 0] = s_velY[i, 2];
            s_velY[i, 1] = 0.0f;
            s_velX[i, 0] = -s_velX[i, 1];

            s_velX[i, NY - 1] = 2.0f - s_velX[i, NY - 2];//上境界度を1とする(平均値が1となる)の速
            s_velY[i, NY] = s_velY[i, NY - 2];
            s_velY[i, NY - 1] = 0.0f;

        }

        //左右
        for (int j = 0; j <= NY; j++)
        {
            s_velX[0, j] = s_velX[2, j];
            s_velX[1, j] = 0.0f;
            s_velY[0, j] = -s_velY[1, j];

            s_velX[NX, j] = s_velX[NX - 2, j];
            s_velX[NX - 1, j] = 0.0f;
            s_velY[NX - 1, j] = -s_velY[NX - 2, j];
        }

    }

    private void Update_Variables()
    {

        //step5(スタガード格子点の速度ベクトルの更新）
        for (int j = 1; j < NY - 1; j++)
            for (int i = 2; i < NX - 1; i++)
            {
                s_velX[i, j] += -deltaT * (prs[i, j] - prs[i - 1, j]) / DX;
            }
        for (int j = 2; j < NY - 1; j++)
            for (int i = 1; i < NX - 1; i++)
            {
                s_velY[i, j] += -deltaT * (prs[i, j] - prs[i, j - 1]) / DY;
            }

        //表示のための速度は圧力と同じ位置で
        for (int j = 1; j <= NY - 2; j++)
            for (int i = 1; i <= NX - 2; i++)
            {
                f_VelX[i, j] = (s_velX[i, j] + s_velX[i + 1, j]) / 2.0f;
                f_VelY[i, j] = (s_velY[i, j] + s_velY[i, j + 1]) / 2.0f;
            }

        //Psi
        for (int j = 0; j < NY - 1; j++)
        {
            psi[0, j] = 0.0f;
            for (int i = 1; i < NX - 1; i++)
                psi[i, j] = psi[i - 1, j] - DX * (s_velY[i - 1, j] + s_velY[i, j]) / 2.0f;
        }
        //Omega
        for (int i = 1; i <= NX - 1; i++)
            for (int j = 1; j <= NY - 1; j++)
            {
                omg[i, j] = 0.5f * ((f_VelY[i + 1, j] - f_VelY[i - 1, j]) / DX - (f_VelX[i, j + 1] - f_VelX[i, j - 1]) / DY);
            }

        //流れ関数，圧力、渦度の最小値，最大値
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
            {
                if (prs[i, j] > maxPrs0) maxPrs0 = prs[i, j];
                if (prs[i, j] < minPrs0) minPrs0 = prs[i, j];
                if (psi[i, j] > maxPsi0) maxPsi0 = psi[i, j];
                if (psi[i, j] < minPsi0) minPsi0 = psi[i, j];
                if (omg[i, j] > maxOmg0) maxOmg0 = omg[i, j];
                if (omg[i, j] < minOmg0) minOmg0 = omg[i, j];
            }

    }

    /// <summary>
    /// ポアソン方程式の計算.
    /// </summary>
    private void Calculate_Poisson()
    {
        float tolerance = 0.00001f;//許容誤差
        float maxError = 0.0f;

        float A1 = 0.5f * (DY * DY) / ((DX * DX) + (DY * DY));
        float A2 = 0.5f * (DX * DX) / ((DX * DX) + (DY * DY));
        float A3 = 0.25f * (DX * DX) * (DY * DY) / ((DX * DX) + (DY * DY));

        // ポアソン方程式の右辺.
        float[,] D = new float[NY, NX];
        for (int j = 1; j < NY - 1; j++)
            for (int i = 1; i < NX - 1; i++)
            {
                float a = (s_velX[i + 1, j] - s_velX[i, j]) / DX;
                float b = (s_velY[i, j + 1] - s_velY[i, j]) / DY;
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
                prs[0, j] = prs[1, j] - 2.0f * s_velX[0, j] / (DX * Re);//左端
                prs[NX - 1, j] = prs[NX - 2, j] + 2.0f * s_velX[NX, j] / (DX * Re);//右端
            }
            for (int i = 1; i < NX; i++)
            {
                prs[i, 0] = prs[i, 1] - 2.0f * s_velY[i, 0] / (DY * Re);//下端
                prs[i, NY - 1] = prs[i, NY - 2] + 2.0f * s_velY[i, NY] / (DY * Re);//上端
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

    private void Calculate_CIP()
    {

        float[,] vel = new float[NY, NX];

        //x方向速度定義点における速度
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
                vel[i, j] = (s_velY[i - 1, j] + s_velY[i, j] + s_velY[i - 1, j + 1] + s_velY[i, j + 1]) / 4.0f;

        methodCIP(s_velX, velXgx, velXgy, s_velX, vel);

        //y成分
        //y方向速度定義点における速度
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
                vel[i, j] = (s_velX[i, j] + s_velX[i, j - 1] + s_velX[i + 1, j - 1] + s_velX[i + 1, j]) / 4.0f;

        methodCIP(s_velY, velYgx, velYgy, vel, s_velY);
    }

    void methodCIP(float[,] f, float[,] gx, float[,] gy, float[,] vx, float[,] vy)
    {
        float[,] newF = new float[NY + 1, NX + 1];//関数
        float[,] newGx = new float[NY + 1, NX + 1];//x方向微分
        float[,] newGy = new float[NY + 1, NX + 1];//y方向微分

        float c11, c12, c21, c02, c30, c20, c03, a, b, sx, sy, x, y, dx, dy, dx2, dy2, dx3, dy3;

        int ip, jp;
        for (int i = 1; i < NX; i++)
            for (int j = 1; j < NY; j++)
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
                f[i, j] = newF[i, j];
                gx[i, j] = newGx[i, j];
                gy[i, j] = newGy[i, j];
            }


    }

    #endregion

    #region 描画用の関数
    private GameObject InstanceRoomObject(int X, int Y)
    {

        // Debug.Log(X.ToString() + "," + Y.ToString());

        // 壁なら.
        if (Y == 0 || X == 0 || Y == ROOM_MAX_Y - 1 || X == ROOM_MAX_X - 1)
        {
            //Debug.Log("Wall");

            return Instantiate(wallPrefab);
        }

        //Debug.Log("Room");
        return Instantiate(roomPrefab);
    }

    private void DrawVelocity()
    {

        for (int Y = 0; Y < ROOM_MAX_Y; Y++)
        {
            for (int X = 0; X < ROOM_MAX_X; X++)
            {

                PhysicsRoom currentRoom = rooms[Y][X].GetComponent<PhysicsRoom>();

                if (currentRoom.isWall == false)
                {

                    currentRoom.UpdateVelocity(f_VelX[Y,X],f_VelY[Y,X]);

                }

            }
        }
    }
    #endregion

}
