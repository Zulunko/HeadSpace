using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

namespace NarrativePlanning
{
    public enum PLANNING_MODE
    {
        SINGLE,
        MULTI,
        KNOWLEDGE
    }

    [Serializable]
    public class PlanningProblem
    {
        public WorldState w0;
        public WorldState goal;
        public List<Operator> groundedoperators;
        public List<Desire> desires;
        public List<CounterAction> counteractions;
        public Preferences preferences;

        /// <summary>
        /// A Planning Problem consists of the initial state, the goal state
        /// and the operators possible.
        /// </summary>
        /// <param name="initial">initial WorldState</param>
        /// <param name="goal">Goal worldstate</param>
        /// <param name="operators">List of grounded operators</param>
        /// <param name="desires">List of desires</param>
        /// <param name="counteractions">List of counteractions</param>
        public PlanningProblem(WorldState initial, WorldState goal, List<Operator> operators, List<Desire> desires, List<CounterAction> counteractions)
        {
            initialize(initial, goal, operators, desires, counteractions);
        }

        /// <summary>
        /// A Planning Problem consists of the initial state, the goal state
        /// and the operators possible.
        /// </summary>
        /// <param name="initial">initial WorldState</param>
        /// <param name="goal">Goal worldstate</param>
        /// <param name="operators">List of grounded operators</param>
        /// <param name="desires">List of desires</param>
        public PlanningProblem(WorldState initial, WorldState goal, List<Operator> operators, List<Desire> desires)
        {
            List<CounterAction> counters = new List<CounterAction>();
            initialize(initial, goal, operators, desires, counters);
        }

        /// <summary>
        /// A Planning Problem consists of the initial state, the goal state
        /// and the operators possible.
        /// </summary>
        /// <param name="initial">initial WorldState</param>
        /// <param name="goal">Goal worldstate</param>
        /// <param name="operators">List of grounded operators</param>
        public PlanningProblem(WorldState initial, WorldState goal, List<Operator> operators)
        {
            List<Desire> desires = new List<Desire>();
            List<CounterAction> counters = new List<CounterAction>();
            initialize(initial, goal, operators, desires, counters);
        }

        public PlanningProblem(WorldState initial, WorldState goal, List<Operator> operators, Preferences p)
        {
            w0 = initial;
            this.goal = goal;
            this.groundedoperators = operators;
            this.preferences = p;
        }

        public void initialize(WorldState initial, WorldState goal, List<Operator> operators, List<Desire> desires, List<CounterAction> counteractions)
        {
            w0 = initial;
            this.goal = goal;
            this.groundedoperators = operators;
            this.desires = desires;
            this.counteractions = counteractions;
        }
        

       



        /// <summary>
        /// DFS didn't run because it would result in infinite plans
        /// with recursive actions, hence not reaching the goal state.
        /// </summary>
        /// <returns>Nothing.</returns>
        //public List<Tuple<String, WorldState>> DFS(WorldState w, List<Tuple<String, WorldState>> steps){
        //    List<Tuple<String, WorldState>> nextStateTuples = w.getPossibleNextStatesTuples(operators, gops);
        //    foreach(Tuple<String,WorldState> next in nextStateTuples){
        //        if (next.Item2.isGoalState(goal)){
        //            steps.Add(next);
        //            return steps;
        //        }
        //        else{
        //            steps.AddRange(DFS(next.Item2, new List<Tuple<string, WorldState>>()));
        //            return steps;
        //        }
        //    }
        //}

        public Plan BFSSolution()
        {
            List<Tuple<String, WorldState>> nextStateTuples = w0.getPossibleApparentNextStatesTuples(groundedoperators);
            int depth = 1;
            int bfactor = 0;
            int nnodes = 0;
            Plan solutionPlan = null;
            Queue<Plan> queue = new Queue<Plan>();
            while (solutionPlan == null)
            {
                //if first time do not look at queue
                if (depth == 1)
                {
                    foreach (Tuple<String, WorldState> next in w0.getPossibleApparentNextStatesTuples(groundedoperators))
                    {
                        nnodes++;
                        bfactor++;
                        Plan p = new Plan(this);
                        p.steps.Add(next);
                        queue.Enqueue(p);
                        if (next.Item2.isGoalState(this.goal))
                        {
                            //solution found!
                            solutionPlan = p;
                            UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                            return solutionPlan;
                        }

                    }
                }
                else
                {
                    while (queue.Peek().steps.Count == depth)
                    {
                        Plan p = queue.Dequeue();
                        WorldState w = p.steps[p.steps.Count - 1].Item2;
                        int x = 0;
                        foreach (Tuple<String, WorldState> next in w.getPossibleApparentNextStatesTuples(groundedoperators))
                        {
                            x++;
                            nnodes++;
                            Plan q = p.clone();
                            q.steps.Add(next);
                            queue.Enqueue(q);
                            if (next.Item2.isGoalState(this.goal))
                            {
                                //solution found!
                                solutionPlan = q;
                                UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + Math.Max(bfactor, x) + ".");
                                return solutionPlan;
                            }
                        }
                        if (x > bfactor)
                            bfactor = x;
                    }
                }
                depth++;
            }
            return solutionPlan;
        }

