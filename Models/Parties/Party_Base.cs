using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class Party_Base : AutomaticParty
    {
        public bool IsPartyFull()
        {
            return this.Party.Heroes.Count >= this.Party.MaxHeroes;
        }

        public void ResurectDeadHeroes(List<PartyMember> partyMembers)
        {
            if (!IsPartyFull())
            {
                foreach (var partyMember in partyMembers)
                {
                    if (IsPartyFull())
                    {
                        return;
                    }
                    bool playerExist = this.Party.Heroes.FirstOrDefault(h => h.name == partyMember.Name) != null;


                    if (!playerExist)
                    {
                        var hero = this.CreateHero(partyMember.Name, partyMember.ClassName);
                        hero.StartBotting();
                    }
                }
            }
        }

        
    }
}
