using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGSFramework.Core.Grammar
{
    public class Relation
    {
        //A dictionary signifying a relation between a token and 0 or more
        //other tokens
        public Dictionary<string, HashSet<string>> Relations => this._relations;
        private readonly Dictionary<string, HashSet<string>> _relations;
        public Relation()
        {
            this._relations = new Dictionary<string, HashSet<string>>();
        }
        //Add a (right) relation to the left token
        public void AddRelation(string left, string right)
        {
            HashSet<string> relations;
            if (!this.Relations.TryGetValue(left, out relations))
            {
                relations = new HashSet<string>();
                this.Relations.Add(left, relations);
            }
            relations.Add(right);
        }

        /// <summary>
        /// Compute all possible relations for each token, while keeping order of
        /// the relations(transitive closure using Floyd-Warshall)
        /// </summary>
        /// <param name="relation"></param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> Closure(Relation relation)
        {
            HashSet<string> keys = new HashSet<string>();
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();

            //Copy the relation and build the set of unique keys
            foreach (string i in relation.Relations.Keys)
            {
                keys.Add(i);

                result.Add(i, new HashSet<string>());
                foreach (string j in relation.Relations[i])
                {
                    keys.Add(j);

                    if (relation.Relations[i].Contains(j))
                    {
                        result[i].Add(j);
                    }
                }
            }

            //Initialise all relation hashsets
            foreach (string i in keys)
            {
                if (!result.ContainsKey(i))
                {
                    result.Add(i, new HashSet<string>());
                }
            }

            //Perform the transitive closure and add new relations
            foreach (string k in keys)
            {
                foreach (string i in keys)
                {
                    foreach (string j in keys)
                    {
                        if (result[i].Contains(j)
                            || (result[j].Contains(k)
                            && result[k].Contains(j)))
                        {
                            result[i].Add(j);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Propagate the immediate relation using the (closure of the)
        /// propagation relation
        /// </summary>
        /// <param name="immediate">all terminals symbols in the first or follow list</param>
        /// <param name="propagation">all nonterminals symbols in the first or follow list</param>
        /// <returns></returns>
        public static Dictionary<string, HashSet<string>> Propagate(Relation immediate, Relation propagation)
        {
            Dictionary<string, HashSet<string>> result =
                new Dictionary<string, HashSet<string>>();
            //Compute all possible relations between the propagation set
            Dictionary<string, HashSet<string>> closed =
                Relation.Closure(propagation);

            //Add the immediate tokens and their relation to the result list
            foreach (string k in immediate.Relations.Keys)
            {
                result.Add(k, new HashSet<string>());

                foreach (string l in immediate.Relations[k])
                {
                    if (immediate.Relations[k].Contains(l))
                    {
                        result[k].Add(l);
                    }
                }
            }
            //Merge the immediate set and the propagation set
            foreach (string s in closed.Keys)
            {
                foreach (string t in closed[s])
                {
                    //Only use tokens that exist in the immediate set
                    if (!immediate.Relations.ContainsKey(t))
                    {
                        continue;
                    }

                    if (!result.ContainsKey(s))
                    {
                        result.Add(s, new HashSet<string>());
                    }

                    //If <t> has a relation to <s> and <u> has a relation to <t>, then
                    //<t> has a relation to <s> 
                    //<t> -> <s>
                    //<u> -> <t>
                    //<u> -> <s> 
                    foreach (string u in immediate.Relations[t])
                    {
                        result[s].Add(u);
                    }
                }
            }
            return result;
        }
        public static IEnumerable<string> Cycle(Relation relation)
        {
            IEnumerable<string> dfs(string k, List<string> v)
            {
                //If no relations are defined for k, return empty set
                if (!relation.Relations.ContainsKey(k))
                {
                    return Enumerable.Empty<string>();
                }

                //Find the relations of that token k
                foreach (string l in relation.Relations[k])
                {
                    //If the relation already exists, a cycle has been detected
                    if (v.Contains(l))
                    {
                        if (l == k)
                        {
                            //Add the start token (k)
                            return new List<string>(v)
                            {
                                k
                            };
                        }
                        else
                        {
                            //Add the start token k and the current token
                            return new List<string>(v)
                            {
                                k,
                                l
                            };
                        }
                    }

                    //Recurse further into element l
                    IEnumerable<string> w = dfs
                    (
                        l,
                        new List<string>(v)
                        {
                            k
                        }
                    );

                    if (w.Any())
                    {
                        return w;
                    }
                }

                return Enumerable.Empty<string>();
            }

            IEnumerable<string> result;

            //Foreach token in a relation
            foreach (string token in relation.Relations.Keys)
            {
                result = dfs(token, new List<string>());
                if (result.Any())
                {
                    return result;
                }
            }

            return Enumerable.Empty<string>();
        }
    }
}