        /// <summary>
        /// Returns a plan using a FF-based solution.
        /// </summary>
        /// <returns> A solution plan</returns>
        public Plan FFSolution()
        {
            List<Tuple<String, WorldState>> nextStateTuples = w0.getPossibleApparentNextStatesTuples(groundedoperators);
            int depth = 1;
            int bfactor = 0;
            int avg_branching_factor = 0;
            int nnodes = 0;
            Plan solutionPlan = null;
            Plan current = new Plan(this);
            //Queue<Plan> queue = new Queue<Plan>();
            int min = -1;
            Tuple<String, WorldState> best = null;
            while (solutionPlan == null)
            {
                min = 100;
                WorldState w = current.steps[current.steps.Count - 1].Item2;
                int tmp = 0;

                List<Tuple<String, WorldState>> n = w.getPossibleApparentNextStatesTuples(groundedoperators);
                //check every node in the frontier    
                foreach (Tuple<String, WorldState> next in n)
                {
                    if (next.Item1.Contains("-false"))
                        continue;
                    nnodes++;
                    tmp++;
                    Plan p = new Plan(this);
                    //queue.Enqueue(p);
                    Operator op = groundedoperators.Find(xy => xy.text.Equals(next.Item1));
                    int x;

                    String charactername = op.character;
                    int y = FastForward.extractCharacterRPSize(FastForward.computeCharacterRPG(groundedoperators, next.Item2, this.goal, charactername), this.goal, groundedoperators, charactername);

                    Tuple<string, WorldState> res;
                    if (!WorldState.isExecutable(op, w))
                    {
                        NarrativePlanning.Operator failedop = NarrativePlanning.Operator.getFailedOperator(groundedoperators, op);
                        res = new Tuple<string, WorldState>(failedop.text, WorldState.getNextState(w, failedop));
                        p.steps.Add(res);
                        x = FastForward.extractRPSize(FastForward.computeRPG(groundedoperators, res.Item2, this.goal), this.goal, groundedoperators);
                    }
                    else
                    {
                        res = next;
                        p.steps.Add(next);
                        x = FastForward.extractRPSize(FastForward.computeRPG(groundedoperators, next.Item2, this.goal), this.goal, groundedoperators);
                    }
                    UnityConsole.Write("Possible apparent next step " + next.Item1 + ", but actual step " + res.Item1 + " with global hueristic of " + x + " and character heuristic of " + y + "\n");
                    if (y < min && y != -1)
                    {
                        best = res;
                        min = y;
                    }

                    if (res.Item2.isGoalState(this.goal))
                    {
                        //solution found!
                        current.steps.Add(res);
                        solutionPlan = current;
                        UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                        return solutionPlan;
                    }
                }

                if (n.Count == 0)
                    return null;

                UnityConsole.Write("STEP SELECTED: " + best.Item1 + "\n");
                UnityConsole.Write("----------\n");
                if (tmp > bfactor)
                    bfactor = tmp;

                //add best node to plan
                current.steps.Add(best);
                if (current.steps[current.steps.Count - 1].Item2.isGoalState(this.goal))
                {
                    //solution found!
                    solutionPlan = current;
                    UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                    return solutionPlan;
                }
                depth++;
            }
            return solutionPlan;
        }

        private Plan RewindAndEliminateAction(Plan p, int numToRewind)
        {
            UnityConsole.Log("Rewinding " + numToRewind + " steps from current plan:", LOGMODE.MEMOIZE);
            int i = p.steps.Count - 1 - numToRewind;
            for (int j = 0; j < p.steps.Count - 1; j++)
            {
                UnityConsole.Log("    " + p.steps[j].Item1, LOGMODE.MEMOIZE);
                if (j == i) UnityConsole.Log("    ----MEMOIZATION CUTOFF", LOGMODE.MEMOIZE);
            }
            String nextstep = p.steps[i + 1].Item1;
            p.steps.RemoveRange(i + 1, p.steps.Count - i - 1);
            WorldState w = p.steps[i].Item2;
            int toremove = -1;
            if (w.prunedOperators != null)
            {
                for (int j = 0; j < w.prunedOperators.Count; j++)
                {
                    if (w.prunedOperators[j].text == nextstep)
                    {
                        toremove = j;
                        break;
                    }
                }
            }
            if (toremove != -1)
            {
                w.prunedOperators.RemoveAt(toremove);
            }
            else
            {
                if (w.eliminatedOperators == null) w.eliminatedOperators = new List<string>();
                w.eliminatedOperators.Add(p.steps[i].Item1);
            }
            return p;
        }

        private Plan RewindAndEliminateActionKnowledge(Plan p, int numToRewind)
        {
            UnityConsole.Log("Rewinding " + numToRewind + " steps from current plan:", LOGMODE.MEMOIZE);
            int i = p.knowledgeSteps.Count - 1 - numToRewind;
            for (int j = 0; j < p.knowledgeSteps.Count - 1; j++)
            {
                UnityConsole.Log("    " + p.knowledgeSteps[j].Item1, LOGMODE.MEMOIZE);
                if (j == i) UnityConsole.Log("    ----MEMOIZATION CUTOFF", LOGMODE.MEMOIZE);
            }
            String nextstep = p.knowledgeSteps[i + 1].Item1;
            p.knowledgeSteps.RemoveRange(i + 1, p.knowledgeSteps.Count - i - 1);
            // pruned ops are stored in knowledge worldstate
            WorldState w = p.knowledgeSteps[i].Item3;
            int toremove = -1;
            if (w.prunedOperators != null)
            {
                for (int j = 0; j < w.prunedOperators.Count; j++)
                {
                    if (w.prunedOperators[j].text == nextstep)
                    {
                        toremove = j;
                        break;
                    }
                }
            }
            if (toremove != -1)
            {
                w.prunedOperators.RemoveAt(toremove);
                w.operatorPrefs.RemoveAt(toremove);
            }
            //else
            //{
                if (w.eliminatedOperators == null) w.eliminatedOperators = new List<string>();
                w.eliminatedOperators.Add(nextstep);
            //}
            return p;
        }

