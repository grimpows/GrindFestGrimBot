using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Scripts.Models.Hero_Base;

namespace Scripts.Models
{
    public class HeroUI_EquipedItem
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }          // your own classification
        public EquipmentSlot Slot { get; set; }     // 🔹 link to GrindFest.EquipmentSlot
        public Double Durability { get; set; }    // example additional property

        public EquipableBehaviour? Behaviour { get; set; }

        public HeroUI_EquipedItem(string name, EquipmentSlot slot, double durability, EquipableBehaviour? behaviour)
        {
            Name = name;
            Slot = slot;
            Durability = durability;
            Behaviour = behaviour;
        }
    }

}
