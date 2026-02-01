using GrindFest;
using System;

namespace Scripts.Models
{
    public class HeroUI_EquipedItem
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }          // your own classification
        public EquipmentSlot Slot { get; set; }     // 🔹 link to GrindFest.EquipmentSlot
        public Double DurabilityPercentage { get; set; }    // example additional property
        public Double DurabilityFlat { get; set; }   
        public Double DurabilityMax { get; set; }   

        public EquipableBehaviour? Behaviour { get; set; }

        public HeroUI_EquipedItem(string name, EquipmentSlot slot, double durability, EquipableBehaviour? behaviour)
        {
            Name = name;
            Slot = slot;
            DurabilityPercentage = durability;
            Behaviour = behaviour;
        }
    }

}
