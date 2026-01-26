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
    public class SkillUI
    {
        private Hero_Base _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = false;
        private int _windowID;
        

        // requied rects
        private Rect _skillsWindowRect = new Rect(100, 100, 800, 800);

        
        public void OnGUI()
        {
            Event e = Event.current;

            if (e.type == EventType.KeyDown && e.keyCode == _toggleShowKey)
            {
                _isShow = !_isShow;
                e.Use();
            }

            if (_isShow)
            {
                _skillsWindowRect = GUI.Window(_windowID, _skillsWindowRect, DrawSkillsWindow, "Skills");
            }
        }

       

        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;
        }

    

        public void OnUpdate()
        {
            

        }

        private void DrawSkillsWindow(int windowID)
        {
            //GUILayout.BeginArea(new Rect(10, 10, _skillsWindowRect.width - 20, _skillsWindowRect.height  - 40));
            try
            {
                GUILayout.BeginVertical();
                
                if (_hero == null)
                {
                    GUILayout.Label("Hero not initialized");
                }
                else
                {
                    GUILayout.Label("Skills Window");
                    GUILayout.Label($"Points Available: {_hero.SkillPoints}");
                    foreach (var skill in _hero.Skills)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"Skill: {skill.name}");
                        GUILayout.Label($"Level: {skill.Level}");
                        GUILayout.Label($"command: {skill.Command}");
                        GUILayout.Label($"Range: {skill.Range}");

                        if (GUILayout.Button("Upgrade"))
                        {
                            _hero.AllocateSkillPoints(skill.name, 1);
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Label($"{skill.Description}");
                    }
                }
                
                GUILayout.EndVertical();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error drawing skills window: {ex.Message}");
                GUILayout.BeginVertical();
                GUILayout.Label($"Error: {ex.Message}");
                GUILayout.EndVertical();
            }

            //GUILayout.EndArea();

            GUI.DragWindow();
        }



    }
}
