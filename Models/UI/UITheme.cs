using UnityEngine;

namespace Scripts.Models
{
    /// <summary>
    /// Centralized AAA-quality UI theme for all game interfaces.
    /// Uses lazy initialization to avoid loading resources until needed.
    /// </summary>
    public static class UITheme
    {
        #region ???????????????????????????????????????????????????????????????
        //  LAYOUT CONSTANTS
        #endregion ????????????????????????????????????????????????????????????

        // Window sizing
        public const int BUTTON_SIZE = 50;
        public const int BUTTON_SIZE_SMALL = 28;
        public const int BUTTON_SPACING = 4;
        public const int WINDOW_PADDING = 12;
        public const int SECTION_SPACING = 10;
        public const int ELEMENT_SPACING = 6;

        // Common element sizes
        public const int HEADER_HEIGHT = 35;
        public const int TAB_HEIGHT = 35;
        public const int INPUT_FIELD_WIDTH = 60;
        public const int TOGGLE_WIDTH = 60;
        public const int TOGGLE_HEIGHT = 22;
        public const int CLOSE_BUTTON_SIZE = 28;

        // Card sizing
        public const float CARD_PADDING = 10f;
        public const float ITEM_CARD_WIDTH = 280f;
        public const float ITEM_CARD_HEIGHT = 180f;

        // Font sizes
        public const int FONT_SIZE_TITLE = 18;
        public const int FONT_SIZE_SUBTITLE = 13;
        public const int FONT_SIZE_NORMAL = 11;
        public const int FONT_SIZE_SMALL = 10;
        public const int FONT_SIZE_BUTTON = 11;
        public const int FONT_SIZE_BUTTON_LARGE = 22;

        #region ???????????????????????????????????????????????????????????????
        //  COLORS - Primary Palette
        #endregion ????????????????????????????????????????????????????????????

        // Background colors
        public static readonly Color WindowBackground = new Color(0.08f, 0.08f, 0.1f, 0.98f);
        public static readonly Color SectionBackground = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        public static readonly Color CardBackground = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        public static readonly Color CardBackgroundLight = new Color(0.18f, 0.18f, 0.22f, 0.95f);
        public static readonly Color InputBackground = new Color(0.12f, 0.12f, 0.15f, 0.95f);

        // Accent colors
        public static readonly Color Accent = new Color(0.95f, 0.75f, 0.3f, 1f);
        public static readonly Color AccentLight = new Color(1f, 0.85f, 0.4f, 1f);

        // State colors
        public static readonly Color Positive = new Color(0.4f, 1f, 0.55f, 1f);
        public static readonly Color Warning = new Color(1f, 0.7f, 0.3f, 1f);
        public static readonly Color Danger = new Color(1f, 0.4f, 0.4f, 1f);
        public static readonly Color Info = new Color(0.4f, 0.7f, 1f, 1f);

        // Text colors
        public static readonly Color TextLight = new Color(0.95f, 0.95f, 0.95f, 1f);
        public static readonly Color TextMuted = new Color(0.65f, 0.65f, 0.7f, 1f);
        public static readonly Color TextDark = new Color(0.1f, 0.1f, 0.1f, 1f);

        // Button colors
        public static readonly Color ButtonNormal = new Color(0.15f, 0.15f, 0.18f, 0.95f);
        public static readonly Color ButtonHover = new Color(0.22f, 0.22f, 0.26f, 0.95f);
        public static readonly Color ButtonActive = new Color(0.25f, 0.45f, 0.65f, 0.95f);
        public static readonly Color ButtonActiveHover = new Color(0.3f, 0.52f, 0.72f, 0.95f);

        // Tab colors
        public static readonly Color TabActive = new Color(0.25f, 0.5f, 0.8f, 0.95f);
        public static readonly Color TabInactive = new Color(0.18f, 0.18f, 0.22f, 0.9f);
        public static readonly Color TabHover = new Color(0.25f, 0.25f, 0.3f, 0.95f);

        // Toggle colors
        public static readonly Color ToggleOn = new Color(0.2f, 0.6f, 0.3f, 0.95f);
        public static readonly Color ToggleOff = new Color(0.25f, 0.25f, 0.3f, 0.9f);

        // Slot colors
        public static readonly Color SlotEquipped = new Color(0.15f, 0.22f, 0.32f, 0.98f);
        public static readonly Color SlotEmpty = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        public static readonly Color SlotHover = new Color(0.22f, 0.32f, 0.45f, 0.98f);

