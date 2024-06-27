using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDDLNET;

namespace NarrativePlanning
{
    class pddl2json
    {
        public pddl2json(string domain, string problem)
        {
            new DomainProblem(domain, problem);
        }
    }
}