        /// <summary>
        /// Returns a plan using a FF-based solution.
        /// </summary>
        /// <returns> A solution plan</returns>
        public Plan FFPreferenceSolution(PLANNING_MODE mode)
        {
            Console.WriteLine("---------------------PLANNING PROCESS BEGUN");
            int depth = 1;
            int bfactor = 0;
            int avg_branching_factor = 0;
            int nnodes = 0;
            Plan solutionPlan = null;
            Plan current = new Plan(this);
            //Queue<Plan> queue = new Queue<Plan>();
            float min = -1;
            float ps = -1;
            Tuple<String, WorldState> best = null;
            while (solutionPlan == null)
            {
                min = 100;
                WorldState w = current.steps[current.steps.Count - 1].Item2;
                
                // XXX Check if w is in our memoized states. If it is, rewind to the step that matches and
                // restore prune list but remove operator that was chosen last time.
                for (int i = 0; i < current.steps.Count-1; i++)
                {
                    if (current.steps[i].Item2.tWorld.Cast<DictionaryEntry>().Union(w.tWorld.Cast<DictionaryEntry>()).Count() == current.steps[i].Item2.tWorld.Count &&
                        current.steps[i].Item2.fWorld.Cast<DictionaryEntry>().Union(w.fWorld.Cast<DictionaryEntry>()).Count() == current.steps[i].Item2.fWorld.Count)
                        //XXX 9/7/22: memoization checks for truth only.
                        // Above: this is a hack to make incomplete domains work. I have disabled the hack for now.
                    {
                        UnityConsole.Log("MEMOIZATION TRIGGERED after " + current.steps[current.steps.Count - 1].Item1, LOGMODE.MEMOIZE);
                        //RewindAndEliminateAction(current, current.steps.Count - 1 - i);
                        RewindAndEliminateAction(current, 1);
                        w = current.steps[current.steps.Count - 1].Item2;
                        break;
                    }
                }
                w.PrintFullState();


                int tmp = 0;
                List<Tuple<String, WorldState>> n;
                // XXX PRUNING
                if (w.prunedOperators != null && w.prunedOperators.Count > 0) n = w.getPrunedNextStatesTuplesWithPrefs(preferences);
                else n = w.getPossibleNextStatesTuplesWithPrefs(groundedoperators, preferences);
                switch (mode)
                {
                    case PLANNING_MODE.SINGLE:
                        n.Sort((a, b) => preferences.GetActionPreference(b.Item1.Split(' ')[0]).CompareTo(preferences.GetActionPreference(a.Item1.Split(' ')[0])));
                        break;
                    case PLANNING_MODE.MULTI:
                        n.Sort((a, b) => preferences.GetActionPreferenceForCharacter(b.Item1.Split(' ')[1], b.Item1.Split(' ')[0]).CompareTo(preferences.GetActionPreferenceForCharacter(a.Item1.Split(' ')[1], a.Item1.Split(' ')[0])));
                        break;
                    case PLANNING_MODE.KNOWLEDGE:
                        UnityConsole.Log("FAIL: PLANNING_MODE must be either SINGLE or MULTI.", LOGMODE.ERROR);
                        return null;
                }
                //foreach (Tuple<String, WorldState> t in n)
                //{
                //    Console.WriteLine("ACT: " + t.Item1);
                //    Console.WriteLine("VAL: " + preferences.GetActionPreference(t.Item1.Split(' ')[0]));
                //}
                //check every node in the frontier
                foreach (Tuple<String, WorldState> next in n)
                {
                    //Console.WriteLine(n.Count + " FRONTIER NODES: " + next.Item1);
                    if (next.Item1.Contains("-false"))
                        continue;
                    nnodes++;
                    tmp++;
                    Plan p = new Plan(this);
                    //queue.Enqueue(p);
                    Operator op = groundedoperators.Find(xy => xy.text.Equals(next.Item1));

                    //String charactername = op.character;
                    //Console.WriteLine();
                    FastForward.Layers prefRPG;
                    switch (mode)
                    {
                        case PLANNING_MODE.SINGLE:
                            prefRPG = FastForward.computePreferenceRPG(groundedoperators, next.Item2, this.goal, this.preferences);
                            break;
                        case PLANNING_MODE.MULTI:
                            prefRPG = FastForward.computeMultiPreferenceRPG(groundedoperators, next.Item2, this.goal, this.preferences);
                            break;
                        default:
                            UnityConsole.Log("FAIL: PLANNING_MODE must be either SINGLE or MULTI.", LOGMODE.ERROR);
                            return null;
                    }
                    Tuple<int, float> heuristicData = FastForward.extractPrefRPSizeAndPrunedOps(prefRPG, this.goal, next.Item2);
                    Tuple<string, WorldState> res = next;
                    //p.steps.Add(next);
                    float y;
                    if (heuristicData.Item1 == -1)
                        y = -1;
                    else
                        y = heuristicData.Item1;// + (1f - heuristicData.Item2);
                    UnityConsole.Log("        Value for " + next.Item1 + ": " + y, LOGMODE.PLANNER);
                    UnityConsole.Log("        Playstyle for " + next.Item1 + ": " + heuristicData.Item2, LOGMODE.PLANNER);

                    if (y != -1)
                    {               // Uncomment the below to prioritize higher playstyle value paths in ties
                        if (y < min || (y == min && heuristicData.Item2 > ps) || (y == min && heuristicData.Item2 == ps && best.Item1.Contains("move") && !res.Item1.Contains("move")))
                        {
                            best = res;
                            min = y;
                            ps = heuristicData.Item2;
                        }
                    }

                    /*if (res.Item2.isGoalState(this.goal))
                    {
                        //solution found!
                        current.steps.Add(res);
                        solutionPlan = current;
                        UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                        return solutionPlan;
                    }*/
                }

                // XXX MEMOIZATION: rewind if n.Count == 0
                if (n.Count == 0)
                {
                    // Possibly here: instead keep a list of eliminated actions, redo this add without pruned actions but while still avoiding eliminated actions
                    RewindAndEliminateAction(current, 1);
                    continue;
                    //return null;
                }

                UnityConsole.Log("STEP SELECTED: " + best.Item1 + "\n", LOGMODE.PLANNER);
                //if (best.Item1 == "proceed Act2 Act3")
                //{
                //    UnityConsole.Log("stop.", LOGMODE.PLANNER);
                //}
                //UnityConsole.Write("----------\n");
                if (tmp > bfactor)
                    bfactor = tmp;

                //add best node to plan
                current.steps.Add(best);
                if (current.steps[current.steps.Count - 1].Item2.isGoalState(this.goal))
                {
                    //solution found!
                    solutionPlan = current;
                    UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                    Console.WriteLine("---------------------PLANNING PROCESS ENDED");
                    return solutionPlan;
                }

                // XXX MEMO: removed this, can change to make more accurate maybe
                //depth++;
            }
            Console.WriteLine("---------------------PLANNING PROCESS ENDED");
            return solutionPlan;
        }

