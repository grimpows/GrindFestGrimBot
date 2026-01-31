using GrindFest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scripts.Models
{
    /// <summary>
    /// Represents a single message in the AI chat conversation.
    /// </summary>
    public class AIChatMessage
    {
        public string Content { get; set; }
        public bool IsUserMessage { get; set; }
        public DateTime Timestamp { get; set; }

        public AIChatMessage(string content, bool isUserMessage)
        {
            Content = content;
            IsUserMessage = isUserMessage;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>
    /// Manages the AI chat functionality as an in-game assistant chatbot.
    /// Uses LLM when available, falls back to local responses when LLM is unavailable.
    /// </summary>
    public class AIChat
    {
        private AutomaticHero _hero;
        private List<AIChatMessage> _messages = new List<AIChatMessage>();
        private bool _isWaitingForResponse = false;
        private string _lastError = string.Empty;
        private bool _useFallbackMode = false;

        /// <summary>
        /// Maximum number of messages to keep in history.
        /// </summary>
        public int MaxMessageHistory { get; set; } = 50;

        /// <summary>
        /// Gets the list of chat messages.
        /// </summary>
        public IReadOnlyList<AIChatMessage> Messages => _messages.AsReadOnly();

        /// <summary>
        /// Indicates if the AI is currently processing a response.
        /// </summary>
        public bool IsWaitingForResponse => _isWaitingForResponse;

        /// <summary>
        /// Gets the last error message if any.
        /// </summary>
        public string LastError => _lastError;

        /// <summary>
        /// Indicates if the chat is using fallback mode (local responses instead of LLM).
        /// </summary>
        public bool UseFallbackMode
        {
            get => _useFallbackMode;
            set => _useFallbackMode = value;
        }

        /// <summary>
        /// Event triggered when a new message is added to the chat.
        /// </summary>
        public event EventHandler<AIChatMessage> OnMessageAdded;

        /// <summary>
        /// Event triggered when the AI response is received.
        /// </summary>
        public event EventHandler<AIChatMessage> OnResponseReceived;

        public AIChat()
        {
        }

        /// <summary>
        /// Initializes the AI Chat with a hero reference.
        /// </summary>
        public void OnStart(AutomaticHero hero)
        {
            _hero = hero;
            AddSystemMessage("AI Assistant ready! Ask me about your hero, enemies, or get gameplay tips.");
        }

        /// <summary>
        /// Sends a message to the AI and gets a response.
        /// </summary>
        public async void SendMessage(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return;

            if (_hero == null)
            {
                _lastError = "Hero not initialized";
                return;
            }

            if (_isWaitingForResponse)
            {
                _lastError = "Already waiting for a response";
                return;
            }

            _lastError = string.Empty;
            _isWaitingForResponse = true;

            // Add user message to history
            var userMsg = new AIChatMessage(userMessage, true);
            AddMessage(userMsg);

            string response;

            // Use fallback mode if enabled, otherwise try LLM
            if (_useFallbackMode)
            {
                response = ProcessLocalMessage(userMessage.ToLower());
            }
            else
            {
                response = await TryGetLLMResponse(userMessage);
            }

            var aiMsg = new AIChatMessage(response, false);
            AddMessage(aiMsg);
            OnResponseReceived?.Invoke(this, aiMsg);
            _isWaitingForResponse = false;
        }

        /// <summary>
        /// Tries to get a response from the LLM, falls back to local if it fails.
        /// </summary>
        private async Task<string> TryGetLLMResponse(string userMessage)
        {
            try
            {
                // Build a simple context about the hero's current state
                string heroContext = BuildHeroContext();

                // Create a concise prompt for the LLM
                string prompt = $"[Hero: {heroContext}]\n\nPlayer: {userMessage}\n\nRespond briefly as a helpful game assistant.";

                // Use the hero's Think() method to get LLM response
                string response = await _hero.Think(prompt);

                if (string.IsNullOrWhiteSpace(response))
                {
                    return ProcessLocalMessage(userMessage.ToLower());
                }

                return response.Trim();
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;

                // Check if it's a connection/retry error
                if (ex.Message.Contains("Retry") || ex.Message.Contains("retry") || 
                    ex.Message.Contains("request") || ex.Message.Contains("connection"))
                {
                    // Suggest enabling fallback mode
                    string fallbackResponse = ProcessLocalMessage(userMessage.ToLower());
                    return $"[LLM unavailable - using local mode]\n\n{fallbackResponse}\n\n(Tip: Check LLM settings in game options, or enable 'Fallback Mode' in chat settings)";
                }

                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Processes the user message locally and generates an appropriate response.
        /// </summary>
        private string ProcessLocalMessage(string message)
        {
            // Health related queries
            if (ContainsAny(message, "health", "hp", "vie", "sante"))
                return GetHealthStatus();

            // Location queries
            if (ContainsAny(message, "where", "location", "area", "zone"))
                return GetLocationInfo();

            // Enemy queries
            if (ContainsAny(message, "enemy", "enemies", "monster", "ennemi", "nearby"))
                return GetEnemyInfo();

            // Inventory queries
            if (ContainsAny(message, "inventory", "items", "inventaire", "bag"))
                return GetInventoryInfo();

            // Level/stats queries
            if (ContainsAny(message, "level", "stats", "stat", "strength", "dexterity", "intelligence"))
                return GetStatsInfo();

            // Strategy/advice
            if (ContainsAny(message, "strategy", "advice", "tips", "help", "aide", "what should"))
                return GetAdvice();

            // Equipment queries
            if (ContainsAny(message, "equipment", "armor", "weapon", "gear"))
                return GetEquipmentInfo();

            // Bot status
            if (ContainsAny(message, "bot", "status"))
                return GetBotStatus();

            // Potions
            if (ContainsAny(message, "potion", "heal"))
                return GetPotionInfo();

            // Level up / faster
            if (ContainsAny(message, "level up", "faster", "xp", "experience", "grind"))
                return GetLevelingTips();

            // Greetings
            if (ContainsAny(message, "hello", "hi", "bonjour", "salut", "hey"))
                return "Hello! I can help with: health, location, enemies, stats, inventory, equipment, potions, leveling tips, and advice.";

            // Default response
            return "I can help with: health, location, enemies, stats, inventory, equipment, potions, leveling tips, and battle advice. Just ask!";
        }

        private bool ContainsAny(string text, params string[] keywords)
        {
            foreach (var keyword in keywords)
                if (text.Contains(keyword)) return true;
            return false;
        }

        private string GetHealthStatus()
        {
            int health = _hero?.Health ?? 0;
            int maxHealth = _hero?.MaxHealth ?? 1;
            int pct = (int)((float)health / maxHealth * 100);
            int potions = _hero?.HealthPotionCount() ?? 0;

            string status = pct < 25 ? "CRITICAL!" : pct < 50 ? "Low" : pct < 75 ? "Moderate" : "Good";
            return $"Health: {health}/{maxHealth} ({pct}%) - {status}\nPotions: {potions}";
        }

        private string GetLocationInfo()
        {
            string area = _hero?.CurrentArea?.name ?? "Unknown";
            string region = _hero?.CurrentArea?.Root?.name ?? "Unknown";
            return $"Area: {area}\nRegion: {region}";
        }

        private string GetEnemyInfo()
        {
            try
            {
                var enemies = _hero?.FindNearestEnemies(maxDistance: 50);
                int count = 0;
                if (enemies != null)
                    foreach (var e in enemies)
                        if (e != null) count++;

                return count == 0 ? "No enemies nearby. Safe to explore!" : $"Enemies nearby: {count}";
            }
            catch { return "Unable to scan for enemies."; }
        }

        private string GetInventoryInfo()
        {
            int items = _hero?.Character?.Inventory?.Items?.Count ?? 0;
            int potions = _hero?.HealthPotionCount() ?? 0;
            return $"Items: {items}\nHealth Potions: {potions}";
        }

        private string GetStatsInfo()
        {
            return $"Level: {_hero?.Level ?? 1}\nSTR: {_hero?.Strength ?? 0} | DEX: {_hero?.Dexterity ?? 0} | INT: {_hero?.Intelligence ?? 0}\nClass: {_hero?.Hero?.Class?.name ?? "Unknown"}";
        }

        private string GetEquipmentInfo()
        {
            int armor = _hero?.EquipedItems_TotalArmor() ?? 0;
            return $"Total Armor: {armor}";
        }

        private string GetBotStatus()
        {
            bool active = _hero?.IsBotting ?? false;
            return $"Bot: {(active ? "Active" : "Inactive")}\nPress B to toggle bot panel.";
        }

        private string GetPotionInfo()
        {
            int potions = _hero?.HealthPotionCount() ?? 0;
            string tip = potions == 0 ? "No potions! Farm easier areas." :
                         potions < 5 ? "Running low, stock up!" :
                         potions < 10 ? "Decent supply." : "Well stocked!";
            return $"Health Potions: {potions}\n{tip}";
        }

        private string GetLevelingTips()
        {
            int level = _hero?.Level ?? 1;
            string area = level < 3 ? "Stony Plains" :
                          level < 5 ? "Crimson Meadows" :
                          level < 7 ? "Rotten Burrows" :
                          level < 9 ? "Ashen Pastures" : "Canyon of Death";
            return $"Level: {level}\nSuggested Area: {area}\n\nTips: Fight monsters near your level, keep gear upgraded, use the bot to farm.";
        }

        private string GetAdvice()
        {
            int health = _hero?.Health ?? 0;
            int maxHealth = _hero?.MaxHealth ?? 1;
            float pct = (float)health / maxHealth;
            int potions = _hero?.HealthPotionCount() ?? 0;

            if (pct < 0.3f) return "HEAL NOW! Your health is critical.";
            if (potions == 0) return "Get potions! Farm an easier area first.";
            if (pct < 0.6f) return "Consider healing before your next fight.";
            return "You're in good shape! Keep grinding.";
        }

        /// <summary>
        /// Builds a compact context string with the hero's current state.
        /// </summary>
        private string BuildHeroContext()
        {
            var parts = new List<string>();
            try
            {
                int health = _hero?.Health ?? 0;
                int maxHealth = _hero?.MaxHealth ?? 1;
                int healthPct = (int)((float)health / maxHealth * 100);
                parts.Add($"HP:{healthPct}%");
                parts.Add($"Lv:{_hero?.Level ?? 1}");
                parts.Add($"Area:{_hero?.CurrentArea?.Root?.name ?? "?"}");
                parts.Add($"Potions:{_hero?.HealthPotionCount() ?? 0}");
            }
            catch { return "Status unavailable"; }
            return string.Join(", ", parts);
        }

        public void AddSystemMessage(string message)
        {
            var sysMsg = new AIChatMessage("[System] " + message, false);
            AddMessage(sysMsg);
        }

        private void AddMessage(AIChatMessage message)
        {
            _messages.Add(message);
            while (_messages.Count > MaxMessageHistory)
                _messages.RemoveAt(0);
            OnMessageAdded?.Invoke(this, message);
        }

        public void ClearHistory()
        {
            _messages.Clear();
            _hero?.ClearThoughts();
            AddSystemMessage("Chat history cleared.");
        }

        public List<string> GetQuickActions()
        {
            return new List<string>
            {
                "Health?",
                "Where am I?",
                "Enemies?",
                "Advice",
                "Stats",
                "Potions",
                "Level tips"
            };
        }
    }
}
