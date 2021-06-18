using System;
using System.Collections;
using System.Collections.Generic;

namespace NarrativePlanning.DomainBuilder
{
    public class StateCreator
    {
        public StateCreator()
        {
        }

        public static WorldState getState(String filename)
        {
            WorldState state = null;
            Hashtable tWorld = new Hashtable();
            Hashtable fWorld = new Hashtable();
            List<Character> characters = new List<Character>();

            String[] lines = readFile(filename);
            int i = 0;
            if (lines[0].Trim().Equals("t:"))
            {
                i++;
                //each next line is a literal which is true;
                while (!lines[i].Trim().Equals("f:"))
                {
                    //Literal l = new Literal();
                    String l = lines[i].Trim().Trim('(', ')').Trim();

                    //l.relation = allterms[0];
                    //for (int j = 1; j < allterms.Length; ++j){
                    //    l.terms.Add(allterms[j]);
                    //}
                    tWorld.Add(l, 1);
                    ++i;
                }
                //now i is at f:
                i++;
                while (!lines[i].Trim().Contains("{"))
                {
                    //Literal l = new Literal();
                    String l = lines[i].Trim().Trim('(', ')').Trim();

                    //l.relation = allterms[0];
                    //for (int j = 1; j < allterms.Length; ++j)
                    //{
                    //    l.terms.Add(allterms[j]);
                    //}
                    fWorld.Add(l, 1);
                    ++i;
                }
                //now i is at the first character opening brace
                for (int j = i; j < lines.Length; ++j)
                {
                    if (lines[j].Trim().Equals("}"))
                    {
                        Character c = createCharacter(lines, i, j);
                        characters.Add(c);
                        i = j + 1;
                    }
                }
            }
            state = new WorldState(tWorld, fWorld, characters);
            return state;
        }

        private static Character createCharacter(string[] lines, int i, int j)
        {
            Character c = new Character();
            int x = i + 1;
            c.name = lines[x].Trim();
            x++;
            x++;
            while (!lines[x].Trim().Equals("bminus:"))
            {
                //Literal l = new Literal();
                String l = lines[x].Trim().Trim('(', ')').Trim();

                //l.relation = allterms[0];
                //for (int k = 1; k < allterms.Length; ++k)
                //{
                //    l.terms.Add(allterms[k]);
                //}
                c.bPlus.Add(l, 1);
                ++x;
            }
            ++x;
            while (!lines[x].Trim().Equals("unsure:"))
            {
                //Literal l = new Literal();
                String l = lines[x].Trim().Trim('(', ')').Trim();

                //l.relation = allterms[0];
                //for (int k = 1; k < allterms.Length; ++k)
                //{
                //    l.terms.Add(allterms[k]);
                //}
                c.bMinus.Add(l, 1);
                ++x;
            }
            ++x;
            while (!lines[x].Trim().Equals("}"))
            {
                //Literal l = new Literal();
                String l = lines[x].Trim().Trim('(', ')').Trim();

                //l.relation = allterms[0];
                //for (int k = 1; k < allterms.Length; ++k)
                //{
                //    l.terms.Add(allterms[k]);
                //}
                c.unsure.Add(l, 1);
                ++x;
            }
            return c;
        }

        public static String[] readFile(String file)
        {
            return System.IO.File.ReadAllLines(file);
        }


        public static WorldState getState(JSONDomain.Final inputstate)
        {
            WorldState state = null;
            Hashtable tWorld = new Hashtable();
            Hashtable fWorld = new Hashtable();
            List<Character> characters = new List<Character>();

            foreach (String lit in inputstate.T)
            {
                String l = lit.Trim().Trim('(', ')').Trim();
                tWorld.Add(l, 1);
            }
            foreach (String lit in inputstate.F)
            {
                String l = lit.Trim().Trim('(', ')').Trim();
                fWorld.Add(l, 1);
            }
            foreach (JSONDomain.Character c in inputstate.Characters)
            {
                characters.Add(createCharacter(c));
            }

            state = new WorldState(tWorld, fWorld, characters);
            return state;
        }

        public static WorldState getState(JSONDomain.Final inputstate, Preferences prefs)
        {
            WorldState state = null;
            Hashtable tWorld = new Hashtable();
            Hashtable fWorld = new Hashtable();
            List<Character> characters = new List<Character>();

            foreach (String lit in inputstate.T)
            {
                String l = lit.Trim().Trim('(', ')').Trim();
                tWorld.Add(l, prefs.GetPropositionPreference(l, true));
            }
            foreach (String lit in inputstate.F)
            {
                String l = lit.Trim().Trim('(', ')').Trim();
                fWorld.Add(l, prefs.GetPropositionPreference(l, false));
            }
            foreach (JSONDomain.Character c in inputstate.Characters)
            {
                characters.Add(createCharacter(c));
            }

            state = new WorldState(tWorld, fWorld, characters);
            return state;
        }

        private static Character createCharacter(JSONDomain.Character character)
        {
            Character c = new Character();
            c.name = character.Name;
            foreach (String lit in character.Bplus)
            {
                String l = lit.Trim().Trim('(', ')').Trim();
                c.bPlus.Add(l, 1);
            }
            foreach (String lit in character.Bminus)
            {
                String l = lit.Trim().Trim('(', ')').Trim();
                c.bMinus.Add(l, 1);
            }
            foreach (String lit in character.Unsure)
            {
                String l = lit.Trim().Trim('(', ')').Trim();
                c.unsure.Add(l, 1);
            }
            return c;
        }
    }
}
