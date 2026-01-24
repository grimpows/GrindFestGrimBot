using GrindFest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Timers;

namespace Scripts.Models
{
    public class MinimapUI
    {
        private Hero_Base _hero;
        private KeyCode _toggleShowKey;
        private bool _isShow = true;
        private int _windowID;

        private Rect _minimapRect = new Rect(Screen.width - 310, 70, 300, 200);

        private Vector3 _lastPosition;

        private DateTime _lastSaveTime = DateTime.Now;

        private Texture2D _redTex;
        private Texture2D _blueTex;
        private Texture2D _greenTex;
        void InitTextures()
        {
            if (_redTex == null)
            {
                _redTex = new Texture2D(1, 1);
                _redTex.SetPixel(0, 0, Color.red);
                _redTex.Apply();
            }

            if (_blueTex == null)
            {
                _blueTex = new Texture2D(1, 1);
                _blueTex.SetPixel(0, 0, Color.blue);
                _blueTex.Apply();
            }

            if (_greenTex == null)
            {
                _greenTex = new Texture2D(1, 1);
                _greenTex.SetPixel(0, 0, Color.green);
                _greenTex.Apply();
            }
        }

        public void OnStart(Hero_Base hero, KeyCode toggleShowKey, int windowID)
        {
            InitTextures();
            _hero = hero;
            _toggleShowKey = toggleShowKey;
            _windowID = windowID;
        }

        public void On1SecTick()
        {
            _lastPosition = _hero.transform.position;
        }

        public void OnUpdate()
        {
            if ((DateTime.Now - _lastSaveTime).TotalSeconds >= 1)
            {
                On1SecTick();
                _lastSaveTime = DateTime.Now;
            }
        }

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
                _minimapRect = GUI.Window(WindowsConstants.MINIMAP_WINDOW_ID, _minimapRect, DrawMinimapWindow, "Minimap");
            }
        }

        void DrawMinimapWindow(int windowID)
        {
            // Draw minimap content here

            //draw "Dots" for nearby enemies
            var enemies = _hero.FindNearestEnemies(maxDistance: 100);
            foreach (var enemy in enemies)
            {
                Vector3 pos = enemy.transform.position;
                Vector3 screen = Camera.main.WorldToScreenPoint(pos);
                screen.y = Screen.height - screen.y; // on optien la position écran (y inversé)

                float dotSize = 1;
                float half = dotSize * 0.5f;

                //ratio  de screen.y par rapport à la taille de l'écran fois la taille de la minimap
                float dotY = (screen.y / Screen.height) * _minimapRect.height - half;
                // ratio  de screen.x par rapport à la taille de l'écran fois la taille de la minimap
                float dotX = (screen.x / Screen.width) * _minimapRect.width - half;


                //Rect dotRect = new Rect(dotX, dotY, dotSize, dotSize);
                var prevBg = GUI.skin.box.normal.background;
                GUI.skin.box.normal.background = _redTex;
                GUI.Box(new Rect(dotX, dotY, dotSize, dotSize), GUIContent.none);
                GUI.skin.box.normal.background = prevBg;
                // restaurer les couleurs
            }



            if (_lastPosition != null)
            {
                Vector3 screen = Camera.main.WorldToScreenPoint(_lastPosition);
                screen.y = Screen.height - screen.y; // on optien la position écran (y inversé)
                float dotSize = 2;
                float half = dotSize * 0.5f;
                //ratio  de screen.y par rapport à la taille de l'écran fois la taille de la minimap
                float dotY = (screen.y / Screen.height) * _minimapRect.height - half;
                // ratio  de screen.x par rapport à la taille de l'écran fois la taille de la minimap
                float dotX = (screen.x / Screen.width) * _minimapRect.width - half;

                var prevBg = GUI.skin.box.normal.background;
                GUI.skin.box.normal.background = _blueTex;
                GUI.Box(new Rect(dotX, dotY, dotSize, dotSize), GUIContent.none);
                GUI.skin.box.normal.background = prevBg;
            }

            //draw center point
            float centerX = _minimapRect.width / 2 - 1;
            float centerY = _minimapRect.height / 2 - 1;
            var prevBgCenter = GUI.skin.box.normal.background;
            GUI.skin.box.normal.background = _greenTex;
            GUI.Box(new Rect(centerX, centerY, 2, 2), GUIContent.none);
            GUI.skin.box.normal.background = prevBgCenter;



            GUI.DragWindow();
        }


    }
}
