using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using ExitGames.Client.Photon;
using MobaCommon.Dto;
using MobaCommon.OpCode;
using UnityEngine;
using LitJson;
using MobaCommon.Config;
using System;
using Random = UnityEngine.Random;

public class FightReceiver : MonoBehaviour,IReceiver,IResourceListener
{
    private HeroModel[] Heros;
    private BuildModel[] Builds;
    private List<DogModel> Dogs;
    [Header("队伍1")] [SerializeField] private Transform team1Parent;
    [SerializeField]
    private Transform[] team1HeroPoints;
    [SerializeField]
    private GameObject[] team1Builds;
    /// <summary>
    /// 队伍1的士兵出发地点
    /// </summary>
    [SerializeField]
    private Transform[] team1DogPoints;
    /// <summary>
    /// 队伍1的士兵的父物体
    /// </summary>
    [SerializeField]
    private Transform team1DogParent;
    /// <summary>
    /// 队伍1的士兵目的地
    /// </summary>
    [SerializeField]
    private Transform team1DogTarget;
    [Header("队伍2")]
    [SerializeField]
    private Transform team2Parent;
    [SerializeField]
    private Transform[] team2HeroPoints;
    [SerializeField]
    private GameObject[] team2Builds;
    /// <summary>
    /// 队伍2的士兵出发地点
    /// </summary>
    [SerializeField]
    private Transform[] team2DogPoints;
    /// <summary>
    /// 队伍1的士兵的父物体
    /// </summary>
    [SerializeField]
    private Transform team2DogParent;
    /// <summary>
    /// 队伍1的士兵目的地
    /// </summary>
    [SerializeField]
    private Transform team2DogTarget;

    private Dictionary<int, BaseControl> idControlDict = new Dictionary<int, BaseControl>();

    [Header("视图")]
    [SerializeField]
    private FightView view;

