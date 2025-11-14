using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        ALL = 31
    }

    class MainClass
    {
        public static void Main(string[] args)
        {
            UnityConsole.WriteLine("Hello World!");

            // UNCOMMENT THIS IF YOU WANT TO RECREATE OR UPDATE DOMAIN
            //DomainBuilder.TypeTreeBuilder t = new DomainBuilder.TypeTreeBuilder();
            //DomainBuilder.InstanceAdder i = new DomainBuilder.InstanceAdder(t.root);
            //DomainBuilder.OperationBuilder opb = new DomainBuilder.OperationBuilder(t.root);
            //DomainBuilder.GroundGenerator gg = new DomainBuilder.GroundGenerator(t.root, opb.operators);
            //DomainBuilder.OperationBuilder.storeOperators(gg.grounds, opb.operators, "serialized-ops.txt");

            //DomainBuilder.JSONDomainBuilder j = new DomainBuilder.JSONDomainBuilder("../../JSON Files/breakout.json");
            //List<NarrativePlanning.Operator> o = DomainBuilder.OperationBuilder.getStoredOperators("serialized-ops.txt");
            //WorldState initial = DomainBuilder.StateCreator.getState("../../Text Files/beanstalk-initial.txt");
            //WorldState goal = DomainBuilder.StateCreator.getState("../../Text Files/beanstalk-goal.txt");

            //watch.Stop();
            //UnityConsole.WriteLine("Time taken to prepare everything: " + watch.ElapsedMilliseconds + " milliseconds.");
            //watch.Restart();
            //PlanningProblem problem = new PlanningProblem(j.initial, j.goal, j.operators, j.desires);
            //UnityConsole.WriteLine("Plan is : " + problem.HeadSpaceXSolution().toString());
            //watch.Stop();
            //UnityConsole.WriteLine("Complete, planning algorithm time = " + watch.ElapsedMilliseconds + " milliseconds.");




            ////////////////////////////////////////////////////////
            DomainBuilder.JSONDomainBuilder j = new NarrativePlanning.DomainBuilder.JSONDomainBuilder("../../JSON Files/mp_stealth.json");

            // SMALL TEST
            /*WorldState knowledgeSet = new WorldState();
            UnityConsole.Log("Initial knowledge set (should be empty):", LOGMODE.WORLDSTATE);
            knowledgeSet.PrintFullStateWithUnknowns();
            List<Operator> testOps = new List<Operator>();
            Operator newOp = new Operator();
            newOp.preT.Add("at cat field", 0);
            newOp.preT.Add("at ball house", 0);
            testOps.Add(newOp);
            newOp = new Operator();
            newOp.preF.Add("at cat field", 0);
            newOp.preF.Add("at ball house", 0);
            testOps.Add(newOp);
            UnityConsole.Log("\nKnowledge set updated with unknowns:", LOGMODE.WORLDSTATE);
            FastForward.CreateUnknownKnowledge(testOps, knowledgeSet);
            knowledgeSet.PrintFullStateWithUnknowns();
            UnityConsole.Log("\nKnowledge set updated with the ball being at the house:", LOGMODE.WORLDSTATE);
            knowledgeSet.tWorld.Add("at ball house", 0);
            knowledgeSet.PrintFullStateWithUnknowns();
            UnityConsole.Log("\nAfter applying knowledge consistency:", LOGMODE.WORLDSTATE);
            FastForward.ApplyKnowledgeConsistency(knowledgeSet);
            knowledgeSet.PrintFullStateWithUnknowns();
            UnityConsole.Log("\nKnowledge set updated with the cat being at the field:", LOGMODE.WORLDSTATE);
            knowledgeSet.tWorld.Add("at cat field", 0);
            knowledgeSet.PrintFullStateWithUnknowns();
            UnityConsole.Log("\nAfter applying knowledge consistency:", LOGMODE.WORLDSTATE);
            FastForward.ApplyKnowledgeConsistency(knowledgeSet);
            knowledgeSet.PrintFullStateWithUnknowns();
            UnityConsole.Log("Stop", LOGMODE.WORLDSTATE);*/

            // Test unknown generation with a full domain
            /*WorldState knowledgeSet = new WorldState();
            UnityConsole.Log("Initial knowledge set (should be empty):", LOGMODE.WORLDSTATE);
            knowledgeSet.PrintFullStateWithUnknowns();
            UnityConsole.Log("\nKnowledge set updated with unknowns:", LOGMODE.WORLDSTATE);
            FastForward.CreateUnknownKnowledge(j.operators, knowledgeSet);
            knowledgeSet.PrintFullStateWithUnknowns();
            UnityConsole.Log("Stop", LOGMODE.WORLDSTATE);*/

             // UNCOMMENT THIS IF YOU WANT TO RECREATE OR UPDATE DOMAIN USING TXT FILES
             //DomainBuilder.TypeTreeBuilder t = new DomainBuilder.TypeTreeBuilder();
             //DomainBuilder.InstanceAdder i = new DomainBuilder.InstanceAdder(t.root);
             //DomainBuilder.OperationBuilder opb = new DomainBuilder.OperationBuilder(t.root);
             //DomainBuilder.GroundGenerator gg = new DomainBuilder.GroundGenerator(t.root, opb.operators);
             //DomainBuilder.OperationBuilder.storeOperators(gg.grounds, opb.operators, "serialized-ops.txt");

             NarrativePlanning.PlanningProblem problem = new NarrativePlanning.PlanningProblem(j.initial, j.goal, j.operators, j.characterPreferences, j.initialKnowledge, j.observablePrefixes, j.exclusivePrefixes);

            /////// STEP 2: GENERATE PLAN 
            // Use planner to generate the plan and register it to mapper
            //problem.computeRPGToFixed();
            Stopwatch watch = new Stopwatch();
            //UnityConsole.WriteLine("Solving without prefs...");
            //watch.Start();
            //NarrativePlanning.Plan plan = problem.FFNoPreferenceSolution();
            //watch.Stop();
            //UnityConsole.WriteLine("NOPREFS: " + watch.ElapsedMilliseconds + "ms");
            //watch.Reset();
            watch.Start();
            //NarrativePlanning.Plan plan = problem.FFPreferenceSolution(PLANNING_MODE.MULTI);
            NarrativePlanning.Plan plan = problem.FFKnowledgePrefSolution(PLANNING_MODE.KNOWLEDGE);
            watch.Stop();
            UnityConsole.WriteLine("PREFS: " + watch.ElapsedMilliseconds + "ms");
            //NarrativePlanning.Plan plan = problem.FFSolution();
            //UnityConsole.Write(problem.FFSolution().toString());
            if (plan == null)
                NarrativePlanning.UnityConsole.WriteLine("Planning complete. No plan found");
            else
                NarrativePlanning.UnityConsole.WriteLine("Planning complete. Plan is : " + plan.toString());
            while (true) { }
            return;
        }
    }
    
    public class UnityConsole
    {
        public static LOGMODE logmode = LOGMODE.ERROR;// | LOGMODE.MEMOIZE;// | LOGMODE.HEURISTIC; // LOGMODE.PLANNER | LOGMODE.MEMOIZE;
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
