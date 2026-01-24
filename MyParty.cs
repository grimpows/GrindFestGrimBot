using GrindFest;
using Scripts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;



class MyParty : Party_Base
{
    public static List<PartyMember> PartyMembers =>
         new List<PartyMember>()
        {
            new PartyMember("Grim", "Hero"),
        };

    private WorldUI _worldUI = new WorldUI(KeyCode.W);

    public void OnGUI()
    {
        _worldUI?.OnGUI(SelectedHero);
    }

    public void Update()
    {
        _worldUI?.OnUpdate();

        if (Input.GetKeyDown(KeyCode.F4))
        {
            SelectedHero.OpenInventory();
        }


        BuyGoldShopItems();

        ResurectDeadHeroes(PartyMembers);
    }

   

    public override void OnAllHeroesDied()
    {
        var firstHero = PartyMembers.FirstOrDefault();

        if (firstHero == null)
            return;

        CreateHero(firstHero.Name, firstHero.ClassName).StartBotting();
    }


    

}

