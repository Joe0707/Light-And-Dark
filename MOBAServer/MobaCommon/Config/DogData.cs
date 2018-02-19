using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobaCommon.Config
{
    public class DogData
    {
        public const int Remote = 1;

        /// <summary>
        /// 类型和模型的映射
        /// </summary>
        static Dictionary<int, DogDataModel> idDogDict = new Dictionary<int, DogDataModel>();

        static DogData()
        {
            createDog(Remote, 50, 20, 10, 10, "弓箭手");
        }

        private static void createDog(int typeId, int hp, int attack, int defense, double attackDistance, string name)
        {
            DogDataModel dog = new DogDataModel(typeId, hp, attack, defense, attackDistance, name);
            idDogDict.Add(dog.TypeId, dog);
        }

        public static DogDataModel GetDogData(int typeId)
        {
            DogDataModel dog = null;
            idDogDict.TryGetValue(typeId, out dog);
            return dog;
        }
    }

    public class DogDataModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public int TypeId { get; set; }
        /// <summary>
        /// 血量
        /// </summary>
        public int Hp { get; set; }
        /// <summary>
        /// 攻击
        /// </summary>
        public int Attack { get; set; }
        /// <summary>
        /// 防御
        /// </summary>
        public int Defense { get; set; }
        /// <summary>
        /// 攻击距离
        /// </summary>
        public double AttackDistance { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        public DogDataModel(int typeId, int hp, int attack, int defense, double attackDistance, string name)
        {
            this.TypeId = typeId;
            this.Hp = hp;
            this.Attack = attack;
            this.Defense = defense;
            this.AttackDistance = attackDistance;
            this.Name = name;
        }
    }
}
