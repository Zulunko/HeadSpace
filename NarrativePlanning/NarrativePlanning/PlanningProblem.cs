using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NarrativePlanning
{
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

        /// <summary>
        /// Returns a plan using a FF-based solution.
        /// </summary>
        /// <returns> A solution plan</returns>
        public Plan FFPreferenceSolution()
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
                // XXX PRUNING
                if (w.prunedOperators != null) n = w.getPrunedNextStatesTuplesWithPrefs(preferences);
                else n = w.getPossibleNextStatesTuplesWithPrefs(groundedoperators, preferences);
                n.Sort((a, b) => preferences.GetActionPreference(b.Item1.Split(' ')[0]).CompareTo(preferences.GetActionPreference(a.Item1.Split(' ')[0])));
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
                    FastForward.Layers prefRPG = FastForward.computePreferenceRPG(groundedoperators, next.Item2, this.goal, this.preferences);
                    //PrintRPG(prefRPG);
                    Tuple<int, float> heuristicData = FastForward.extractPrefRPSizeAndPrunedOps(prefRPG, this.goal, next.Item2);

                    Tuple<string, WorldState> res = next;
                    //p.steps.Add(next);
                    float y;
                    if (heuristicData.Item1 == -1)
                        y = -1;
                    else
                        y = heuristicData.Item1;// + (1f - heuristicData.Item2);
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

        public void PrintRPG(FastForward.Layers rpg)
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

        public void PrintPropLayer(WorldState props, StreamWriter file)
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

        public void PrintActionLayer(List<Tuple<Operator, float>> actions, StreamWriter file)
        {
            foreach (Tuple<Operator, float> a in actions)
            {
                file.Write(a.Item1.text + "," + a.Item2 + ",");
            }
            file.Write("\n");
        }
    }
}
