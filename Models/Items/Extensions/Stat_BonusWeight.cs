using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public static partial class ItemExtensions
    {
        public static int Stat_BonusWeight(this ItemBehaviour item)
        {
            int result = 0;

            if (item?.BonusDexterity is not null)
            {
                result += item.BonusDexterity;
            }

            if (item?.BonusStrength is not null)
            {
                result += item.BonusStrength;
            }

            if (item?.BonusIntelligence is not null)
            {
                result += item.BonusIntelligence;
            }

            return result;
        }
    }
}
