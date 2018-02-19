using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogCheck : MonoBehaviour {

    /// <summary>
    /// 表示当前小兵的队伍
    /// </summary>
    private int team;
    /// <summary>
    /// 检测到的敌人列表
    /// </summary>
    public List<BaseControl> conList = new List<BaseControl>();

    public void SetTeam(int team)
    {
        this.team = team;
    }

    void OnTriggerExit(Collider other)
    {
        BaseControl con = other.GetComponent<BaseControl>();
        if (con && conList.Contains(con))
        {
             conList.Remove(con);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        BaseControl con = other.GetComponent<BaseControl>();
        if(con!=null)
        if (con && con.Model.Team != team)
        {
                if(!conList.Contains(con))
            conList.Add(con);
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
