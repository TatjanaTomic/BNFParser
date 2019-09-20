using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNFParser
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Grammar grammar = BNFAnalyzer.CheckBNF();
                WriteGrammar(grammar);
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            Console.ReadKey();
        }

        public static void WriteGrammar(Grammar grammar)
        {
            Console.WriteLine("Root token: {0}", grammar.InitialToken);
            Console.WriteLine();
            Console.WriteLine("Neterminalni tokeni:");
            foreach (var token in grammar.nonterminalTokens)
                Console.WriteLine(token);
            Console.WriteLine();
            Console.WriteLine("Terminalni tokeni: naziv - regex pattern");
            foreach (var token in grammar.terminalTokens)
                Console.WriteLine(token.Name + " - " + token.RegexPattern);
            Console.WriteLine();
            Console.WriteLine("Produkcije: original - slika");
            foreach (var production in grammar.productions)
                Console.WriteLine(production.Item1 + " - " + production.Item2);
        }
    }
}
