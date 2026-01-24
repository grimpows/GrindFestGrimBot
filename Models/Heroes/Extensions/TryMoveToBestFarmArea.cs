using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static bool TryMoveToBestFarmArea(this AutomaticHero hero, bool isHeroAllowedChangeArea = true)
        {
            if (!isHeroAllowedChangeArea)
            {
                return false;
            }
            string areaName = AreaSelector.GetBestAreaForLevel(hero.Level);
            if (hero.CurrentArea?.Root.name != areaName)
            {
                hero.Say($"Changing area to {areaName}");
                hero.GoToArea(areaName);
                return true;
            }
            return false;
        }
    }
}
