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
            //{"stab", 1f },
            //{"kill-loud", -1f },
            //{"eat-poisoned-food", -1f },
            //{"move", 1f },
            //{"steal-knife", 1 },

            //--------WESTERN
            //Fighter
            /*
            {"mount", (4.22f-3)/2 },
            {"approach-loud", (5f-3)/2 },
            {"shoot", (4.7f-3)/2},
            {"threaten-leader", (4.3f-3)/2 },
            {"hire-npc", (2.4f-3)/2 },
            {"capture-bandit", (2.6f-3)/2 },
            {"approach-stealth", (3.1f-3)/2 },
            {"pickup", (4.3f-3)/2 }
            */

            //Tactician
            ///*
            {"mount", (3.2f-3)/2 },
            {"approach-loud", (1.56f-3)/2 },
            {"shoot", (2.9f-3)/2},
            {"threaten-leader", (3.3f-3)/2 },
            {"hire-npc", (4f-3)/2 },
            {"capture-bandit", (3.2f-3)/2 },
            {"approach-stealth", (4.9f-3)/2 },
            {"pickup", (4.8f-3)/2 }
            //*/

            //-------SCIFI
            //Fighter
            /*
            {"shoot", (4.6f-3)/2 },
            {"entermain", (4f-3)/2 },
            {"break-door", (4.1f-3)/2 },
            {"overload-core", (4.3f-3)/2 },
            {"defend-core", (4.4f-3)/2 },
            {"enterbay", (2.8f-3)/2 },
            {"sneak-past", (2.1f-3)/2 },
            {"disable-turrets", (1.8f-3)/2 },
            {"disarm-core", (2.4f-3)/2 },
            {"plant-bomb", (4f-3)/2 }
            */

            //Tactician
            /*
            {"shoot", (3.11f-3)/2 },
            {"entermain", (1.7f-3)/2 },
            {"break-door", (2.89f-3)/2 },
            {"overload-core", (1.8f-3)/2 },
            {"defend-core", (3.3f-3)/2 },
            {"enterbay", (4.2f-3)/2 },
            {"sneak-past", (4.5f-3)/2 },
            {"disable-turrets", (4.6f-3)/2 },
            {"disarm-core", (4.6f-3)/2 },
            {"plant-bomb", (4f-3)/2 }
            */

        };
        private Dictionary<string, float> propositionPrefs = new Dictionary<string, float>()
        {
            //{"hired Sheriff", 1f },
            //{"hired Mercenaries", 1f },
            //{"on Player1 Horse", 1f }
            //{"at Player1 AirVent", 1f }
            //{ "has ChefKnife Player1", 1 },
        };

        private Dictionary<string, Dictionary<string, float>> characterActionPrefs = new Dictionary<string, Dictionary<string, float>>
        {
            /*simple_preference
            {"Player1", new Dictionary<string, float>
            {
                {"shoot", 1}
            }
            },
            {"Player2", new Dictionary<string, float>
            {
                {"push-button", 1}
            }
            }*/

            //valheim_preference
            {"Player1", new Dictionary<string, float>
            {
                {"craft-armor", 1f },
                {"craft-pot", 1f },

                {"harvest", -1f },
                {"deliver-plant", -1f },
                {"mine", -1f },
                {"deliver-ore", -1f },
                {"hunt", -1f },
                {"deliver-hide", -1f }
            }
            },
            {"Player2", new Dictionary<string, float>
            {
                {"harvest", 1f },
                {"deliver-plant", 1f },

                {"craft-armor", -1f },
                {"craft-pot", -1f },
                {"mine", -1f },
                {"deliver-ore", -1f },
                {"hunt", -1f },
                {"deliver-hide", -1f }
            }
            },
            {"Player3", new Dictionary<string, float>
            {
                {"mine", 1f },
                {"deliver-ore", 1f },

                {"craft-armor", -1f },
                {"craft-pot", -1f },
                {"harvest", -1f },
                {"deliver-plant", -1f },
                {"hunt", -1f },
                {"deliver-hide", -1f }
            }
            },
            {"Player4", new Dictionary<string, float>
            {
                {"hunt", 1f },
                {"deliver-hide", 1f },

                {"craft-armor", -1f },
                {"craft-pot", -1f },
                {"harvest", -1f },
                {"deliver-plant", -1f },
                {"mine", -1f },
                {"deliver-ore", -1f }
            }
            }
        };

        /*
         *
                {"craft-armor", -1f },
                {"craft-pot", -1f },
                {"harvest", -1f },
                {"deliver-plant", -1f },
                {"mine", -1f },
                {"deliver-ore", -1f },
                {"hunt", -1f },
                {"deliver-hide", -1f }
         */

        public float GetActionPreference(string action)
        {
            //Console.WriteLine("ACTIONPREF " + action);
            if (actionPrefs.ContainsKey(action))
                return actionPrefs[action];
            return 0;
        }

        public float GetActionPreferenceForCharacter(string character, string action)
        {
            if (characterActionPrefs[character].ContainsKey(action))
            {
                //Console.WriteLine("Character " + character + ": " + action + ", " + characterActionPrefs[character][action]);
                return characterActionPrefs[character][action];
            }
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
