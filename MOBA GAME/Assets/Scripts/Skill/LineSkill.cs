﻿using System.Collections;
using System.Collections.Generic;
using MobaCommon.OpCode;
using UnityEngine;

public class LineSkill : MonoBehaviour
{
    /// <summary>
    /// 技能的使用者
    /// </summary>
    private Transform user;
    /// <summary>
    /// 距离
    /// </summary>
    private float distance;
    /// <summary>
    /// 已经移动的距离
    /// </summary>
    private float currDistance;
    /// <summary>
    /// 速度
    /// </summary>
    private float speed;
    /// <summary>
    /// 技能ID
    /// </summary>
    private int skillId;
    /// <summary>
    /// 攻击者ID
    /// </summary>
    private int attackId;
    /// <summary>
    /// 是否需要发送
    /// </summary>
    private bool send;

    public void Init(Transform user, float distance, float speed, int skillId, int attackId, bool send)
    {
        this.user = user;
        this.transform.position = user.position;
        this.transform.rotation = user.rotation;
        this.distance = distance;
        this.speed = speed;
        this.skillId = skillId;
        this.send = send;
        this.attackId = attackId;
        this.send = send;
        this.currDistance = 0;
    }

    private List<int> idList = new List<int>();
    void OnTriggerEnter(Collider other)
    {
        if (send)
            send = false;

        //计算伤害的一部分
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            idList.Add(other.GetComponent<BaseControl>().Model.Id);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (user == null)
            return;
        Vector3 translation = Vector3.forward*speed*Time.deltaTime;
        transform.Translate(translation);
        currDistance += translation.z;
        //达到距离 就隐藏掉
        if (currDistance >= distance)
            PoolManager.Instance.HideObjet(gameObject);
        if (send) { 
            send = false;
        //发送伤害
            //PhotonManager.Instance.Request(OpCode.FightCode, OpFight.Damage, attackId, skillId, idList.ToArray());
        }
    }
}
