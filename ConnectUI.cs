using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ConnectUI : MonoBehaviour {

    //用于接收设定ip的字符串
    public string inputIP;

    //设定端口
    public int inputPort;

    //获取网络管理器组件
    public NetworkManager manager;

    //调节GUI的位置
    public int UI_x = 180;

    public int UI_y = 100;

    //是否处于联网状态下
    private bool isHaveNetworkRole = false;

    ////是否显示GUI
    private bool showGUI = false;

    // Use this for initialization
    void Start ()
    {
        //UI_x = 180;
        //UI_y = 100;
        inputIP = "127.0.0.1";      //输入ip
        //manager.networkPort = 8989; //端口
        manager = this.GetComponent<NetworkManager>();
        //manager.StartHost();
    }

    //帧调用
    private void Update()
    {
        
    }
    
    //更新GUI
    private void OnGUI()
    {
        //输入框
        inputIP = GUI.TextArea(new Rect(UI_x, UI_y + 80, 100, 24), inputIP);
        manager.networkAddress = inputIP;

        if (isHaveNetworkRole)
        {
            //断开连接的按钮
            if (GUI.Button(new Rect(UI_x, UI_y - 12, 100, 24), "断开连接"))
            {

                manager.StopServer();
                manager.StopClient();
                OnDisconnected(null);
                
                SceneManager.LoadScene("ConnectScene");
            }
            return;
        }

        

        //创建服务器端的按钮
        if (GUI.Button(new Rect(UI_x, UI_y - 12, 100, 24), "创建房间"))
        {
            isHaveNetworkRole = true;
            manager.StartHost();
        }

        //创建客户端的按钮
        if (GUI.Button(new Rect(UI_x, UI_y + 50, 100, 24), "加入房间"))
        {
            var client = manager.StartClient();
            client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            isHaveNetworkRole = true;
        }

    }


    //断开连接时执行的
    private void OnDisconnected(NetworkMessage msg)
    {
        isHaveNetworkRole = false;
        Application.LoadLevel(Application.loadedLevel);
    }
}
