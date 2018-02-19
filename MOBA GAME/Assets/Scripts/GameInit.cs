using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Manager.UI;
using UnityEngine;

/// <summary>
/// 游戏初始化脚本
/// </summary>
public class GameInit : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
        //加载登录UI
	    UIManager.Instance.ShowUIPanel(UIdefinit.UIAccount);
	}
	
}
