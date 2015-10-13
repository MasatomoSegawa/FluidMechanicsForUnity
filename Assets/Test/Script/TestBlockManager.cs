using UnityEngine;
using System.Collections;

public class TestBlockManager : MonoBehaviour
{
    // 管理するBlock群.
    public Block[] ManagedBlocks;
    public int MAX_X = 5;
    public int MAX_Y = 5;

    // 計算の都合上2次元配列にする.
    private Block[,] ManagedBlocks_2DArray;
    public GameObject PrefabVeloityPointGameObject;

    public int SORCount = 10;

    // 格子定数
    public float a = 0.1f;
    
    // Δt
    public float deltaT = 0.0001f;

    // Re（レイノルズ数）
    public float Re = 1.0f;

    // ω
    public float omega = 1.8f;

    void Start()
    {

        #region Blockの初期化.
        ManagedBlocks_2DArray = new Block[MAX_Y, MAX_X];

        for (int y = 0; y < MAX_Y; y++)
        {
            for (int x = 0; x < MAX_X; x++)
            {
                ManagedBlocks_2DArray[y, x] = ManagedBlocks[y * MAX_Y + x];
            }
        }
        #endregion

        #region AirVelocityPointの初期化.
        for (int y = 0; y < MAX_Y; y++)
        {
            for (int x = 0; x < MAX_X; x++)
            {

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                {

                    // UP
                    if (ManagedBlocks_2DArray[y - 1, x].blockType != BlockType.Wall)
                    {
                        ManagedBlocks_2DArray[y, x].UpVelocityPoint = ManagedBlocks_2DArray[y - 1, x].DownVelocityPoint;
                    }
                    else
                    {
                        GameObject velocityPointObject = Instantiate(PrefabVeloityPointGameObject);
                        velocityPointObject.transform.position = ManagedBlocks_2DArray[y, x].UpVelocityPointPosition.transform.position;
                        velocityPointObject.transform.parent = ManagedBlocks_2DArray[y, x].gameObject.transform;
                        velocityPointObject.name = "UpVelocityPoint";
                        velocityPointObject.GetComponent<VelocityPoint>().myType = VelocityPointType.Normal;
                        ManagedBlocks_2DArray[y, x].UpVelocityPoint = velocityPointObject.GetComponent<VelocityPoint>();
                    }

                    // Down
                    if (ManagedBlocks_2DArray[y + 1, x].blockType != BlockType.Wall)
                    {
                        GameObject velocityPointObject = Instantiate(PrefabVeloityPointGameObject);
                        velocityPointObject.transform.position = ManagedBlocks_2DArray[y, x].DownVelocityPointPosition.transform.position;
                        velocityPointObject.transform.parent = ManagedBlocks_2DArray[y, x].gameObject.transform;
                        velocityPointObject.name = "DownVelocityPoint";
                        velocityPointObject.GetComponent<VelocityPoint>().myType = VelocityPointType.Normal;
                        ManagedBlocks_2DArray[y, x].DownVelocityPoint = velocityPointObject.GetComponent<VelocityPoint>();
                    }
                    else
                    {
                        GameObject velocityPointObject = Instantiate(PrefabVeloityPointGameObject);
                        velocityPointObject.transform.position = ManagedBlocks_2DArray[y, x].DownVelocityPointPosition.transform.position;
                        velocityPointObject.GetComponent<VelocityPoint>().myType = VelocityPointType.Normal;
                        ManagedBlocks_2DArray[y, x].DownVelocityPoint = velocityPointObject.GetComponent<VelocityPoint>();
                    }

                    // Right
                    if (ManagedBlocks_2DArray[y, x + 1].blockType != BlockType.Wall)
                    {
                        GameObject velocityPointObject = Instantiate(PrefabVeloityPointGameObject);
                        velocityPointObject.transform.position = ManagedBlocks_2DArray[y, x].RightVelocityPointPosition.transform.position;
                        velocityPointObject.transform.parent = ManagedBlocks_2DArray[y, x].gameObject.transform;
                        velocityPointObject.name = "RightVelocityPoint";
                        velocityPointObject.GetComponent<VelocityPoint>().myType = VelocityPointType.Normal;
                        ManagedBlocks_2DArray[y, x].RightVelocityPoint = velocityPointObject.GetComponent<VelocityPoint>();
                    }
                    else
                    {
                        GameObject velocityPointObject = Instantiate(PrefabVeloityPointGameObject);
                        velocityPointObject.transform.position = ManagedBlocks_2DArray[y, x].RightVelocityPointPosition.transform.position;
                        velocityPointObject.GetComponent<VelocityPoint>().myType = VelocityPointType.Normal;
                        ManagedBlocks_2DArray[y, x].RightVelocityPoint = velocityPointObject.GetComponent<VelocityPoint>();
                    }

                    // Left
                    if (ManagedBlocks_2DArray[y, x - 1].blockType != BlockType.Wall)
                    {
                        ManagedBlocks_2DArray[y, x].LeftVelocityPoint = ManagedBlocks_2DArray[y, x - 1].RightVelocityPoint;
                    }
                    else
                    {
                        GameObject velocityPointObject = Instantiate(PrefabVeloityPointGameObject);
                        velocityPointObject.transform.position = ManagedBlocks_2DArray[y, x].LeftVelocityPointPosition.transform.position;
                        velocityPointObject.transform.parent = ManagedBlocks_2DArray[y, x].gameObject.transform;
                        velocityPointObject.name = "LeftVelocityPoint";
                        velocityPointObject.GetComponent<VelocityPoint>().myType = VelocityPointType.noVelocity;
                        ManagedBlocks_2DArray[y, x].LeftVelocityPoint = velocityPointObject.GetComponent<VelocityPoint>();
                    }

                }
            }
        }
        #endregion

    }

