using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GbxMapBrowser
{
    public static class EnviManager
    {
        public static string[] EnviLibrary = new string[] { "Bay", "Canyon", "Coast", "Desert", "Island", "Lagoon", "Rally", "Snow", "Stadium", "Stadium256", "Valley", "Storm" };
        public static List<Tuple<string, string>> TitlepackLibrary = new List<Tuple<string, string>> //tuple contains: tp id, tp image
        { 
            Tuple.Create("TM2U_Island@adamkooo", "TM2Island"),  
            Tuple.Create("TMOneSpeed@unbitn", "TMOneSpeed"),
            Tuple.Create("TMOneAlpine@unbitn", "TMOneAlpine"),
            Tuple.Create("TrackMania", "TMNextStadium")
        };

        //find titlepack first, then environment
        public static Uri GetEnvironmentImagePath(string envi, string titlepackId)
        {
            var foundTitlepackTuple = TitlepackLibrary.FirstOrDefault(tp => tp.Item1 == titlepackId);
            if (foundTitlepackTuple != null) 
            {
                return new Uri(Environment.CurrentDirectory + "\\Data\\Titlepacks\\" + foundTitlepackTuple.Item2 + ".png");
            }

            if(EnviLibrary.Contains(envi))
                return new Uri(Environment.CurrentDirectory + "\\Data\\Environments\\" + envi + ".png");
            else
                return new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");

        }
    }
}