        private Operator SelectAction(List<Operator> suggestedActions, List<float> actionPrefs, WorldState agentKnowledge)
        {
            Operator selectedAction = null;
            float selectedPref = -1000;
            Operator moveAction = null;
            float movePref = -1000;
            List<string> moveActions = new List<string> { "move", "enter-ship", "exit-ship", "drive-alien-transport", "enter-alien-transport", "exit-alien-transport", "takeoff-ship", "land-ship" };
            for (int i = 0; i < suggestedActions.Count; i++)
            {
                Operator possible = suggestedActions[i];
                float currPref = actionPrefs[i];
                if (moveActions.Contains(possible.name))
                {
                    if (moveAction != null && currPref <= movePref) continue;
                    else if (WorldState.isPotentiallyExecutable(possible, agentKnowledge))
                    {
                        moveAction = possible;
                        movePref = currPref;
                    }
                }
                else if (WorldState.isPotentiallyExecutable(possible, agentKnowledge))
                {
                    if (currPref <= selectedPref)
                        continue;
                    else
                    {
                        selectedAction = possible;
                        selectedPref = currPref;
                    }
                }
            }
            if (selectedAction == null) selectedAction = moveAction;
            return selectedAction;
        }
        private void RemoveEliminatedOps(List<Operator> suggestedActions, List<float> actionPrefs, List<string> eliminatedOperators)
        {
            UnityConsole.Log("eliminated actions: ", LOGMODE.MEMOIZE);
            foreach (string s in eliminatedOperators)
                UnityConsole.Log("    " + s, LOGMODE.MEMOIZE);
            List<int> opsToRemove = new List<int>();
            for (int i = 0; i < suggestedActions.Count; i++)
            {
                if (eliminatedOperators.Contains(suggestedActions[i].text) || eliminatedOperators.Contains("FAIL " + suggestedActions[i].text))
                {
                    opsToRemove.Add(i);
                }
            }
            foreach (int i in opsToRemove)
            {
                suggestedActions.RemoveAt(i);
                actionPrefs.RemoveAt(i);
            }
        }

