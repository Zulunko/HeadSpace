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
            //{"approach-loud", 1f}
            //{"hire-npc", 1f },
            //{"capture-bandit", 1f }
            //{"sneak-past", 1f },
            //{"break-door", -0.5f }
            // { "stab", 1f },
            // { "kill-loud", -1f },
            // { "eat-poisoned-food", -1f },
            //{ "move", -1f },
            //{ "steal-knife", 1 },

            //--------WESTERN
            //Fighter
            /*
            {"mount", 0.5f },
            {"approach-loud", 0.5f },
            {"shoot", 0.8325f},
            {"threaten-leader", 0.835f },
            {"hire-npc", 0.0825f },
            {"capture-bandit", -0.25f },
            {"approach-stealth", 0f }
            */

            //Tactician
            /*
            {"mount", -0.165f },
            {"approach-loud", -0.835f },
            {"shoot", -0.33f },
            {"threaten-leader", -0.33f },
            {"hire-npc", 0.835f },
            {"capture-bandit", 0.165f },
            {"approach-stealth", 1f }
            */

            //-------SCIFI
            //Fighter
            /*
            {"shoot", 1f },
            {"sneak-past", -0.835f },
            {"break-door", 1f },
            {"overload-core", 1f },
            {"defend-core", 1f },
            {"disable-turrets", -0.835f },
            {"disarm-core", -0.835f }
            */

            //Tactician
            ///*
            {"shoot", -0.9175f },
            {"sneak-past", 0.835f },
            {"break-door", -0.835f },
            {"overload-core", -0.75f },
            {"defend-core", 0.1665f },
            {"disable-turrets", 1f },
            {"disarm-core", 1f }
            //*/
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
