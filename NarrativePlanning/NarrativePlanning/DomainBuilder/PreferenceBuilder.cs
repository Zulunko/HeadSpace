using JSONDomain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarrativePlanning.DomainBuilder
{
    internal class PreferenceBuilder
    {
        public static NarrativePlanning.Preferences parsePreferences(JSONDomain.CharacterPreference[] jsonprefs)
        {
            NarrativePlanning.Preferences preferences = new NarrativePlanning.Preferences();
            if (jsonprefs != null)
            {
                foreach (CharacterPreference pref in jsonprefs)
                {
                    Dictionary<string, float> actionPrefs = new Dictionary<string, float>();
                    foreach (ActionPreference ap in pref.ActionPreferences)
                    {
                        actionPrefs.Add(ap.Action, ap.Value);
                    }
                    preferences.AddCharacterActionPrefs(pref.Name, actionPrefs);
                    Dictionary<string, float> propPrefs = new Dictionary<string, float>();
                    foreach (PropositionPreference pp in pref.PropositionPreferences)
                    {
                        propPrefs.Add(pp.Proposition, pp.Value);
                    }
                    preferences.AddPropositionPrefs(propPrefs);
                }
            }
            return preferences;
        }
    }
}
