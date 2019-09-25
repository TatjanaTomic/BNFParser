using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Workflow.Activities.Rules;
using System.Xml;

namespace BNFParser
{
    class Parser
    {
        public static List<(string name, string value)> terminalTokens = new List<(string, string)>(); //lista terminalnih tokena pronađenih u tekstu 
        public static List<(string token, string production)> usedProductions = new List<(string, string)>();  //Svaki korak parsiranja, tj zamjene produkcija njihovim "originalom"(neterminalnim tokenom od kog potiču) dodajem u listu da bih kasnije mogla izgenerisati stablo

        public static void ParseText(string txtFile, Grammar grammar, string xmlFile)
        {
            if (!File.Exists(txtFile))
                throw new FileNotFoundException("Greška - fajl {0} nije pronadjen.", txtFile);
           
            StreamReader reader = new StreamReader(new FileStream(txtFile, FileMode.Open));
            string text = reader.ReadToEnd();
            reader.Close();

            if (text == null || text == "")
                throw new RuleException("Greška - fajl " + txtFile + " je prazan.");

            string input = RewriteInput(text, grammar);
            input = input.Replace(" ", "");

            StreamWriter wr = new StreamWriter(new FileStream("medjurezultat.txt", FileMode.Append));
            wr.WriteLine("Zamjena produkcija: ");
            bool flag; //fleg koji govori da li je došlo do zamjene, ako nema više nikakvih zamjena izlazi se iz petlje
            do
            {
                flag = false;
                foreach (var (token, definition) in grammar.productions)
                {
                    if (input.Contains(definition))
                    {
                        input = input.Replace(definition, token);
                        usedProductions.Add((token, definition));
                        flag = true;
                    }
                }
                wr.WriteLine(input);
            } while(flag);
            wr.WriteLine("----------------------------------------------------");
            wr.Close();

            if(input!=grammar.InitialToken)
            {
                Console.WriteLine("Ulazni tekstualni fajl ne odgovara unesenoj BNF notaciji.");
            }
            else
            {
                Console.WriteLine("Tekst uspješno parsiran!");
                XmlTextWriter writer = new XmlTextWriter(xmlFile, Encoding.Default);
                WriteToFile(grammar.InitialToken, writer);
                writer.Close();
            }
        }

        private static string RewriteInput(string line, Grammar grammar)
        {
            StreamWriter wr = new StreamWriter(new FileStream("medjurezultat.txt", FileMode.Append));
            string result = " " + line + " "; //svaki mečirani izraz unutar linije ulaznog teksta zamijeniću nazivom terminalnog tokena kome odgovara mečirani izraz
                                              //tako da dobijem ulazni tekstualni fajl zapisan preko terminalnih simbola
            int i = 1;
            wr.WriteLine("Pročitana linija: " + line);
            wr.WriteLine(" ");
            wr.WriteLine("Zamjena terminalnih tokena:");
            foreach (TerminalToken token in grammar.terminalTokens)
            {
                Match match = token.Definition.Match(result);

                if (match.Success)
                {
                    result = result.Replace(match.Value, " " + token.Name + " ");
                    wr.WriteLine(i++ + ". zamjena: " + result);
                    terminalTokens.Add((token.Name, match.Value));
                }
            }
            wr.WriteLine(" ");
            wr.Close();
            return result;
        }

        public static void WriteToFile(string rootToken, XmlTextWriter writer)
        {
            writer.Formatting = System.Xml.Formatting.Indented;
            string production = FindProduction(rootToken);
            string[] tokens = SplitProduction(production);

            writer.WriteStartElement(rootToken.Substring(1,rootToken.Length - 2));
            foreach (var token in tokens)
            {
                if (token.StartsWith("\"") && token.EndsWith("\"")) //znači da je terminalni i da trebam upisati onu mečiranu vrijednost iz ulaznog teksta
                    writer.WriteString(FindMatchFromText(token));
                else if (token.StartsWith("<") && token.EndsWith(">"))
                    WriteToFile(token, writer);
            }
            writer.WriteEndElement();
        }

        private static string FindMatchFromText(string token)
        {
            foreach (var (name, value) in terminalTokens)
                if (name == token)
                    return value;
            return "DesilaSeGreskaUPronalaskuMeceva";
        }

        private static string[] SplitProduction(string production)
        {
            MatchCollection matches = new Regex("<[A-Za-z0-9_]+>|\"[^\"]+\"").Matches(production);
            List<string> tokens = new List<string>();
            foreach (Match match in matches)
                tokens.Add(match.Value);
            return tokens.ToArray();
        }

        private static string FindProduction(string nonterminal)
        {
            foreach (var production in usedProductions)
                if (production.token == nonterminal)
                    return production.production;
            return "DesilaSeGreskaUPronalaskuProdukcija";
        }
    }
}
