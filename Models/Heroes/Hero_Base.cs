using GrindFest;
using UnityEngine;

namespace Scripts.Models
{
    public class Hero_Base : AutomaticHero
    {


        private Bot _bot = new Bot();
        private HeroUI _heroUI = new HeroUI();
        private MinimapUI _minimapUI = new MinimapUI();
        private InventoryUI _inventoryUI = new InventoryUI();
        private SkillUI _skillUI = new SkillUI();
        private QuickAccessUI _quickAccessUI = new QuickAccessUI();
        private AIChat _aiChat = new AIChat();
        private AIChatUI _aiChatUI = new AIChatUI();
        public SkillBar SkillBar1 = new SkillBar();


        public void Start()
        {
            _heroUI.OnStart(this, KeyCode.C, GLOBALS.WINDOWS.HERO_WINDOW_INFO.ID);
            _minimapUI.OnStart(this, KeyCode.M, GLOBALS.WINDOWS.MINIMAP_WINDOW_INFO.ID);
            _inventoryUI.OnStart(this, KeyCode.I, GLOBALS.WINDOWS.INVENTORY_WINDOW_INFO.ID);
            _skillUI.OnStart(this, KeyCode.S, GLOBALS.WINDOWS.SKILL_WINDOW_INFO.ID);

            _bot.OnStart(this, KeyCode.B, GLOBALS.WINDOWS.BOT_WINDOW_INFO.ID);

            // Initialize AI Chat
            _aiChat.OnStart(this);
            _aiChatUI.OnStart(_aiChat, this, KeyCode.T, GLOBALS.WINDOWS.AICHAT_WINDOW_INFO.ID);

            // Initialize quick access buttons
            _quickAccessUI.OnStart(_heroUI, _inventoryUI, _skillUI, _bot, _aiChatUI);
            SkillBar1.OnStart(this);
        }


        public void OnGUI()
        {
            try
            {
                _heroUI.OnGUI();
                _minimapUI.OnGUI();
                _inventoryUI.OnGUI();
                _skillUI.OnGUI();

                _bot.OnGUI();
                _aiChatUI.OnGUI();


                // Draw quick access buttons
                _quickAccessUI.OnGUI();

                SkillBar1.OnGUI();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Hero_Base OnGUI Error: {ex.Message}\n{ex.StackTrace}");
            }
            
        }

        public void Update()
        {
            try
            {
                _heroUI.OnUpdate();
                _minimapUI.OnUpdate();
                _inventoryUI.OnUpdate();
                _skillUI.OnUpdate();


                _bot.OnUpdate();
                _aiChatUI.OnUpdate();
                SkillBar1.OnUpdate();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Hero_Base Update Error: {ex.Message}\n{ex.StackTrace}");
            }
        }





    }

}
