using System;
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
        public static readonly string SPACE = " *";
        public static readonly string NONTERMINAL_SYMBOL = "<[A-Za-z0-9_]+>";
        public static readonly string TERMINAL_SYMBOL = "\"[A-Za-z0-9_]+\"";
        public static readonly string LEFT_SIDE = SPACE + NONTERMINAL_SYMBOL + SPACE;  //" *<[A-Za-z0-9_]+> *"
        public static readonly string ASSIGNMENT = "::=";
        public static readonly string GROUP1 = "(" + NONTERMINAL_SYMBOL + SPACE + ")";  //"(<[A-Za-z0-9_]+> *)"
        public static readonly string GROUP2 = "(" + TERMINAL_SYMBOL + SPACE + ")";  //"(\"[A-Za-z0-9_]+\" *)"
        public static readonly string RIGHT_SIDE1 = SPACE + "(" + GROUP1 + "|" + GROUP2 + ")" + "+" + "(" + GROUP1 + "*" + GROUP2 + "*" + ")" + "*";
        // RIGHT_SIDE1 mečiraće svaku desnu stranu koja je niz terminalnih i neterminalnih simbola u proizvoljnom redoslijedu
        public static readonly string RIGHT_SIDE = SPACE + "(" + RIGHT_SIDE1 + ")" + "+" + "(" + "\\|" + RIGHT_SIDE1 + ")" + "*";
        // RIGHT_SIDE će mečirati svaku desnu stranu koja je oblika: niz simbola ili drugi niz simbola ili ... - u suštini proizvoljan broj različitih nizova (mečiraće i svaku desnu stranu koja je oblika RIGHT_SIDE1)

        public static readonly string LINE1 = SPACE + NONTERMINAL_SYMBOL + SPACE + ASSIGNMENT + SPACE + RIGHT_SIDE;
        public static readonly string LINE2 = SPACE + NONTERMINAL_SYMBOL + SPACE + ASSIGNMENT + SPACE + "broj_telefona" + SPACE;
        public static readonly string LINE3 = SPACE + NONTERMINAL_SYMBOL + SPACE + ASSIGNMENT + SPACE + "mejl_adresa" + SPACE;
        public static readonly string LINE4 = SPACE + NONTERMINAL_SYMBOL + SPACE + ASSIGNMENT + SPACE + "web_link" + SPACE;
        public static readonly string LINE5 = SPACE + NONTERMINAL_SYMBOL + SPACE + ASSIGNMENT + SPACE + "brojevna_konstanta" + SPACE;
        public static readonly string LINE6 = SPACE + NONTERMINAL_SYMBOL + SPACE + ASSIGNMENT + SPACE + "veliki grad" + SPACE;
        public static readonly string LINE7 = SPACE + NONTERMINAL_SYMBOL + SPACE + ASSIGNMENT + SPACE + "regex\\(.*?\\)" + SPACE;

        public static List<Token> BNFTokens = new List<Token>();
       
        /// <summary>
        /// Funkcija provjerava BNF zapisan u config.bnf fajlu
        /// Čita se linija po linija i provjerava da li odgovara jednom od validnih oblika linije bnf-a
        /// Tokeni koji se nalaze sa desne strane znaka ::= i koji se neterminalni dodaju se u listu expectedTokens kako bi se provjerilo da li su svi definisani u config.bnf fajlu
        /// </summary>
        /// <returns></returns>
        public bool CheckBNF()
        {
            if (!File.Exists("config.bnf"))
                throw new FileNotFoundException("Greška - ne postoji config.bnf fajl!");
            else
            {
                StreamReader reader = new StreamReader(new FileStream("config.bnf", FileMode.Open));
                string line = reader.ReadLine();
                int count = 1;
                List<string> expectedTokens = new List<string>();

                do
                {
                    if(line == "")
                    {
                        line = reader.ReadLine();
                        count++;
                        continue;
                    }
                    if (CheckLine(line))
                    {
                        line = line.Trim();
                        string[] tokens = ReadTokensFromLine(line);
                        string rootToken = tokens[0];

                        for (int i = 1; i < tokens.Length; i++)
                            expectedTokens.Add(tokens[i]);

                        for (int i = 0; i < expectedTokens.Count; i++)
                        {
                            if (expectedTokens[i] == rootToken)
                                expectedTokens.RemoveAt(i);
                        }

                        BNFTokens.Add(new Token(rootToken, line.Substring(line.IndexOf('=') + 1)));
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
                    string tokeni = "";
                    foreach (string token in expectedTokens)
                        tokeni += token + " ";
                    throw new RuleException("Greska - BNF notacija nije zadana pravilno. Sljedeci tokeni nisu definisani: " + tokeni);
                }
            }
            return true;
        }

        /// <summary>
        /// Funkcija na osnovu prethodno navedenih regexa provjerava da li je procitana linija bnf fajla u pravilnom obliku
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private bool CheckLine(string line)
        {
            if (line.Equals(new Regex(LINE1).Match(line).Value) ||
                line.Equals(new Regex(LINE2).Match(line).Value) ||
                line.Equals(new Regex(LINE3).Match(line).Value) ||
                line.Equals(new Regex(LINE4).Match(line).Value) ||
                line.Equals(new Regex(LINE5).Match(line).Value) ||
                line.Equals(new Regex(LINE6).Match(line).Value) ||
                line.Equals(new Regex(LINE7).Match(line).Value))

                return true;
            else
                return false;
        }
       
        /// <summary>
        /// Funkcija vraća sve neterminalne simbole koji se nalaze u jednoj liniji config.bnf fajla
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string[] ReadTokensFromLine(string line)
        {
            MatchCollection matches = new Regex("<[A-Za-z0-9_]+>").Matches(line);
            List<string> tokens = new List<string>();
            foreach (Match match in matches)
                tokens.Add(match.Value);
            return tokens.ToArray();
        }

        public static void CitiesCrawler()
        {
            string html = string.Empty;
            string url = "http://worldpopulationreview.com/continents/cities-in-europe/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }

            Console.WriteLine(html);
            /*
            StringBuilder webPage = new StringBuilder();
            const string citiesURL = "http://worldpopulationreview.com/continents/cities-in-europe/";
            using (WebClient client = new WebClient())
            {
                webPage.Append(client.DownloadString(citiesURL));
            }

            string webPageS = webPage.ToString();
            Console.WriteLine(webPageS);
            Console.ReadKey();
            
            List<string> cities = new List<string>();
            Regex citiesRegex = new Regex(@"<td>([A-Za-z' ]+)</td>");
            MatchCollection matches = citiesRegex.Matches(webPageS);
            foreach(Match match in matches)
                cities.Add(match.Groups[1].ToString());

            int i = 1;
            foreach (string city in cities)
                Console.WriteLine((i++) + " " + city);
            */
            Console.ReadKey();
        }
    }
}
