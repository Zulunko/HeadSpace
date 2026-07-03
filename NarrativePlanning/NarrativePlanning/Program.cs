using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Principal;

namespace NarrativePlanning
{
    [Flags]
    public enum LOGMODE : short
    {
        ERROR = 0, // Output that should always print
        MEMOIZE = 1, // Output relating to memoization and rewinding
        PLANNER = 2, // Output related to the planning process (not heuristic)
        HEURISTIC = 4, // Output related to the heuristic calculations (RPGs, etc)
        WORLDSTATE = 8, // Output to dump world state on each step
        RELAXEDPLAN = 16, // Print the relaxed plan (pruned action list) each step
        DEPENDENCE = 32,
        ALL = 63
    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            UnityConsole.WriteLine("Hello World!");

            ////////////////////////////////////////////////////////
            DomainBuilder.JSONDomainBuilder j = new NarrativePlanning.DomainBuilder.JSONDomainBuilder("../../JSON Files/multi_pref.json");

            NarrativePlanning.PlanningProblem problem = new NarrativePlanning.PlanningProblem(j.initial, j.goal, j.operators, j.characterPreferences, j.initialKnowledge, j.observablePrefixes, j.exclusivePrefixes);

            /////// STEP 2: GENERATE PLAN 
            Stopwatch watch = new Stopwatch();
            //NarrativePlanning.Plan plan = problem.FFNoPreferenceSolution();
            watch.Start();
            NarrativePlanning.Plan plan = problem.FFPreferenceSolution(PLANNING_MODE.MULTI);
            //NarrativePlanning.Plan plan = problem.FFKnowledgePrefSolution(PLANNING_MODE.KNOWLEDGE);
            watch.Stop();
            UnityConsole.WriteLine("PREFS: " + watch.ElapsedMilliseconds + "ms");
            if (plan == null)
                NarrativePlanning.UnityConsole.WriteLine("Planning complete. No plan found");
            else
            {
                NarrativePlanning.UnityConsole.WriteLine("Planning complete. Plan is : " + plan.toString());
                // version 2
                /*j.characterPreferences = new Preferences();
                j.characterPreferences.AddCharacterActionPrefs("RedPlayer", new Dictionary<string, float>()
                {
                    { "open-chest", 0 },
                    { "pickup-hover-wand", 0 },
                    { "cast-hover-spell", 1 },
                    { "pickup-goal-key", 0 },
                    { "unlock-goal-door", -1 },
                    { "open-goal-door", 1 },
                    { "move-to-goal", 1 },
                });
                j.characterPreferences.AddCharacterActionPrefs("BluePlayer", new Dictionary<string, float>()
                {
                    { "open-chest", 1 },
                    { "pickup-hover-wand", 1 },
                    { "cast-hover-spell", -1 },
                    { "pickup-goal-key", -1 },
                    { "unlock-goal-door", 1 },
                    { "open-goal-door", 0 },
                    { "move-to-goal", 1 },
                });*/
                GetStrongDependencies(j, plan);
            }
            while (true) { } // infinite loop to wait for user to exit manually
            return;
        }