    void Update()
    {

        // 1.移流粘性項
        AdvectionViscosityUpdate();

        // 2. 外力項
        //ForceUpdate();

        // 3.ダイバージェンス項
        DivergenceUpdate();

        // 4. 圧力のポアソン方程式.
        AirDensityUpdate();

        // 5. 圧力項(修正)
        ReAirDensity();

        // 6. VelocityPointの初期化.
        InitAllVelocityPoint();

        ConstantUpdate();

        // 7. BlockVelocityのアップデート.
        BlockVelocityUpdate();

    }

    /// <summary>
    /// 1.移流粘性項
    /// </summary>
    void AdvectionViscosityUpdate()
    {

        #region 移流更新.
        // 全てのvelocityPointのafterVelocityをアップデートする.
        for (var y = 0; y < MAX_Y; y++){
            for (var x = 0; x < MAX_X; x++){

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                {
                    Block myBlock = ManagedBlocks_2DArray[y, x];

                    // myBlockを中心に上下左右のブロック.
                    Block UpBlock = ManagedBlocks_2DArray[y - 1, x];
                    Block RightBlock = ManagedBlocks_2DArray[y, x + 1];
                    Block LeftBlock = ManagedBlocks_2DArray[y, x - 1];
                    Block DownBlock = ManagedBlocks_2DArray[y + 1, x];

                    // myBlockを中心に右斜めのブロック.
                    Block RightUpBlock = ManagedBlocks_2DArray[y - 1, x + 1];

                    // myBlockを中心に左下のブロック.
                    Block LeftDownBlock = ManagedBlocks_2DArray[y + 1, x - 1];

                    #region LeftX

                    float LL_x = 0.0f;
                    float LDL_x = 0.0f;
                    float LDR_x = 0.0f;

                    if(LeftBlock.LeftVelocityPoint != null){
                        LL_x = LeftBlock.LeftVelocityPoint.velocity.x;
                    }

                    if(LeftDownBlock.LeftVelocityPoint != null){
                        LDL_x = LeftDownBlock.LeftVelocityPoint.velocity.x;
                    }

                    if(LeftDownBlock.RightVelocityPoint != null){
                        LDR_x = LeftDownBlock.RightVelocityPoint.velocity.x;
                    }                    

                    float L_vx = myBlock.RightVelocityPoint.velocity.x;
                    float uLeft = myBlock.LeftVelocityPoint.velocity.x;
                    float vLeft = (
                        myBlock.RightVelocityPoint.velocity.x + 
                        LL_x + 
                        LDL_x +
                        LDR_x
                        ) / 4;

                    if(uLeft >= 0 && vLeft >= 0){
                        myBlock.RightVelocityPoint.velocityAfter.x = 
                            L_vx -
                            uLeft * (L_vx - LL_x) * deltaT -
                            vLeft * (L_vx - LDL_x) * deltaT;
                    }
                    else if(uLeft < 0 && vLeft >= 0){
                        myBlock.RightVelocityPoint.velocityAfter.x =
                            L_vx -
                            uLeft * (myBlock.LeftVelocityPoint.velocity.x - L_vx) * deltaT -
                            vLeft * (L_vx - LL_x) * deltaT;
                    }
                    else if(uLeft >= 0 && vLeft < 0){
                        myBlock.RightVelocityPoint.velocityAfter.x = 
                            L_vx - 
                            uLeft * (L_vx - LL_x) * deltaT -
                            vLeft * (LDL_x - L_vx) * deltaT;
                    }
                    else if(uLeft < 0 && vLeft < 0){
                        myBlock.RightVelocityPoint.velocityAfter.x = 
                            L_vx -
                            uLeft * (myBlock.LeftVelocityPoint.velocity.x - L_vx) * deltaT -
                            vLeft * (LDL_x - L_vx) * deltaT;
                    }
                    #endregion  

                    #region RightX

                    float DL_x = 0.0f;
                    float DR_x = 0.0f;
                    float UL_x = 0.0f;

                    if(DownBlock.LeftVelocityPoint != null){
                        DL_x = DownBlock.LeftVelocityPoint.velocity.x;
                    }

                    if(DownBlock.RightVelocityPoint != null){
                        DR_x = DownBlock.RightVelocityPoint.velocity.x;
                    }

                    if(UpBlock.LeftVelocityPoint != null){
                        UL_x = UpBlock.LeftVelocityPoint.velocity.x;
                    }

                    float R_vx = myBlock.RightVelocityPoint.velocity.x;
                    float u_Right = myBlock.RightVelocityPoint.velocity.x;
                    float v_Right = (
                        myBlock.LeftVelocityPoint.velocity.x + 
                        R_vx +
                        DL_x +
                        DR_x
                        ) / 4;

                    if(uLeft >= 0 && vLeft >= 0){
                        myBlock.RightVelocityPoint.velocity.x = 
                            R_vx - 
                            uLeft * (R_vx - myBlock.LeftVelocityPoint.velocity.x) * deltaT -
                            vLeft * (R_vx - UL_x) * deltaT;                           
                    }
                    else if(uLeft < 0 && vLeft >= 0){
                        myBlock.RightVelocityPoint.velocity.x = 
                            R_vx - 
                            uLeft * (LL_x - R_vx) * deltaT -
                            vLeft * (R_vx - UL_x) * deltaT;
                    }else if(uLeft >= 0 && vLeft < 0){
                        myBlock.RightVelocityPoint.velocity.x = 
                            R_vx -
                            uLeft * (R_vx - myBlock.LeftVelocityPoint.velocity.x) * deltaT - 
                            vLeft * (LL_x - R_vx) * deltaT;
                    }else if(uLeft < 0 && vLeft < 0){
                        myBlock.RightVelocityPoint.velocity.x = 
                            R_vx -
                            uLeft * (LL_x - R_vx) * deltaT -
                            vLeft * (LL_x - R_vx) * deltaT;
                    }
                    #endregion

                    #region UpY

                    float LU_y = 0.0f;
                    float LD_y = 0.0f;
                    float UU_y = 0.0f;

                    if(LeftBlock.UpVelocityPoint != null){
                        LU_y = LeftBlock.UpVelocityPoint.velocity.y;
                    }

                    if(LeftBlock.DownVelocityPoint != null){
                        LD_y = LeftBlock.DownVelocityPoint.velocity.y;
                    }

                    if(UpBlock.UpVelocityPoint != null){
                        UU_y = UpBlock.UpVelocityPoint.velocity.y;
                    }

                    float U_vy = myBlock.UpVelocityPoint.velocity.y;
                    float uUp = (
                        LU_y + 
                        myBlock.UpVelocityPoint.velocity.y +
                        LD_y + 
                        myBlock.DownVelocityPoint.velocity.y
                        ) / 4;
                    float vUp = myBlock.UpVelocityPoint.velocity.y;

                    if(uUp >= 0 && vUp >= 0){
                        myBlock.UpVelocityPoint.velocity.y = 
                            U_vy -
                            uUp * (U_vy - LU_y) * deltaT -
                            vUp * (U_vy - UU_y) * deltaT;
                    }
                    else if(uUp < 0 && vUp >= 0){
                        myBlock.UpVelocityPoint.velocity.y = 
                            U_vy -
                            uUp * (LU_y - U_vy) * deltaT -
                            vUp * (U_vy - UU_y) * deltaT;
                    }else if(uUp >= 0 && vUp < 0){
                        myBlock.UpVelocityPoint.velocity.y = 
                            U_vy -
                            uUp * (U_vy - UU_y) * deltaT -
                            vUp * (LU_y - U_vy) * deltaT;
                    }else if(uUp < 0 && vUp < 0){
                        myBlock.UpVelocityPoint.velocity.y =
                            U_vy -
                            uUp * (LU_y - U_vy) * deltaT -
                            vUp * (myBlock.DownVelocityPoint.velocity.y - U_vy) * deltaT;
                    }
                    #endregion

                    #region DownY

                    float LDD_y = 0.0f;
                    float DD_y = 0.0f;
                    float RD_y = 0.0f;
                    LD_y = 0.0f;

                    if(LeftDownBlock.DownVelocityPoint != null){
                        LDD_y = LeftDownBlock.DownVelocityPoint.velocity.y;
                    }

                    if(DownBlock.DownVelocityPoint != null){
                        DD_y = DownBlock.DownVelocityPoint.velocity.y;
                    }

                    if(RightBlock.DownVelocityPoint != null){
                        RD_y = RightBlock.DownVelocityPoint.velocity.y;
                    }

                    float D_vy = myBlock.DownVelocityPoint.velocity.y;
                    float uDown = (
                        D_vy + 
                        LD_y +
                        LDD_y + 
                        DD_y
                        ) / 4;
                    float vDown = myBlock.DownVelocityPoint.velocity.y;

                    if(uDown >= 0 && vDown >= 0){
                        myBlock.DownVelocityPoint.velocity.y =
                            D_vy -
                            uDown * (D_vy - LD_y) * deltaT -
                            vDown * (D_vy - myBlock.UpVelocityPoint.velocity.y) * deltaT;
                    }
                    else if(uDown < 0 && vDown >= 0){
                        myBlock.DownVelocityPoint.velocity.y = 
                            D_vy -
                            uDown * (RD_y - D_vy) * deltaT -
                            vDown * (D_vy - myBlock.UpVelocityPoint.velocity.y) * deltaT;
                    }else if(uDown >= 0 && vDown < 0){
                        myBlock.DownVelocityPoint.velocity.y = 
                            D_vy - 
                            uDown * (D_vy - LD_y) * deltaT -
                            vDown * (RD_y - D_vy) * deltaT;
                    }else if(uDown < 0 && vDown < 0){
                        myBlock.DownVelocityPoint.velocity.y = 
                            D_vy - 
                            uDown * (RD_y - D_vy) * deltaT -
                            vDown * (DD_y - D_vy) * deltaT;
                    }
                    #endregion
                    
                }

            }
        }
        #endregion


        #region 粘性を計算する.

        for (var y = 0; y < MAX_Y; y++){
            for (var x = 0; x < MAX_X; x++) {

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                {

                    Block myBlock = ManagedBlocks_2DArray[y, x];

                    Block RightBlock = ManagedBlocks_2DArray[y , x + 1];
                    Block UpBlock = ManagedBlocks_2DArray[y - 1, x];
                    Block LeftBlock = ManagedBlocks_2DArray[y , x - 1];
                    Block DownBlock = ManagedBlocks_2DArray[y + 1, x];

                    #region UpVelocityPointの更新.

                    float RU_y = 0.0f;
                    float UU_y = 0.0f;
                    float LU_y = 0.0f;

                    if (RightBlock.UpVelocityPoint != null)
                    {
                        RU_y = RightBlock.UpVelocityPoint.velocity.y;
                    }

                    if (UpBlock.UpVelocityPoint != null)
                    {
                        UU_y = UpBlock.UpVelocityPoint.velocity.y;
                    }

                    if (LeftBlock.UpVelocityPoint != null)
                    {
                        LU_y = LeftBlock.UpVelocityPoint.velocity.y;
                    }

                    float vy_Up = myBlock.UpVelocityPoint.velocity.y;

                        myBlock.UpVelocityPoint.velocityAfter.y =
                            vy_Up -
                            (1 / Re) *
                            (
                             myBlock.DownVelocityPoint.velocity.y +
                             RU_y +
                             UU_y +
                             LU_y
                            ) * deltaT;

                    #endregion 

                    #region DonwVelocityPointの更新.

                    float vy_Down = myBlock.DownVelocityPoint.velocity.y;

                    float DD_y = 0.0f;
                    float LD_y = 0.0f;
                    LU_y = 0.0f;

                    if (DownBlock.DownVelocityPoint != null)
                    {
                        DD_y = DownBlock.DownVelocityPoint.velocity.y;
                    }

                    if (LeftBlock.DownVelocityPoint != null)
                    {
                        LD_y = LeftBlock.DownVelocityPoint.velocity.y;
                    }

                    if (LeftBlock.UpVelocityPoint != null)
                    {
                        LU_y = LeftBlock.UpVelocityPoint.velocity.y;
                    }

                        myBlock.DownVelocityPoint.velocityAfter.y =
                            vy_Down -
                            (1 / Re) *
                            (
                             DD_y +
                             LD_y +
                             myBlock.UpVelocityPoint.velocity.y +
                             LU_y
                             ) * deltaT;
                    #endregion

                    #region LeftVelocityPointの更新.

                    float vx_Left = myBlock.LeftVelocityPoint.velocity.x;

                    float LL_x = 0.0f;
                    float UL_x = 0.0f;
                    float DL_x = 0.0f;

                    if (LeftBlock.LeftVelocityPoint != null)
                    {
                        LL_x = LeftBlock.LeftVelocityPoint.velocity.x;
                    }

                    if (UpBlock.LeftVelocityPoint != null)
                    {
                        UL_x = UpBlock.LeftVelocityPoint.velocity.x;
                    }

                    if (DownBlock.LeftVelocityPoint != null)
                    {
                        DL_x = DownBlock.LeftVelocityPoint.velocity.x;
                    }

                        myBlock.LeftVelocityPoint.velocityAfter.x =
                            vx_Left -
                            (1 / Re) *
                            (
                             LL_x +
                             UL_x +
                             DL_x +
                             myBlock.RightVelocityPoint.velocity.x
                            ) * deltaT;

                    #endregion

                    #region RightVelocityPointの更新.

                    float vx_Right = myBlock.RightVelocityPoint.velocity.x;

                    float UR_x = 0.0f;
                    LL_x = 0.0f;
                    float DR_x = 0.0f;

                    if (UpBlock.RightVelocityPoint != null)
                    {
                        UR_x = UpBlock.RightVelocityPoint.velocity.x;
                    }

                    if (LeftBlock.LeftVelocityPoint != null)
                    {
                        LL_x = LeftBlock.LeftVelocityPoint.velocity.x;
                    }

                    if (DownBlock.RightVelocityPoint != null)
                    {
                        DR_x = DownBlock.RightVelocityPoint.velocity.x;
                    }

                        myBlock.RightVelocityPoint.velocityAfter.x =
                            vx_Right -
                            (1 / Re) *
                            (
                             myBlock.LeftVelocityPoint.velocity.x +
                             UR_x +
                             LL_x +
                             DR_x
                            ) * deltaT;

                    #endregion

                }

            }
        }

        #endregion 

        UpdateBlockVelocitys();
    }

