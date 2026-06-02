using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            DomainBuilder.JSONDomainBuilder j = new NarrativePlanning.DomainBuilder.JSONDomainBuilder("../../JSON Files/mpk_eval_tutorial.json");

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
                GetStrongDependencies(j, plan);
            }
            while (true) { } // infinite loop to wait for user to exit manually
            return;
        }

        public static void GetStrongDependencies(DomainBuilder.JSONDomainBuilder j, Plan p)
        {
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
            // Dictionary<Tuple<bool, proposition>, stepIdx>
            Dictionary<string, int> searchDictT = new Dictionary<string, int>();
            Dictionary<string, int> searchDictF = new Dictionary<string, int>();
            // List<stepIdx1, stepIdx2, bool, proposition>
            List<Tuple<int, int, Tuple<bool, string>>> causalLinks = new List<Tuple<int, int, Tuple<bool, string>>>();

            for (int i = stepOperators.Count-1; i >= 0; i--)
            {
                if (stepOperators[i] == null) continue;
                UnityConsole.Log(""+i, LOGMODE.DEPENDENCE);
                // Detect the end of causal links first (more efficient, also avoids issues where someone accidentally puts a prop as both a precond and effect)
                List<string> effT = stepOperators[i].effT.Keys.OfType<string>().ToList();
                List<string> effF = stepOperators[i].effF.Keys.OfType<string>().ToList();
                foreach (string effect in effT)
                {
                    UnityConsole.Log("effT: " + effect, LOGMODE.DEPENDENCE);
                    if (searchDictT.ContainsKey(effect))
                    {
                        Tuple<bool, string> propTuple = new Tuple<bool, string>(true, effect);
                        causalLinks.Add(new Tuple<int, int, Tuple<bool, string>>(searchDictT[effect], i, propTuple));
                        searchDictT.Remove(effect);
                    }
                }
                foreach (string effect in effF)
                {
                    UnityConsole.Log("effF: " + effect, LOGMODE.DEPENDENCE);
                    if (searchDictF.ContainsKey(effect))
                    {
                        Tuple<bool, string> propTuple = new Tuple<bool, string>(false, effect);
                        causalLinks.Add(new Tuple<int, int, Tuple<bool, string>>(searchDictF[effect], i, propTuple));
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
                        searchDictT[precond] = i;
                    }
                    else
                    {
                        searchDictT.Add(precond, i);
                    }
                }
                foreach (string precond in preF)
                {
                    UnityConsole.Log("preF: " + precond, LOGMODE.DEPENDENCE);
                    if (searchDictT.ContainsKey(precond)) UnityConsole.Log("ERROR: Adding precond " + precond + " for step " + i + " (" + stepOperators[i].text + ") to searchDictF when it is still open in searchDictT!", LOGMODE.ERROR);
                    if (searchDictF.ContainsKey(precond))
                    {
                        searchDictF[precond] = i;
                    }
                    else
                    {
                        searchDictF.Add(precond, i);
                    }
                }
            }

            // We should now have a (probably big) list of causal links! All steps that are not in a causal link must have their preconditions satisfied by the initial state.
            UnityConsole.Log("\nCausal Links:", LOGMODE.DEPENDENCE);
            foreach(Tuple<int, int, Tuple<bool, string>> link in causalLinks)
            {
                int i = 0;
                UnityConsole.Log("Causal Link " + i + ": " + "(" + link.Item2 + ") " + p.steps[link.Item2].Item1 + " TO " + "(" + link.Item1 + ") " + p.steps[link.Item1].Item1 + " ON " + link.Item3.Item1 + " " + link.Item3.Item2, LOGMODE.DEPENDENCE);
                i++;
            }

            // Trim causal links to only inter-agent dependencies
            List<Tuple<int, int, Tuple<bool, string>>> dependencies = new List<Tuple<int, int, Tuple<bool, string>>>();
            foreach(Tuple<int, int, Tuple<bool, string>> link in causalLinks)
            {
                if (stepOperators[link.Item1].character != stepOperators[link.Item2].character)
                {
                    dependencies.Add(link);
                }
            }
            UnityConsole.Log("\nInter-Agent Dependencies:", LOGMODE.DEPENDENCE);
            foreach(Tuple<int, int, Tuple<bool, string>> dependency in dependencies)
            {
                UnityConsole.Log("Dependency between character " + stepOperators[dependency.Item2].character + " on step " + dependency.Item2 + " and " + stepOperators[dependency.Item1].character + " on step " + dependency.Item1 + " on prop " + dependency.Item3.Item1 + " " + dependency.Item3.Item2, LOGMODE.DEPENDENCE);
            }
        }
    }
    
    public class UnityConsole
    {
        public static LOGMODE logmode = LOGMODE.ERROR | LOGMODE.DEPENDENCE;// | LOGMODE.MEMOIZE;// | LOGMODE.HEURISTIC; // LOGMODE.PLANNER | LOGMODE.MEMOIZE;
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
            if (logmode.HasFlag(type))
            {
                Console.WriteLine(str);
            }
        }
    }
}
