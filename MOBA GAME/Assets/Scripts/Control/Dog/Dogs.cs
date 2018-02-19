using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Data;
using MobaCommon.OpCode;
using UnityEngine;

namespace Assets.Scripts.Control.Dog
{
    public class Dogs : BaseControl
    {
        
        /// <summary>
        /// 是否是已方单位
        /// </summary>
        private bool isFriend;
        /// <summary>
        /// 检测的脚本
        /// </summary>
        [SerializeField]
        private DogCheck check;
        /// <summary>
        /// 计时器
        /// </summary>
        private float timer = 3f;
        /// <summary>
        /// 攻击间隔
        /// </summary>
        private float intevalTime = 3f;
        /// <summary>
        /// 攻击的发起点
        /// </summary>
        [SerializeField]
        private Transform atkPoint;

        public override void RequestAttack()
        {
            //向目标发一个攻击特效 碰到敌人之后 再计算伤害
            GameObject go = PoolManager.Instance.GetObject("atkTurrent");
            go.transform.position = atkPoint.position;
            int attackId = Model.Id;
            int targetId = target.GetComponent<BaseControl>().Model.Id;
            go.GetComponent<TargetSkill>().Init(target.transform, 1, attackId, targetId, isFriend);

        }
        public override void AttackResponse(params Transform[] target)
        {
            print("开始攻击");
            this.target = target[0].GetComponent<BaseControl>();
            if (state == AnimState.DEATH)
                return;
            state = AnimState.ATTACK;
            //停止寻路
            agent.Stop();
            //一定要面向目标
            transform.LookAt(target[0].transform);
            //播放动画
            animControl.Attack();
            //改变状态
            state = AnimState.FREE;
        }

        public override void DeathResponse()
        {
            //停止寻路
            agent.Stop();
            //播放动画
            animControl.Death();
            //改变状态
            state = AnimState.DEATH;
            Destroy(gameObject, 5);
            ////播放声音
            //PlayAudio("Death");
        }
        protected override void Start()
        {
            base.Start();
            //赋值队伍信息
            if (check == null) return;
            check.SetTeam(Model.Team);
            isFriend = GameData.MyControl.Model.Team == Model.Team;
        }
        protected override void Update()
        {
            base.Update();
            if (!isFriend||state==AnimState.DEATH)
                return;
            //先检测有没有目标
            if (target == null)
            {
                if (check.conList.Count == 0)
                {
                    if (state != AnimState.WALK)
                    {
                        Move(des);
                    }
                    return;
                }
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
            PhotonManager.Instance.Request(OpCode.FightCode, OpFight.Skill, 1, attackId, targetId, -1f, -1f, -1f);
            //重置计时器
            timer = 0f;
        }
    }
}
