using System.Collections;
using System.Collections.Generic;
using MobaCommon.OpCode;
using UnityEngine;

public class UIItem : MonoBehaviour
{
    [SerializeField]
    private int Id;
    /// <summary>
    /// 向服务器发起购买的请求
    /// </summary>
    public void OnClick()
    {
        PhotonManager.Instance.Request(OpCode.FightCode, OpFight.Buy, this.Id);
    }
}
