using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data;
using MobaCommon.Dto;
using MobaCommon.OpCode;
using UnityEngine;

public class KeyControl : MonoBehaviour
{
    [SerializeField]
    private KeyCode Skill_E = KeyCode.E;

    [SerializeField] private UISkill uiSkill_E;
    // Update is called once per frame
    void Update()
    {
        #region 当鼠标右键点击
        //
        if (Input.GetMouseButtonDown(1))
        {
            Vector2 mouse = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mouse);
            RaycastHit[] his = Physics.RaycastAll(ray);
            for (int i = his.Length - 1; i >= 0; i--)
            {
                RaycastHit hit = his[i];
                //如果点到了地面 那就移动
                if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Ground")))
                {
                    move(hit.point);
                }
                //如果点到了敌方单位 那就攻击
                else if (hit.collider.gameObject.layer.Equals(LayerMask.NameToLayer("Enemy")))
                {
                    attack(hit.collider.gameObject);
                    //如果攻击到了敌方 就不往下继续判断了 以为不需要行走了
                    break;
                }
            }
        }
        #endregion

        #region 空格

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //焦距到自己的英雄
            Camera.main.GetComponent<CameraControl>().FocusOn();
        }


        #endregion

        #region 技能释放

        if (Input.GetKeyDown(Skill_E)&&uiSkill_E.CanUse)
        {
            Vector2 mouse = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //释放技能
                skill(3, hit.point);
            }

        }
        #endregion
    }
    /// <summary>
    /// 释放技能
    /// </summary>
    /// <param name="index"></param>
    /// <param name="target"></param>
    private void skill(int index, Vector3 targetPos)
    {
        HeroModel myHero = (HeroModel)GameData.MyControl.Model;
        int skillId = myHero.Skills[index - 1].Id;
        int attackId = myHero.Id;
        //向服务器发起请求 参数：1.技能的Id 2.攻击者id 3.目标Id 4.目标点的坐标
        PhotonManager.Instance.Request(OpCode.FightCode, OpFight.Skill, skillId, attackId, -1, targetPos.x,targetPos.y,targetPos.z);
    }
    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="point"></param>
    private void move(Vector3 point)
    {
        //显示一个特效
        GameObject go = PoolManager.Instance.GetObject("ClickMove");
        go.transform.position = point + Vector3.up;
        //给服务器发一个请求 参数：点的坐标
        PhotonManager.Instance.Request(OpCode.FightCode, OpFight.Walk, point.x, point.y, point.z);
    }
    /// <summary>
    /// 攻击
    /// </summary>
    /// <param name="target">目标</param>
    private void attack(GameObject target)
    {
        //获取目标的Id
        int targetId = target.GetComponent<BaseControl>().Model.Id;
        //int myId = GameData.MyControl.Model.Id;
        //向服务器发起请求 参数：1.技能的Id 2.攻击者id 3.目标Id
        int attackId = GameData.MyControl.Model.Id;
        PhotonManager.Instance.Request(OpCode.FightCode, OpFight.Skill ,1,attackId,targetId,-1f,-1f,-1f);
    }

}