    /// <summary>
    /// 2. 外力項
    /// 壁などの本来速度が生まれない点を０にする.
    /// </summary>
    void ForceUpdate()
    {

        #region WallにくっついてるAirVelocityPointを０にする.

        for (var y = 0; y < MAX_Y; y++)
        {
            for (var x = 0; x < MAX_X; x++)
            {

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                {

                    Block myBlock = ManagedBlocks_2DArray[y, x];
                    Block LeftBlock = ManagedBlocks_2DArray[y, x - 1];
                    Block UpBlock = ManagedBlocks_2DArray[y - 1, x];
                    Block DownBlock = ManagedBlocks_2DArray[y + 1, x];
                    Block RightBlock = ManagedBlocks_2DArray[y, x + 1];

                    if (LeftBlock.blockType == BlockType.Wall)
                    {
                        myBlock.LeftVelocityPoint.velocity.x = 0;
                    }

                    if (RightBlock.blockType == BlockType.Wall)
                    {
                        myBlock.RightVelocityPoint.velocity.x = 0;
                    }

                    if (UpBlock.blockType == BlockType.Wall)
                    {
                        myBlock.UpVelocityPoint.velocity.y = 0;
                    }

                    if (DownBlock.blockType == BlockType.Wall)
                    {
                        myBlock.DownVelocityPoint.velocity.y = 0;
                    }

                }

            }
        }

        #endregion 

    }

