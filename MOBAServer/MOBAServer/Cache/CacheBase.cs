﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MOBAServer.Room;

namespace MOBAServer.Cache
{
    public class RoomCacheBase<TRoom>where TRoom:Room.RoomBase<MobaClient>
    {
        /// <summary>
        /// 房间ID对应的房间数据
        /// </summary>
        protected ConcurrentDictionary<int, TRoom> idRoomDict = new ConcurrentDictionary<int, TRoom>();

        /// <summary>
        /// 玩家ID对应的房间ID
        /// </summary>
        protected ConcurrentDictionary<int, int> playerRoomDict = new ConcurrentDictionary<int, int>();

        //重用的队列
        protected ConcurrentQueue<TRoom> roomQue = new ConcurrentQueue<TRoom>();

        /// <summary>
        /// 主键ID
        /// </summary>
        protected int index = 0;
    }
}
