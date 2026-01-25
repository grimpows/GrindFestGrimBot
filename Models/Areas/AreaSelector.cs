using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    public static class AreaSelector
    {
        public static string GetBestAreaForLevel(int level)
        {
            //    area = ((Level < 5) ? "Stony Plains" : ((Level < 8) ? "Crimson Meadows" : ((Level < 11) ? "Rotten Burrows" : "Ashen Pastures")));

            switch (level)
            {
                case int n when (n < 4):
                    return "Stony Plains";
                case int n when (n < 8):
                    return "Crimson Meadows";
                case int n when (n < 15):
                    return "Rotten Burrows";
                default:
                    return "Ashen Pastures";
            }

            //return "Human Encampment";
        }


    }
}
