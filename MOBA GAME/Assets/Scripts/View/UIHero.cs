using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Manager;
using MobaCommon.Config;
using MobaCommon.Dto;
using MobaCommon.OpCode;
using UnityEngine;
using UnityEngine.UI;

public class UIHero : MonoBehaviour, IResourceListener
{
    [SerializeField]
    private Image imgHead;
    [SerializeField]
    private Button btnHead;
    [SerializeField]
    private int HeroId;
    public int Id { get { return HeroId; } }
    [SerializeField]
    private string heroName;
    [SerializeField]
    private AudioClip ac;
    private AudioClip acSelect;

    /// <summary>
    /// 初始化视图
    /// </summary>
    public void InitView(HeroDataModel hero)
    {
        //保存ID
        this.HeroId = hero.TypeId;
        this.heroName = hero.Name;
        //加载音效文件
        ResourcesManager.Instance.Load(Paths.RES_SOUND_SELECT + hero.Name, typeof(AudioClip), this);
        ResourcesManager.Instance.Load(Paths.RES_SOUND_UI + "Select", typeof(AudioClip), this);
        //更新图片
        string assetName = Paths.RES_HEAD + HeroData.GetHeroData(hero.TypeId).Name;
        ResourcesManager.Instance.Load(assetName, typeof(Sprite), this);
    }

    public void OnLoaded(string assetName, object asset)
    {
        if (assetName == Paths.RES_SOUND_SELECT + heroName)
            this.ac = asset as AudioClip;
        else if (assetName == Paths.RES_HEAD + heroName)
            this.imgHead.sprite = asset as Sprite;
        else if (assetName == Paths.RES_SOUND_UI + "Select")
            acSelect = asset as AudioClip;
    }
    /// <summary>
    /// 英雄是否可选择
    /// </summary>
    public bool Interactable
    {
        get { return btnHead.interactable; }
        set { btnHead.interactable = value; }
    }
    /// <summary>
    /// 选择英雄事件
    /// </summary>
    public void onClick()
    {
        //播放音乐
        //SoundManager.Instance.PlayEffectMusice(acSelect);
        //播放音乐
        SoundManager.Instance.PlayEffectMusice(ac);
        //发起选人的请求
        PhotonManager.Instance.Request(OpCode.SelectCode, OpSelect.Select, HeroId);

    }
}
