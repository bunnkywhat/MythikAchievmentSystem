﻿//#define STOREONITEM

using Server;
using System;
using System.Collections.Generic;

using Scripts.Mythik.Mobiles;
using Scripts.Mythik.Systems.Achievements.Gumps;
using Server.Mobiles;
using Server.Items;
using Server.Commands;
using Server.Misc;
using Scripts.Mythik.Systems.Achievements.Items;

namespace Scripts.Mythik.Systems.Achievements
{
    //TODO
    //subcategories X
    //page limit?
    // Achievement prereq achieve before showing
    //TODO Skill gain achieves needs event
    //TODO ITEM crafted event sink
    // 
    public class AchievmentSystem
    {
        public class AchievementCategory
        {
            public int ID { get; set; }
            public int Parent { get; set; } = 0;
            public string Name;


            public AchievementCategory(int id, int parent, string v3)
            {
                ID = id;
                Parent = parent;
                Name = v3;
            }
        }
        public static List<Achievement> Achievements = new List<Achievement>();
        public static List<AchievementCategory> Categories = new List<AchievementCategory>();

        public static void Initialize()
        {
            Categories.Add(new AchievementCategory(1, 0, "Exploration"));
                Categories.Add(new AchievementCategory(2, 1, "Towns"));
                Categories.Add(new AchievementCategory(3, 1, "Dungeons"));

            Categories.Add(new AchievementCategory(4, 0, "Crafting"));
            Categories.Add(new AchievementCategory(5, 0, "Resource Gathering"));
            Categories.Add(new AchievementCategory(6, 0, "Hunting"));
            Categories.Add(new AchievementCategory(7, 0, "Character Development"));
            Categories.Add(new AchievementCategory(8, 0, "Other"));

            Achievements.Add(new DiscoveryAchievement(0, 2, 0x96E, false, null, "Minoc!", "Discover Minoc Township", 5, "Minoc"));
            Achievements.Add(new DiscoveryAchievement(1, 2, 0x96E, false, null, "Yew!", "Discover the Yew Township", 5, "Yew"));
            Achievements.Add(new DiscoveryAchievement(2, 2, 0x96E, false, null, "Trinsic!", "Discover the Trinsic Township", 5, "Trinsic"));
            Achievements.Add(new DiscoveryAchievement(3, 2, 0x96E, false, null, "Cove!", "Discover the Cove Township", 5, "Cove"));
            Achievements.Add(new DiscoveryAchievement(4, 3, 0x96E, false, null, "Wrong!", "Discover the dungeon of Wrong", 5, "Wrong"));
            Achievements.Add(new DiscoveryAchievement(5, 3, 0x96E, false, null, "Shame!", "Discover the dungeon of Shame", 5, "Shame"));
            var achieve = new HarvestAchievement(100, 5, 0, false, null, 500, "500 Iron Ore", "Mine 500 Iron Ore", 5, typeof(IronOre), typeof(AncientSmithyHammer));
            Achievements.Add(achieve);
            Achievements.Add(new HarvestAchievement(100, 5, 0, false, achieve, 500, "50000 Iron Ore", "Mine 500 Iron Ore", 5, typeof(IronOre), typeof(AncientSmithyHammer)));

            Achievements.Add(new HunterAchievement(300, 6, 0x25D1, false, null, 5, "Dog Slayer", "Slay 5 Dogs", 5, typeof(Dog)));
            Achievements.Add(new HunterAchievement(301, 6, 0x25D1, false, null, 50, "Dragon Slayer", "Slay 50 Dragon", 5, typeof(Dragon), typeof(Mythik.Items.Rares.RareClothDyeTub)));




            CommandSystem.Register("feats", AccessLevel.Player, new CommandEventHandler(OpenGump));

        }

        private static void OpenGump(CommandEventArgs e)
        {
            var player = e.Mobile as PlayerMobile;
            if(player != null)
            {
#if STOREONITEM
           if (!AcheivmentSystemMemoryStone.GetInstance().Achievements.ContainsKey(player.Serial))
                AcheivmentSystemMemoryStone.GetInstance().Achievements.Add(player.Serial, new Dictionary<int, AcheiveData>());
            var achieves = AcheivmentSystemMemoryStone.GetInstance().Achievements[player.Serial];
                var total = AcheivmentSystemMemoryStone.GetInstance().PointsTotals[player.Serial];
#else
                var achieves = (player as MythikPlayerMobile).Achievements;
                var total = (player as MythikPlayerMobile).AchievementPointsTotal;
#endif
                e.Mobile.SendGump(new AchievementGump(achieves, total));
            }
            
        }

        internal static void SetAchievementStatus(PlayerMobile player, Achievement ach, int progress)
        {

#if STOREONITEM
           if (!AcheivmentSystemMemoryStone.GetInstance().Achievements.ContainsKey(player.Serial))
                AcheivmentSystemMemoryStone.GetInstance().Achievements.Add(player.Serial, new Dictionary<int, AcheiveData>());
            var achieves = AcheivmentSystemMemoryStone.GetInstance().Achievements[player.Serial];
            
#else
            var achieves = (player as MythikPlayerMobile).Achievements;

#endif


            if (achieves.ContainsKey(ach.ID))
            {
                if (achieves[ach.ID].Progress >= ach.CompletionTotal)
                    return;
                achieves[ach.ID].Progress += progress;
            }
            else
            {
                achieves.Add(ach.ID, new AcheiveData() { Progress = progress });
            }

            if (achieves[ach.ID].Progress >= ach.CompletionTotal) {
                player.SendGump(new AchievementObtainedGump(ach),false);
                achieves[ach.ID].CompletedOn = DateTime.Now;


#if STOREONITEM
                if (!AcheivmentSystemMemoryStone.GetInstance().PointsTotals.ContainsKey(player.Serial))
                    AcheivmentSystemMemoryStone.GetInstance().PointsTotals.Add(player.Serial, 0);
                AcheivmentSystemMemoryStone.GetInstance().PointsTotals[player.Serial] += ach.RewardPoints;
#else
                (player as MythikPlayerMobile).AchievementPointsTotal += ach.RewardPoints;
#endif
                if (ach.RewardItems?.Length > 0)
                {
                    try
                    {
                        var item = (Item)Activator.CreateInstance(ach.RewardItems[0]);
                        if (!WeightOverloading.IsOverloaded(player))
                            player.Backpack.DropItem(item);
                        else
                            player.BankBox.DropItem(item);
                    }
                    catch { }
                }
            }
        }
    }
}