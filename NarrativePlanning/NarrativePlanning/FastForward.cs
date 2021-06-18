using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NarrativePlanning
{
    public class FastForward
    {
        public class Layers
        {
            public Layers(){
                F = new Hashtable();
                A = new Hashtable();
            }

            // Prop layers:
            // <int, WorldState>
            public Hashtable F {
                get;
                set;
            }
            // Action layers:
            // <int, List<Operator>>
            public Hashtable A {
                get;
                set;
            }
            // Total layers
            public int k {
                get;
                set;
            }

            //public void add(Hashtable table, int t, Literal l){
            //    if (table.ContainsKey(t))
            //        table.Add(t, new Hashtable());
            //    table.Add(t, );
            //}
        }

        public FastForward()
        {
        }

        /// <summary>
        /// Accessor function for accessing the right hashtable in the object.
        /// </summary>
        /// <param name="obj">Basically takes a character or worldstate object</param>
        /// <returns>Should return the correct hashtable</returns>
        delegate Hashtable del(Object obj);

        delegate List<Operator> del2(Object obj);

        delegate List<Tuple<Operator, float>> del3(Object obj);

        /// <summary>
        /// Computes the relaxed plan graph for a given input
        /// </summary>
        /// <param name="operators">Grounded operators</param>
        /// <param name="initial">Initial world state</param>
        /// <param name="goal">Goal worldstate</param>
        /// <returns>Returns the RPG in a Layers form.</returns>
        public static Layers computeRPG(List<Operator> operators, WorldState initial, WorldState goal){
            Layers l = null;
            int t = 0;
            l = new Layers();
            l.F.Add(0, initial);

            while(!((WorldState)l.F[t]).isGoalState(goal)){
                t++;
                List<Operator> At = new List<Operator>();
                foreach(Operator o in operators){
                    if(WorldState.isExecutable(o, ((WorldState)l.F[t-1]))){
                        At.Add(o);
                    }
                }
                l.A.Add(t, At);
                l.F.Add(t, ((WorldState)l.F[t-1]).clone());
                foreach(Operator o in At){
                    l.F[t] = WorldState.getNextRelaxedState(((WorldState)l.F[t]), o);
                }
                if ((l.F[t] as WorldState).HasChangedFrom(l.F[t - 1] as WorldState)){
                    l.k = t;
                    return l;
                }
                    
                
            }
            l.k = t;
            return l;
        }

        /// <summary>
        /// Computes the relaxed plan graph for a given input to the fixed point
        /// </summary>
        /// <param name="operators">Grounded operators</param>
        /// <param name="initial">Initial world state</param>
        /// <param name="goal">Goal worldstate</param>
        /// <returns>Returns the RPG in a Layers form.</returns>
        public static Layers computeRPGToFixed(List<Operator> operators, WorldState initial, WorldState goal)
        {
            Layers l = null;
            int t = 0;
            l = new Layers();
            l.F.Add(0, initial);

            while (true)
            {
                t++;
                List<Operator> At = new List<Operator>();
                foreach (Operator o in operators)
                {
                    if (WorldState.isExecutable(o, ((WorldState)l.F[t - 1])))
                    {
                        At.Add(o);
                    }
                }
                l.A.Add(t, At);
                l.F.Add(t, ((WorldState)l.F[t - 1]).clone());
                foreach (Operator o in At)
                {
                    l.F[t] = WorldState.getNextRelaxedState(((WorldState)l.F[t]), o);
                }
                if ((l.F[t] as WorldState).HasChangedFrom(l.F[t - 1] as WorldState))
                {
                    l.k = t;
                    if (!((WorldState)l.F[t]).isGoalState(goal))
                    {
                        Console.WriteLine("HEY U HECCIN FAILED");
                    }
                    return l;
                }
            }
        }

        /// <summary>
        /// Computes the relaxed plan graph for a given input to the fixed point with preference metadata
        /// </summary>
        /// <param name="operators">Grounded operators</param>
        /// <param name="initial">Initial world state</param>
        /// <param name="goal">Goal worldstate</param>
        /// <param name="preferences">Preference set</param>
        /// <returns>Returns the RPG in a Layers form.</returns>
        public static Layers computePreferenceRPG(List<Operator> operators, WorldState initial, WorldState goal, Preferences preferences)
        {
            Layers l = null;
            int t = 0;
            l = new Layers();
            l.F.Add(0, initial);

            while (true)
            {
                t++;
                List<Tuple<Operator, float>> At = new List<Tuple<Operator, float>>();
                // Add operators for every executable op
                foreach (Operator o in operators)
                {
                    if (WorldState.isExecutable(o, ((WorldState)l.F[t - 1])))
                    {
                        At.Add(new Tuple<Operator, float>(o, WorldState.getPrefForActionInstance(o, (WorldState)l.F[t-1], preferences)));
                    }
                }
                l.A.Add(t, At);
                l.F.Add(t, ((WorldState)l.F[t - 1]).clone());
                foreach (Tuple<Operator, float> tuple in At)
                {
                    l.F[t] = WorldState.getNextRelaxedState(((WorldState)l.F[t]), tuple);
                }
                if ((l.F[t] as WorldState).HasChangedFrom(l.F[t - 1] as WorldState))
                {
                    l.k = t;
                    if (!((WorldState)l.F[t]).isGoalState(goal))
                    {
                        Console.WriteLine("HEY U HECCIN FAILED");
                    }
                    return l;
                }
            }
        }

        /// <summary>
        /// Computes the relaxed plan graph but for the character 
        /// and not the world
        /// </summary>
        /// <param name="operators">Lisat of grounded operators</param>
        /// <param name="initial">Initial worldstate</param>
        /// <param name="goal">Goal worldstate</param>
        /// <param name="charactername">Character to compute the RPG for.</param>
        /// <returns>The RPG in layers form constructed using acitons that the character can perform.</returns>
        public static Layers computeCharacterRPG(List<Operator> operators, WorldState initial, WorldState goal, String charactername)
        {
            Character characteri = initial.characters.Find(x => x.name.Equals(charactername));
            Layers l = null;
            int t = 0;
            l = new Layers();
            l.F.Add(0, characteri);

            Character characterf = goal.characters.Find(x => x.name.Equals(charactername));
            if(characterf == null)
            {
                //the character had no goal.
                l.k = t;
                return l;
            }

            while (!((Character)l.F[t]).isGoalState(characterf))
            {
                t++;
                List<Operator> At = new List<Operator>();
                foreach (Operator o in operators)
                {
                    if (o.character.Equals(charactername) && Character.isApparentlyExecutable(o, ((Character)l.F[t - 1])))
                    {
                        At.Add(o);
                    }
                }
                l.A.Add(t, At);
                l.F.Add(t, ((Character)l.F[t - 1]).clone());
                foreach (Operator o in At)
                {
                    l.F[t] = Character.getNextRelaxedState(((Character)l.F[t]), o);
                }
                if ((l.F[t] as Character).Equals(l.F[t - 1] as Character))
                {
                    //could not reach goal.
                    l.k = t;
                    return l;
                }


            }
            l.k = t;
            return l;
        }

		/// <summary>
		/// Computes the relaxed plan graph but takes all goals (char and author)
		///  into consideration
		/// and also applies effects of failing and succeeeding operator
        /// </summary>
        /// <param name="operators">List of grounded operators</param>
        /// <param name="current">Initial worldstate</param>
        /// <param name="goal">Goal worldstate</param>
        /// <returns>The RPG in layers form constructed using acitons that the character can perform.</returns>
		public static Layers computeCompleteRPG(List<Operator> operators, WorldState current, WorldState goal)
        {
            Layers l = null;
            int t = 0;
            l = new Layers();
			//add everything to layer 0
			l.F.Add(0, current);
            
            //if (characterf == null)
            //{
            //    //the character had no goal.
            //    l.k = t;
            //    return l;
            //}

			while (!(((WorldState)l.F[t]).isGoalState(goal)  ))
            {
                t++;
                List<Operator> At = new List<Operator>();
                //change this to next actions in intention plan
                foreach (Operator o in operators)
                {
					Character character = ((WorldState)(l.F[t - 1])).characters.Find(x => x.name.Equals(o.character));
					if (Character.isApparentlyExecutable(o, character))
                    {
                        At.Add(o);
                    }
                }
                l.A.Add(t, At);
				l.F.Add(t, ((WorldState)l.F[t - 1]).clone());
                foreach (Operator o in At)
                {
					if(o.name.Contains("-false"))
					{
						NarrativePlanning.Operator failedop = NarrativePlanning.Operator.getFailedOperator(operators, o);
						l.F[t] = WorldState.getNextRelaxedState((WorldState)l.F[t], failedop);

					}
					l.F[t] = WorldState.getNextRelaxedState((WorldState)l.F[t], o);
                }
				if ((l.F[t] as WorldState).Equals(l.F[t - 1] as WorldState))
                {
                    //could not reach goal.
                    l.k = t;
                    return l;
                }


            }
            l.k = t;
            return l;
        }

        /// <summary>
        /// Finds the hueristic measure for the RPG.
        /// </summary>
        /// <param name="l">The layers</param>
        /// <param name="g">Goal worldstate</param>
        /// <param name="operators">List of grounded operators</param>
        /// <returns> A heuristic number, -1 for failure. Lower number is better.</returns>
        public static int extractRPSize(Layers l, WorldState g, List<Operator> operators){
            int selectedActions = 0;

            if(!((WorldState)l.F[l.k]).isGoalState(g))
            {
                //if (l.k == 0)
                //    return 0;
                //else
                    return -1;
            }
            List<int> firstlevels = new List<int>();
            foreach(String lit in g.tWorld.Keys){
                firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld));
            }
            foreach (String lit in g.fWorld.Keys)
            {
                firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld));
            }
            //character states ignored!!
            int m = firstlevels.Max();

            Hashtable Gt = new Hashtable();
            for (int t = 0; t <= m; t++){
                WorldState w = new WorldState(new Hashtable(), new Hashtable(), null);
                foreach(String lit in g.tWorld.Keys){
                    if(firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld) == t){
                        w.tWorld.Add(lit, 1);
                    }
                }
                foreach (String lit in g.fWorld.Keys)
                {
                    if (firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld) == t)
                    {
                        w.fWorld.Add(lit, 1);
                    }
                }
                Gt.Add(t, w);
            }

            for (int t = m; t >= 1; --t){
                foreach(String lit in (Gt[t] as WorldState).tWorld.Keys){
                    foreach(Operator o in operators){
                        //check if effects of action have literal as a component
                        if(o.effT.Contains(lit) || o.preT.Contains(lit)){
                            //check if that action appeared first time at t
                            if(firstLevel(o, l.A, ((obj) => (obj as List<Operator>))) == t){
                                selectedActions++;
                                //now add all of its preconditions as subgoals in Gts
                                foreach(String prelit in o.preT.Keys){
                                    int level = firstLevel(prelit, l.F, (obj) => ((WorldState)obj).tWorld);
                                    if (level == -1 || level>= t)
                                        level = t - 1;
                                    if (!(Gt[level] as WorldState).tWorld.Contains(prelit))
                                        (Gt[level] as WorldState).tWorld.Add(prelit, 1);
                                }
                                foreach (String prelit in o.preF.Keys)
                                {
                                    int level = firstLevel(prelit, l.F, (obj) => ((WorldState)obj).fWorld);
                                    if (level == -1 || level >= t)
                                        level = t - 1;
                                    if (!(Gt[level] as WorldState).fWorld.Contains(prelit))
                                        (Gt[level] as WorldState).fWorld.Add(prelit, 1);
                                }
                            }
                        }
                    }
                }
                foreach (String lit in (Gt[t] as WorldState).fWorld.Keys)
                {
                    foreach (Operator o in operators)
                    {
                        //check if effects of action have literal as a component
                        if (o.effF.Contains(lit) || o.preF.Contains(lit))
                        {
                            //check if that action appeared first time at t
                            if (firstLevel(o, l.A, ((obj) => (obj as List<Operator>))) == t)
                            {
                                selectedActions++;
                                //now add all of its preconditions as subgoals in Gts
                                foreach (String prelit in o.preT.Keys)
                                {
                                    int level = firstLevel(prelit, l.F, (obj) => ((WorldState)obj).tWorld);
                                    if (level == -1 || level >= t)
                                        level = t - 1;
                                    if(!(Gt[level] as WorldState).tWorld.Contains(prelit))
                                        (Gt[level] as WorldState).tWorld.Add(prelit, 1);
                                }
                                foreach (String prelit in o.preF.Keys)
                                {
                                    int level = firstLevel(prelit, l.F, (obj) => ((WorldState)obj).fWorld);
                                    if (level == -1 || level >= t)
                                        level = t - 1;
                                    if (!(Gt[level] as WorldState).fWorld.Contains(prelit))
                                        (Gt[level] as WorldState).fWorld.Add(prelit, 1);
                                }
                                //break;
                            }
                        }
                    }
                }
            }
            return selectedActions;
        }

        /// <summary>
        /// Finds the hueristic measure for the RPG.
        /// </summary>
        /// <param name="l">The layers</param>
        /// <param name="g">Goal worldstate</param>
        /// <param name="operators">List of grounded operators</param>
        /// <returns> A heuristic number, -1 for failure. Lower number is better.</returns>
        public static int extractRPSizeAndPrunedOps(Layers l, WorldState g, WorldState i)
        {
            int selectedActions = 0;

            if (!((WorldState)l.F[l.k]).isGoalState(g))
            {
                return -1;
            }

            // 1. Calculate max level for any goal literal at first equivalent value to final layer. 
            int goalCount = 0;
            float prefMatch = 0;
            List<int> firstlevels = new List<int>();
            foreach (String lit in g.tWorld.Keys)
            {
                firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld));
                goalCount++;
            }
            foreach (String lit in g.fWorld.Keys)
            {
                firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld));
                goalCount++;
            }
            int m = firstlevels.Max();

            // 2. Add first equivalent level (to final layer) goal props to goal set at that layer.
            Hashtable Gt = new Hashtable();
            for (int t = 0; t <= m; t++)
            {
                WorldState w = new WorldState(new Hashtable(), new Hashtable(), null);
                foreach (String lit in g.tWorld.Keys)
                {
                    if (firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld) == t)
                        w.tWorld.Add(lit, 1);
                }
                foreach (String lit in g.fWorld.Keys)
                {
                    if (firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld) == t)
                        w.fWorld.Add(lit, 1);
                }
                Gt.Add(t, w);
            }

            // 3. Iterate from final first level layer and work backwards.
            for (int t = m; t >= 1; --t)
            {
                // 3a. For each layer, merge the list of true and false goal predicates to simplify logic and prevent copy-pasting.
                //  Also, extract preference values for convenience.
                List<Tuple<string, bool, float>> goalData = new List<Tuple<string, bool, float>>();
                foreach (string lit in (Gt[t] as WorldState).tWorld.Keys)
                {
                    goalData.Add(new Tuple<string, bool, float>(lit, true, Convert.ToSingle((Gt[t] as WorldState).tWorld[lit])));
                }
                foreach (string lit in (Gt[t] as WorldState).fWorld.Keys)
                {
                    goalData.Add(new Tuple<string, bool, float>(lit, false, Convert.ToSingle((Gt[t] as WorldState).fWorld[lit])));
                }

                HashSet<string> satisfiedGoals = new HashSet<string>();
                // 3c. Iterate through each goal to find an action that provides that goal.
                foreach (Tuple<string, bool, float> goalLit in goalData)
                {
                    //Console.WriteLine("Goal: " + goalLit.Item1 + " at " + goalLit.Item3);
                    //Check if this goal has already been satisfied by actions added in this layer.
                    if (satisfiedGoals.Contains(goalLit.Item1 + "!" + goalLit.Item2.ToString()))
                    {
                        Console.WriteLine("IGNORING GOAL " + goalLit.Item1);
                        continue;
                    }

                    // 3c1. Sort the operators that can fulfill this goal to try to find the best operator first.
                    string lit = goalLit.Item1;
                    List<Operator> actions = (List<Operator>)l.A[t];
                    foreach (Operator o in actions)
                    {
                        bool found = false;

                        // 3c1a. Item2 says whether the literal is positive or negative, so we check if the literal is positive whether
                        //  it's in the positive effects of this action and if it's negative whether it's in the negative effects of this action.
                        //check if effects of action have literal as a component
                        if ((goalLit.Item2 && o.effT.Contains(lit)) || (!goalLit.Item2 && o.effF.Contains(lit)))
                        {
                            // EWL: The below is unnecessary because we want to add the highest-value action which this one is guaranteed
                            //  to be. We don't care if it's the first time the action has appeared, as it provides more preference-matching
                            //  actions prior to this layer if not.
                            //check if that action appeared first time at t
                            //if (firstLevelIgnorePrefs(o, l.A, ((obj) => (obj as List<Tuple<Operator, float>>))) == t)
                            //{

                            // 3c1a1. We have found an action that provides our goal. This action is guaranteed to be the highest-value action
                            //  available because of our operation sorting, so we select this action and set all of its preconditions as subgoals.
                            //PRUNING
                            if (t == 1)
                            {
                                //Console.WriteLine("----PRUNING LAYER");
                                //Console.Write(o.text);
                                //Console.WriteLine();
                                if (i.prunedOperators == null)
                                    i.prunedOperators = new List<Operator>();
                                i.prunedOperators.Add(o);
                                //Console.WriteLine("----");
                            }
                            if (found) continue;
                            found = true;
                            selectedActions++;
                            //Console.WriteLine("Selected: " + o.name);
                            //now add all of its preconditions as subgoals in Gts
                            foreach (String prelit in o.preT.Keys)
                            {
                                // 3c1a1a. We add the precond as a subgoal to the lowest proposition level where this precond has a value equal to
                                //  its current value.
                                int level = firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld);
                                if (level == -1 || level >= t)
                                    level = t - 1;
                                if (!(Gt[level] as WorldState).tWorld.Contains(prelit))
                                    (Gt[level] as WorldState).tWorld.Add(prelit, ((WorldState)l.F[level]).tWorld[lit]);
                            }
                            foreach (String prelit in o.preF.Keys)
                            {
                                int level = firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld);
                                if (level == -1 || level >= t)
                                    level = t - 1;
                                if (!(Gt[level] as WorldState).fWorld.Contains(prelit))
                                    (Gt[level] as WorldState).fWorld.Add(prelit, ((WorldState)l.F[level]).fWorld[lit]);
                            }

                            // Register goals that were satisfied by this action, regardless of whether they're the goal we're seeking.
                            foreach (String efflit in o.effT.Keys)
                            {
                                satisfiedGoals.Add(efflit + "!" + true.ToString());
                            }
                            foreach (String efflit in o.effF.Keys)
                            {
                                satisfiedGoals.Add(efflit + "!" + false.ToString());
                            }

                            // EWL: Once we find an action for this goal, we should really just break so we can continue to the next goal.
                            //break;
                            //}
                        }
                    }
                }
            }
            return selectedActions;
        }

        /// <summary>
        /// Extracts RP with preference data.
        /// </summary>
        /// <param name="l">The layers</param>
        /// <param name="g">Goal worldstate</param>
        /// <returns> A heuristic number, -1 for failure. Lower number is better.</returns>
        public static Tuple<int, float> extractPrefRPSize(Layers l, WorldState g)//, List<Operator> operators) <- Trying to rely on the operator lists in l, not sure why we need this.
        {
            int selectedActions = 0;

            if (!((WorldState)l.F[l.k]).isGoalState(g))
            {
                return new Tuple<int, float>(-1, 0);
            }

            // 1. Calculate max level for any goal literal at first equivalent value to final layer. 
            int goalCount = 0;
            float prefMatch = 0;
            List<int> firstlevels = new List<int>();
            foreach (String lit in g.tWorld.Keys)
            {
                //firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld));
                float goalVal = Convert.ToSingle(((WorldState)l.F[l.k]).tWorld[lit]);
                firstlevels.Add(firstEqualPrefLevel(l.k, lit, l.F, true, goalVal));
                prefMatch += goalVal;
                goalCount++;
            }
            foreach (String lit in g.fWorld.Keys)
            {
                //firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld));
                float goalVal = Convert.ToSingle(((WorldState)l.F[l.k]).fWorld[lit]);
                firstlevels.Add(firstEqualPrefLevel(l.k, lit, l.F, false, goalVal));
                prefMatch += goalVal;
                goalCount++;
            }
            prefMatch /= goalCount;
            //Console.WriteLine("------Pref match value: " + prefMatch);
            //character states ignored!!
            int m = firstlevels.Max();
            //Console.WriteLine("MAX CONSIDERED: " + m + " OUT OF " + l.k);

            // 2. Add first equivalent level (to final layer) goal props to goal set at that layer.
            Hashtable Gt = new Hashtable();
            for (int t = 0; t <= m; t++)
            {
                WorldState w = new WorldState(new Hashtable(), new Hashtable(), null);
                foreach (String lit in g.tWorld.Keys)
                {
                    float prefVal = Convert.ToSingle(((WorldState)l.F[l.k]).tWorld[lit]);
                    //if (firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld) == t)
                    if (firstEqualPrefLevel(l.k, lit, l.F, true, prefVal) == t)
                    {
                        w.tWorld.Add(lit, prefVal);
                    }
                }
                foreach (String lit in g.fWorld.Keys)
                {
                    float prefVal = Convert.ToSingle(((WorldState)l.F[l.k]).fWorld[lit]);
                    //if (firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld) == t)
                    if (firstEqualPrefLevel(l.k, lit, l.F, false, prefVal) == t)
                    {
                        w.fWorld.Add(lit, prefVal);
                    }
                }
                Gt.Add(t, w);
            }

            // 3. Iterate from final first level layer and work backwards.
            for (int t = m; t >= 1; --t)
            {
                // 3a. For each layer, merge the list of true and false goal predicates to simplify logic and prevent copy-pasting.
                //  Also, extract preference values for convenience.
                List<Tuple<string, bool, float>> goalData = new List<Tuple<string, bool, float>>();
                foreach (string lit in (Gt[t] as WorldState).tWorld.Keys)
                {
                    goalData.Add(new Tuple<string, bool, float>(lit, true, Convert.ToSingle((Gt[t] as WorldState).tWorld[lit])));
                }
                foreach (string lit in (Gt[t] as WorldState).fWorld.Keys)
                {
                    goalData.Add(new Tuple<string, bool, float>(lit, false, Convert.ToSingle((Gt[t] as WorldState).fWorld[lit])));
                }
                // 3b. Sort the goal literals in order of highest preference to lowest preference to ensure we fulfill the highest preference goal first.
                goalData.Sort((a, b) => b.Item3.CompareTo(a.Item3));

                HashSet<string> satisfiedGoals = new HashSet<string>();
                // 3c. Iterate through each goal to find an action that provides that goal.
                foreach (Tuple<string, bool, float> goalLit in goalData)
                {
                    //Console.WriteLine("Goal: " + goalLit.Item1 + " at " + goalLit.Item3);
                    //Check if this goal has already been satisfied by actions added in this layer.
                    if (satisfiedGoals.Contains(goalLit.Item1 + "!" + goalLit.Item2.ToString()))
                    {
                        Console.WriteLine("IGNORING GOAL " + goalLit.Item1);
                        continue;
                    }

                    // 3c1. Sort the operators that can fulfill this goal to try to find the best operator first.
                    string lit = goalLit.Item1;
                    List<Tuple<Operator, float>> actTuples = (List<Tuple<Operator, float>>)l.A[t];
                    actTuples.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                    foreach (Tuple<Operator, float> prefTuple in actTuples)
                    {
                        Operator o = prefTuple.Item1;
                        
                        // 3c1a. Item2 says whether the literal is positive or negative, so we check if the literal is positive whether
                        //  it's in the positive effects of this action and if it's negative whether it's in the negative effects of this action.
                        //check if effects of action have literal as a component
                        if ((goalLit.Item2 && o.effT.Contains(lit)) || (!goalLit.Item2 && o.effF.Contains(lit)))
                        {
                            // EWL: The below is unnecessary because we want to add the highest-value action which this one is guaranteed
                            //  to be. We don't care if it's the first time the action has appeared, as it provides more preference-matching
                            //  actions prior to this layer if not.
                            //check if that action appeared first time at t
                            //if (firstLevelIgnorePrefs(o, l.A, ((obj) => (obj as List<Tuple<Operator, float>>))) == t)
                            //{

                            // 3c1a1. We have found an action that provides our goal. This action is guaranteed to be the highest-value action
                            //  available because of our operation sorting, so we select this action and set all of its preconditions as subgoals.
                            selectedActions++;
                            //Console.WriteLine("Selected: " + o.name);
                            //now add all of its preconditions as subgoals in Gts
                            foreach (String prelit in o.preT.Keys)
                            {
                                // 3c1a1a. We add the precond as a subgoal to the lowest proposition level where this precond has a value equal to
                                //  its current value.
                                float prefVal = Convert.ToSingle(((WorldState)l.F[t - 1]).tWorld[lit]);
                                int level = firstEqualPrefLevel(t - 1, prelit, l.F, true, prefVal);
                                if (level == -1 || level >= t)
                                    level = t - 1;
                                if (!(Gt[level] as WorldState).tWorld.Contains(prelit))
                                    (Gt[level] as WorldState).tWorld.Add(prelit, ((WorldState)l.F[level]).tWorld[lit]);
                            }
                            foreach (String prelit in o.preF.Keys)
                            {
                                float prefVal = Convert.ToSingle(((WorldState)l.F[t-1]).fWorld[lit]);
                                int level = firstEqualPrefLevel(t - 1, prelit, l.F, false, prefVal);
                                if (level == -1 || level >= t)
                                    level = t - 1;
                                if (!(Gt[level] as WorldState).fWorld.Contains(prelit))
                                    (Gt[level] as WorldState).fWorld.Add(prelit, ((WorldState)l.F[level]).fWorld[lit]);
                            }

                            // Register goals that were satisfied by this action, regardless of whether they're the goal we're seeking.
                            foreach (String efflit in o.effT.Keys)
                            {
                                satisfiedGoals.Add(efflit + "!" + true.ToString());
                            }
                            foreach (String efflit in o.effF.Keys)
                            {
                                satisfiedGoals.Add(efflit + "!" + false.ToString());
                            }

                            // EWL: Once we find an action for this goal, we should really just break so we can continue to the next goal.
                            break;
                            //}
                        }
                    }
                }
            }
            return new Tuple<int, float>(selectedActions, prefMatch);
        }

        /// <summary>
        /// Extracts RP with preference data and sets pruned operators on the initial world state.
        /// </summary>
        /// <param name="l">The layers</param>
        /// <param name="g">Goal worldstate</param>
        /// <param name="i">Initial worldstate, solely passed in to add pruned operators</param>
        /// <returns> A heuristic number, -1 for failure. Lower number is better.</returns>
        public static Tuple<int, float> extractPrefRPSizeAndPrunedOps(Layers l, WorldState g, WorldState i)//, List<Operator> operators) <- Trying to rely on the operator lists in l, not sure why we need this.
        {
            int selectedActions = 0;

            if (!((WorldState)l.F[l.k]).isGoalState(g))
            {
                return new Tuple<int, float>(-1, 0);
            }

            // 1. Calculate max level for any goal literal at first equivalent value to final layer. 
            int goalCount = 0;
            float prefMatch = 0;
            List<int> firstlevels = new List<int>();
            foreach (String lit in g.tWorld.Keys)
            {
                //firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld));
                float goalVal = Convert.ToSingle(((WorldState)l.F[l.k]).tWorld[lit]);
                firstlevels.Add(firstEqualPrefLevel(l.k, lit, l.F, true, goalVal));
                prefMatch += goalVal;
                goalCount++;
            }
            foreach (String lit in g.fWorld.Keys)
            {
                //firstlevels.Add(firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld));
                float goalVal = Convert.ToSingle(((WorldState)l.F[l.k]).fWorld[lit]);
                firstlevels.Add(firstEqualPrefLevel(l.k, lit, l.F, false, goalVal));
                prefMatch += goalVal;
                goalCount++;
            }
            prefMatch /= goalCount;
            //Console.WriteLine("------Pref match value: " + prefMatch);
            //character states ignored!!
            int m = firstlevels.Max();
            //Console.WriteLine("MAX CONSIDERED: " + m + " OUT OF " + l.k);

            // 2. Add first equivalent level (to final layer) goal props to goal set at that layer.
            Hashtable Gt = new Hashtable();
            for (int t = 0; t <= m; t++)
            {
                WorldState w = new WorldState(new Hashtable(), new Hashtable(), null);
                foreach (String lit in g.tWorld.Keys)
                {
                    float prefVal = Convert.ToSingle(((WorldState)l.F[l.k]).tWorld[lit]);
                    //if (firstLevel(lit, l.F, (obj) => ((WorldState)obj).tWorld) == t)
                    if (firstEqualPrefLevel(l.k, lit, l.F, true, prefVal) == t)
                    {
                        w.tWorld.Add(lit, prefVal);
                    }
                }
                foreach (String lit in g.fWorld.Keys)
                {
                    float prefVal = Convert.ToSingle(((WorldState)l.F[l.k]).fWorld[lit]);
                    //if (firstLevel(lit, l.F, (obj) => ((WorldState)obj).fWorld) == t)
                    if (firstEqualPrefLevel(l.k, lit, l.F, false, prefVal) == t)
                    {
                        w.fWorld.Add(lit, prefVal);
                    }
                }
                Gt.Add(t, w);
            }

            // 3. Iterate from final first level layer and work backwards.
            for (int t = m; t >= 1; --t)
            {
                // 3a. For each layer, merge the list of true and false goal predicates to simplify logic and prevent copy-pasting.
                //  Also, extract preference values for convenience.
                List<Tuple<string, bool, float>> goalData = new List<Tuple<string, bool, float>>();
                foreach (string lit in (Gt[t] as WorldState).tWorld.Keys)
                {
                    goalData.Add(new Tuple<string, bool, float>(lit, true, Convert.ToSingle((Gt[t] as WorldState).tWorld[lit])));
                }
                foreach (string lit in (Gt[t] as WorldState).fWorld.Keys)
                {
                    goalData.Add(new Tuple<string, bool, float>(lit, false, Convert.ToSingle((Gt[t] as WorldState).fWorld[lit])));
                }
                // 3b. Sort the goal literals in order of highest preference to lowest preference to ensure we fulfill the highest preference goal first.
                goalData.Sort((a, b) => b.Item3.CompareTo(a.Item3));

                HashSet<string> satisfiedGoals = new HashSet<string>();
                // 3c. Iterate through each goal to find an action that provides that goal.
                foreach (Tuple<string, bool, float> goalLit in goalData)
                {
                    //Console.WriteLine("Goal: " + goalLit.Item1 + " at " + goalLit.Item3);
                    //Check if this goal has already been satisfied by actions added in this layer.
                    if (satisfiedGoals.Contains(goalLit.Item1 + "!" + goalLit.Item2.ToString()))
                    {
                        Console.WriteLine("IGNORING GOAL " + goalLit.Item1);
                        continue;
                    }

                    // 3c1. Sort the operators that can fulfill this goal to try to find the best operator first.
                    string lit = goalLit.Item1;
                    List<Tuple<Operator, float>> actTuples = (List<Tuple<Operator, float>>)l.A[t];
                    actTuples.Sort((a, b) => b.Item2.CompareTo(a.Item2));
                    foreach (Tuple<Operator, float> prefTuple in actTuples)
                    {
                        bool found = false;
                        Operator o = prefTuple.Item1;

                        // 3c1a. Item2 says whether the literal is positive or negative, so we check if the literal is positive whether
                        //  it's in the positive effects of this action and if it's negative whether it's in the negative effects of this action.
                        //check if effects of action have literal as a component
                        if ((goalLit.Item2 && o.effT.Contains(lit)) || (!goalLit.Item2 && o.effF.Contains(lit)))
                        {
                            // EWL: The below is unnecessary because we want to add the highest-value action which this one is guaranteed
                            //  to be. We don't care if it's the first time the action has appeared, as it provides more preference-matching
                            //  actions prior to this layer if not.
                            //check if that action appeared first time at t
                            //if (firstLevelIgnorePrefs(o, l.A, ((obj) => (obj as List<Tuple<Operator, float>>))) == t)
                            //{

                            // 3c1a1. We have found an action that provides our goal. This action is guaranteed to be the highest-value action
                            //  available because of our operation sorting, so we select this action and set all of its preconditions as subgoals.
                            //PRUNING
                            if (t == 1)
                            {
                                //Console.WriteLine("----PRUNING LAYER");
                                //Console.Write(o.text);
                                //Console.WriteLine();
                                if (i.prunedOperators == null)
                                    i.prunedOperators = new List<Operator>();
                                i.prunedOperators.Add(o);
                                //Console.WriteLine("----");
                            }
                            if (found) continue;
                            found = true;
                            selectedActions++;
                            //Console.WriteLine("Selected: " + o.name);
                            //now add all of its preconditions as subgoals in Gts
                            foreach (String prelit in o.preT.Keys)
                            {
                                // 3c1a1a. We add the precond as a subgoal to the lowest proposition level where this precond has a value equal to
                                //  its current value.
                                float prefVal = Convert.ToSingle(((WorldState)l.F[t - 1]).tWorld[lit]);
                                int level = firstEqualPrefLevel(t - 1, prelit, l.F, true, prefVal);
                                if (level == -1 || level >= t)
                                    level = t - 1;
                                if (!(Gt[level] as WorldState).tWorld.Contains(prelit))
                                    (Gt[level] as WorldState).tWorld.Add(prelit, ((WorldState)l.F[level]).tWorld[lit]);
                            }
                            foreach (String prelit in o.preF.Keys)
                            {
                                float prefVal = Convert.ToSingle(((WorldState)l.F[t - 1]).fWorld[lit]);
                                int level = firstEqualPrefLevel(t - 1, prelit, l.F, false, prefVal);
                                if (level == -1 || level >= t)
                                    level = t - 1;
                                if (!(Gt[level] as WorldState).fWorld.Contains(prelit))
                                    (Gt[level] as WorldState).fWorld.Add(prelit, ((WorldState)l.F[level]).fWorld[lit]);
                            }

                            // Register goals that were satisfied by this action, regardless of whether they're the goal we're seeking.
                            foreach (String efflit in o.effT.Keys)
                            {
                                satisfiedGoals.Add(efflit + "!" + true.ToString());
                            }
                            foreach (String efflit in o.effF.Keys)
                            {
                                satisfiedGoals.Add(efflit + "!" + false.ToString());
                            }

                            // EWL: Once we find an action for this goal, we should really just break so we can continue to the next goal.
                            //break;
                            //}
                        }
                    }
                }
            }
            return new Tuple<int, float>(selectedActions, prefMatch);
        }

        /// <summary>
        /// Finds the hueristic measure for the character RPG
        /// </summary>
        /// <param name="l">Layers</param>
        /// <param name="g">Goal worldstate</param>
        /// <param name="operators">List of grounded operators</param>
        /// <param name="charactername"> The character name</param>
        /// <returns> A heuristic number, -1 for failure. Lower number is better.</returns>
        public static int extractCharacterRPSize(Layers l, WorldState g, List<Operator> operators, String charactername)
		{
			int selectedActions = 0;
			Character characterg = g.characters.Find(x => x.name.Equals(charactername));
            if(characterg == null)
            {
                //no goal exists for the character
                return -1;
            }
			if (!((Character)l.F[l.k]).isGoalState(characterg))
			{
				//if (l.k == 0)
				//    return 0;
				//else
				return -1;
			}
            if(l.k==0)
            {
                return 0;
            }
			List<int> firstlevels = new List<int>();
			foreach (String lit in characterg.bPlus.Keys)
			{
				firstlevels.Add(firstLevel(lit, l.F, (obj) => ((Character)obj).bPlus));
			}
			foreach (String lit in characterg.bMinus.Keys)
			{
				firstlevels.Add(firstLevel(lit, l.F, (obj) => ((Character)obj).bMinus));
			}
			foreach (String lit in characterg.unsure.Keys)
			{
				firstlevels.Add(firstLevel(lit, l.F, (obj) => ((Character)obj).unsure));
			}

			int m = firstlevels.Max();

			Hashtable Gt = new Hashtable();
			for (int t = 0; t <= m; ++t)
			{
				Character newCharacter = new Character(new Hashtable(), new Hashtable(), new Hashtable());
				foreach (String lit in characterg.bPlus.Keys)
				{
					if (firstLevel(lit, l.F, (obj) => ((Character)obj).bPlus) == t)
					{
						newCharacter.bPlus.Add(lit, 1);
					}
				}
				foreach (String lit in characterg.bMinus.Keys)
				{
					if (firstLevel(lit, l.F, (obj) => ((Character)obj).bMinus) == t)
					{
						newCharacter.bMinus.Add(lit, 1);
					}
				}
				foreach (String lit in characterg.unsure.Keys)
				{
					if (firstLevel(lit, l.F, (obj) => ((Character)obj).unsure) == t)
					{
						newCharacter.unsure.Add(lit, 1);
					}
				}
				Gt.Add(t, newCharacter);
			}

			for (int t = m; t >= 1; --t)
			{
				foreach (String lit in (Gt[t] as Character).bPlus.Keys)
				{
					foreach (Operator o in operators)
					{
						//ensure character is the one performing the operation
						if (o.character.Equals(charactername))
						{
							//check if perceived positive effects of action have literal as a component
							if (o.effBPlus.Contains(lit))
							{
								//check if that action appeared first time at t
								if (firstLevel(o, l.A, ((obj) => (obj as List<Operator>))) == t)
								{
									selectedActions++;
									//now add all of its preconditions as subgoals in Gts
									foreach (String prelit in o.preBPlus.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).bPlus);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).bPlus.Contains(prelit))
											(Gt[level] as Character).bPlus.Add(prelit, 1);
									}
									foreach (String prelit in o.preBMinus.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).bMinus);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).bMinus.Contains(prelit))
											(Gt[level] as Character).bMinus.Add(prelit, 1);
									}
									foreach (String prelit in o.preUnsure.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).unsure);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).unsure.Contains(prelit))
											(Gt[level] as Character).unsure.Add(prelit, 1);
									}
								}
							}
						}
					}
				}
				foreach (String lit in (Gt[t] as Character).bMinus.Keys)
				{
					foreach (Operator o in operators)
					{
						//ensure character is the one performing the operation
						if (o.character.Equals(charactername))
						{
							//check if perceived negative effects of action have literal as a component
							if (o.effBMinus.Contains(lit))
							{
								//check if that action appeared first time at t
								if (firstLevel(o, l.A, ((obj) => (obj as List<Operator>))) == t)
								{
									selectedActions++;
									//now add all of its preconditions as subgoals in Gts
									foreach (String prelit in o.preBPlus.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).bPlus);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).bPlus.Contains(prelit))
											(Gt[level] as Character).bPlus.Add(prelit, 1);
									}
									foreach (String prelit in o.preBMinus.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).bMinus);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).bMinus.Contains(prelit))
											(Gt[level] as Character).bMinus.Add(prelit, 1);
									}
									foreach (String prelit in o.preUnsure.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).unsure);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).unsure.Contains(prelit))
											(Gt[level] as Character).unsure.Add(prelit, 1);
									}
								}
							}
						}
					}
				}
				foreach (String lit in (Gt[t] as Character).unsure.Keys)
				{
					foreach (Operator o in operators)
					{
						//ensure character is the one performing the operation
						if (o.character.Equals(charactername))
						{
							//check if perceived unsure effects of action have literal as a component
							if (o.effUnsure.Contains(lit))
							{
								//check if that action appeared first time at t
								if (firstLevel(o, l.A, ((obj) => (obj as List<Operator>))) == t)
								{
									selectedActions++;
									//now add all of its preconditions as subgoals in Gts
									foreach (String prelit in o.preBPlus.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).bPlus);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).bPlus.Contains(prelit))
											(Gt[level] as Character).bPlus.Add(prelit, 1);
									}
									foreach (String prelit in o.preBMinus.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).bMinus);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).bMinus.Contains(prelit))
											(Gt[level] as Character).bMinus.Add(prelit, 1);
									}
									foreach (String prelit in o.preUnsure.Keys)
									{
										int level = firstLevel(prelit, l.F, (obj) => ((Character)obj).unsure);
										if (level == -1 || level >= t)
											level = t - 1;
										if (!(Gt[level] as Character).unsure.Contains(prelit))
											(Gt[level] as Character).unsure.Add(prelit, 1);
									}
								}
							}
						}
					}
				}
			}
			return selectedActions;
		}

        // Return the first instance of a specific prop in a prop layer.
        private static int firstLevel(String l, Hashtable table, del accessor){
            ArrayList keys = new ArrayList(table.Keys);
            keys.Sort();

            int last = (int)keys[keys.Count - 1];
            int first = (int)keys[0];
            for (int i = first; i <= last; ++i){
                if (accessor(table[i]).Contains(l))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Returns the first level where this prop matches the given prefVal.
        /// </summary>
        /// <param name="layer">The layer to start looking back from in the RPG (typically the precondition layer for the action providing a goal).</param>
        /// <param name="lit">The proposition being searched for.</param>
        /// <param name="propLayers">The proposition layers of the RPG.</param>
        /// <param name="isTrue">Whether this is a true or a false proposition.</param>
        /// <param name="prefVal">The current value of the proposition.</param>
        /// <returns></returns>
        private static int firstEqualPrefLevel(int layer, string lit, Hashtable propLayers, bool isTrue, float prefVal)
        {
            //Console.WriteLine("STARTING PREF LEVEL EVAL AT: " + layer);
            for (int i = layer-1; i >= 0; i--)
            {
                if (isTrue)
                {
                    //Console.WriteLine(Convert.ToSingle(((WorldState)propLayers[i]).tWorld[lit]));
                    if (!((WorldState)propLayers[i]).tWorld.Contains(lit) || Convert.ToSingle(((WorldState)propLayers[i]).tWorld[lit]) < prefVal)
                    {
                        //Console.WriteLine("PREF EVAL FINISHED AT: " + (i+1));
                        return i + 1;
                    }
                }
                else
                {
                    //Console.WriteLine(Convert.ToSingle(((WorldState)propLayers[i]).fWorld[lit]));
                    if (!((WorldState)propLayers[i]).fWorld.Contains(lit) || Convert.ToSingle(((WorldState)propLayers[i]).fWorld[lit]) < prefVal)
                    {
                        //Console.WriteLine("PREF EVAL FINISHED AT: " + (i+1));
                        return i + 1;
                    }
                }
            }
            //Console.WriteLine("PREF EVAL FINISHED AT: " + 0);
            return 0;
        }

        // Return the first instance of a specific operation in a prop layer?
        private static int firstLevel(Operator l, Hashtable table, del2 accessor)
        {
            ArrayList keys = new ArrayList(table.Keys);
            keys.Sort();
            int last = (int)keys[keys.Count - 1];
            int first = (int)keys[0];
            for (int i = first; i <= last; ++i)
            {
                if (accessor(table[i]).Contains(l))
                    return i;
            }
            return -1;
        }

        // XXX: only temporary
        private static int firstLevelIgnorePrefs(Operator l, Hashtable table, del3 accessor)
        {
            ArrayList keys = new ArrayList(table.Keys);
            keys.Sort();
            int last = (int)keys[keys.Count - 1];
            int first = (int)keys[0];
            for (int i = first; i <= last; ++i)
            {
                foreach(Tuple<Operator, float> entry in accessor(table[i]))
                {
                    if (entry.Item1 == l)
                        return i;
                }
            }
            return -1;
        }

        public static void printRPG(Layers layers)
        {
            Console.WriteLine("LAYER 0");
            int i = 0;
            while (i <= layers.k)
            {
                Console.WriteLine("\nPROPS");
                foreach (String lit in ((WorldState)layers.F[i]).tWorld.Keys)
                {
                    Console.Write(lit + " ");
                }
                foreach (String lit in ((WorldState)layers.F[i]).fWorld.Keys)
                {
                    Console.Write(lit + " ");
                }
                if (i == layers.k) break;
                i++;
                Console.WriteLine("\n\n\nLAYER " + i);
                Console.WriteLine("\nACTIONS");
                foreach (Operator op in (List<Operator>)layers.A[i])
                {
                    Console.Write(op.name + " ");
                }
            }
        }
    }
}
