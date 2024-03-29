﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Workflow.Activities.Rules;

namespace BNFParser
{
    class BNFAnalyzer
    {
        private static readonly string SPACE = " *";
        private static readonly string NONTERMINAL_SYMBOL = "<[A-Za-z0-9_]+>";
        private static readonly string TERMINAL_SYMBOL = "\"[A-Za-z0-9_]+\"";
        private static readonly string LEFT_SIDE = SPACE + NONTERMINAL_SYMBOL + SPACE;  //" *<[A-Za-z0-9_]+> *"
        private static readonly string ASSIGNMENT = "::=";
        private static readonly string GROUP1 = "(" + NONTERMINAL_SYMBOL + SPACE + ")";  //"(<[A-Za-z0-9_]+> *)"
        private static readonly string GROUP2 = "(" + TERMINAL_SYMBOL + SPACE + ")";  //"(\"[A-Za-z0-9_]+\" *)"
        private static readonly string RIGHT_SIDE1 = SPACE + "(" + GROUP1 + "|" + GROUP2 + ")" + "+" + "(" + GROUP1 + "*" + GROUP2 + "*" + ")" + "*";
        // RIGHT_SIDE1 mečiraće svaku desnu stranu koja je niz terminalnih i neterminalnih simbola u proizvoljnom redoslijedu
        private static readonly string RIGHT_SIDE = SPACE + "(" + RIGHT_SIDE1 + ")" + "+" + "(" + "\\|" + RIGHT_SIDE1 + ")" + "*";
        // RIGHT_SIDE će mečirati svaku desnu stranu koja je oblika: niz simbola ili drugi niz simbola ili ... - u suštini proizvoljan broj različitih nizova (mečiraće i svaku desnu stranu koja je oblika RIGHT_SIDE1)

        private static readonly string LINE1 = LEFT_SIDE + ASSIGNMENT + SPACE + RIGHT_SIDE;
        private static readonly string LINE2 = LEFT_SIDE + ASSIGNMENT + SPACE + "broj_telefona" + SPACE;
        private static readonly string LINE3 = LEFT_SIDE + ASSIGNMENT + SPACE + "mejl_adresa" + SPACE;
        private static readonly string LINE4 = LEFT_SIDE + ASSIGNMENT + SPACE + "web_link" + SPACE;
        private static readonly string LINE5 = LEFT_SIDE + ASSIGNMENT + SPACE + "brojevna_konstanta" + SPACE;
        private static readonly string LINE6 = LEFT_SIDE + ASSIGNMENT + SPACE + "veliki_grad" + SPACE;
        private static readonly string LINE7 = LEFT_SIDE + ASSIGNMENT + SPACE + "regex\\(.*?\\)" + SPACE;

        /// <summary>
        /// Funkcija provjerava BNF zapisan u config.bnf fajlu
        /// Čita se linija po linija i provjerava da li odgovara jednom od validnih oblika linije bnf-a
        /// Tokeni koji se nalaze sa desne strane znaka ::= i koji se neterminalni dodaju se u listu expectedTokens kako bi se provjerilo da li su svi definisani u config.bnf fajlu
        /// [1] Kod dodavanja produkcija u gramatiku - desnu stranu razdvojim na osnovu "|" ako postoji, svaki dobijeni izraz predstavlja po jednu definiciju odgovarajućeg neterminalnog simbola, tj. po jednu produkciju koju trebam dodati u gramatiku
        /// Moli Boga da neko ne unese <izraz>=regex(nesto_sto_ima_razmak) jer tad baš i neće raditi ovaj programčić 
        /// </summary>
        /// <returns></returns>
        public static Grammar CheckBNF()
        {
            Grammar grammar = new Grammar();

            if (!File.Exists("config.bnf"))
                throw new FileNotFoundException("Greška - ne postoji config.bnf fajl.");

            string inputPath = Directory.GetCurrentDirectory();
            string path =  inputPath + "\\config.bnf";
            StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open));
            string line = reader.ReadLine();
            int count = 1;
            if (line == null)
                    throw new RuleException("Greška - fajl config.bnf je prazan.");

            List<string> expectedTokens = new List<string>();

