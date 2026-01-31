using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.Models
{
    public class Bot_Agent_TravelerUI
    {
        private Bot_Agent_Traveler _travelerAgent;
      
        public Bot_Agent_TravelerUI(Bot_Agent_Traveler travelerAgent)
        {
            _travelerAgent = travelerAgent;
        }


        public void DrawTravelerAgentPanel(Rect contentArea)
        {
            if (_travelerAgent == null)
                return;


        }

      
    }
}
