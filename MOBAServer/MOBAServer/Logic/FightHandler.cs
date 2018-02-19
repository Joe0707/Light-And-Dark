using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobaCommon.Dto;
using MobaCommon.OpCode;
using MOBAServer.Cache;
using MOBAServer.Room;
using Photon.SocketServer;
using LitJson;
using MobaCommon.Config;
using MobaCommon.Dto.Skill;
using MOBAServer.Model;

namespace MOBAServer.Logic
{
    
    public class FightHandler: SingleSend,IOpHandler
    {

        #region 缓存层
        public FightCache fightCache
        {
            get
            {
                return Caches.Fight;
            }
        }

        public PlayerCache playerCache
        {
            get { return Caches.Player; }
        }
        #endregion
        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="team1"></param>
        /// <param name="team2"></param>
        public void StartFight(List<SelectModel> team1, List<SelectModel> team2)
        {
            fightCache.CreateRoom(team1, team2);
        }

        public void OnRequest(MobaClient client, byte subCode, OperationRequest request)
        {
            switch (subCode)
            {
                case OpFight.Enter:
                    onEnter(client, (int)request[0]);
                    break;
                case OpFight.Walk:
                    onWalk(client, (float)request[0], (float)request[1], (float)request[2]);
                    break;
                case OpFight.Skill:
                    onSkill(client, (int)request[0], (int)request[1], (int)request[2],(float)request[3],(float)request[4],(float)request[5]);
                    break;
                case OpFight.Damage:
                    onDamage(client, (int)request[0], (int)request[1], (int[])request[2]);
                    break;
                //买装备 服务器收到的请求参数：装备的ID
                case OpFight.Buy:
                    onBuy(client, (int)request[0]);
                    break;
                case OpFight.Sale:
                    onSale(client, (int)request[0]);
                    break;
                case OpFight.SkillUp:
                    onSkillUp(client, (int) request[0]);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// 技能升级
        /// </summary>
        /// <param name="client"></param>
        /// <param name="v"></param>
        private void onSkillUp(MobaClient client, int skillId)
        {
            //1.获取房间模型
            int playerid = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerid);
            if (room == null)
                return;
            //获取英雄数据模型
            HeroModel hero = room.GetHeroModel(playerid);
            if (hero == null)
                return;
            //没有技能点数了 就不作处理
            if (hero.Points <= 0)
                return;
            //可以加点
            foreach (var item in hero.Skills)
            {
                if (item.Id != skillId)
                    continue;
                //如果玩家等级没有到达 技能学习的要求 或者技能已满级
                if (item.LearnLevel > hero.Level||item.LearnLevel==-1)
                    return;
                //扣点数
                hero.Points--;
                //先获取技能下一级的数据
                SkillLevelDataModel data = SkillData.GetSkillData(skillId).LvModels[++item.Level];
                //修改技能
                item.LearnLevel = data.LearnLv;
                item.Distance = data.Distance;
                item.CoolDown = data.CoolDown;
                //等等 在这里修改想修改的数据
                //广播 谁 更新了 什么技能
                room.Brocast(OpCode.FightCode, OpFight.SkillUp, 0, "有人点技能了", null, playerid,JsonMapper.ToJson(item));
                return;
            }
           


        }


        /// <summary>
        /// 卖装备
        /// </summary>
        /// <param name="client"></param>
        /// <param name="itemId"></param>
        private void onSale(MobaClient client, int itemId)
        {
            //检测有没有装备
            ItemModel item = ItemData.GetItem(itemId);
            if (item == null)
                return;
            //1.获取房间模型
            int playerid = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerid);
            if (room == null)
                return;
            //获取英雄数据模型
            HeroModel hero = room.GetHeroModel(playerid);
            for (int i = 0; i < hero.Equipments.Length; i++)
            {
                if (hero.Equipments[i] == itemId)
                {
                    //开始出售
                    hero.Money += item.Price;
                    //给他装备的ID
                    hero.Equipments[i] = -1;
                    //增加属性
                    hero.Attack -= item.Attack;
                    hero.Defense -= item.Defense;
                    hero.MaxHp -= item.Hp;
                    //给房间内所有客户端发消息了 HeroModel
                    room.Brocast(OpCode.FightCode, OpFight.Buy, 0, "有人出售装备了", null, JsonMapper.ToJson(hero));
                    return;
                }
            }
            //走到这里就代表出售失败了
            Send(client, OpCode.FightCode, OpFight.Sale, -1, "出售失败");
            return;
        }
        /// <summary>
        /// 买装备
        /// </summary>
        /// <param name="client"></param>
        /// <param name="v"></param>
        private void onBuy(MobaClient client, int itemId)
        {
            //检测有没有装备
            ItemModel item = ItemData.GetItem(itemId);
            if (item == null)
                return;
            //1.获取房间模型
            int playerid = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerid);
            if (room == null)
                return;
            //获取英雄数据模型
            HeroModel hero = room.GetHeroModel(playerid);
            //检测钱够不够
            if (hero.Money < item.Price)
            {
                Send(client, OpCode.FightCode, OpFight.Buy, -1, "金币不足");
                return;
            }
            //添加装备
            for (int i = 0; i < hero.Equipments.Length; i++)
            {
                if (hero.Equipments[i] == -1)
                {
                    //开始购买
                    hero.Money -= item.Price;
                    //给他装备的ID
                    hero.Equipments[i] = itemId;
                    //增加属性
                    hero.Attack += item.Attack;
                    hero.Defense += item.Defense;
                    hero.MaxHp += item.Hp;
                    //给房间内所有客户端发消息了 HeroModel
                    room.Brocast(OpCode.FightCode, OpFight.Buy, 0, "有人购买装备了", null, JsonMapper.ToJson(hero));
                    return;
                }
            }
            //如果走到这里 就代表没买成功 肯定是一种结构 没有格子了 装备已满
                Send(client, OpCode.FightCode, OpFight.Buy, -2, "装备已满");
                return;

        }
        /// <summary>
        /// 计算伤害
        /// </summary>
        private void onDamage(MobaClient client, int attackId, int skillId, int[] targetId)
        {
            //1.获取房间模型
            int playerid = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerid);
            //2.判断是谁攻击 谁被攻击
            //攻击者的数据模型
            DogModel attackModel = null;

