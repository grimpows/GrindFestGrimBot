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

    private WorldUI _worldUI = new WorldUI();

    private GoldShopManager _goldShopManager = null;

    public void OnGUI()
    {
        _worldUI?.OnGUI(SelectedHero, 15);
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

}

