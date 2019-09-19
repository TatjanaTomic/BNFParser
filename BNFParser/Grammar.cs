using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BNFParser
{
    class Grammar
    {
        public List<string> nonterminals; //Zasad je dovoljno samo da čuvam naazive neterminalnih simbola
        public List<TerminalSymbol> terminals; //zasad imam naziv i regex pattern, trebalo bi dodati još i polje tipa Regex
        public Dictionary<string, string> productions;

        /// <summary>
        /// Funkcija dodaje nazive neterminalnih simbola na osnovu liste tokena koju BNFAnalyzer napravi
        /// </summary>
        private void AddNonterminals()
        {
            foreach(Token t in BNFAnalyzer.BNFTokens)
                nonterminals.Add(t.Name);
        }

        private void AddTerminals()
        {
            //TODO: AddSpecialTerminals
            foreach(Token t in BNFAnalyzer.BNFTokens)
            {
                MatchCollection matches = new Regex("\"[A-Za-z0-9_]+\"").Matches(t.Definition);
                foreach (Match match in matches)
                    terminals.Add(new TerminalSymbol(match.Value, ));
            }
        }

    }
}
