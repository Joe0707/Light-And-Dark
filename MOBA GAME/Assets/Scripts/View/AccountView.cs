using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Manager;
using Assets.Scripts.Manager.UI;
using MobaCommon.Dto;
using MobaCommon.OpCode;
using UnityEngine;
using UnityEngine.UI;
using LitJson;

public class AccountView : UIBase, IResourceListener
{
    private AudioClip bgClilp;
    private AudioClip enterClip;
    private AudioClip acClick;
    public void OnLoaded(object asset)
    {
        Instantiate(asset as GameObject);
    }
    private void LoadAudio()
    {
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "英雄", typeof(AudioClip), this);
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "EnterGame", typeof(AudioClip), this);
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "Click", typeof(AudioClip), this);
    }
    public void OnLoaded(string assetName, object asset)
    {
        AudioClip clip = asset as AudioClip;
        switch (assetName)
        {
            case Paths.RES_SOUND_UI + "英雄":
                bgClilp = clip;
                SoundManager.Instance.PlayBgMusic(bgClilp);
                break;
            case Paths.RES_SOUND_UI + "EnterGame":
                enterClip = clip;
                break;
            case Paths.RES_SOUND_UI + "Click":
                acClick = clip;
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 播放点击音效
    /// </summary>
    public void PlayClickAudio()
    {
        SoundManager.Instance.PlayEffectMusice(acClick);
    }
    #region 注册模块
    [Header("注册模块")]
    [SerializeField]
    private InputField inAcc4Register;
    [SerializeField]
    private InputField inPwd4Register;
    [SerializeField]
    private InputField inPwd4Repeat;

    public void ResetRegisterPanel()
    {
        inAcc4Register.text = "";
        inPwd4Register.text = "";
        inPwd4Repeat.text = "";
    }
    /// <summary>
    /// 注册的点击事件
    /// </summary>
    public void OnRegisterClick()
    {
        if (string.IsNullOrEmpty(inAcc4Register.text) || string.IsNullOrEmpty(inPwd4Register.text) || !inPwd4Repeat.text.Equals(inPwd4Register.text))
        {
            return;
        }
        ////播放声音
        //SoundManager.Instance.PlayEffectMusice(enterClip);

        string Account = inAcc4Register.text;
        string Password = inPwd4Register.text;
        //1 account 2 password
        PhotonManager.Instance.Request(OpCode.AccountCode, OpAccount.Register, Account, Password);
    }
    #endregion
    #region 登录模块
    [Header("登录模块")]
    [SerializeField]
    private InputField inAcc4Login;
    [SerializeField]
    private InputField inPwd4Login;
    [SerializeField]
    private Button btnEnter;
    /// <summary>
    /// 进入按钮是否可以点击
    /// </summary>
    public bool EnterInteractable
    {

        set { btnEnter.interactable = value; }
    }

    public void OnResetPanel()
    {
        inAcc4Register.text = string.Empty;
        inPwd4Register.text = string.Empty;
        inPwd4Repeat.text = string.Empty;
    }
    /// <summary>
    /// 进入游戏点击事件
    /// </summary>
    public void OnEnterClick()
    {
        if (string.IsNullOrEmpty(inAcc4Login.text) || string.IsNullOrEmpty(inPwd4Login.text))
        {
            //提示
            return;
        }
        //创建传输模型
        AccountDto dto = new AccountDto()
        {
            Account = inAcc4Login.text,
            Password = inPwd4Login.text
        };
        //发送
        PhotonManager.Instance.Request(OpCode.AccountCode, OpAccount.Login, JsonMapper.ToJson(dto));
        //设置不可点击状态
        EnterInteractable = false;
    }
    /// <summary>
    /// 重置登录面板的输入
    /// </summary>
    public void ResetLoginInput()
    {
        inAcc4Login.text = string.Empty;
        inPwd4Login.text = string.Empty;
    }


    #endregion
    #region UIBase
    public override string UIName()
    {
        return UIdefinit.UIAccount;
    }
    public override void Init()
    {
        print(gameObject.name);
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "英雄", typeof(AudioClip), this);
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "EnterGame", typeof(AudioClip), this);
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "Click", typeof(AudioClip), this);
    }

    public override void OnShow()
    {
        //角色已登录 就不用再次登录了
        if (GameData.Player != null)
        {
            UIManager.Instance.HideUIPanel(UIdefinit.UIAccount);
            UIManager.Instance.ShowUIPanel();
        }
    }



    public override void OnDestroy()
    {
         bgClilp=null;
     enterClip = null;
     acClick = null;
}

    #endregion
}
