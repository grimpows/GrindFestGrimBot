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
            new PartyMember("Grim", "Warrior"),
        };

    private WorldUI _worldUI = new WorldUI(KeyCode.W);

    private GoldShopManager _goldShopManager = null;

    public void OnGUI()
    {
        _worldUI?.OnGUI(SelectedHero, 20);
        _goldShopManager?.OnGUI();


    }

    public void Update()
    {
        if (_goldShopManager == null)
        {
            Debug.Log("Initializing Gold Shop Manager");
            _goldShopManager = new GoldShopManager(this,KeyCode.A);
            Debug.Log($"Gold Shop Manager Initialized with KeyCode: {KeyCode.A}");
        }


        _goldShopManager?.OnUpdate();

        _worldUI?.OnUpdate();


        if (Input.GetKeyDown(KeyCode.F2))
        {
            AutomaticParty.ClearFlags();
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            // This is a static method, that means you don't need to have an instance of the class to call it
            AutomaticParty.PlaceFlag();
            
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            SelectedHero.OpenInventory();
        }


        ResurectDeadHeroes(PartyMembers);
    }

   

    public override void OnAllHeroesDied()
    {
        var firstHero = PartyMembers.FirstOrDefault();

        if (firstHero == null)
            return;

        CreateHero(firstHero.Name, firstHero.ClassName).StartBotting();
    }

    //public bool IsAnyHeroReachedLevelCap()
    //{

    //}


    

}