            do
            {
                if (line == "")
                {
                    line = reader.ReadLine();
                    count++;
                    continue;
                }
                int flag = CheckLine(line);
                if (flag == 1)
                {
                    line = line.Replace(" ", "");
                    string[] tokens1 = ReadNonterminalTokensFromLine(line);
                    string[] tokens2 = ReadTerminalTokensFromLine(line);
                    string rightSide = GetRightSide(line);
                    string rootToken = tokens1[0];

                    if (grammar.InitialToken == "")
                        grammar.InitialToken = rootToken;
                    
                    if (tokens2.Count() != 0 && IsRecursion(line, rootToken, tokens2[0]))
                        grammar.AddRecursion(rootToken, tokens2[0]);
                    else
                    {
                        grammar.AddNonterminalToken(rootToken); // dodajem samo onaj neterminalni token koji je definisan, ako je BNF pravilno napisan svi će biti dodani
                        foreach (string token in tokens2)
                            grammar.AddTerminalToken(token); //dodajem sve neterminalne tokene koji se nađu u jednoj liniji
                        string[] definitions = SplitRightSide(rightSide); // [1]
                        foreach (var definition in definitions)
                            grammar.AddProduction(rootToken, definition);
                    }
                    for (int i = 1; i < tokens1.Length; i++)
                        expectedTokens.Add(tokens1[i]);

                    expectedTokens = RemoveExpectedToken(rootToken, expectedTokens);
                }
                else if (flag == 2)
                {
                    line = line.Replace(" ", "");
                    string[] tokens1 = ReadNonterminalTokensFromLine(line);
                    string rootToken = tokens1[0];
                    string rightSide = GetRightSide(line);

                    if (grammar.InitialToken == "")
                        grammar.InitialToken = rootToken;
                    grammar.AddNonterminalToken(rootToken);
                    grammar.AddSpecialTerminalToken(rightSide);
                    grammar.AddProduction(rootToken, "\"" + rightSide + "\"");

                    expectedTokens = RemoveExpectedToken(rootToken, expectedTokens);
                }
                else if (flag == 3)
                {
                    string rootToken = ReadNonterminalTokensFromLine(line)[0];
                    string rightSide = Regex.Match(line, "regex\\(.*?\\)").Value;

                    if (grammar.InitialToken == "")
                        grammar.InitialToken = rootToken;
                    grammar.AddNonterminalToken(rootToken);
                    grammar.AddSpecialTerminalToken(rightSide);
                    grammar.AddProduction(rootToken, "\"" + rightSide.Replace(" ", "_") + "\"");

                    expectedTokens = RemoveExpectedToken(rootToken, expectedTokens);
                }
                else
                {
                    reader.Close();
                    throw new RuleException("Greska - config.bnf file ne odgovara BNF notaciji. Ispravite liniju " + count.ToString());
                }

                line = reader.ReadLine();
                count++;
            }
            while (line != null);
            reader.Close();

            if (expectedTokens.Count != 0)
            {
                string tokens = "";
                foreach (string token in expectedTokens)
                   tokens += token + " ";
                throw new RuleException("Greska - BNF notacija nije zadana pravilno. Sljedeci tokeni nisu definisani: " + tokens);
            }
 
            return grammar;
        }

        private static bool IsRecursion(string line, string rootToken, string word)
        {
            string pattern = rootToken + "::=" + word + "\\|" + "(" + word + rootToken + "|" + rootToken + word + ")";
            Regex recursion = new Regex(pattern);
            if (recursion.Match(line).Success && recursion.Match(line).Value == line)
                return true;
            return false;
        }

        private static string[] SplitRightSide(string rightSide)
        {
            string[] result;
            if (rightSide.Contains("|"))
                result = rightSide.Split('|');
            else
            {
                result = new string[1];
                result[0] = rightSide;
            }
            return result;
        }

        /// <summary>
        /// Funkcija na osnovu prethodno navedenih regexa provjerava da li je procitana linija bnf fajla u pravilnom obliku
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static int CheckLine(string line)
        {
            if (line.Equals(new Regex(LINE1).Match(line).Value))
                return 1;
            else if (line.Equals(new Regex(LINE2).Match(line).Value) ||
                     line.Equals(new Regex(LINE3).Match(line).Value) ||
                     line.Equals(new Regex(LINE4).Match(line).Value) ||
                     line.Equals(new Regex(LINE5).Match(line).Value) ||
                     line.Equals(new Regex(LINE6).Match(line).Value))
                return 2;
            else if (line.Equals(new Regex(LINE7).Match(line).Value))
                return 3;
            else
                return -1;
        }

        private static string GetRightSide(string line)
        {
            return line.Substring(line.IndexOf('=') + 1);
        }
       
        /// <summary>
        /// Funkcija vraća sve neterminalne simbole koji se nalaze u jednoj liniji config.bnf fajla
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string[] ReadNonterminalTokensFromLine(string line)
        {
            MatchCollection matches = new Regex("<[A-Za-z0-9_]+>").Matches(line);
            List<string> tokens = new List<string>();
            foreach (Match match in matches)
                tokens.Add(match.Value);
            return tokens.ToArray();
        }

        private static string[] ReadTerminalTokensFromLine(string line)
        {
            MatchCollection matches = new Regex("\"[A-Za-z0-9 _]+\"").Matches(line);
            List<string> tokens = new List<string>();
            foreach (Match match in matches)
                tokens.Add(match.Value);
            return tokens.ToArray();
        }

        private static List<string> RemoveExpectedToken(string rootToken, List<string> expectedTokens)
        {
            List<string> tmpExpectedTokens = new List<string>();
            foreach (string token in expectedTokens)
            {
                if (token != rootToken)
                    tmpExpectedTokens.Add(token);
            }
            return tmpExpectedTokens;
        }
    }
}