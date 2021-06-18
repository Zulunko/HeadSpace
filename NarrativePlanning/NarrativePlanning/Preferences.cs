using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarrativePlanning
{
    // Will work into parsing later. For now, manually filling data for hitman_preference domain.
    public class Preferences
    {
        private Dictionary<string, float> actionPrefs = new Dictionary<string, float>()
        {
           // { "stab", 1f },
           // { "kill-loud", -1f },
           // { "eat-poisoned-food", -1f },
            //{ "move", -1f },
            //{ "steal-knife", 1 },
        };
        private Dictionary<string, float> propositionPrefs = new Dictionary<string, float>()
        {
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