        #region ???????????????????????????????????????????????????????????????
        //  COLORS - Semantic / Game-specific
        #endregion ????????????????????????????????????????????????????????????

        // Resources
        public static readonly Color Health = new Color(0.9f, 0.3f, 0.3f, 1f);
        public static readonly Color Mana = new Color(0.3f, 0.5f, 0.95f, 1f);
        public static readonly Color Gold = new Color(1f, 0.85f, 0.3f, 1f);
        public static readonly Color Soul = new Color(0.6f, 0.4f, 1f, 1f);
        public static readonly Color Gem = new Color(0.3f, 0.9f, 0.7f, 1f);

        // Attributes
        public static readonly Color Strength = new Color(1f, 0.5f, 0.4f, 1f);
        public static readonly Color Dexterity = new Color(0.4f, 1f, 0.5f, 1f);
        public static readonly Color Intelligence = new Color(0.5f, 0.7f, 1f, 1f);
        public static readonly Color Armor = new Color(0.7f, 0.7f, 0.8f, 1f);

        // Items
        public static readonly Color ItemWeapon = new Color(1f, 0.5f, 0.5f, 1f);
        public static readonly Color ItemArmor = new Color(0.4f, 0.7f, 1f, 1f);
        public static readonly Color ItemConsumable = new Color(0.2f, 0.9f, 0.4f, 1f);
        public static readonly Color ItemBook = new Color(0.9f, 0.7f, 0.3f, 1f);
        public static readonly Color ItemMisc = new Color(0.6f, 0.6f, 0.6f, 1f);

        #region ???????????????????????????????????????????????????????????????
        //  TEXTURES - Lazy Loaded
        #endregion ????????????????????????????????????????????????????????????

        // Window textures
        private static Texture2D _windowBgTexture;
        public static Texture2D WindowBgTexture => _windowBgTexture ??= CreateTexture(WindowBackground);

        private static Texture2D _sectionBgTexture;
        public static Texture2D SectionBgTexture => _sectionBgTexture ??= CreateTexture(SectionBackground);

        private static Texture2D _cardBgTexture;
        public static Texture2D CardBgTexture => _cardBgTexture ??= CreateTexture(CardBackground);

        private static Texture2D _cardBgLightTexture;
        public static Texture2D CardBgLightTexture => _cardBgLightTexture ??= CreateTexture(CardBackgroundLight);

        private static Texture2D _inputBgTexture;
        public static Texture2D InputBgTexture => _inputBgTexture ??= CreateTexture(InputBackground);

        // Button textures
        private static Texture2D _buttonNormalTexture;
        public static Texture2D ButtonNormalTexture => _buttonNormalTexture ??= CreateTexture(ButtonNormal);

        private static Texture2D _buttonHoverTexture;
        public static Texture2D ButtonHoverTexture => _buttonHoverTexture ??= CreateTexture(ButtonHover);

        private static Texture2D _buttonActiveTexture;
        public static Texture2D ButtonActiveTexture => _buttonActiveTexture ??= CreateTexture(ButtonActive);

        private static Texture2D _buttonActiveHoverTexture;
        public static Texture2D ButtonActiveHoverTexture => _buttonActiveHoverTexture ??= CreateTexture(ButtonActiveHover);

        // Tab textures
        private static Texture2D _tabActiveTexture;
        public static Texture2D TabActiveTexture => _tabActiveTexture ??= CreateTexture(TabActive);

        private static Texture2D _tabInactiveTexture;
        public static Texture2D TabInactiveTexture => _tabInactiveTexture ??= CreateTexture(TabInactive);

        private static Texture2D _tabHoverTexture;
        public static Texture2D TabHoverTexture => _tabHoverTexture ??= CreateTexture(TabHover);

        // Toggle textures
        private static Texture2D _toggleOnTexture;
        public static Texture2D ToggleOnTexture => _toggleOnTexture ??= CreateTexture(ToggleOn);

        private static Texture2D _toggleOffTexture;
        public static Texture2D ToggleOffTexture => _toggleOffTexture ??= CreateTexture(ToggleOff);

        // Slot textures
        private static Texture2D _slotEquippedTexture;
        public static Texture2D SlotEquippedTexture => _slotEquippedTexture ??= CreateTexture(SlotEquipped);

        private static Texture2D _slotEmptyTexture;
        public static Texture2D SlotEmptyTexture => _slotEmptyTexture ??= CreateTexture(SlotEmpty);

