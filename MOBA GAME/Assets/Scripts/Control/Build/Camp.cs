using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Control.Build
{
    public class Camp:BaseControl
    {
        public override void DeathResponse()
        {
            gameObject.SetActive(false);
        }
        /// <summary>
        /// 复活
        /// </summary>
        public override void ResurgeResponse()
        {
            this.gameObject.SetActive(true);
        }

    }
}
