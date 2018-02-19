﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Manager.UI
{
    /// <summary>
    /// UI管理类
    /// </summary>
    public class UIManager : Singleton<UIManager>,IResourceListener
    {
        public GameObject UIMain;
        /// <summary>
        /// UI名字和面板的映射关系
        /// </summary>
        private Dictionary<string, UIBase> nameUIDict = new Dictionary<string, UIBase>();
        /// <summary>
        /// 添加UI
        /// </summary>
        /// <param name="ui"></param>
        public void AddUI(UIBase ui)
        {
            if (ui == null)
                return;
            nameUIDict.Add(ui.UIName(), ui);
        }
        /// <summary>
        /// 删除UI
        /// </summary>
        /// <param name="ui"></param>
        public void RemoveUI(UIBase ui)
        {
            if (ui == null)
                return;
            if (!nameUIDict.ContainsValue(ui))
                return;
            nameUIDict.Remove(ui.UIName());
        }
        /// <summary>
        /// 显示UI
        /// </summary>
        public void ShowUIPanel()
        {
            GameObject uiPrefab = Instantiate(UIMain as GameObject);
            UIBase ui = uiPrefab.GetComponent<UIBase>();
            ui.OnShow();
            AddUI(ui);
        }
        /// <summary>
        /// 显示UI 没有就创建一个
        /// </summary>
        /// <param name="uiName"></param>
        public void ShowUIPanel(string uiName)
        {
            if (nameUIDict.ContainsKey(uiName))
            {
                UIBase ui = nameUIDict[uiName];
                ui.OnShow();
                return;
            }
            else
            ResourcesManager.Instance.Load(uiName, typeof (GameObject), this);
        }

        public void OnLoaded(string assetName, object asset)
        {
            GameObject uiPrefab = Instantiate(asset as GameObject);
            UIBase ui = uiPrefab.GetComponent<UIBase>();
            ui.OnShow();
            AddUI(ui);
        }
        /// <summary>
        /// 关闭UI
        /// </summary>
        /// <param name="uiName"></param>
        public void HideUIPanel(string uiName)
        {
            if (!nameUIDict.ContainsKey(uiName))
                return;
                UIBase ui = nameUIDict[uiName];
            ui.OnHide();
        }
    }
}
