using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Resource
{
    /// <summary>
    /// 资源类
    /// </summary>
   public class LoadAsset
    {
        /// <summary>
        /// 资源信息
        /// </summary>
        public ResourceRequest request;
        /// <summary>
        /// 资源名称
        /// </summary>
        public string AssetName;
        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType;
        /// <summary>
        /// 是否加载完成
        /// </summary>
        public bool IsDone
        {
            get { return request != null && request.isDone; }
        }
        /// <summary>
        /// 获取资源
        /// </summary>
        public object GetAsset
        {
            get
            {
                if (request == null) return null;
                return request.asset;
            }
        }

        public void LoadAsync()
        {
           request = Resources.LoadAsync(AssetName, AssetType);
        }
        /// <summary>
        /// 回调的集合
        /// </summary>
        public List<IResourceListener> Listeners;

        public void AddListener(IResourceListener listener)
        {
            if (Listeners == null)
            {
                Listeners = new List<IResourceListener>();
            }
            if (Listeners.Contains(listener))
                return;
            Listeners.Add(listener);
        }


    }
}