        private static Texture2D _slotHoverTexture;
        public static Texture2D SlotHoverTexture => _slotHoverTexture ??= CreateTexture(SlotHover);

        // Utility textures
        private static Texture2D _barFillTexture;
        public static Texture2D BarFillTexture => _barFillTexture ??= CreateTexture(Color.white);

        private static Texture2D _tooltipBgTexture;
        public static Texture2D TooltipBgTexture => _tooltipBgTexture ??= CreateTexture(new Color(0.05f, 0.05f, 0.08f, 0.98f));

        private static Texture2D _barBgTexture;
        public static Texture2D BarBgTexture => _barBgTexture ??= CreateTexture(new Color(0.12f, 0.12f, 0.15f, 0.95f));

        #region ???????????????????????????????????????????????????????????????
        //  STYLES - Lazy Loaded
        #endregion ????????????????????????????????????????????????????????????

        private static bool _stylesInitialized = false;

        // Window styles
        private static GUIStyle _windowStyle;
        public static GUIStyle WindowStyle { get { EnsureStylesInitialized(); return _windowStyle; } }

        // Text styles
        private static GUIStyle _titleStyle;
        public static GUIStyle TitleStyle { get { EnsureStylesInitialized(); return _titleStyle; } }

        private static GUIStyle _subtitleStyle;
        public static GUIStyle SubtitleStyle { get { EnsureStylesInitialized(); return _subtitleStyle; } }

        private static GUIStyle _labelStyle;
        public static GUIStyle LabelStyle { get { EnsureStylesInitialized(); return _labelStyle; } }

        private static GUIStyle _valueStyle;
        public static GUIStyle ValueStyle { get { EnsureStylesInitialized(); return _valueStyle; } }

        // Container styles
        private static GUIStyle _sectionStyle;
        public static GUIStyle SectionStyle { get { EnsureStylesInitialized(); return _sectionStyle; } }

        private static GUIStyle _cardStyle;
        public static GUIStyle CardStyle { get { EnsureStylesInitialized(); return _cardStyle; } }

        // Button styles
        private static GUIStyle _buttonStyle;
        public static GUIStyle ButtonStyle { get { EnsureStylesInitialized(); return _buttonStyle; } }

        private static GUIStyle _closeButtonStyle;
        public static GUIStyle CloseButtonStyle { get { EnsureStylesInitialized(); return _closeButtonStyle; } }

        // Tab styles
        private static GUIStyle _tabActiveStyle;
        public static GUIStyle TabActiveStyle { get { EnsureStylesInitialized(); return _tabActiveStyle; } }

        private static GUIStyle _tabInactiveStyle;
        public static GUIStyle TabInactiveStyle { get { EnsureStylesInitialized(); return _tabInactiveStyle; } }

        // Toggle styles
        private static GUIStyle _toggleOnStyle;
        public static GUIStyle ToggleOnStyle { get { EnsureStylesInitialized(); return _toggleOnStyle; } }

        private static GUIStyle _toggleOffStyle;
        public static GUIStyle ToggleOffStyle { get { EnsureStylesInitialized(); return _toggleOffStyle; } }

        // Input styles
        private static GUIStyle _inputStyle;
        public static GUIStyle InputStyle { get { EnsureStylesInitialized(); return _inputStyle; } }

        // Tooltip style
        private static GUIStyle _tooltipStyle;
        public static GUIStyle TooltipStyle { get { EnsureStylesInitialized(); return _tooltipStyle; } }

        #region ???????????????????????????????????????????????????????????????
        //  INITIALIZATION
        #endregion ????????????????????????????????????????????????????????????

        private static void EnsureStylesInitialized()
        {

            if (_stylesInitialized) return;
            //if (GUI.skin == null)
            //{
            //    return;
            //}
            //initialise own skin if needed


            InitializeStyles();
            _stylesInitialized = true;
        }

