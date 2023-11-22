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
        //multipref
        private static Dictionary<string, float> NoPrefs = new Dictionary<string, float>()
        {
        };
        private static Dictionary<string, float> Tactician = new Dictionary<string, float>() {
                {"solve-puzzle", 1f},
                {"sneak-by-camp", 1f },
                {"sneak-elementals", 1f },
                {"sleep-ritualists", 1f },
                {"fight-ritualists", -1f }
        };
        private static Dictionary<string, float> Harvester = new Dictionary<string, float>()
        {
                {"gather", 1f },
                {"chop", 1f },
                {"mine", 1f }
        };
        private static Dictionary<string, float> Crafter = new Dictionary<string, float>()
        {
                {"craft-mortar-and-pestle", 1f },
                {"craft-chameleon-salve", 1f },
                {"craft-sword", 1f },
                {"craft-torch", 1f },
                {"craft-rope", 1f },
                {"craft-house", 1f },
                {"craft-firesuit", 1f },
                {"craft-sleepbomb", 1f },
                {"craft-demobomb", 1f }
        };
        private static Dictionary<string, float> Fighter = new Dictionary<string, float>()
        {
            {"attack-camp", 1f },
            {"fight-bandits", 1f },
            {"threaten-goblin", 1f },
            {"fight-elementals", 1f },
            {"fight-ritualists", 1f },
            {"destroy-monolith", 1f }
        };
        private static Dictionary<string, float> Helper = new Dictionary<string, float>()
        {
            {"captain-boat", 0.1f },
            {"disembark", 0.1f },
            {"exit", 0.1f },
            {"mine", 0.1f },
            {"chop", 0.1f },
            {"gather", 0.1f },
            {"craft-mortar-and-pestle", 0.1f },
            {"craft-chameleon-salve", 0.1f },
            {"craft-sword", 0.1f },
            {"craft-torch", 0.1f },
            {"craft-rope", 0.1f },
            {"craft-house", 0.1f },
            {"craft-firesuit", 0.1f },
            {"craft-sleepbomb", 0.1f },
            {"craft-demobomb", 0.1f },
            {"solve-puzzle", 0.1f },
            {"attack-camp", 0.1f },
            {"sneak-by-camp", 0.1f },
            {"fight-bandits", 0.1f },
            {"pay-goblin", 0.1f },
            {"threaten-goblin", 0.1f },
            {"gift-goblin", 0.1f },
            {"fight-elementals", 0.1f },
            {"sneak-elementals", 0.1f },
            {"fight-ritualists", 0.1f },
            {"sleep-ritualists", 0.1f },
            {"destroy-monolith", 0.1f },
            {"disable-monolith", 0.1f }
        };

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
            /*
            {"mount", (3.2f-3)/2 },
            {"approach-loud", (1.56f-3)/2 },
            {"shoot", (2.9f-3)/2},
            {"threaten-leader", (3.3f-3)/2 },
            {"hire-npc", (4f-3)/2 },
            {"capture-bandit", (3.2f-3)/2 },
            {"approach-stealth", (4.9f-3)/2 },
            {"pickup", (4.8f-3)/2 }
            */

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
            /*
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
            }*/

            //simple_multiagent
            /*
            {"Player1", new Dictionary<string, float>
            {
            }
            },
            {"Player2", new Dictionary<string, float>
            {
                {"button-press", 1f },
            }
            },
            {"Player3", new Dictionary<string, float>
            {
                {"kill", 1f }
            }
            },
            {"Player4", new Dictionary<string, float>
            {
                {"steal-from", 1f },
            }
            }
            */

            //multipref
            {"Red", NoPrefs },
            {"Blue", NoPrefs },
            {"Green", NoPrefs }
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

        private const float _scaleval = 0;

        private float _Scaled(float pref)
        {
            if (_scaleval == 0)
                return pref;
            if (pref >= 0)
                return pref * (1 - _scaleval) + _scaleval;
            else
                return pref * (1 + _scaleval) + _scaleval;
        }

        public float GetActionPreference(string action)
        {
            //Console.WriteLine("ACTIONPREF " + action);
            if (actionPrefs.ContainsKey(action))
                return _Scaled(actionPrefs[action]);
            return _Scaled(0);
        }

        public float GetActionPreferenceForCharacter(string character, string action)
        {
            if (!characterActionPrefs.ContainsKey(character))
            {
                UnityConsole.Log("Character [" + character + "] not found for action [" + action + "].", LOGMODE.HEURISTIC);
                return _Scaled(0);
            }
            if (characterActionPrefs[character].ContainsKey(action))
            {
                //Console.WriteLine("Character " + character + ": " + action + ", " + characterActionPrefs[character][action]);
                return _Scaled(characterActionPrefs[character][action]);
            }
            return _Scaled(0);
        }

        public float GetPropositionPreference(string prop, bool isTrue)
        {
            if (propositionPrefs.ContainsKey(prop))
            {
                if (isTrue)
                    return _Scaled(propositionPrefs[prop]);
                else
                    return _Scaled(-propositionPrefs[prop]);
            }
            return _Scaled(0);
        }
    }
}