            if (attackId >= 0)
            {
                //攻击者的ID大于等于0 就是英雄攻击
                attackModel = room.GetHeroModel(attackId);
            }
            else if (attackId <= -10 && attackId >= -30)
            {
                //ID为-10~-30 防御塔攻击
                attackModel = room.GetBuildModel(attackId);
            }
            else if (attackId <= -1000)
            {
                //小兵攻击
                attackModel = room.GetDogModel(attackId);
            }
            //被攻击者的数据模型
            DogModel[] targetModels = new DogModel[targetId.Length];
            for (int i = 0; i < targetId.Length; i++)
            {
                if (targetId[i] >= 0)
                {
                    //被攻击者的ID大于等于0 就是英雄被攻击
                    targetModels[i] = room.GetHeroModel(targetId[i]);
                }
                else if (targetId[i] <= -10 && targetId[i] >= -30)
                {
                    //ID为-10~-30 防御塔被攻击
                    targetModels[i] = room.GetBuildModel(targetId[i]);
                }
                else if (targetId[i] <= -1000)
                {
                    //小兵被攻击
                    targetModels[i] = room.GetDogModel(targetId[i]);
                }
            }
            //3.根据技能ID判断出 是 普通攻击 还是特殊技能
            //4.根据伤害表 根据技能id获取ISKILL 调用damage 计算伤害
            ISkill skill = null;
            List<DamageModel> damages = null;
            //获取skill
            skill = DamageData.GetSkill(skillId);
            //计算出伤害
            damages = skill.Damage(skillId, 0, attackModel, targetModels);
            //6.给房间内的客户端广播数据模型
            room.Brocast(OpCode.FightCode, OpFight.Damage, 0, "有伤害产生", null, JsonMapper.ToJson(damages.ToArray()));
            //结算
            foreach (DogModel item in targetModels)
            {
                if (item.CurrHp <= 0)
                {
                    switch (item.ModelType)
                    {
                        case ModelType.DOG:
                            #region 小兵
                            //如果是英雄攻击导致死亡 那么就给奖励
                            if (attackModel.Id >= 0)
                            {
                                //加钱
                                ((HeroModel)attackModel).Money += 20;
                                //加经验
                                ((HeroModel)attackModel).Exp += 20;
                                //检测是否升级
                                if (((HeroModel)attackModel).Exp > ((HeroModel)attackModel).Level * 100)
                                {
                                    //升级
                                    ((HeroModel)attackModel).Level++;
                                    ((HeroModel)attackModel).Points++;
                                    //重置经验值
                                    ((HeroModel)attackModel).Exp = 0;
                                    HeroDataModel data = HeroData.GetHeroData(attackModel.Id);
                                    //英雄成长属性 增加
                                    ((HeroModel)attackModel).Attack += data.GrowAttack;
                                    ((HeroModel)attackModel).Defense += data.GrowDefense;
                                    ((HeroModel)attackModel).MaxHp += data.GrowHp;
                                    ((HeroModel)attackModel).MaxMp += data.GrowMp;
                                }
                                //给客户端发送 这个数据模型 attackModel 客户端更新
                                room.Brocast(OpCode.FightCode, OpFight.UpdateModel, 0, "更新数据模型", null,
                                    JsonMapper.ToJson((HeroModel)attackModel));
                            }
                            //移除小兵
                            room.RemoveDog(item);
                            #endregion
                            break;
                        case ModelType.BUILD:
                            #region 建筑
                            //判断是不是英雄击杀
                            if (attackModel.Id >= 0)
                            {
                                //加钱
                                ((HeroModel)attackModel).Money += 150;
                                //给客户端发attackModel
                                room.Brocast(OpCode.FightCode, OpFight.UpdateModel, 0, "更新数据模型", null, JsonMapper.ToJson((HeroModel)attackModel));
                            }
                            //检测防御塔是否可重生
                            if (((BuildModel)item).Rebirth)
                            {
                                //开启一个定时任务 在指定的事件之后复活
                                room.StartSchedule(DateTime.UtcNow.AddSeconds((double)((BuildModel)item).RebirthTime),
                                    () =>
                                    {
                                        //满状态
                                        ((BuildModel)item).CurrHp = ((BuildModel)item).MaxHp;
                                        //给客户端发送一个复活的消息 参数 item
                                        room.Brocast(OpCode.FightCode, OpFight.Resurge, 1, "有模型复活", null, JsonMapper.ToJson((BuildModel)item));

                                    });
                            }
                            //不重生 直接移除数据模型
                            else
                                room.RemoveBuild((BuildModel)item);
                            //游戏结束的判断
                            if (item.Id == -10)
                            {
                                onGameOver(room, 2);
                            }
                            else if (item.Id == -20)
                            {
                                onGameOver(room, 1);
                            }
                            #endregion
                            break;
                        case ModelType.HERO:
                            #region 英雄

                            //发奖励
                            if (attackModel.Id >= 0)
                            {
                                //加杀人数
                                ((HeroModel)attackModel).Kill++;
                                //加钱
                                ((HeroModel)attackModel).Money += 300;
                                //加经验
                                ((HeroModel)attackModel).Exp += 50;
                                //检测是否升级
                                if (((HeroModel)attackModel).Exp > ((HeroModel)attackModel).Level * 100)
                                {
                                    //升级
                                    ((HeroModel)attackModel).Level++;
                                    ((HeroModel)attackModel).Points++;
                                    //重置经验值
                                    ((HeroModel)attackModel).Exp = 0;
                                    HeroDataModel data = HeroData.GetHeroData(attackModel.Id);
                                    //英雄成长属性 增加
                                    ((HeroModel)attackModel).Attack += data.GrowAttack;
                                    ((HeroModel)attackModel).Defense += data.GrowDefense;
                                    ((HeroModel)attackModel).MaxHp += data.GrowHp;
                                    ((HeroModel)attackModel).MaxMp += data.GrowMp;
                                }
                                //给客户端发送 这个数据模型 attackModel 客户端更新
                                room.Brocast(OpCode.FightCode, OpFight.UpdateModel, 0, "更新数据模型", null, JsonMapper.ToJson((HeroModel)attackModel));
                            }
                            //目标英雄死亡
                            //加死亡数
                            ((HeroModel)item).Dead++;
                            //开启一个定时任务 在指定的事件之后复活
                            room.StartSchedule(DateTime.UtcNow.AddSeconds(((HeroModel)attackModel).Level * 5),
                                () =>
                                {
                                    //满状态
                                    ((HeroModel)item).CurrHp = ((HeroModel)item).MaxHp;
                                    //给客户端发送一个复活的消息 参数 item
                                    room.Brocast(OpCode.FightCode, OpFight.Resurge, 0, "有模型复活", null, JsonMapper.ToJson((HeroModel)item));

                                });


                            #endregion
                            break;
                        default:
                            break;
                    }

                }
            }

        }
        /// <summary>
        /// 游戏结束
        /// </summary>
        /// <param name="room"></param>
        /// <param name="winTeam"></param>
        private void onGameOver(FightRoom room, int winTeam)
        {
            //广播游戏结束的消息,参数是胜利的部队
            room.Brocast(OpCode.FightCode, OpFight.GameOver, 0, "游戏结束", null, winTeam);
            //更新玩家的数据
            foreach (MobaClient client in room.ClientList)
            {
                PlayerModel model = playerCache.GetModel(client);
                //检测是否逃跑
                if (room.LeaveClient.Contains(client))
                {
                    //更新逃跑场次
                    playerCache.UpdateModel(model, 2);
                }
                //队伍1赢了
                if (winTeam == 1)
                {
                    HeroModel hero = room.GetHeroModel(model.Id);
                    if (hero.Team == winTeam)
                    {
                        //赢了
                        playerCache.UpdateModel(model, 0);

                    }
                    else
                    {
                        //输了
                        playerCache.UpdateModel(model, 1);

                    }
                }

            }
            //销毁房间
            fightCache.Destroy(room.Id);

        }
        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        private void onSkill(MobaClient client,int skillId, int attackId,int targetId,float x,float y ,float z)
        {
                int playerId = playerCache.GetId(client);
                FightRoom room = fightCache.GetRoom(playerId);
            //先判断 是不是普通攻击
            if (skillId == 1)
            {
                //TODO
                //参数：1.攻击者ID 2.被攻击者ID
                room.Brocast(OpCode.FightCode, OpFight.Skill, 0, "有人普通攻击", null, attackId, targetId);
            }
            //是技能 从技能配置表里面通过技能ID获取到技能信息 然后再广播
            else
            {
                if (targetId == -1)
                {
                    //指定点的技能
                    room.Brocast(OpCode.FightCode, OpFight.Skill, 1, "有人释放指定点的技能了", null,skillId, attackId, -1,x,y,z);
                }
                else
                {
                    //指定目标的技能
                    room.Brocast(OpCode.FightCode, OpFight.Skill, 1, "有人释放指定目标的技能了", null,skillId, attackId, targetId);

                }
            }
        }

        /// <summary>
        /// 玩家移动
        /// </summary>
        /// <param name="client"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void onWalk(MobaClient client, float x, float y, float z)
        {
            int playerId = playerCache.GetId(client);
            FightRoom room = fightCache.GetRoom(playerId);
            if (room == null)
                return;
            //给每一个客户端发送信息：谁 移动到 哪
            MobaApplication.LogInfo("123");
            room.Brocast(OpCode.FightCode, OpFight.Walk, 0, "有玩家移动", null, playerId, x, y, z);
        }

        /// <summary>
        /// 玩家ID
        /// </summary>
        /// <param name="playerId"></param>
        private void onEnter(MobaClient client, int playerId)
        {
            FightRoom room = fightCache.Enter(playerId, client);
            if (room == null)
                return;
            //首先要判断 是否全部进入了
            //作用 保证竞技游戏的公平
            if (!room.IsAllEnter)
                return;
            room.spawnDog();
            //给每个客户端发送战斗房间的信息
            room.Brocast(OpCode.FightCode, OpFight.GetInfo, 0, "加载战斗场景", null, JsonMapper.ToJson(room.Heros),
                JsonMapper.ToJson(room.Builds));
            

        }
        public void OnDisconnect(MobaClient client)
        {
            fightCache.Offline(client, playerCache.GetId(client));
        }
    }
}
