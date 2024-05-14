using System;
using System.Collections.Generic;

namespace NarrativePlanning
{
    /// <summary>
    /// The data structure that stores a plan.
    /// </summary>
    [Serializable]
    public class Plan
    {
        public PlanningProblem pp;
        public List<Tuple<String, WorldState>> steps;
        public List<Tuple<String, WorldState, WorldState>> knowledgeSteps;

        public Plan(PlanningProblem pp)
        {
            this.pp = pp;
            steps = new List<Tuple<String, WorldState>>();
            steps.Add(new Tuple<String, WorldState>("null", pp.w0));
            knowledgeSteps = new List<Tuple<String, WorldState, WorldState>>();
        }

        public String toString(){
            String s = "\n";
            if (steps.Count > 0)
                steps.ForEach(step=>s=s+step.Item1+"\n");
            if (knowledgeSteps.Count > 0)
                knowledgeSteps.ForEach(step => s = s + step.Item1 + "\n");
            return s;
        }
        
        public Plan clone(){
            Plan p = new Plan(this.pp.clone());
            p.steps = new List<Tuple<string, WorldState>>();
            foreach(Tuple<String, WorldState> t in this.steps){
                p.steps.Add(t);
            }
            p.knowledgeSteps = new List<Tuple<String, WorldState, WorldState>>();
            foreach (Tuple<String, WorldState, WorldState> t in this.knowledgeSteps)
            {
                p.knowledgeSteps.Add(t);
            }
            return p;
        }
    }

    public class Microplan
    {
        public List<String> steps
        {
            get;
            set;
        }

        public List<bool> executed;
        public List<CausalLink> links;


        public Microplan()
        {
            this.steps = new List<String>();
            //this.steps.Add("null");
            executed = new List<bool>();
            //executed.Add(true);
            links = new List<CausalLink>();
        }

        public Microplan(Plan p)
        {
            this.steps = new List<string>();
            this.steps.Add("null1");
            this.steps.AddRange(getStringSteps(p.steps));
            this.steps.Add("null2");
            executed = new List<bool>();
            executed.Add(true);
            links = new List<CausalLink>();
        }

        //public Microplan(List<String> steps)
        //{
        //    this.steps = steps;
        //    executed = new List<bool>();
        //    executed.Add(true);
        //    links = new List<CausalLink>();
        //}

        public List<String> getStringSteps(List<Tuple<String, WorldState>> steps)
        {
            List<String> res = new List<string>();

            for (int i = 1; i< steps.Count; ++i)
            {
                res.Add(steps[i].Item1);
            }
            return res;
        }

        public void computeCLinks(List<Operator> grounds, Character initial, Character goal)
        {
            this.links = CausalLink.findLinks(this, grounds, initial, goal);
        }

        //built for FailureManager
        public void computeCLinks(List<Operator> grounds, WorldState initial, WorldState goal)
        {
            this.links = CausalLink.findLinks(this, grounds, initial, goal);
        }

        public Microplan clone()
        {
            Microplan p = new Microplan();
            for(int i = 0; i <this.steps.Count; ++i)
            {
                p.steps.Add(this.steps[i]);
            }
            for (int i = 0; i < this.executed.Count; ++i)
            {
                p.executed.Add(this.executed[i]);
            }
            for (int i = 0; i < this.links.Count; ++i)
            {
                p.links.Add(this.links[i].clone());
            }
            return p;
        }
    }
}
