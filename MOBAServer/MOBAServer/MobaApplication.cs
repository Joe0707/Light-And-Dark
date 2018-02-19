﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net;
using log4net.Config;
using Photon.SocketServer;
using LogManager = ExitGames.Logging.LogManager;

namespace MOBAServer
{
   public class MobaApplication:ApplicationBase
    {
       protected override PeerBase CreatePeer(InitRequest initRequest)
       {
           return new MobaClient(initRequest);
       }

       protected override void Setup()
       {
           InitLogging();
       }
        protected override void TearDown()
        {
        }
        #region 日志功能

       private static readonly ILogger log = ExitGames.Logging.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected virtual void InitLogging()
        {
            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationRootPath, "log");
            GlobalContext.Properties["LogFileName"] = this.ApplicationName;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(this.BinaryPath, "log4net.config")));
        }
        /// <summary>
        /// 输出
        /// </summary>
        /// <param name="text"></param>
       public static void LogInfo(string text)
       {
           log.Info(text);
       }


        #endregion

    }
}