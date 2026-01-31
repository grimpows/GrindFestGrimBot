namespace Scripts.Models
{
    /// <summary>
    /// Represents the current activity status of the bot.
    /// </summary>
    public enum BotStatus
    {
        /// <summary>Bot is inactive or disabled.</summary>
        INACTIVE,
        
        /// <summary>Bot is idle, waiting for something to do.</summary>
        IDLE,
        
        /// <summary>Bot is currently fighting enemies.</summary>
        FIGHTING,
        
        /// <summary>Bot is looting items from the ground.</summary>
        LOOTING,
        
        /// <summary>Bot is consuming items (potions, food, etc.).</summary>
        CONSUMING,
        
        /// <summary>Bot is traveling to a different area.</summary>
        TRAVELING,
        
        /// <summary>Bot is running around in the current area.</summary>
        RUNAROUND,
        
        /// <summary>Bot is in unstick mode, trying to get unstuck.</summary>
        UNSTICKING,
        
        /// <summary>Bot is interacting with objects (chests, shrines, etc.).</summary>
        INTERACTING,
        
        /// <summary>Bot is upgrading stats or equipment.</summary>
        UPGRADING
    }
}