    /// <summary>
    /// 3. ダイバージェンス項
    /// 涌きを計算する.
    /// </summary>
    void DivergenceUpdate()
    {

        for (var y = 0; y < MAX_Y; y++){
            for (var x = 0; x < MAX_X; x++){

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                {

                    Block myBlock = ManagedBlocks_2DArray[y, x];

                    myBlock.s =
                        (
                         myBlock.RightVelocityPoint.velocity.x -
                         myBlock.LeftVelocityPoint.velocity.x +
                         myBlock.DownVelocityPoint.velocity.y -
                         myBlock.UpVelocityPoint.velocity.y
                        ) / deltaT;


                }

            }
        }

    }

    /// <summary>
    /// 4. 圧力のポアソン方程式.
    /// 圧力の計算をする.
    /// </summary>
    void AirDensityUpdate()
    {

        for (var count = 0; count < SORCount; count++)
        {
            for (var y = 0; y < MAX_Y; y++)
            {
                for (var x = 0; x < MAX_X; x++)
                {

                    // 壁じゃなければ.
                    if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                    {

                        Block block = ManagedBlocks_2DArray[y, x];

                        Block UpBlock = ManagedBlocks_2DArray[y - 1, x];
                        Block LeftBlock = ManagedBlocks_2DArray[y, x + 1];
                        Block RightBlock = ManagedBlocks_2DArray[y, x - 1];
                        Block DownBlock = ManagedBlocks_2DArray[y + 1, x];

                        float pUp = UpBlock.airDensity;
                        float pLeft = LeftBlock.airDensity;
                        float pRight = RightBlock.airDensity;
                        float pDown = DownBlock.airDensity;

                        if (UpBlock.blockType == BlockType.Wall)
                        {
                            pUp = block.airDensity;
                        }

                        if (LeftBlock.blockType == BlockType.Wall)
                        {
                            pLeft = block.airDensity;
                        }

                        if (DownBlock.blockType == BlockType.Wall)
                        {
                            pDown = block.airDensity;
                        }

                        if (RightBlock.blockType == BlockType.Wall)
                        {
                            pRight = block.airDensity;
                        }

                        // 1ループ前の気圧を保存.
                        block.airDensityAfter = block.airDensity;

                        block.airDensity = (1.0f - omega) * block.airDensity + omega * (pUp + pLeft + pDown + pRight) - block.s;

                    }
                    

                }
            }

        }

    }

