using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarrativePlanning.DomainBuilder
{
    internal class InitialKnowledgeBuilder
    {
        public static NarrativePlanning.WorldState parseInitialKnowledge(WorldState initialKnowledge, JSONDomain.InitialKnowledge jsonK)
        {
            if (jsonK != null)
            {
                foreach (string s in jsonK.T)
                {
                    string k = s.Substring(1, s.Length - 2);
                    if (!initialKnowledge.tWorld.ContainsKey(k))
                        initialKnowledge.tWorld.Add(k, 0);
                }
                foreach (string s in jsonK.F)
                {
                    string k = s.Substring(1, s.Length - 2);
                    if (!initialKnowledge.fWorld.ContainsKey(k))
                        initialKnowledge.fWorld.Add(k, 0);
                }
            }
            return initialKnowledge;
        }
    }
}
