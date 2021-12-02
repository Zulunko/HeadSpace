using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NarrativePlanning
{
    [Flags]
    public enum LOGMODE : short
    {
        MEMOIZE = 1, // Output relating to memoization and rewinding
        PLANNER = 2, // Output related to the planning process (not heuristic)
        HEURISTIC = 4, // Output related to the heuristic calculations (RPGs, etc)
        WORLDSTATE = 8, // Output to dump world state on each step
        RELAXEDPLAN = 16, // Print the relaxed plan (pruned action list) each step
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
            DomainBuilder.JSONDomainBuilder j = new NarrativePlanning.DomainBuilder.JSONDomainBuilder("../../JSON Files/scifi_preference.json", new Preferences());


            // UNCOMMENT THIS IF YOU WANT TO RECREATE OR UPDATE DOMAIN USING TXT FILES
            //DomainBuilder.TypeTreeBuilder t = new DomainBuilder.TypeTreeBuilder();
            //DomainBuilder.InstanceAdder i = new DomainBuilder.InstanceAdder(t.root);
            //DomainBuilder.OperationBuilder opb = new DomainBuilder.OperationBuilder(t.root);
            //DomainBuilder.GroundGenerator gg = new DomainBuilder.GroundGenerator(t.root, opb.operators);
            //DomainBuilder.OperationBuilder.storeOperators(gg.grounds, opb.operators, "serialized-ops.txt");

            NarrativePlanning.PlanningProblem problem = new NarrativePlanning.PlanningProblem(j.initial, j.goal, j.operators, new Preferences());

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
            NarrativePlanning.Plan plan = problem.FFPreferenceSolution();
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
        public static LOGMODE logmode = LOGMODE.PLANNER | LOGMODE.MEMOIZE | LOGMODE.RELAXEDPLAN;
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
