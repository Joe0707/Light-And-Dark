using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Manager;
using Assets.Scripts.Manager.UI;
using MobaCommon.Dto;
using MobaCommon.OpCode;
using UnityEngine;
using UnityEngine.UI;

public class MainView : UIBase,IResourceListener
{
    private AudioClip acClick;
    [HideInInspector]
    public AudioClip acCountDown;
    #region UIBASE
    public override void Init()
    {
        acClick = ResourcesManager.Instance.GetAsset(Paths.GetSoundFullName("Click")) as AudioClip;
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "CountDown", typeof(AudioClip), this);
        //添加监听
        btnCreate.onClick.AddListener(OnCreateClick);

    }

    public void OnLoaded(string assetName, object asset)
    {
        if (assetName == Paths.RES_SOUND_UI + "CountDown")
            acCountDown = asset as AudioClip;
    }

    public override void OnShow()
    {
        base.OnShow();
        PhotonManager.Instance.Request(OpCode.PlayerCode, OpPlayer.GetInfo);
    }
    public override void OnDestroy()
    {
    }

    public override string UIName()
    {
        return UIdefinit.UIMain;
    }
    #endregion
    #region 创建模块
    [Header("创建模块")]
    [SerializeField]
    private InputField inName;
    [SerializeField]
    public Button btnCreate;

    [SerializeField]
    private GameObject createPanel;

    /// <summary>
    /// 创建按钮的可点状态
    /// </summary>
    public bool CreateInteractable
    {
        set { btnCreate.interactable = value; }
    }
    /// <summary>
    /// 创建面板可见
    /// </summary>
    public bool CreatePanelActive
    {
        set
        {
            createPanel.SetActive(value);
        }
    }

    public void OnCreateClick()
    {
        //播放声音
        SoundManager.Instance.PlayEffectMusice(acClick);
        //输入检测
        if (string.IsNullOrEmpty(inName.text))
            return;
        //发起创建请求
        PhotonManager.Instance.Request(OpCode.PlayerCode, OpPlayer.Create, inName.text);
        //禁用按钮
        CreateInteractable = false;
    }


    #endregion
    [Header("角色信息")]
    [SerializeField]
    private Text txtName;
    [SerializeField]
    private Slider barExp;


    /// <summary>
    /// 更新显示
    /// </summary>
    public void UpdateView(PlayerDto player)
    {
        txtName.text = player.name;
        barExp.value = (float)player.exp / (player.lv * 100);
        //加载好友列表
        Friend[] friends = player.friends;
        friendList.Clear();
        GameObject go = null;
        foreach (Friend item in friends)
        {
            if (item == null) continue;
            go = Instantiate(UIFriend);
            go.transform.SetParent(friendTran);
            FriendView fv = go.GetComponent<FriendView>();
            fv.InitView(item.Id, item.Name, item.isOnline);
            friendList.Add(fv);
        }
    }
    #region 好友模块
    [Header("好友信息的预设")]
    [SerializeField]
    private GameObject UIFriend;
    [SerializeField]
    private InputField inAddName;
    [SerializeField]
    private Transform friendTran;

    private List<FriendView> friendList = new List<FriendView>();
    /// <summary>
    /// 点击添加好友按钮
    /// </summary>
    public void onAddClick()
    {
        //播放声音
        SoundManager.Instance.PlayEffectMusice(acClick);
        //输入检测
        if (string.IsNullOrEmpty(inAddName.text))
            return;
        //发送添加好友请求
        PhotonManager.Instance.Request(OpCode.PlayerCode, OpPlayer.RequestAdd, inAddName.text);
    }
    [SerializeField]
    private ToClientAddView toClientAddPanel;
    /// <summary>
    /// 显示加好友面板
    /// </summary>
    public void ShowToClientAddPanel(PlayerDto dto)
    {
        toClientAddPanel.gameObject.SetActive(true);
        toClientAddPanel.UpdateView(dto);
    }
    /// <summary>
    /// 添加结果点击事件
    /// </summary>
    /// <param name="res"></param>
    public void OnResClick(bool result)
    {
        int id = toClientAddPanel.GetComponent<ToClientAddView>().id;
        PhotonManager.Instance.Request(OpCode.PlayerCode, OpPlayer.ToClientAdd, result,
           id);
        toClientAddPanel.gameObject.SetActive(false);

    }
    /// <summary>
    /// 更新好友界面
    /// </summary>
    /// <param name="friendId"></param>
    /// <param name="isOnline"></param>
    public void UpdateFriendView(int friendId, bool isOnline)
    {
        foreach (FriendView item in friendList)
        {
            if (item.id == friendId)
            {
                item.UpdateView(isOnline);
            }
        }
    }
    
    #endregion
    #region 匹配模块
    [Header("匹配模块")]
    [SerializeField]
    private Button btnOne;
    [SerializeField]
    private Button btnTwo;
    [SerializeField]
    private MatchView matchView;
    /// <summary>
    /// 单人匹配
    /// </summary>
    public void OnOneMatch()
    {
        //播放声音
        SoundManager.Instance.PlayEffectMusice(acClick);
        //发起请求
        int id = GameData.Player.id;
        PhotonManager.Instance.Request(OpCode.PlayerCode, OpPlayer.StartMatch, id);
        //显示匹配面板
        matchView.StartMatch();
        //禁用按钮
        btnOne.interactable = false;
        btnTwo.interactable = false;
    }

    public void onStopMatch()
    {
        //播放声音
        SoundManager.Instance.PlayEffectMusice(acClick);
        //发起请求
        int id = GameData.Player.id;
        PhotonManager.Instance.Request(OpCode.PlayerCode, OpPlayer.StopMatch, id);
        //显示匹配面板
        matchView.StopMatch();
        //禁用按钮
        btnOne.interactable = true;
        btnTwo.interactable = true;
    }
    /// <summary>
    /// 多人匹配
    /// </summary>
    /// <param name="id"></param>
    public void OnTwoMatch(int[] ids)
    {
        PhotonManager.Instance.Request(OpCode.PlayerCode, OpPlayer.StartMatch, ids);
        
    }
    #endregion

    
}
