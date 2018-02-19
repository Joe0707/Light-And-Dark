using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobaCommon.OpCode;
using MOBAServer.Logic;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;

namespace MOBAServer
{
    public class MobaClient:ClientPeer
    {
        //账号逻辑
        private AccountHandler account;
        private PlayerHandler player;
        private SelectHandler select;
        private FightHandler fight;
        public MobaClient(InitRequest initRequest) : base(initRequest)
        {
            account = new AccountHandler();
            player = new PlayerHandler();
            select = new SelectHandler();
            fight = new FightHandler();
            player.StartSelectEvent += select.StartSelect;
            select.StartFightEvent = fight.StartFight;
        }
        /// <summary>
        /// 客户端发起请求
        /// </summary>
        /// <param name="operationRequest"></param>
        /// <param name="sendParameters"></param>
        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            MobaApplication.LogInfo((account.cache == player.accountCache )+ "是否相同");
            byte opCode = operationRequest.OperationCode;
            byte subCode = (byte)operationRequest[80];
            switch (opCode)
            {
                case OpCode.AccountCode:
                    account.OnRequest(this, subCode, operationRequest);
                    break;
                case OpCode.PlayerCode:
                    player.OnRequest(this, subCode, operationRequest);
                    break;
                case OpCode.SelectCode:
                    select.OnRequest(this, subCode, operationRequest);
                    break;
                case OpCode.FightCode:
                    fight.OnRequest(this, subCode, operationRequest);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 客户端断开连接
        /// </summary>
        /// <param name="reasonCode"></param>
        /// <param name="reasonDetail"></param>
        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            fight.OnDisconnect(this);
            select.OnDisconnect(this);
            player.OnDisconnect(this);
            account.OnDisconnect(this);
        }
    }
}