        public static void GetStrongDependencies(DomainBuilder.JSONDomainBuilder j, Plan p)
        {
            Stopwatch watch = new Stopwatch();
            // Basically, what I need to start out with is finding causal links in p. In order to do that, I need to know what the preconditions/effects of each step are.
            // Then I can work backwards and determine what granted those preconditions/effects.
            // I can be intelligent about this; first, don't need to start with the first step. Second, for a given step's precondition, I only need to check other steps where:
            //  1. the precondition is not already satisfied in the initial state
            //  2. the step is before this step
            //  3. the step is executed by the other agent (not the agent of this step)
            // One way to do this is to actually check the world states in the plan, rather than starting from operators. We can infer the effects of an action based on how the world state changed.
            // We can not do the same for preconditions, so we will need to use the grounded operators specifically for the preconditions.
            // ...might as well just get the grounded operator for every step, then?
            List<Operator> stepOperators = new List<Operator>();
            foreach (Tuple<string, WorldState> step in p.steps)
            {
                stepOperators.Add(j.operators.Find(xy => xy.text.Equals(step.Item1)));
                //stepOperators.Add(Operator.getOperator(j.operators, step.Item1));
            }
            // Create a dictionary of needed preconditions as you iterate backwards, find operators which supply those preconditions
            // Dictionary<Tuple<bool, proposition>, List<stepIdx>>
            Dictionary<string, List<int>> searchDictT = new Dictionary<string, List<int>>();
            Dictionary<string, List<int>> searchDictF = new Dictionary<string, List<int>>();
            // List<stepIdx1, stepIdx2, bool, proposition>
            List<Tuple<int, int, Tuple<bool, string>>> causalLinks = new List<Tuple<int, int, Tuple<bool, string>>>();

            for (int i = stepOperators.Count - 1; i >= 0; i--)
            {
                if (stepOperators[i] == null) continue;
                UnityConsole.Log("" + i, LOGMODE.DEPENDENCE);
                // Detect the end of causal links first (more efficient, also avoids issues where someone accidentally puts a prop as both a precond and effect)
                List<string> effT = stepOperators[i].effT.Keys.OfType<string>().ToList();
                List<string> effF = stepOperators[i].effF.Keys.OfType<string>().ToList();
                foreach (string effect in effT)
                {
                    UnityConsole.Log("effT: " + effect, LOGMODE.DEPENDENCE);
                    if (searchDictT.ContainsKey(effect))
                    {
                        Tuple<bool, string> propTuple = new Tuple<bool, string>(true, effect);
                        foreach (int stepIdx in searchDictT[effect])
                            causalLinks.Add(new Tuple<int, int, Tuple<bool, string>>(stepIdx, i, propTuple));
                        searchDictT.Remove(effect);
                    }
                }
                foreach (string effect in effF)
                {
                    UnityConsole.Log("effF: " + effect, LOGMODE.DEPENDENCE);
                    if (searchDictF.ContainsKey(effect))
                    {
                        Tuple<bool, string> propTuple = new Tuple<bool, string>(false, effect);
                        foreach (int stepIdx in searchDictF[effect])
                            causalLinks.Add(new Tuple<int, int, Tuple<bool, string>>(stepIdx, i, propTuple));
                        searchDictF.Remove(effect);
                    }
                }

                // Set up what we need for the start of causal links
                List<string> preT = stepOperators[i].preT.Keys.OfType<string>().ToList();
                List<string> preF = stepOperators[i].preF.Keys.OfType<string>().ToList();
                foreach (string precond in preT)
                {
                    UnityConsole.Log("preT: " + precond, LOGMODE.DEPENDENCE);
                    if (searchDictF.ContainsKey(precond)) UnityConsole.Log("ERROR: Adding precond " + precond + " for step " + i + " (" + stepOperators[i].text + ") to searchDictT when it is still open in searchDictF!", LOGMODE.ERROR);
                    if (searchDictT.ContainsKey(precond))
                    {
                        searchDictT[precond].Add(i);
                    }
                    else
                    {
                        searchDictT.Add(precond, new List<int>() { i });
                    }
                }
                foreach (string precond in preF)
                {
                    UnityConsole.Log("preF: " + precond, LOGMODE.DEPENDENCE);
                    if (searchDictT.ContainsKey(precond)) UnityConsole.Log("ERROR: Adding precond " + precond + " for step " + i + " (" + stepOperators[i].text + ") to searchDictF when it is still open in searchDictT!", LOGMODE.ERROR);
                    if (searchDictF.ContainsKey(precond))
                    {
                        searchDictF[precond].Add(i);
                    }
                    else
                    {
                        searchDictF.Add(precond, new List<int>() { i });
                    }
                }
            }

            // We should now have a (probably big) list of causal links! All steps that are not in a causal link must have their preconditions satisfied by the initial state.
            UnityConsole.Log("\nDirect Causal Links:", LOGMODE.DEPENDENCE);
            foreach (Tuple<int, int, Tuple<bool, string>> link in causalLinks)
            {
                int i = 0;
                UnityConsole.Log("Causal Link " + i + ": " + "(" + link.Item2 + ") " + p.steps[link.Item2].Item1 + " TO " + "(" + link.Item1 + ") " + p.steps[link.Item1].Item1 + " ON " + link.Item3.Item1 + " " + link.Item3.Item2, LOGMODE.DEPENDENCE);
                i++;
            }

            watch.Start();
            // FOR DERIVED CAUSAL LINKS: only look for derived *dependencies* (i.e. stop when we detect the same character is in the causal link)
            List<Tuple<int, int, Tuple<bool, string>>> derivedDependencies = new List<Tuple<int, int, Tuple<bool, string>>>();
            // Causal chain: 
            List<Tuple<List<int>, int, Tuple<bool, string>>> iaCausalChains = new List<Tuple<List<int>, int, Tuple<bool, string>>>();
            foreach (Tuple<int, int, Tuple<bool, string>> link in causalLinks)
            {
                if (stepOperators[link.Item1].character == "") continue; // don't want the dependent to be a non-agent action
                // For each causal link effect, derive all dependencies that are provided by a different character.
                List<int> derivedSource = InterAgentLinkSearch(link.Item1, causalLinks, stepOperators, stepOperators[link.Item1].character);
                List<int> removeList = new List<int>();
                List<int> dupeList = new List<int>();
                foreach (int val in derivedSource)
                {
                    if (stepOperators[val].character == "" || dupeList.Contains(val))
                        removeList.Add(val);
                    else
                        dupeList.Add(val);
                }
                foreach (int val in removeList)
                {
                    derivedSource.Remove(val);
                }


                // Need to save the chains for preference detection
                iaCausalChains.Add(new Tuple<List<int>, int, Tuple<bool, string>>(derivedSource, link.Item1, link.Item3));
                foreach (int src in derivedSource)
                {
                    derivedDependencies.Add(new Tuple<int, int, Tuple<bool, string>>(link.Item1, src, link.Item3));
                }
            }
            watch.Stop();

            UnityConsole.Log("\n(" + watch.ElapsedMilliseconds + "ms) Inter-Agent Dependent Pairs:", LOGMODE.DEPENDENCE);
            foreach (Tuple<int, int, Tuple<bool, string>> dependency in derivedDependencies)
            {
                int i = 0;
                UnityConsole.Log("Dependency between character " + stepOperators[dependency.Item2].character + " on step " + dependency.Item2 + " and " + stepOperators[dependency.Item1].character + " on step " + dependency.Item1 + " on prop " + dependency.Item3.Item1 + " " + dependency.Item3.Item2, LOGMODE.DEPENDENCE);
                i++;
            }

            // Trim causal links to only inter-agent dependencies
            /*
            List<Tuple<int, int, Tuple<bool, string>>> dependencies = new List<Tuple<int, int, Tuple<bool, string>>>();
            foreach (Tuple<int, int, Tuple<bool, string>> link in causalLinks)
            {
                if (stepOperators[link.Item1].character != stepOperators[link.Item2].character)
                {
                    dependencies.Add(link);
                }
            }
            UnityConsole.Log("\nInter-Agent Dependencies:", LOGMODE.DEPENDENCE);
            foreach (Tuple<int, int, Tuple<bool, string>> dependency in dependencies)
            {
                UnityConsole.Log("Dependency between character " + stepOperators[dependency.Item2].character + " on step " + dependency.Item2 + " and " + stepOperators[dependency.Item1].character + " on step " + dependency.Item1 + " on prop " + dependency.Item3.Item1 + " " + dependency.Item3.Item2, LOGMODE.DEPENDENCE);
            }
            */
            watch.Restart();
            List<Tuple<int, int, Tuple<bool, string>>> strongDependencies = new List<Tuple<int, int, Tuple<bool, string>>>();
            // Detect if inter-agent dependencies are "strong dependencies"
            foreach (Tuple<int, int, Tuple<bool, string>> dependency in derivedDependencies)
            {
                // Retrieve inter-agent dependency's initial world state (prior to first step)
                WorldState depInitial = new WorldState(p.steps[dependency.Item2 - 1].Item2.tWorld, p.steps[dependency.Item2 - 1].Item2.fWorld, j.initial.characters);
                /* XXX Create goal state with only the needed proposition --- this seems wrong, going to do it based on the action effects instead. This would not work if the player traps themselves trying to satisfy the precond.
                WorldState depGoal;
                System.Collections.Hashtable goalHash = new System.Collections.Hashtable();
                goalHash.Add(dependency.Item3.Item2, 1);
                if (dependency.Item3.Item1)
                {
                    depGoal = new WorldState(goalHash, new System.Collections.Hashtable(), j.initial.characters);
                }
                else
                {
                    depGoal = new WorldState(new System.Collections.Hashtable(), goalHash, j.initial.characters);
                }
                */
                // Create goal state as effects of dependent action
                WorldState depGoal = new WorldState(stepOperators[dependency.Item1].effT, stepOperators[dependency.Item1].effF, j.initial.characters);
                // Attempt to create plan from initial to goal using only the executing agent
                List<Operator> currCharOps = new List<Operator>();
                foreach (Operator o in j.operators)
                {
                    if (o.character == stepOperators[dependency.Item1].character) {
                        //UnityConsole.WriteLine(o.text);
                        currCharOps.Add(o);
                    }
                }
                PlanningProblem problem = new PlanningProblem(depInitial, depGoal, currCharOps, j.characterPreferences, j.initialKnowledge, j.observablePrefixes, j.exclusivePrefixes);
                Plan depPlan = problem.FFPreferenceSolution(PLANNING_MODE.MULTI, p.steps.Count + dependency.Item1 - dependency.Item2, 5000);
                // If fail: strong dependency detected
                if (depPlan == null)
                {
                    //UnityConsole.Log("depPlan was null for dependency from step " + dependency.Item2 + " to " + dependency.Item1, LOGMODE.DEPENDENCE);
                    strongDependencies.Add(dependency);
                }
                // If success: compare length of plan against length of dependency to determine if it's a strong dependency
                else
                {
                    //UnityConsole.Log("depPlan was length " + depPlan.steps.Count + " for dependency from step " + dependency.Item2 + " to " + dependency.Item1, LOGMODE.DEPENDENCE);
                    if (depPlan.steps.Count > (p.steps.Count + dependency.Item1 - dependency.Item2))
                    {
                        //UnityConsole.Log("Strong dependency detected due to plan length", LOGMODE.DEPENDENCE);
                        strongDependencies.Add(dependency);
                    }
                }
            }
            watch.Stop();
            UnityConsole.Log("\n("+watch.ElapsedMilliseconds+"ms) Strong Dependencies:", LOGMODE.DEPENDENCE);
            foreach (Tuple<int, int, Tuple<bool, string>> dependency in strongDependencies)
            {
                UnityConsole.Log("Strong dependency between character " + stepOperators[dependency.Item2].character + " on step " + dependency.Item2 + " and " + stepOperators[dependency.Item1].character + " on step " + dependency.Item1 + " on prop " + dependency.Item3.Item1 + " " + dependency.Item3.Item2, LOGMODE.DEPENDENCE);
            }
            watch.Restart();

            // Pref deps
            /* old
            foreach (Tuple<List<int>, int, Tuple<bool, string>> iaChain in iaCausalChains)
            {
                if (iaChain.Item1.Count == 0) continue;
                List<int> generalChain = GeneralLinkSearch(iaChain.Item2, causalLinks, stepOperators);
                Tuple<List<int>, int, Tuple<bool, string>> generalCausalChain = new Tuple<List<int>, int, Tuple<bool, string>>(generalChain, iaChain.Item2, iaChain.Item3);

                // First, find the beginning of the IA chain.
                int startStep = iaChain.Item1.Min();
                // Then, calculate the action preferences for all agents in the current chain
                float basePrefs = 0;
                List<string> participatingCharacters = new List<string>();
                for (int i = startStep; i <= iaChain.Item2; i++)
                {
                    basePrefs += j.characterPreferences.GetActionPreferenceForCharacter(stepOperators[i].character, stepOperators[i].name);
                    if (!participatingCharacters.Contains(stepOperators[i].character))
                        participatingCharacters.Add(stepOperators[i].character);
                }
                float avgBasePref = basePrefs / (iaChain.Item2 - startStep + 1);
                string dependentCharacter = stepOperators[iaChain.Item2].character;
                participatingCharacters.Remove(dependentCharacter);
                float highestAvgPref = float.MinValue;
                foreach (string swapCharacter in participatingCharacters)
                {
                    float avgPrefs = 0;
                    for (int i = startStep; i <= iaChain.Item2; i++)
                    {
                        if (stepOperators[i].character == swapCharacter)
                            avgPrefs += j.characterPreferences.GetActionPreferenceForCharacter(dependentCharacter, stepOperators[i].name);
                        else if (stepOperators[i].character == dependentCharacter)
                            avgPrefs += j.characterPreferences.GetActionPreferenceForCharacter(swapCharacter, stepOperators[i].name);
                        else
                            avgPrefs += j.characterPreferences.GetActionPreferenceForCharacter(stepOperators[i].character, stepOperators[i].name);
                    }
                    avgPrefs = avgPrefs / (iaChain.Item2 - startStep + 1);
                    if (avgPrefs > highestAvgPref)
                        highestAvgPref = avgPrefs;
                }

                if (avgBasePref > highestAvgPref)
                    UnityConsole.Log("Pref dep detected on chain ending with " + iaChain.Item2 + " with value " + (avgBasePref - highestAvgPref), LOGMODE.DEPENDENCE);
            }
            */

            // new pref dep
            foreach (Tuple<List<int>, int, Tuple<bool, string>> iaChain in iaCausalChains)
            {
                if (iaChain.Item1.Count == 0) continue;
                List<int> generalChain = GeneralLinkSearch(iaChain.Item2, causalLinks, stepOperators);
                Tuple<List<int>, int, Tuple<bool, string>> generalCausalChain = new Tuple<List<int>, int, Tuple<bool, string>>(generalChain, iaChain.Item2, iaChain.Item3);

                // First, find the beginning of the IA chain.
                int iaChainStart = iaChain.Item1.Min();

                // Need to iterate through each agent involved in the IA chain
                List<string> participatingAgents = new List<string>();
                for (int i = iaChainStart; i <= iaChain.Item2; i++)
                {
                    if (iaChain.Item1.Contains(i) && !participatingAgents.Contains(stepOperators[i].character))
                        participatingAgents.Add(stepOperators[i].character);
                }

                string dependentCharacter = stepOperators[iaChain.Item2].character;

                foreach (string agent in participatingAgents)
                {
                    // Find beginning of this agent's actions in the IA chain
                    int agentStartStep = int.MaxValue;
                    // Also calculate prefs at the same time, because why not
                    float prefVal = 0;
                    float swapPrefVal = 0;
                    int prefStepCount = 0;
                    foreach (int step in iaChain.Item1)
                    {
                        if (stepOperators[step].character == agent)
                        {
                            prefVal += j.characterPreferences.GetActionPreferenceForCharacter(agent, stepOperators[step].name);
                            swapPrefVal += j.characterPreferences.GetActionPreferenceForCharacter(dependentCharacter, stepOperators[step].name);
                            prefStepCount++;
                            if (step < agentStartStep)
                                agentStartStep = step;
                        }
                    }
                    foreach (int step in generalCausalChain.Item1)
                    {
                        if (stepOperators[step].character == dependentCharacter)
                        {
                            prefVal += j.characterPreferences.GetActionPreferenceForCharacter(dependentCharacter, stepOperators[step].name);
                            swapPrefVal += j.characterPreferences.GetActionPreferenceForCharacter(agent, stepOperators[step].name);
                            prefStepCount++;
                        }
                    }
                    // Add dependent step to calculation
                    prefVal += j.characterPreferences.GetActionPreferenceForCharacter(dependentCharacter, stepOperators[generalCausalChain.Item2].name);
                    swapPrefVal += j.characterPreferences.GetActionPreferenceForCharacter(agent, stepOperators[generalCausalChain.Item2].name);
                    prefStepCount++;
                    float prefDepVal = (prefVal / prefStepCount) - (swapPrefVal / prefStepCount);
                    if (prefDepVal > 0)
                        UnityConsole.Log("Pref dep detected between providing player " + agent + " and dependent player " + dependentCharacter + " on chain ending with " + iaChain.Item2 + " with value " + prefDepVal, LOGMODE.DEPENDENCE);
                    else
                        UnityConsole.Log("NON-pref dep detected between providing player " + agent + " and dependent player " + dependentCharacter + " on chain ending with " + iaChain.Item2 + " with value " + prefDepVal, LOGMODE.DEPENDENCE);
                }


                // Then, calculate the action preferences for all agents in the current chain
                /*float basePrefs = 0;
                List<string> participatingCharacters = new List<string>();
                for (int i = startStep; i <= iaChain.Item2; i++)
                {
                    basePrefs += j.characterPreferences.GetActionPreferenceForCharacter(stepOperators[i].character, stepOperators[i].name);
                    if (!participatingAgents.Contains(stepOperators[i].character))
                        participatingAgents.Add(stepOperators[i].character);
                }
                float avgBasePref = basePrefs / (iaChain.Item2 - startStep + 1);
                string dependentCharacter = stepOperators[iaChain.Item2].character;
                participatingAgents.Remove(dependentCharacter);
                float highestAvgPref = float.MinValue;
                foreach (string swapCharacter in participatingAgents)
                {
                    float avgPrefs = 0;
                    for (int i = startStep; i <= iaChain.Item2; i++)
                    {
                        if (stepOperators[i].character == swapCharacter)
                            avgPrefs += j.characterPreferences.GetActionPreferenceForCharacter(dependentCharacter, stepOperators[i].name);
                        else if (stepOperators[i].character == dependentCharacter)
                            avgPrefs += j.characterPreferences.GetActionPreferenceForCharacter(swapCharacter, stepOperators[i].name);
                        else
                            avgPrefs += j.characterPreferences.GetActionPreferenceForCharacter(stepOperators[i].character, stepOperators[i].name);
                    }
                    avgPrefs = avgPrefs / (iaChain.Item2 - startStep + 1);
                    if (avgPrefs > highestAvgPref)
                        highestAvgPref = avgPrefs;
                }

                if (avgBasePref > highestAvgPref)
                    UnityConsole.Log("Pref dep detected on chain ending with " + iaChain.Item2 + " with value " + (avgBasePref - highestAvgPref), LOGMODE.DEPENDENCE);
                */
            }
            watch.Stop();
            UnityConsole.Log("\n(" + watch.ElapsedMilliseconds + "ms) for calculating prefdeps", LOGMODE.DEPENDENCE);
        }