        private static void InitializeStyles()
        {
            // Window Style
            _windowStyle = new GUIStyle(GUI.skin.window);
            _windowStyle.normal.background = WindowBgTexture;
            _windowStyle.onNormal.background = WindowBgTexture;
            _windowStyle.focused.background = WindowBgTexture;
            _windowStyle.onFocused.background = WindowBgTexture;
            _windowStyle.active.background = WindowBgTexture;
            _windowStyle.onActive.background = WindowBgTexture;

            // Title Style
            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = FONT_SIZE_TITLE;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.normal.textColor = Accent;

            // Subtitle Style
            _subtitleStyle = new GUIStyle(GUI.skin.label);
            _subtitleStyle.fontSize = FONT_SIZE_SUBTITLE;
            _subtitleStyle.fontStyle = FontStyle.Bold;
            _subtitleStyle.alignment = TextAnchor.MiddleLeft;
            _subtitleStyle.normal.textColor = TextLight;

            // Label Style
            _labelStyle = new GUIStyle(GUI.skin.label);
            _labelStyle.fontSize = FONT_SIZE_NORMAL;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
            _labelStyle.normal.textColor = TextMuted;

            // Value Style
            _valueStyle = new GUIStyle(GUI.skin.label);
            _valueStyle.fontSize = FONT_SIZE_NORMAL;
            _valueStyle.fontStyle = FontStyle.Bold;
            _valueStyle.alignment = TextAnchor.MiddleRight;
            _valueStyle.normal.textColor = TextLight;

            // Section Style
            _sectionStyle = new GUIStyle(GUI.skin.box);
            _sectionStyle.normal.background = SectionBgTexture;
            _sectionStyle.padding = new RectOffset(WINDOW_PADDING, WINDOW_PADDING, WINDOW_PADDING, WINDOW_PADDING);

            // Card Style
            _cardStyle = new GUIStyle(GUI.skin.box);
            _cardStyle.normal.background = CardBgTexture;
            _cardStyle.padding = new RectOffset((int)CARD_PADDING, (int)CARD_PADDING, 8, 8);

            // Button Style
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = FONT_SIZE_BUTTON;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.normal.background = ButtonNormalTexture;
            _buttonStyle.normal.textColor = TextLight;
            _buttonStyle.hover.background = ButtonHoverTexture;
            _buttonStyle.hover.textColor = TextLight;
            _buttonStyle.active.background = ButtonNormalTexture;
            _buttonStyle.active.textColor = TextLight;
            _buttonStyle.focused.background = ButtonNormalTexture;
            _buttonStyle.focused.textColor = TextLight;

            // Close Button Style
            _closeButtonStyle = new GUIStyle(GUI.skin.button);
            _closeButtonStyle.fontSize = 14;
            _closeButtonStyle.fontStyle = FontStyle.Bold;
            _closeButtonStyle.normal.textColor = Color.white;
            _closeButtonStyle.hover.textColor = Color.white;
            _closeButtonStyle.active.textColor = Color.white;

            // Tab Active Style
            _tabActiveStyle = new GUIStyle(GUI.skin.button);
            _tabActiveStyle.fontSize = FONT_SIZE_BUTTON;
            _tabActiveStyle.fontStyle = FontStyle.Bold;
            _tabActiveStyle.normal.background = TabActiveTexture;
            _tabActiveStyle.normal.textColor = TextLight;
            _tabActiveStyle.hover.background = TabActiveTexture;
            _tabActiveStyle.hover.textColor = TextLight;
            _tabActiveStyle.active.background = TabActiveTexture;
            _tabActiveStyle.focused.background = TabActiveTexture;

            // Tab Inactive Style
            _tabInactiveStyle = new GUIStyle(GUI.skin.button);
            _tabInactiveStyle.fontSize = FONT_SIZE_BUTTON;
            _tabInactiveStyle.normal.background = TabInactiveTexture;
            _tabInactiveStyle.normal.textColor = TextMuted;
            _tabInactiveStyle.hover.background = TabHoverTexture;
            _tabInactiveStyle.hover.textColor = TextLight;
            _tabInactiveStyle.active.background = TabInactiveTexture;
            _tabInactiveStyle.focused.background = TabInactiveTexture;

            // Toggle On Style
            _toggleOnStyle = new GUIStyle(GUI.skin.button);
            _toggleOnStyle.fontSize = FONT_SIZE_SMALL;
            _toggleOnStyle.fontStyle = FontStyle.Bold;
            _toggleOnStyle.normal.background = ToggleOnTexture;
            _toggleOnStyle.normal.textColor = TextLight;
            _toggleOnStyle.hover.background = ToggleOnTexture;
            _toggleOnStyle.hover.textColor = TextLight;

            // Toggle Off Style
            _toggleOffStyle = new GUIStyle(GUI.skin.button);
            _toggleOffStyle.fontSize = FONT_SIZE_SMALL;
            _toggleOffStyle.normal.background = ToggleOffTexture;
            _toggleOffStyle.normal.textColor = TextMuted;
            _toggleOffStyle.hover.background = TabHoverTexture;
            _toggleOffStyle.hover.textColor = TextLight;

            // Input Style
            _inputStyle = new GUIStyle(GUI.skin.textField);
            _inputStyle.normal.background = InputBgTexture;
            _inputStyle.normal.textColor = TextLight;
            _inputStyle.fontSize = FONT_SIZE_NORMAL;
            _inputStyle.alignment = TextAnchor.MiddleCenter;

            // Tooltip Style
            _tooltipStyle = new GUIStyle(GUI.skin.box);
            _tooltipStyle.normal.background = TooltipBgTexture;
            _tooltipStyle.normal.textColor = TextLight;
            _tooltipStyle.fontSize = FONT_SIZE_NORMAL;
            _tooltipStyle.fontStyle = FontStyle.Bold;
            _tooltipStyle.alignment = TextAnchor.MiddleCenter;
            _tooltipStyle.padding = new RectOffset(10, 10, 6, 6);
        }

