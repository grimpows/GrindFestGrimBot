using UnityEngine;

namespace Scripts.Models
{
    public interface IToggleableWindow
    {
        bool IsShown { get; }
        KeyCode KeyCode { get; }
        void ToggleUI();
    }
}


