using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Manager;
using Assets.Scripts.Receiver;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using MobaCommon.OpCode;
using UnityEngine;
using UnityEngine.UI;

public class PhotonManager : Singleton<PhotonManager>, IPhotonPeerListener
{
    #region
    //账号
    private AccountReceiver account;

    public AccountReceiver Account
    {
        get {
            if (account == null)
                account = FindObjectOfType<AccountReceiver>();
            return account;
        }
    }


    //角色
    public PlayerReceiver player;
    public PlayerReceiver Player
    {
        get
        {
            print(GameObject.Find("UIMain(Clone)"));
            if (player == null)
                player = FindObjectOfType<PlayerReceiver>();
            return player;
        }

    }
    //选人
    public SelectReveiver select;
    public SelectReveiver Select
    {
        get
        {
            print(GameObject.Find("UIMain(Clone)"));
            if (select == null)
                select = FindObjectOfType<SelectReveiver>();
            return select;
        }

    }

    //战斗
    public FightReceiver fight;
    public FightReceiver Fight
    {
        get
        {
            if (fight == null)
                fight = FindObjectOfType<FightReceiver>();
            return fight;
        }

    }

    #endregion

    /// <summary>
    /// 代表客户端
    /// </summary>
    private PhotonPeer peer;
    /// <summary>
    /// IP地址
    /// </summary>
    private string serverAddress = "127.0.0.1:5055";
    /// <summary>
    /// 应用名字
    /// </summary>
    private string applicationName = "MOBA";

    private ConnectionProtocol protocol = ConnectionProtocol.Udp;
    private bool isConnect = false;

    #region Photon接口
    public void DebugReturn(DebugLevel level, string message)
    {
    }
    /// <summary>
    /// 接受服务器的响应
    /// </summary>
    /// <param name="operationResponse"></param>
    public void OnOperationResponse(OperationResponse operationResponse)
    {
        Log.Debug(operationResponse.ToStringFull());
        byte opCode = operationResponse.OperationCode;
        byte subCode = (byte)operationResponse[80];
        switch (opCode)
        {
            case OpCode.AccountCode:
                Account.OnReceive(subCode, operationResponse);
                break;
            case OpCode.PlayerCode:
                Player.OnReceive(subCode, operationResponse);
                break;
            case OpCode.SelectCode:
                Select.OnReceive(subCode, operationResponse);
                break;
            case OpCode.FightCode:
                Fight.OnReceive(subCode, operationResponse);
                break;
            default:
                break;
        }
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
        Log.Debug(statusCode.ToString());
        switch (statusCode)
        {
            case StatusCode.Connect:
                isConnect = true;
                break;
            case StatusCode.Disconnect:
                isConnect = false;
                break;
            default:
                break;
        }
    }

    public void OnEvent(EventData eventData)
    {
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        peer = new PhotonPeer(this, protocol);
        peer.Connect(serverAddress, applicationName);
        //设置自身物体是跨场景存在的
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!isConnect)
        {
            peer.Connect(serverAddress, applicationName);
        }
        peer.Service();
    }

    void OnApplicationQuit()
    {
        peer.Disconnect();
    }
    /// <summary>
    /// 向服务器发请求
    /// </summary>
    /// <param name="OpCode">操作码</param>
    /// <param name="SubCode">子操作码</param>
    /// <param name="parameters">参数</param>
    public void Request(byte OpCode,byte SubCode,params object[] parameters)
    {
        Dictionary<byte, object> dict = new Dictionary<byte, object>();
        dict[80] = SubCode;
        for (byte i = 0; i < parameters.Length; i++)
        {
            dict[(byte) i] = parameters[i];
        }
        bool ispeer = peer.OpCustom(OpCode, dict, true);

    }
    

}