        #region ???????????????????????????????????????????????????????????????
        //  UTILITY METHODS
        #endregion ????????????????????????????????????????????????????????????

        /// <summary>
        /// Creates a solid color texture.
        /// </summary>
        public static Texture2D CreateTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        /// <summary>
        /// Draws a separator line with the accent color.
        /// </summary>
        public static void DrawSeparator(float height = 2f)
        {
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));
            GUI.color = Accent;
            GUI.DrawTexture(sepRect, BarFillTexture);
            GUI.color = Color.white;
        }

        /// <summary>
        /// Draws a separator line with a custom color.
        /// </summary>
        public static void DrawSeparator(Color color, float height = 1f)
        {
            Rect sepRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));
            GUI.color = color;
            GUI.DrawTexture(sepRect, BarFillTexture);
            GUI.color = Color.white;
        }

        /// <summary>
        /// Draws a border around a rect.
        /// </summary>
        public static void DrawBorder(Rect rect, Color color, float width = 2f)
        {
            GUI.color = color;
            // Top
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, width), BarFillTexture);
            // Bottom
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - width, rect.width, width), BarFillTexture);
            // Left
            GUI.DrawTexture(new Rect(rect.x, rect.y, width, rect.height), BarFillTexture);
            // Right
            GUI.DrawTexture(new Rect(rect.x + rect.width - width, rect.y, width, rect.height), BarFillTexture);
            GUI.color = Color.white;
        }

        /// <summary>
        /// Draws a progress bar.
        /// </summary>
        public static void DrawProgressBar(Rect rect, float progress, Color fillColor)
        {
            GUI.DrawTexture(rect, BarBgTexture);

            if (progress > 0)
            {
                Rect fillRect = new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(progress), rect.height);
                GUI.color = fillColor;
                GUI.DrawTexture(fillRect, BarFillTexture);
                GUI.color = Color.white;
            }
        }

        /// <summary>
        /// Creates a label style with a specific color.
        /// </summary>
        public static GUIStyle CreateLabelStyle(Color color, int fontSize = FONT_SIZE_NORMAL, FontStyle fontStyle = FontStyle.Normal)
        {
            EnsureStylesInitialized();
            GUIStyle style = new GUIStyle(_labelStyle);
            style.normal.textColor = color;
            style.fontSize = fontSize;
            style.fontStyle = fontStyle;
            return style;
        }

        /// <summary>
        /// Creates a value style with a specific color.
        /// </summary>
        public static GUIStyle CreateValueStyle(Color color, int fontSize = FONT_SIZE_NORMAL)
        {
            EnsureStylesInitialized();
            GUIStyle style = new GUIStyle(_valueStyle);
            style.normal.textColor = color;
            style.fontSize = fontSize;
            return style;
        }

        /// <summary>
        /// Gets a color for durability percentage.
        /// </summary>
        public static Color GetDurabilityColor(float percentage)
        {
            if (percentage > 0.75f) return Positive;
            if (percentage > 0.5f) return new Color(0.95f, 0.85f, 0.25f);
            if (percentage > 0.25f) return Warning;
            return Danger;
        }

        /// <summary>
        /// Gets the color for a time-based status (e.g., time since last action).
        /// </summary>
        public static Color GetTimeStatusColor(double seconds, double goodThreshold = 5, double warningThreshold = 15)
        {
            if (seconds < goodThreshold) return Positive;
            if (seconds < warningThreshold) return Warning;
            return Danger;
        }
    }
}