    [Header("掉血数字")]
    [SerializeField]
    private bl_HUDText HUDText;
    /// <summary>
    /// 自己的队伍
    /// </summary>
    private int myTeam;
    public void OnReceive(byte subCode, OperationResponse response)
    {
        switch (subCode)
        {
            case OpFight.GetInfo:
                onGetInfo(
                    JsonMapper.ToObject<HeroModel[]>(response[0].ToString()),
                    JsonMapper.ToObject<BuildModel[]>(response[1].ToString()));
                break;
            case OpFight.Walk:
                onWalk((int)response[0], (float)response[1], (float)response[2], (float)response[3]);
                break;
            case OpFight.Skill:
                if (response.ReturnCode == 0)
                    onAttack((int)response[0], (int)response[1]);
                else if (response.ReturnCode == 1)
                {
                    //是技能
                    onSkill((int)response[0], (int)response[1], (int)response[2], (float)response[3], (float)response[4], (float)response[5]);
                }
                break;
            case OpFight.Damage:
                onDamage(JsonMapper.ToObject<DamageModel[]>(response[0].ToString()));
                break;
            case OpFight.Buy:
                onBuy(JsonMapper.ToObject<HeroModel>(response[0].ToString()));
                break;
            case OpFight.SkillUp:
                onSkillUp((int)response[0], JsonMapper.ToObject<SkillModel>(response[1].ToString()));
                break;
            case OpFight.UpdateModel:
                onUpdateModel(JsonMapper.ToObject<HeroModel>(response[0].ToString
                    ()));
                break;
            case OpFight.Resurge:
                if (response.ReturnCode == 0)
                    onResurge(JsonMapper.ToObject<HeroModel>(response[0].ToString()));
                else if(response.ReturnCode==1)
                    onResurge(JsonMapper.ToObject<BuildModel>(response[0].ToString()));
                break;
            case OpFight.GameOver:
                //TODO
                int winTeam = (int) response[0];
                view.GameOver(GameData.MyControl.Model.Team==winTeam);
                break;
            case OpFight.DogEnter:
                onGetDogInfo(JsonMapper.ToObject<DogModel[]>(response[0].ToString()));
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 英雄复活
    /// </summary>
    /// <param name="hero"></param>
    private void onResurge(HeroModel hero)
    {
        BaseControl con = idControlDict[hero.Id];
        con.Model = hero;
        con.OnHpChanged();
        //在里面 重置状态机 等等需要的部分
        con.ResurgeResponse();
        //重置位置
        if (hero.Team == 1)
            con.transform.position = team1HeroPoints[0].position;
        else if (hero.Team == 2)
            con.transform.position = team2HeroPoints[0].position;
        //如果是自身 就更新视图
        if (GameData.MyControl == con)
            view.UpdateView(hero);


    }
    /// <summary>
    /// 塔的复活
    /// </summary>
    /// <param name="build"></param>
    private void onResurge(BuildModel build)
    {
        BaseControl con = idControlDict[build.Id];
        //保存数据
        con.Model = build;
        con.ResurgeResponse();
    }
    /// <summary>
    /// 更新英雄数据
    /// </summary>
    /// <param name="heroModel"></param>
    private void onUpdateModel(HeroModel heroModel)
    {
        //获取控制器
        BaseControl con = idControlDict[heroModel.Id];
        //更新模型
        con.Model = heroModel;
        //如果是自身 就更新视图
        if (GameData.MyControl == con)
            view.UpdateView(heroModel);


    }
    /// <summary>
    /// 使用技能
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="attackId"></param>
    /// <param name="targetId"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="v5"></param>
    private void onSkill(int skillId, int attackId,int targetId, float x, float y, float z)
    {
        //获取攻击者的控制器
        BaseControl con = idControlDict[attackId];
        //判断技能类型
        if (targetId == -1)
        {

            con.SkillResponse(skillId, null, new Vector3(x, y, z));
        }
        else
        {
            //TODO
        }
        //如果是我们自身释放 就要刷新UI的显示
        if (con == GameData.MyControl)
        {
            view.UpdateCoolDown(skillId);
        }
    }


    /// <summary>
    /// 技能升级
    /// </summary>
    /// <param name="v"></param>
    /// <param name="skillModel"></param>
    private void onSkillUp(int playerid, SkillModel skillModel)
    {
        //先获取控制器
        BaseControl con = idControlDict[playerid];
        //遍历英雄的技能
        for (int i = 0; i < ((HeroModel) con.Model).Skills.Length;i++)
        {
            SkillModel skill = ((HeroModel)con.Model).Skills[i];
            //如果不是我们要点的技能 直接下一次循环
            if (skill.Id != skillModel.Id)
                continue;
            //如果是 直接复制就行
            ((HeroModel) con.Model).Skills[i] = skillModel;
            //刷新这个技能显示
            if (con == GameData.MyControl)
                view.UpdateSkills(((HeroModel)con.Model));
            break;
        }


    }



    /// <summary>
    /// 购买装备
    /// </summary>
    /// <param name="hero"></param>
    private void onBuy(HeroModel hero)
    {
        //获取ID
        int id = hero.Id;
        idControlDict[id].Model = hero;
        //如果是自身买装备了
        if (GameData.MyControl.Model.Id == id)
        {
            view.UpdateView(hero);
        }

    }
    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damages"></param>
    private void onDamage(DamageModel[] damages)
    {
        foreach (var item in damages)
        {
            //目标ID
            int toId = item.toId;
            //获取控制器
            BaseControl con = idControlDict[toId];
            //受伤
            con.Model.CurrHp -= item.damage;
            con.OnHpChanged();
            //实例化出来一个掉血数字
            HUDText.NewText("- " + item.damage, con.transform, Color.red, 16, 20f, -1f, 2.2f, bl_Guidance.LeftDown);
            //如果受伤的是自身 就要更新UI的显示
            if (con == GameData.MyControl)
            {
                //更新视图的显示
                view.UpdateView((HeroModel) con.Model);
                //如果死亡了 就灰白屏幕
                if (item.isDead)
                {
                    con.DeathResponse();
                }
            }
            else //别人掉血
            {
                //如果别人死亡了
                if (item.isDead)
                {
                    con.DeathResponse();
                }
            }
        }
    }
    /// <summary>
    /// 接收到普通的响应
    /// </summary>
    /// <param name="fromId"></param>
    /// <param name="toId"></param>
    private void onAttack(int fromId, int toId)
    {
        //使用者控制器
        BaseControl fromCon = idControlDict[fromId];
        //目标控制器
        BaseControl toCon = idControlDict[toId];
        //调用攻击方法
        fromCon.AttackResponse(toCon.transform);


    }


    /// <summary>
    /// 接受到移动端额响应
    /// </summary>
    /// <param name="id"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    private void onWalk(int id, float x, float y, float z)
    {
        BaseControl con = idControlDict[id];
        con.Move(new Vector3(x, y, z));
    }

    private void onGetDogInfo(DogModel[] dogs)
    {
        //要把战斗房间内的模型 先保存到本地
        #region 小兵

        GameObject go = null;
        foreach (DogModel item in dogs)
        {
            Transform target = null;
            //创建小兵的游戏物体 首先要加载预设
            if (item.Team == 1)
            {
                go = Instantiate(Resources.Load<GameObject>(Paths.RES_HERO + item.Name), team1DogPoints[0].position,
                    Quaternion.identity);
                go.transform.SetParent(team1DogParent);
                target = team1DogTarget;
            }
            else if (item.Team == 2)
            {
                go = Instantiate(Resources.Load<GameObject>(Paths.RES_HERO + item.Name), team2HeroPoints[0].position,
    Quaternion.identity);
                go.transform.SetParent(team2DogParent);
                target = team2DogTarget;
            }
            BaseControl con = go.GetComponent<BaseControl>();
            con.Init(item, item.Team == myTeam);
            con.Move(target.position);
            
            //添加到字典里
            idControlDict.Add(item.Id, con);

        }




        #endregion



    }
    /// <summary>
    /// 获取数据
    /// </summary>
    /// <param name="heros">英雄</param>
    /// <param name="builds">建筑</param>
    private void onGetInfo(HeroModel[] heros, BuildModel[] builds)
    {
        //要把战斗房间内的模型 先保存到本地
        this.Heros = heros;
        this.Builds = builds;

         myTeam = this.GetTeam(heros, GameData.Player.id);

        //创建游戏物体
        GameObject go=null;
        //英雄
        #region 英雄
        foreach (HeroModel item in heros)
        {
            //创建英雄的游戏物体 首先要加载预设
            if (item.Team == 1)
            {
                go = Instantiate(Resources.Load<GameObject>(Paths.RES_HERO + item.Name), team1HeroPoints[0].position,
                    Quaternion.identity);
                go.transform.SetParent(team1Parent);
            }
            else if (item.Team == 2)
            {
                go = Instantiate(Resources.Load<GameObject>(Paths.RES_HERO + item.Name), team2HeroPoints[0].position,
    Quaternion.identity);
                go.transform.SetParent(team2Parent);
            }
            BaseControl con = go.GetComponent<BaseControl>();
            con.Init(item, item.Team == myTeam);
            //判断这个英雄是不是自己的英雄
            if (item.Id == GameData.Player.id)
            {
                //保存当前的英雄
                GameData.MyControl = con;
                //初始化UIFight
                view.InitView(item);
                //焦距到自己的英雄
                Camera.main.GetComponent<CameraControl>().FocusOn();

            }
            //初始化控制器
            con.Init(item, item.Team == myTeam);
            //添加到字典里
            idControlDict.Add(item.Id, con);
        }
        #endregion
        //建筑
        #region 建筑
        for (int i = 0; i < builds.Length; i++)
        {
            BuildModel build = builds[i];
            if (build.Team == 1)
            {
                //Main 1 Camp 2 Turret 3
                go = team1Builds[build.TypeId - 1];
                go.SetActive(true);
            }
            else if (build.Team == 2)
            {
                go = team2Builds[build.TypeId - 1];
                go.SetActive(true);
            }
            //初始化控制器
            BaseControl con = go.GetComponent<BaseControl>();
            con.Init(build, build.Team == myTeam);
            //添加到字典里
            idControlDict.Add(build.Id, con);

        } 
        #endregion
    }
    /// <summary>
    /// 获取队伍
    /// </summary>
    /// <param name="heros"></param>
    /// <param name="playerId"></param>
    /// <returns></returns>
    private int GetTeam(HeroModel[] heros, int playerId)
    {
        foreach (var item in heros)
        {
            if (item.Id == playerId)
                return item.Team;
        }
        return -1;
    }

    public void OnLoaded(string assetName, object asset)
    {
        for (int i = 0; i < Heros.Length; i++)
        {
            //if(Paths.RES_HERO+Heros[i].Name==assetName)

        }

        switch (assetName)
        {


            default:
                break;

        }
    }
}
