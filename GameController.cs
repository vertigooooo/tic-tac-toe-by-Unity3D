using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameController : NetworkBehaviour
{
    //等待遮罩
    public GameObject waitRoom;

    //要加载的材质，分别代表红绿两方
    public Material greenTeam;
    public Material redTeam;
    public Material thisTeam;

    //如果队伍设置过了，则为true
    private bool isTeamSeted;

    //棋盘的变化状态信息
    //如果有人落子则isChanged=1，index为落子的位置
    [SyncVar]
    private ChangeInfo changeInfo;

    //在线人数
    [SyncVar]
    public int onlineNum;

    //玩家ID
    [SyncVar]
    private PlayerID playerID;

    //棋盘状态信息的数组，10代表还未落子。
    //如果红方落子赋值1，蓝方落子赋值-1.也就是说某个方向上和的绝对值为3时表示赢了
    //[SyncVar]
    public SyncListInt chessBoard = new SyncListInt();


    //存储游戏结果，可能的游戏结果为：暂时没有结果，绿赢，红赢，平局
    //这三种情况对应的数据应该是  0,1,2,3
    [SyncVar]
    private int result;

    //玩家ID（是红方还是绿方）
    private bool isRed;


    public static int NO_RESULT = 0;
    public static int GREEN_WIN = 1;
    public static int RED_WIN = 2;
    public static int NO_WIN = 3;


    //初始化调用
    void Start () {

        //如果是server，要先等待，防止偷抢
        if (isServer)
            waitRoom = Instantiate(waitRoom);


        //进行初始化检查
        Debug.Log("这个游戏中目前有" + onlineNum + "个玩家");

        isTeamSeted = false;
        int ID = GetInstanceID();
        Debug.Log("本玩家的ID是" + ID);

        for (int i = 0; i < 9; i++)
            chessBoard.Add(10);
        

        result = 0; //暂未决出胜负

        changeInfo.isChanged = false;
        changeInfo.index = -1;
    }
	
    //每帧调用
	void Update () {

        //检查在线人数
        checkOnlineNum();

        //更改棋盘的外观
        changeBoard();

        //查看是否有输入
        checkInput();

        //检查是否结束了
        checkFinish();  
	}

    //GUI调用
    private void OnGUI()
    {
        
    }

    //创建客户端玩家时执行的函数
    //创建之后立刻告知服务器
    public override void OnStartLocalPlayer()
    {

        //先调用原本的函数避免不兼容
        base.OnStartLocalPlayer();
        
        //让服务器去判断这个新的玩家是否应该被加入
        CmdAddPlayer(GetInstanceID(),this.gameObject);  
        
    }

    //让服务器去判断这个新的玩家是否应该被加入
    [Command]
    private void CmdAddPlayer(int id, GameObject o)
    {
        //如果在线人数大于2了，就不应该再让玩家加入了
        if (onlineNum > 2)
            Destroy(o);
    }
    
    //对点击到的物体 伪染色 
    //--实际上只是改变chessboard的值，染色将在下一帧的changeBoard中完成
    [Command]
    void CmdPaint(string blockTag)
    {
        if (blockTag.Substring(0,5) == "block")
        {
            GameObject.Find("Main Camera").GetComponent<AudioSource>().Play();

            Debug.Log("截取到的字符串是："+ blockTag[5]);
            
            int i = int.Parse(blockTag[5]+"");  //此处截取到的应该是数字
            if (thisTeam == redTeam)
                chessBoard[i] = 1;
            else
                chessBoard[i] = -1;
        }

        //涂色完成后再检查一下是否有结果了
        checkResult();
    }
    //根据 chessBoard[,] 数组更改棋盘的外观
    void changeBoard()
    {
        for (int i = 0; i < 9; i++)     //遍历棋盘数组，给每个棋盘块赋颜色
        {
            string tag_block = "block" + i;                     //要改变外观的block的tag
            GameObject o = GameObject.FindWithTag(tag_block);   //根据tag查找到的物体（也就是棋格）

            //Debug.Log("chessBoard[" + i + "]当前值为:" + chessBoard[i]);

            if (chessBoard[i] == 10)             //如果这里的值为10则不用变色，否则变色
                continue;
            else
            {
                o.GetComponent<MeshRenderer>().material = (
                    (chessBoard[i] == 1) ? redTeam : greenTeam
                );
            }
        }
    }

    //检查是否有输入
    //如果有，就调用对应的函数
    void checkInput()
    {
        if (!isLocalPlayer) //只允许本地玩家输入
            return;

        //视线，是过摄像头和鼠标指向的直线
        Ray myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit myHit;

        //点击棋盘上每个空位时给每个空位染色
        if (Physics.Raycast(myRay, out myHit))
        {
            string blockTag = myHit.collider.gameObject.tag;
            if (Input.GetMouseButtonDown(0))    //如果检测到左键单击
            {
                CmdPaint(blockTag); //对点击到的物体染色
            }
            //else if (Input.GetMouseButtonDown(1))
            //{
            //    CmdPaint(myHit.collider.gameObject); //染色
            //}

        }

    }

    //检查是否游戏结束
    void checkFinish()
    {
        if(result != 0)
        {
            Result.result = result;
            if (isServer)
                Invoke("GameFinish",0.2f);
            else
                Invoke("GameFinish",0);
        }
    }

    //游戏结束，调用下一个场景
    void GameFinish()
    {
        SceneManager.LoadScene("FinishScene");
    }

    //检查在线人数
    void checkOnlineNum()
    {
        bool isChanged = false;
        if (isServer)   //服务器端才会执行的代码
        {
            if (onlineNum > 1)  //只有服务器自己在看
            {
                Debug.Log("访问到这里啦");
                GameObject.DestroyImmediate(waitRoom,true);

            }
            if (onlineNum != NetworkManager.singleton.numPlayers)    //如果人数发生了改变的话
            {
                isChanged = true;
                onlineNum = NetworkManager.singleton.numPlayers;    //更新人数
                Debug.Log("人数更新啦：当前在线人数为：" + onlineNum);//并显示
            }
        }
        
        if (isChanged && !isTeamSeted)  //如果队伍没被设置过而且人数改变了
        {
            if (onlineNum == 1)
                thisTeam = redTeam;
            else if(onlineNum == 2)
                thisTeam = greenTeam;

            isTeamSeted = true;
        }


    }



    //检查chessBoard[,]，看是否已经有人赢了
    //这个函数为了安全性最好在服务器调用
    void checkResult()
    {
        //检查是否平局
        bool isPeace = true;

        //遍历检查
        for (int i = 0; i < 9; i++)
            if (chessBoard[i] == 10)
                isPeace = false;

        for (int i = 0; i < 3; i++)   //检查行列的和是不是三
        {
            if (abs3(chessBoard[3 * i + 0], chessBoard[3 * i + 1], chessBoard[3 * i + 2]))     //检查行的和
            {
                //游戏结束的处理代码
                result = (chessBoard[3 * i + 0] > 0 ? RED_WIN : GREEN_WIN);
            }
            else if (abs3(chessBoard[3 * 0 + i], chessBoard[3 * 1 + i], chessBoard[3 * 2 + i])) //检查列的和
            {
                //游戏结束的处理代码
                result = (chessBoard[3 * 0 + i] > 0 ? RED_WIN : GREEN_WIN);
            }
        }

        if (abs3(chessBoard[0], chessBoard[4], chessBoard[8])           //检查对角线的和
            || abs3(chessBoard[2], chessBoard[4], chessBoard[6]))
        {
            //游戏结束的处理代码
            result = (chessBoard[4] > 0 ? RED_WIN : GREEN_WIN);
        }

        if (isPeace)
            result = 3;
    }

    //绝对值是三吗
    private static bool abs3(int i1, int i2, int i3)
    {
        return 3 == System.Math.Abs(i1 + i2 + i3) ? true : false;
    }

}


/*************************自定义的数据结构*************************/

//棋盘的变化状态信息
//如果有人落子则isChanged=1，index为落子的位置
public struct ChangeInfo
{
    public bool isChanged;  //棋盘改变了吗？（有人下子了吗）
    public bool isRed;      //如果改变了，是谁改变的呢？true说明是red改变的
    public int index;       //如果改变了，改变的是几号？没改变的话默认为-1
}

//玩家ID
public struct PlayerID
{
    public int redTeam;
    public int greenTeam;
}