        /// <summary>
        /// Returns a plan using a FF-based solution.
        /// </summary>
        /// <returns> A solution plan</returns>
        public Plan FFKnowledgePrefSolution(PLANNING_MODE mode)
        {
            if (mode != PLANNING_MODE.KNOWLEDGE)
            {
                UnityConsole.Log("FAIL: PLANNING_MODE must be KNOWLEDGE.", LOGMODE.ERROR);
                return null;
            }
            Console.WriteLine("---------------------PLANNING PROCESS BEGUN");
            int depth = 1;
            int bfactor = 0;
            int avg_branching_factor = 0;
            int nnodes = 0;
            Plan current = new Plan(this);
            //Queue<Plan> queue = new Queue<Plan>();
            float min = -1;
            float ps = -1;
            Tuple<String, WorldState> best = null;
            WorldState w = current.steps.Last().Item2;

            WorldState agentKnowledge = w.clone();
            agentKnowledge.tWorld.Clear();
            agentKnowledge.fWorld.Clear();
            // XXX HERE: ADD KNOWN KNOWLEDGE FROM DESIGNER
            agentKnowledge = FastForward.ApplyInitialObservability(agentKnowledge, w);
            /* mpk_eval_tutorial
            agentKnowledge.fWorld.Add("connected StartRoom GoalRoom", 0);
            agentKnowledge.fWorld.Add("open Chest", 0);
            */
            /* mpk_ship
            agentKnowledge.tWorld.Add("ship-at StartGalaxy", 0);
            agentKnowledge.tWorld.Add("jump-route StartGalaxy AttackGalaxy", 0);
            agentKnowledge.tWorld.Add("jump-route AttackGalaxy HomeGalaxy", 0);

            agentKnowledge.tWorld.Add("needs JumpControls JumpStarted", 0);
            agentKnowledge.tWorld.Add("needs NavigationControls ShipPowered", 0);
            agentKnowledge.tWorld.Add("needs AirlockControls ShipPowered", 0);
            agentKnowledge.tWorld.Add("needs JumpStabilizer ShipPowered", 0);

            agentKnowledge.fWorld.Add("needs JumpControls ShipPowered", 0);
            agentKnowledge.fWorld.Add("needs NavigationControls JumpStarted", 0);
            agentKnowledge.fWorld.Add("needs AirlockControls JumpStarted", 0);
            agentKnowledge.fWorld.Add("needs JumpStabilizer JumpStarted", 0);

            agentKnowledge.fWorld.Add("ship-at AttackGalaxy", 0);
            agentKnowledge.fWorld.Add("ship-at HomeGalaxy", 0);
            agentKnowledge.fWorld.Add("jump-route StartGalaxy HomeGalaxy", 0);
            */
            /* mpk_eval_task */
            List<string> knownTruths = new List<string>()
            {
                "(known-location CraggyRocks)",
                "(known-location AlienSettlement)",
                "(ship-flying PlayerShip)",
                "(flight-ready PlayerShip)",
                "(on-ship Cargo CrashedShip)",
                "(open PlayerShip)",
                "(explosives-on-ship)"
           };
            List<string> knownFalses = new List<string>()
            {
                "(at-ship PlayerShip CraggyRocks)",
                "(at-ship PlayerShip Forest)",
                "(at-ship PlayerShip AlienSettlement)",
                "(ship-flying CrashedShip)",
                "(known-location Forest)",
                "(beacons-setup)",
                "(damaged PlayerShip)",
                "(explosives-ready)",
                "(baydoors-open)",
                "(on-transport Cargo)",
                "(on-ship Cargo PlayerShip)",
                "(explosives-on-transport)",
                "(victory)"
            };

            foreach (string s in knownTruths)
            {
                string k = s.Substring(1, s.Length - 2);
                if (!agentKnowledge.tWorld.ContainsKey(k))
                    agentKnowledge.tWorld.Add(s.Substring(1, s.Length - 2), 0);
            }
            foreach (string s in knownFalses)
            {
                string k = s.Substring(1, s.Length - 2);
                if (!agentKnowledge.fWorld.ContainsKey(k))
                    agentKnowledge.fWorld.Add(s.Substring(1, s.Length - 2), 0);
            }

            agentKnowledge = FastForward.CreateUnknownKnowledge(groundedoperators, agentKnowledge);
            agentKnowledge = FastForward.ApplyKnowledgeConsistency(agentKnowledge);

            current.knowledgeSteps.Add(new Tuple<string, WorldState, WorldState>(current.steps.Last().Item1, current.steps.Last().Item2, agentKnowledge));
            current.steps.Clear();

            while (true)
            {
                min = 100;
                w = current.knowledgeSteps.Last().Item2;
                agentKnowledge = current.knowledgeSteps.Last().Item3;

                // XXX Check if w is in our memoized states. If it is, rewind to the step that matches and
                // restore prune list but remove operator that was chosen last time.
                // ELIMINATING MEMOIZATION FOR NOW
                bool memoized = false;
                for (int i = 0; i < current.knowledgeSteps.Count - 1; i++)
                {
                    if (current.knowledgeSteps[i].Item2.tWorld.Cast<DictionaryEntry>().Union(w.tWorld.Cast<DictionaryEntry>()).Count() == current.knowledgeSteps[i].Item2.tWorld.Count &&
                        current.knowledgeSteps[i].Item2.fWorld.Cast<DictionaryEntry>().Union(w.fWorld.Cast<DictionaryEntry>()).Count() == current.knowledgeSteps[i].Item2.fWorld.Count &&
                        current.knowledgeSteps[i].Item3.tWorld.Cast<DictionaryEntry>().Union(agentKnowledge.tWorld.Cast<DictionaryEntry>()).Count() == current.knowledgeSteps[i].Item3.tWorld.Count &&
                        current.knowledgeSteps[i].Item3.fWorld.Cast<DictionaryEntry>().Union(agentKnowledge.fWorld.Cast<DictionaryEntry>()).Count() == current.knowledgeSteps[i].Item3.fWorld.Count)
                    // XXX we probably only need to check the Item3 state here, not the Item2 state. World can't change without agent knowledge, so we don't need to check the real world state.
                    {
                        UnityConsole.Log("MEMOIZATION TRIGGERED after " + current.knowledgeSteps[current.knowledgeSteps.Count - 1].Item1, LOGMODE.MEMOIZE);
                        //current = RewindAndEliminateActionKnowledge(current, current.knowledgeSteps.Count - 1 - i);

                        // ONLY EVER REWIND BY 1. This prevents situations where you eliminate actions that should remain.
                        current = RewindAndEliminateActionKnowledge(current, 1);
                        memoized = true;
                        //w = current.knowledgeSteps[current.knowledgeSteps.Count - 1].Item2;
                        //agentKnowledge = current.knowledgeSteps[current.knowledgeSteps.Count - 1].Item3;
                        break;
                    }
                }
                if (memoized) continue;
                //UnityConsole.Log("t: " + current.knowledgeSteps.Last().Item2.tWorld.Count, LOGMODE.ERROR);
                //UnityConsole.Log("f: " + current.knowledgeSteps.Last().Item2.fWorld.Count, LOGMODE.ERROR);
                //UnityConsole.Log("kt: " + current.knowledgeSteps.Last().Item3.tWorld.Count, LOGMODE.ERROR);
                //UnityConsole.Log("kf: " + current.knowledgeSteps.Last().Item3.fWorld.Count, LOGMODE.ERROR);
                //foreach(string p in current.knowledgeSteps.Last().Item3.fWorld.Keys)
                //{
                //    UnityConsole.Log("  " + p, LOGMODE.ERROR);
                //}
                w.PrintFullState();

                List<Operator> suggestedActions = null;
                List<float> actionPrefs;
                Operator selectedAction = null;
                // pruned ops are stored in knowledge worldstate (item3).
                if (current.knowledgeSteps.Last().Item3.prunedOperators != null)
                {
                    suggestedActions = current.knowledgeSteps.Last().Item3.prunedOperators;
                    actionPrefs = current.knowledgeSteps.Last().Item3.operatorPrefs;
                    RemoveEliminatedOps(suggestedActions, actionPrefs, current.knowledgeSteps.Last().Item3.eliminatedOperators);
                    selectedAction = SelectAction(suggestedActions, actionPrefs, agentKnowledge);
                }
                if (selectedAction == null)
                {
                    FastForward.Layers prefRPG = FastForward.computeMultiPreferenceKnowledgeRPG(groundedoperators, agentKnowledge, goal, preferences);
                    Tuple<List<Operator>, List<float>, float> heuristicData = FastForward.extractRPKnowledge(prefRPG, goal, agentKnowledge);
                    suggestedActions = heuristicData.Item1;
                    actionPrefs = heuristicData.Item2;
                    suggestedActions.Reverse();
                    actionPrefs.Reverse();
                    RemoveEliminatedOps(suggestedActions, actionPrefs, current.knowledgeSteps.Last().Item3.eliminatedOperators);
                    UnityConsole.Log("Heuristic relaxed plan actions (post-elimination):", LOGMODE.HEURISTIC);
                    foreach (Operator o in suggestedActions)
                    {
                        UnityConsole.Log("    " + o.text, LOGMODE.HEURISTIC);
                    }
                    selectedAction = SelectAction(suggestedActions, actionPrefs, agentKnowledge);
                }

                if (selectedAction == null)
                {
                    UnityConsole.Log("SelectedAction null! SuggestedActions and their executability: ", LOGMODE.ERROR);
                    foreach (Operator o in suggestedActions)
                    {
                        UnityConsole.Log("    " + o.text, LOGMODE.ERROR);
                        UnityConsole.Log("    " + WorldState.isPotentiallyExecutable(o, agentKnowledge).ToString(), LOGMODE.ERROR);
                    }
                    current = RewindAndEliminateActionKnowledge(current, 1);
                    memoized = true;
                    continue;
                    //this is dead somehow
                }

                // Apply observability of preconds (happens regardless of success)
                WorldState newKnowledge = WorldState.getKnowledgeUpdateActionPreconditions(w, selectedAction, agentKnowledge);
                if (WorldState.isExecutable(selectedAction, w))
                {
                    // Apply world state changes / observability of effects
                    Tuple<WorldState, WorldState> newStates = WorldState.getNextStateWithKnowledgeUpdate(w, selectedAction, newKnowledge);
                    WorldState newWorld = newStates.Item1;
                    newKnowledge = newStates.Item2;
                    current.knowledgeSteps.Add(new Tuple<string, WorldState, WorldState>(selectedAction.text, newWorld, FastForward.ApplyKnowledgeConsistency(newKnowledge)));
                    UnityConsole.Log("Selected: " + current.knowledgeSteps.Last().Item1, LOGMODE.ERROR);
                    if (current.knowledgeSteps.Last().Item1.StartsWith("takeoff-ship GreenPlayer CrashedShip AlienSettlement"))
                    {
                        UnityConsole.Log("Stop", LOGMODE.ERROR);
                    }
                    if (newWorld.isGoalState(goal))
                    {
                        return current;
                    }
                } else
                {
                    current.knowledgeSteps.Add(new Tuple<string, WorldState, WorldState>("FAIL " + selectedAction.text, w.clone(), FastForward.ApplyKnowledgeConsistency(newKnowledge)));
                    UnityConsole.Log("Selected: " + current.knowledgeSteps.Last().Item1, LOGMODE.ERROR);
                }
            }
            return null;

            // XXX EWLANG THIS NOW NEEDS TO BE MODIFIED TO EVALUATE CURRENT STATE!
            /*int tmp = 0;
            List<Tuple<String, WorldState>> n;
            if (w.prunedOperators != null && w.prunedOperators.Count > 0) n = w.getPrunedNextStatesTuplesWithPrefs(preferences);
            else n = w.getPossibleNextStatesTuplesWithPrefs(groundedoperators, preferences);
            n.Sort((a, b) => preferences.GetActionPreferenceForCharacter(b.Item1.Split(' ')[1], b.Item1.Split(' ')[0]).CompareTo(preferences.GetActionPreferenceForCharacter(a.Item1.Split(' ')[1], a.Item1.Split(' ')[0])));

            //check every node in the frontier
            foreach (Tuple<String, WorldState> next in n)
            {
                if (next.Item1.Contains("-false"))
                    continue;
                nnodes++;
                tmp++;
                Plan p = new Plan(this);
                Operator op = groundedoperators.Find(xy => xy.text.Equals(next.Item1));

                FastForward.Layers prefRPG;
                switch (mode)
                {
                    case PLANNING_MODE.SINGLE:
                        prefRPG = FastForward.computePreferenceRPG(groundedoperators, next.Item2, this.goal, this.preferences);
                        break;
                    case PLANNING_MODE.MULTI:
                        prefRPG = FastForward.computeMultiPreferenceRPG(groundedoperators, next.Item2, this.goal, this.preferences);
                        break;
                    default:
                        UnityConsole.Log("FAIL: PLANNING_MODE must be either SINGLE or MULTI.", LOGMODE.ERROR);
                        return null;
                }
                Tuple<int, float> heuristicData = FastForward.extractPrefRPSizeAndPrunedOps(prefRPG, this.goal, next.Item2);
                Tuple<string, WorldState> res = next;

                float y;
                if (heuristicData.Item1 == -1)
                    y = -1;
                else
                    y = heuristicData.Item1;// + (1f - heuristicData.Item2);
                UnityConsole.Log("        Value for " + next.Item1 + ": " + y, LOGMODE.PLANNER);
                UnityConsole.Log("        Playstyle for " + next.Item1 + ": " + heuristicData.Item2, LOGMODE.PLANNER);

                if (y != -1)
                {
                    if (y < min || (y == min && heuristicData.Item2 > ps) || (y == min && heuristicData.Item2 == ps && best.Item1.Contains("move") && !res.Item1.Contains("move")))
                    {
                        best = res;
                        min = y;
                        ps = heuristicData.Item2;
                    }
                }
            }

            // XXX MEMOIZATION: rewind if n.Count == 0
            if (n.Count == 0)
            {
                // Possibly here: instead keep a list of eliminated actions, redo this add without pruned actions but while still avoiding eliminated actions
                RewindAndEliminateAction(current, 1);
                continue;
                //return null;
            }

            UnityConsole.Log("STEP SELECTED: " + best.Item1 + "\n", LOGMODE.PLANNER);
            //if (best.Item1 == "proceed Act2 Act3")
            //{
            //    UnityConsole.Log("stop.", LOGMODE.PLANNER);
            //}
            //UnityConsole.Write("----------\n");
            if (tmp > bfactor)
                bfactor = tmp;

            //add best node to plan
            current.steps.Add(best);
            if (current.steps[current.steps.Count - 1].Item2.isGoalState(this.goal))
            {
                //solution found!
                solutionPlan = current;
                UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                Console.WriteLine("---------------------PLANNING PROCESS ENDED");
                return solutionPlan;
            }

            // XXX MEMO: removed this, can change to make more accurate maybe
            //depth++;
        }
        Console.WriteLine("---------------------PLANNING PROCESS ENDED");
        return solutionPlan;
        }*/
        }