        private static List<int> InterAgentLinkSearch(int current, List<Tuple<int, int, Tuple<bool, string>>> links, List<Operator> stepOperators, string character)
        {
            List<int> ret = new List<int>();
            foreach (Tuple<int, int, Tuple<bool, string>> link in links)
            {
                if (link.Item1 == current && stepOperators[link.Item2].character != character) {
                    ret.Add(link.Item2);
                    ret.AddRange(InterAgentLinkSearch(link.Item2, links, stepOperators, character));
                }
            }
            return ret;
        }

        private static List<int> GeneralLinkSearch(int current, List<Tuple<int, int, Tuple<bool, string>>> links, List<Operator> stepOperators)
        {
            List<int> ret = new List<int>();
            foreach (Tuple<int, int, Tuple<bool, string>> link in links)
            {
                if (link.Item1 == current)
                {
                    ret.Add(link.Item2);
                    ret.AddRange(GeneralLinkSearch(link.Item2, links, stepOperators));
                }
            }
            return ret;
        }
    }
    
    public class UnityConsole
    {
        public static LOGMODE logmode = LOGMODE.DEPENDENCE;// | LOGMODE.MEMOIZE;// | LOGMODE.HEURISTIC; // LOGMODE.PLANNER | LOGMODE.MEMOIZE;
        public static void WriteLine(String str)
        {
            Console.WriteLine(str);
            //UnityEngine.Debug.Log(str);
        }
        public static void Write(String str)
        {
            //UnityEngine.Debug.Log(str);
            Console.WriteLine(str);
        }
        public static void Log(String str, LOGMODE type)
        {
            if (logmode.HasFlag(type) && type != LOGMODE.ERROR)
            {
                Console.WriteLine(str);
            }
        }
    }
}
