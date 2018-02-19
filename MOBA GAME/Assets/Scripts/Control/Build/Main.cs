using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Control.Build
{
    /// <summary>
    /// 大本营
    /// </summary>
   public class Main:BaseControl
    {
        public override void DeathResponse()
        {
            GetComponent<Animation>().CrossFade("death");
        }
    }
}