            /// <summary>
            /// Returns a plan using a FF-based solution.
            /// </summary>
            /// <returns> A solution plan</returns>
            public Plan FFNoPreferenceSolution()
        {
            int depth = 1;
            int bfactor = 0;
            int avg_branching_factor = 0;
            int nnodes = 0;
            Plan solutionPlan = null;
            Plan current = new Plan(this);
            //Queue<Plan> queue = new Queue<Plan>();
            float min = -1;
            Tuple<String, WorldState> best = null;
            while (solutionPlan == null)
            {
                min = 100;
                WorldState w = current.steps[current.steps.Count - 1].Item2;
                int tmp = 0;

                List<Tuple<String, WorldState>> n;
                if (w.prunedOperators == null)
                    n = w.getPossibleNextStatesTuples(groundedoperators);
                else
                    n = w.getPrunedNextStatesTuples(w.prunedOperators);
                //n.Sort((a, b) => preferences.GetActionPreference(b.Item1.Split(' ')[0]).CompareTo(preferences.GetActionPreference(a.Item1.Split(' ')[0])));
                //foreach (Tuple<String, WorldState> t in n)
                //{
                //    Console.WriteLine("ACT: " + t.Item1);
                //    Console.WriteLine("VAL: " + preferences.GetActionPreference(t.Item1.Split(' ')[0]));
                //}
                //check every node in the frontier    
                foreach (Tuple<String, WorldState> next in n)
                {
                    //Console.WriteLine(n.Count + " FRONTIER NODES: " + next.Item1);
                    if (next.Item1.Contains("-false"))
                        continue;
                    nnodes++;
                    tmp++;
                    Plan p = new Plan(this);
                    //queue.Enqueue(p);
                    Operator op = groundedoperators.Find(xy => xy.text.Equals(next.Item1));

                    //String charactername = op.character;
                    //Console.WriteLine();
                    FastForward.Layers prefRPG = FastForward.computeRPG(groundedoperators, next.Item2, this.goal);
                    //PrintRPG(prefRPG);
                    int heuristicData = FastForward.extractRPSizeAndPrunedOps(prefRPG, this.goal, next.Item2);

                    Tuple<string, WorldState> res = next;
                    //p.steps.Add(next);
                    float y;
                    if (heuristicData == -1)
                        y = -1;
                    else
                        y = heuristicData;// + (1f - heuristicData.Item2);
                    //Console.WriteLine("        Value for " + next.Item1 + ": " + y);

                    if (y < min && y != -1)
                    {
                        best = res;
                        min = y;
                    }

                    /*if (res.Item2.isGoalState(this.goal))
                    {
                        //solution found!
                        current.steps.Add(res);
                        solutionPlan = current;
                        UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                        return solutionPlan;
                    }*/
                }

                if (n.Count == 0)
                    return null;

                //UnityConsole.Write("STEP SELECTED: " + best.Item1 + "\n");
                //UnityConsole.Write("----------\n");
                if (tmp > bfactor)
                    bfactor = tmp;

                //add best node to plan
                current.steps.Add(best);
                if (current.steps[current.steps.Count - 1].Item2.isGoalState(this.goal))
                {
                    //solution found!
                    solutionPlan = current;
                    //UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                    return solutionPlan;
                }
                depth++;
            }
            return solutionPlan;
        }

