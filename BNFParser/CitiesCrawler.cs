using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BNFParser
{
    class CitiesCrawler
    {
        private static readonly string Link = "http://worldpopulationreview.com/continents/cities-in-europe/";

        public static void CreateCitiesRegex()
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Link);    
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            Stream responseStream = webResponse.GetResponseStream();
            responseStream = new GZipStream(responseStream, CompressionMode.Decompress);

            StreamReader reader = new StreamReader(responseStream, Encoding.Default);

            string html = reader.ReadToEnd();

            webResponse.Close();
            responseStream.Close();

            StreamWriter writer = new StreamWriter(new FileStream("html.txt", FileMode.Create));
            writer.Write(html);
            writer.Close();

            string content = new Regex("<tbody>(.*)</tbody>").Match(html).Value;
            StreamWriter writer2 = new StreamWriter(new FileStream("content.txt", FileMode.Create));
            writer2.Write(content);
            writer2.Close();

            MatchCollection cities = new Regex("<td>([A-Za-z '-]+)<\\/td>").Matches(content);
            List<string> citiesS = new List<string>();
            for (int i = 0; i < 200; i++)
                citiesS.Add(cities[i].Groups[1].Value);

            StreamWriter writer3 = new StreamWriter(new FileStream("citiesRegex.txt", FileMode.Create));
            for (int i = 0; i < 199; i++)
                writer3.Write(citiesS[i] + "|");
            writer3.Write(citiesS[199]);
            writer3.Close();
        }
    }
}
