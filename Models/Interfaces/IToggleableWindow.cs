using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public interface IToggleableWindow
    {
        bool IsShown { get; }
        KeyCode KeyCode { get;  }
        void ToggleUI();
    }
}


