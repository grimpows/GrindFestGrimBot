using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public static partial class HeroExtensions
    {
        public static bool CanPickUp(this AutomaticHero hero, ItemBehaviour item)
        {
            PickUpSkill pickUpSkill = hero.Character.SkillUser.GetSkill<PickUpSkill>();

            if (Vector3.Distance(hero.Character.transform.position, item.transform.position) > pickUpSkill.Range)
            {
                return false;
            }

            return true;
        }


    }
}