        /// <summary>
        /// Returns an apparent plan using a FF-based solution.
        /// </summary>
        /// <returns> A solution plan</returns>
        public Plan FFApparentSolution()
        {
            List<Tuple<String, WorldState>> nextStateTuples = w0.getPossibleApparentNextStatesTuples(groundedoperators);
            int depth = 1;
            int bfactor = 0;
            int nnodes = 0;
            int avg_branching_factor = 0;
            Plan solutionPlan = null;
            Plan current = new Plan(this);
            //Queue<Plan> queue = new Queue<Plan>();
            int min = -1;
            Tuple<String, WorldState> best = null;
            while (solutionPlan == null)
            {
                min = 100;
                WorldState w = current.steps[current.steps.Count - 1].Item2;
                int tmp = 0;
                List<Tuple<String, WorldState>> n = w.getPossibleApparentNextStatesTuples(groundedoperators);
                //check every node in the frontier    
                foreach (Tuple<String, WorldState> next in n)
                {
                    if (next.Item1.Contains("-false"))
                        continue;
                    nnodes++;
                    tmp++;
                    Plan p = new Plan(this);
                    //queue.Enqueue(p);
                    Operator op = groundedoperators.Find(xy => xy.text.Equals(next.Item1));
                    int x;

                    String charactername = op.character;
                    int y = FastForward.extractCharacterRPSize(FastForward.computeCharacterRPG(groundedoperators, next.Item2, this.goal, charactername), this.goal, groundedoperators, charactername);

                    Tuple<string, WorldState> res;
                    //if (!WorldState.isExecutable(op, w))
                    //{

                    //    NarrativePlanning.Operator failedop = NarrativePlanning.Operator.getFailedOperator(groundedoperators, op);
                    //    res = new Tuple<string, WorldState>(failedop.text, WorldState.getNextState(w, failedop));
                    //    p.steps.Add(res);
                    //    x = FastForward.extractRPSize(FastForward.computeRPG(groundedoperators, res.Item2, this.goal), this.goal, groundedoperators);
                    //}
                    //else
                    //{
                        res = next;
                        p.steps.Add(next);
                        //x = FastForward.extractRPSize(FastForward.computeRPG(groundedoperators, next.Item2, this.goal), this.goal, groundedoperators);
                    //}

                    UnityConsole.Write("Possible apparent next step " + next.Item1 + " and character heuristic of " + y + "\n");
                    if (y < min && y != -1)
                    {
                        best = res;
                        min = y;
                    }

                    if (res.Item2.isGoalState(this.goal) && y!=-1)
                    {
                        //solution found!
                        current.steps.Add(res);
                        solutionPlan = current;
                        avg_branching_factor = avg_branching_factor / depth;
                        UnityConsole.Write("\n Number of nodes = " + nnodes
                                      + ", max branching factor = " + bfactor
                                      + ", avg branching factor = " + avg_branching_factor);
                        return solutionPlan;
                    }
                }


                if (n.Count == 0 || best == null)
                    return null;

                UnityConsole.Write("STEP SELECTED: " + best.Item1 + "\n");
                UnityConsole.Write("----------\n");
                avg_branching_factor += tmp;
                if (tmp > bfactor)
                    bfactor = tmp;

                //add best node to plan
                current.steps.Add(best);
                if (current.steps[current.steps.Count - 1].Item2.isGoalState(this.goal))
                {
                    //solution found!
                    solutionPlan = current;
                    UnityConsole.Write("\n Number of nodes = " + nnodes + " and branching factor = " + bfactor);
                    return solutionPlan;
                }
                depth++;
            }
            return solutionPlan;
        }


        public Plan HeadSpaceXSolution()
        {
            List<Tuple<String, WorldState>> nextStateTuples = w0.getPossibleApparentNextStatesTuples(groundedoperators);
            //int depth = 1;
            //int bfactor = 0;
            //int nnodes = 0;
            //Plan solutionPlan = null;
            Plan current = new Plan(this);
            ////Queue<Plan> queue = new Queue<Plan>();
            //int min = -1;
            //Tuple<String, WorldState> best = null;

            return HeadSpaceX(this.desires, current, this.goal, groundedoperators, this.counteractions);
            
        }