    /// <summary>
    /// 5. 圧力項(修正)
    /// vx,vyを更新する.
    /// </summary>
    void ReAirDensity()
    {

        //vx[i][j]-=( p[i][j]-p[i-1][j] )*Δt
        //vy[i][j]-=( p[i][j]-p[i][j-1] )*Δt

        for (var y = 0; y < MAX_Y; y++){
            for (var x = 0; x < MAX_X; x++){

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                {

                    Block myBlock = ManagedBlocks_2DArray[y, x];

                    Block LeftBlock = ManagedBlocks_2DArray[y, x - 1];
                    Block UpBlock = ManagedBlocks_2DArray[y - 1, x];

                    // ↑
                    if (myBlock.UpVelocityPoint.isCalculate == false)
                    {
                        myBlock.UpVelocityPoint.velocity.y -= (myBlock.airDensity - UpBlock.airDensity) * deltaT;
                        //myBlock.UpVelocityPoint.isCalculate = true;
                    }

                    // ↓
                    if (myBlock.DownVelocityPoint.isCalculate == false)
                    {
                        myBlock.DownVelocityPoint.velocity.y -= (myBlock.airDensity - UpBlock.airDensity) * deltaT;
                        //myBlock.DownVelocityPoint.isCalculate = true;
                    }


                    // ←
                    if (myBlock.LeftVelocityPoint.isCalculate == false)
                    {
                        myBlock.LeftVelocityPoint.velocity.x -= (myBlock.airDensity - LeftBlock.airDensity) * deltaT;
                        //myBlock.LeftVelocityPoint.isCalculate = true;
                    }

                    // →
                    if (myBlock.RightVelocityPoint.isCalculate == false)
                    {
                        myBlock.RightVelocityPoint.velocity.x -= (myBlock.airDensity - LeftBlock.airDensity) * deltaT;
                        //myBlock.RightVelocityPoint.isCalculate = true;
                    }

                }

            }
        }

    }

