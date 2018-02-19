using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using MobaCommon.OpCode;
using UnityEngine;

/// <summary>
/// 炮塔的控制器
/// </summary>
public class Turret : BaseControl
{
    [SerializeField]
    private TurretCheck check;
    /// <summary>
    /// 攻击点
    /// </summary>
    [SerializeField]
    private Transform atkPoint;
    /// <summary>
    /// 是否是已方单位
    /// </summary>
    private bool isFriend;
    /// <summary>
    /// 攻击间隔
    /// </summary>
    private float intevalTime = 3f;
    /// <summary>
    /// 计时器
    /// </summary>
    private float timer = 3f;
    protected override void Start()
    {
        base.Start();
        //赋值队伍信息
        check.SetTeam(Model.Team);
        isFriend = GameData.MyControl.Model.Team == Model.Team;
    }

    public override void AttackResponse(params Transform[] target)
    {
        //向目标发一个攻击特效 碰到敌人之后 再计算伤害
        GameObject go = PoolManager.Instance.GetObject("atkTurrent");
        go.transform.position = atkPoint.position;
        int attackId = Model.Id;
        int targetId = target[0].GetComponent<BaseControl>().Model.Id;
        go.GetComponent<TargetSkill>().Init(target[0], 1, attackId, targetId, isFriend);
    }

    public override void DeathResponse()
    {
        this.gameObject.SetActive(false);
    }

    public override void ResurgeResponse()
    {
        this.gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();

        if (!isFriend)
            return;

        //先检测有没有目标
        if (target == null)
        {
            if (check.conList.Count == 0)
                return;
            this.target = check.conList[0];
        }
        //检测目标有没有死亡
        if (target.Model.CurrHp <= 0)
        {
            check.conList.Remove(target);
            this.target = null;
            return;
        }
        //判断一下攻击距离
        float d = Vector3.Distance(transform.position
            , target.transform.position);
        if (d >= Model.AttackDistance)
        {
            target = null;
            //重置攻击事件
            timer = intevalTime;
            return;
        }
        //开始攻击
        if (timer <= intevalTime)
        {
            timer += Time.deltaTime;
            return;
        }
        //向服务器发起攻击的请求
        int attackId = Model.Id;
        int targetId = target.Model.Id;
        PhotonManager.Instance.Request(OpCode.FightCode, OpFight.Skill, 1, attackId, targetId,-1f,-1f,-1f);
        //重置计时器
        timer = 0f;
    }

}
