using GrindFest;
using Scripts.Models;
using System.Collections.Generic;
using System.Linq;
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
        Color originalBgColor = GUI.backgroundColor;
        Color orignalCntColor = GUI.contentColor;
        GUISkin orignalSkin = GUI.skin;

        //GUI.skin = UITheme.Skin;
        GUI.backgroundColor = Color.white;
        GUI.contentColor = Color.white;


        _worldUI?.OnGUI(SelectedHero, 15);
        _goldShopManager?.OnGUI();

        GUI.backgroundColor = originalBgColor;
        GUI.contentColor = orignalCntColor;
        GUI.skin = orignalSkin;

    }

    public void Update()
    {
        if (_goldShopManager == null)
        {
            _goldShopManager = new GoldShopManager(this, GLOBALS.WINDOWS.GOLD_SHOP_MANAGER_WINDOW_INFO.ToggleKey);
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