    /// <summary>
    /// 6. VelocityPointの初期化.
    /// </summary>
    void InitAllVelocityPoint()
    {
        for (var y = 0; y < MAX_Y; y++)
        {
            for (var x = 0; x < MAX_X; x++)
            {

                Block myBlock = ManagedBlocks_2DArray[y,x];
                if (myBlock.blockType != BlockType.Wall)
                {

                    myBlock.UpVelocityPoint.isCalculate = false;
                    myBlock.DownVelocityPoint.isCalculate = false;
                    myBlock.RightVelocityPoint.isCalculate = false;
                    myBlock.LeftVelocityPoint.isCalculate = false;

                }

            }
        }

    }

    /// <summary>
    /// 7. 各Blockの速度をアップデートする.
    /// </summary>
    void BlockVelocityUpdate()
    {

        for (var y = 0; y < MAX_Y; y++)
        {
            for (var x = 0; x < MAX_X; x++)
            {

                Block myBlock = ManagedBlocks_2DArray[y, x];
                if (myBlock.blockType != BlockType.Wall)
                {

                    myBlock.UpdateBlockVelocity();
                    

                }

            }
        }

    }

    /// <summary>
    /// 8. constant計算.
    /// </summary>
    void ConstantUpdate()
    {

        for (int y = 0; y < MAX_Y; y++)
        {
            for (int x = 0; x < MAX_X; x++)
            {

                Block myBlock = ManagedBlocks_2DArray[y, x];
                if (myBlock.blockType != BlockType.Wall)
                {

                    if (myBlock.LeftVelocityPoint.myType == VelocityPointType.constant)
                    {
                        myBlock.LeftVelocityPoint.velocity = myBlock.LeftVelocityPoint.ConstantAirVector;
                    }

                    if (myBlock.RightVelocityPoint.myType == VelocityPointType.constant)
                    {
                        myBlock.RightVelocityPoint.velocity = myBlock.RightVelocityPoint.ConstantAirVector;
                    }

                    if (myBlock.UpVelocityPoint.myType == VelocityPointType.constant)
                    {
                        myBlock.UpVelocityPoint.velocity = myBlock.UpVelocityPoint.ConstantAirVector;
                    }

                    if (myBlock.DownVelocityPoint.myType == VelocityPointType.constant)
                    {
                        myBlock.DownVelocityPoint.velocity = myBlock.DownVelocityPoint.ConstantAirVector;
                    }

                }

            }
        }

    }

    /// <summary>
    /// VX,VYを更新させる.
    /// </summary>
    void UpdateBlockVelocitys()
    {

        #region vx,vyを更新させる.

        for (var y = 0; y < MAX_Y; y++)
        {
            for (var x = 0; x < MAX_X; x++)
            {

                if (ManagedBlocks_2DArray[y, x].blockType != BlockType.Wall)
                {

                    Block myBlock = ManagedBlocks_2DArray[y, x];

                    myBlock.UpVelocityPoint.UpdateVelocity();
                    myBlock.DownVelocityPoint.UpdateVelocity();
                    myBlock.LeftVelocityPoint.UpdateVelocity();
                    myBlock.RightVelocityPoint.UpdateVelocity();

                }
            }

        }

        #endregion

    }
}