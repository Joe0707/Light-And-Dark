using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendView : MonoBehaviour
{
    [SerializeField]
    public int id;
    [SerializeField]
    private Text txtName;

    [SerializeField]
    private Text txtState;
    [SerializeField]
    private Image imgBg;
    /// <summary>
    /// 更新显示
    /// </summary>
    public void InitView(int id, string name, bool isOnline)
    {
        this.id = id;
        txtName.text = name;
        string state = isOnline ? "在线" : "离线";
        txtState.text = "状态：" + state;
        imgBg.color = isOnline ? Color.blue : Color.red;
    }

    public void UpdateView(bool isOnline)
    {
        string state = isOnline ? "在线" : "离线";
        txtState.text = "状态：" + state;
        imgBg.color = isOnline ? Color.blue : Color.red;

    }
}
