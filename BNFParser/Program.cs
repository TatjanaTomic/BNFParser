using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNFParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
                Console.WriteLine("Greška - prilikom pokretanja programa potrebno je navesti nazive ulaznog i izlaznog fajla kao argumente komandne linije!");
            else if (!args[0].EndsWith(".txt"))
                Console.WriteLine("Greška - ulazni fajl treba biti tekstualna datoteka!");
            else if (!args[1].EndsWith(".xml"))
                Console.WriteLine("Greška - izlazni fajl treba biti XML fajl!");
            else
            {
                try
                {
                    Grammar grammar = BNFAnalyzer.CheckBNF();
                    //WriteGrammar(grammar);
                    WriteGrammarToFile(grammar);
                    Parser.ParseText(args[0], grammar, args[1]);
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.ReadKey();
        }

        public static void WriteGrammar(Grammar grammar)
        {
            Console.WriteLine("Root token: {0}", grammar.InitialToken);
            Console.WriteLine("\nNeterminalni tokeni:");
            foreach (var token in grammar.nonterminalTokens)
                Console.WriteLine(token);
            Console.WriteLine("\nTerminalni tokeni: naziv - regex pattern");
            foreach (var token in grammar.terminalTokens)
                Console.WriteLine(token.Name + " - " + token.RegexPattern);
            Console.WriteLine("\nProdukcije: original - slika");
            foreach (var production in grammar.productions)
                Console.WriteLine(production.Item1 + " - " + production.Item2);
        }

        public static void WriteGrammarToFile(Grammar grammar)
        {
            StreamWriter writer = new StreamWriter(new FileStream("gramatika.txt", FileMode.Create));
            writer.WriteLine("Root token: {0}", grammar.InitialToken);
            writer.WriteLine("\nNeterminalni tokeni:");
            foreach (var token in grammar.nonterminalTokens)
                writer.WriteLine(token);
            writer.WriteLine("\nTerminalni tokeni: naziv - regex pattern");
            foreach (var token in grammar.terminalTokens)
                writer.WriteLine(token.Name + " - " + token.RegexPattern);
            writer.WriteLine("\nProdukcije: original - slika");
            foreach (var production in grammar.productions)
                writer.WriteLine(production.Item1 + " - " + production.Item2);
            writer.Close();
        }
    }
}
