using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarrativePlanning
{
    // Will work into parsing later. For now, manually filling data for domains.
    public class Preferences
    {
        private Dictionary<string, float> actionPrefs = new Dictionary<string, float>()
        {
            //{"shoot", 1f },
            {"approach-loud", 1f}
            //{"hire-npc", 1f },
            //{"capture-bandit", 1f }
            //{"sneak-past", 1f },
            //{"break-door", -0.5f }
           // { "stab", 1f },
           // { "kill-loud", -1f },
           // { "eat-poisoned-food", -1f },
            //{ "move", -1f },
            //{ "steal-knife", 1 },
        };
        private Dictionary<string, float> propositionPrefs = new Dictionary<string, float>()
        {
            //{"hired Sheriff", 1f },
            //{"hired Mercenaries", 1f },
            //{"on Player1 Horse", 1f }
            //{"at Player1 AirVent", 1f }
            //{ "has ChefKnife Player1", 1 },
        };

        public float GetActionPreference(string action)
        {
            //Console.WriteLine("ACTIONPREF " + action);
            if (actionPrefs.ContainsKey(action))
                return actionPrefs[action];
            return 0;
        }

        public float GetPropositionPreference(string prop, bool isTrue)
        {
            if (propositionPrefs.ContainsKey(prop))
            {
                if (isTrue)
                    return propositionPrefs[prop];
                else
                    return -propositionPrefs[prop];
            }
            return 0;
        }
    }
}