        public Plan HeadSpaceX(List<Desire> desires, Plan plan, WorldState goal, List<Operator> groundedoperators, List<CounterAction> counteractions)
        {
            WorldState w = plan.steps[plan.steps.Count - 1].Item2;
            //UnityConsole.WriteLine("List length is " + desires.Count);
            List<Intention> intentions = w.extractArisingIntentions(desires);
            Boolean haschanged = false;
            /////////// INTENTION ADOPTION STEP ////////////////
            foreach (Intention i in intentions)
            {
                if (!Intention.containsIntention(w.intentions, i))
                {
                    // create the plan for the intention frame
                    PlanningProblem pp = new PlanningProblem(w, Character.createCharacterGoal(i.goals, i.character), groundedoperators);
                    Plan p = pp.FFApparentSolution();

                    // if no plan found do not add intention frame at all
                    if (p != null)
                    {
                        i.plan = new Microplan(p);
                        i.plan.computeCLinks(pp.groundedoperators, w.getCharacter(i.character), i.goals);
                        w.intentions.Add(i);
                        // add motivating step to plan
                        plan.steps.Add(new Tuple<string, WorldState>(i.getDescription(), w.clone()));
                        haschanged = true;
                    }

                }
                else
                {
                    //that intention is already present, add motivations to the intention frame instead.
                    haschanged = true;
                    foreach (Intention i2 in w.intentions)
                    {
                        if(i2.character.Equals(i.character) && i2.goals.Equals(i.goals))
                        {
                            foreach (Character m in i.motivations)
                                if (!i2.motivations.Contains(m))
                                    i2.motivations.Add(m);
                        }
                    }
                }
            }
            if (w.isGoalState(goal))
                return plan;

            /////////////// COMPILING ALL POSSIBLE ACTIONS /////////////////
            List<Tuple<Tuple<String, WorldState>, Intention>> possibleSteps = new List<Tuple<Tuple<string, WorldState>, Intention>>();
            List<Intention> rmv = new List<Intention>();
            foreach (Intention intention in w.intentions)
            {
                //add next step from each plan in the intentions to possible next steps
                int i;
                for(i=0; i<intention.plan.executed.Count; ++i)
                {
                    if (!intention.plan.executed[i])
                        break;
                }
                if (i <= intention.plan.steps.Count - 1)
                {
                    WorldState next = WorldState.getNextState(w, groundedoperators.Find(x => x.text.Equals(intention.plan.steps[i])));
                    possibleSteps.Add(new Tuple<Tuple<String, WorldState>, Intention>(new Tuple<String, WorldState>(intention.plan.steps[i], next), intention));
                }
                       
            }
            if (possibleSteps.Count > 0)
            {
                //TODO: Come up with a better way to choose an action!! Future Work
                Tuple<String, WorldState> step = possibleSteps[0].Item1;
                int selectedindex = 0;

                // Ensure step is the failing step if not executable
                Tuple<string, WorldState> res;
                Operator op = groundedoperators.Find(xy => xy.text.Equals(step.Item1));
                if (!WorldState.isExecutable(op, w))
                {
                    UnityConsole.Write("Action Failed!");
                    NarrativePlanning.Operator failedop = NarrativePlanning.Operator.getFailedOperator(groundedoperators, op);
                    res = new Tuple<string, WorldState>(failedop.text, WorldState.getNextState(w, failedop));
                }
                else
                {
                    res = step;
                }
                plan.steps.Add(res);

                //find the plan which had that step and mark it as executed.
                foreach (Intention i in res.Item2.intentions)
                {
                    if (i.plan.steps[i.plan.executed.Count].Equals(res.Item1))
                        i.plan.executed.Add(true);
                }
            }
            
            
            ////////// COUNTERACTIONS ////////////////
            foreach (CounterAction ca in counteractions)
            {
                if(plan.steps[plan.steps.Count-1].Item2.isGoalState(ca.conditions))
                {
                    //execute counteraction
                    Operator op = groundedoperators.Find(x => x.text.Equals(ca.groundedoperator));
                    WorldState next = WorldState.getNextState( plan.steps[plan.steps.Count -1].Item2, op);
                    plan.steps.Add(new Tuple<string, WorldState>(ca.groundedoperator, next));
                    haschanged = true;
                }
            }


            WorldState last = plan.steps[plan.steps.Count - 1].Item2;

            //////////////// INTENTION UPDATE ////////////////
            WorldState newState = last;
            rmv = new List<Intention>();
            foreach (Intention intention in newState.intentions)
            {
                if (newState.getCharacter(intention.character).isGoalState(intention.goals)
                    || !newState.getCharacter(intention.character).hasMotivations(intention.motivations))
                {
                    rmv.Add(intention);
                    //newState.intentions.Remove(intention);
                }
                else
                {
                    if (CausalLink.isLinkThreatened(intention.plan, newState))
                    {
                        //if a causal link in the plan for the intention is threatened, replace with new plan
                        // if no plan was found, then remove intention from newstate

                        PlanningProblem pp = new PlanningProblem(newState, Character.createCharacterGoal(intention.goals, intention.character), groundedoperators);
                        Plan newPlan = pp.FFApparentSolution();
                        if (newPlan == null)
                        {
                            rmv.Add(intention);
                            //newState.intentions.Remove(intention);
                        }
                        else
                        {
                            intention.plan = new Microplan(newPlan);
                            intention.plan.computeCLinks(pp.groundedoperators, newState.getCharacter(intention.character), intention.goals);
                            haschanged = true;
                        }
                    }

                }
            }
            foreach (Intention r in rmv)
            {
                newState.intentions.Remove(r);
                haschanged = true;
            }

            //if (!last.isGoalState(goal) && last.intentions.Count == 0 && last.extractArisingIntentions(desires).Count == 0)
            //    return null; //FAIL
            if (last.isGoalState(goal))
                return plan;
            if (!haschanged)
                return null; // FAIL
            return HeadSpaceX(desires, plan, goal, groundedoperators, counteractions);
        }

        public void computeRPGOnly()
        {
            FastForward.printRPG(FastForward.computeRPG(groundedoperators, w0, goal));
        }
        public void computeRPGToFixed()
        {
            FastForward.printRPG(FastForward.computeRPGToFixed(groundedoperators, w0, goal));
        }

        public PlanningProblem clone()
        {
            WorldState i = this.w0.clone();
            WorldState g = this.goal.clone();
            List<Operator> o = new List<Operator>();
            foreach (Operator oper in this.groundedoperators)
            {
                o.Add(oper.clone());
            }
            //List<String> gs = new List<string>();
            //foreach(String gop in this.gops){
            //    gs.Add(gop.Clone() as String);
            //}
            return new PlanningProblem(i, g, o);
        }

        private static bool firstPrint = true;

        public static void PrintRPG(FastForward.Layers rpg)
        {
            if (!firstPrint) return;
            firstPrint = false;
            int i = 0;
            StreamWriter file = new StreamWriter("Output.csv");
            PrintPropLayer(rpg.F[i] as WorldState, file);
            while (i < rpg.k)
            {
                i++;
                PrintActionLayer(rpg.A[i] as List<Tuple<Operator, float>>, file);
                PrintPropLayer(rpg.F[i] as WorldState, file);
            }
            file.Close();
        }

        public static void PrintPropLayer(WorldState props, StreamWriter file)
        {
            foreach (DictionaryEntry de in props.tWorld)
            {
                file.Write("TRUE-" + de.Key + "," + de.Value + ",");
            }
            foreach (DictionaryEntry de in props.fWorld)
            {
                file.Write("FALSE-" + de.Key + "," + de.Value + ",");
            }
            file.Write("\n");
        }

        public static void PrintActionLayer(List<Tuple<Operator, float>> actions, StreamWriter file)
        {
            foreach (Tuple<Operator, float> a in actions)
            {
                file.Write(a.Item1.text + "," + a.Item2 + ",");
            }
            file.Write("\n");
        }
    }
}
