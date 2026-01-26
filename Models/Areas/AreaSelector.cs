using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public static class AreaSelector
    {
        public static string GetBestArea(AutomaticHero hero)
        {
         
            int heroLevel = hero.Level;
            var areaForLevel = MinLevelAreaDictionary
                .Where(kv => kv.Key <= heroLevel)
                .OrderByDescending(kv => kv.Key)
                .First();

            var lowerAreaForLevel = MinLevelAreaDictionary.First();

            if (areaForLevel.Key > 1)
            {
                lowerAreaForLevel = MinLevelAreaDictionary
                    .Where(kv => kv.Key < areaForLevel.Key)
                    .OrderByDescending(kv => kv.Key)
                    .First();

            }

            if (hero.HealthPotionCount() < 5 && hero.CurrentArea.name == areaForLevel.Value)
            {
                
                return lowerAreaForLevel.Value;
            }

            if (hero.CurrentArea.name == lowerAreaForLevel.Value && hero.HealthPotionCount() < 20)
            {
                return lowerAreaForLevel.Value;
            }

            return areaForLevel.Value;
        }

        public static Dictionary<int, string> MinLevelAreaDictionary = new Dictionary<int, string>()
        {
            {1, "Stony Plains" },
            {5, "Crimson Meadows" },
            {8, "Rotten Burrows" },
            {11, "Ashen Pastures" }
        };

    }
}
