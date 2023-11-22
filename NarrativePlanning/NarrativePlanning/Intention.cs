using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NarrativePlanning
{
    public class Intention
    {
        public String character;
        /// <summary>
        /// Must store the single literal
        /// which can be either B plus, B minues, or Unsure
        /// </summary>
        public Character goals
        {
            get;
            set;
        }

        /// <summary>
        /// WorldState where this intention was adopted
        /// </summary>
        public WorldState state
        {
            get;
            set;
        }

        public List<Character> motivations
        {
            get;
            set;
        }

        /// <summary>
        /// The plan that the character can come up with 
        /// to achieve the goal.
        /// </summary>
        public Microplan plan
        {
            get;
            set;
        }

        public Intention()
        {
            motivations = new List<Character>();
        }

        public bool hasGoal(Character goal)
        {
            foreach(String lit in this.goals.bPlus.Keys)
            {
                if (this.goals.bPlus.ContainsKey(lit))
                    return true;
            }
            foreach (String lit in this.goals.bMinus.Keys)
            {
                if (this.goals.bMinus.ContainsKey(lit))
                    return true;
            }
            foreach (String lit in this.goals.unsure.Keys)
            {
                if (this.goals.unsure.ContainsKey(lit))
                    return true;
            }
            return false;
        }

        public static bool containsIntention(List<Intention> intentions, Intention intention)
        {
            foreach(Intention i in intentions)
            {
                if (i.character.Equals(intention.character) && i.goals.Equals(intention.goals))
                    return true;
            }
            return false;
        }

        public String getDescription()
        {
            String res = "INTENTION ";
            res = res + this.character;
            if(this.goals.bPlus.Count>0)
            {
                foreach (String lit in this.goals.bPlus.Keys)
                    res += " bplus " + lit;
            }
            if (this.goals.bMinus.Count > 0)
            {
                foreach (String lit in this.goals.bMinus.Keys)
                    res += " bminus " + lit;
            }
            if (this.goals.unsure.Count > 0)
            {
                foreach (String lit in this.goals.unsure.Keys)
                    res += " unsure " + lit;
            }

            return res;
        }

        public Intention clone()
        {
            Intention res = new Intention();
            res.character = this.character;
            res.goals = this.goals.clone();
            res.state = this.state;
            res.plan = this.plan.clone();
            foreach (Character m in this.motivations)
            {
                res.motivations.Add(m.clone());
            }
            

            return res;
        }
        
        public override bool Equals(object obj)
        {
            Intention i = obj as Intention;
            bool x = this.character.Equals(i.character);
            bool a = this.goals.Equals(i.goals);
            bool b = this.state.HasntChangedFrom(i.state);
            bool c = this.motivations.Equals(i.motivations);
            return a && b && c && x;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
