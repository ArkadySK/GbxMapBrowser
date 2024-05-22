using System;
using System.Linq;

namespace GbxMapBrowser
{
    public static class EnviManager
    {
        public static readonly string[] EnviLibrary = ["Alpine", "Bay", "Canyon", "Coast", "Island", "Lagoon", "Rally", "Speed", "Stadium", "Stadium2020", "Stadium256", "Storm", "Valley"];
        public static readonly Tuple<string, string>[] TitlepackLibrary =
        //tuple contains: tp id, tp image
        [
            Tuple.Create("TM2U_Island@adamkooo", "TM2Island"),
            Tuple.Create("TM2_Coast@tushy444trackmaniagamer", "TM2Coast"),
            Tuple.Create("TMOneBay@unbitn", "TMOneBay"),
            Tuple.Create("TMOneSpeed@unbitn", "TMOneSpeed"),
            Tuple.Create("TMOneAlpine@unbitn", "TMOneAlpine"),
            Tuple.Create("TM2Rally@plantathon", "TM2Rally"),
            Tuple.Create("Trackmania", "TMNextStadium"),
            Tuple.Create("TMStadium", "TMNextStadium"),
            Tuple.Create("OrbitalDev", "TMNextStadium"),
            Tuple.Create("TMNextPlatform@Arkady_TM", "TMNextPlatform")
        ];

        /// <summary>
        /// Find titlepack first, then environment (as fallback value).
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="titlepackId"></param>
        /// <returns></returns>
        public static Uri GetEnvironmentImagePath(string environment, string titlepackId)
        {
            var foundTitlepackTuple = TitlepackLibrary.FirstOrDefault(tp => tp.Item1 == titlepackId);
            return foundTitlepackTuple != null
                ? new Uri(Environment.CurrentDirectory + "\\Data\\Titlepacks\\" + foundTitlepackTuple.Item2 + ".png")
                : EnviLibrary.Contains(environment)
                ? new Uri(Environment.CurrentDirectory + "\\Data\\Environments\\" + environment + ".png")
                : new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");
        }
    }
}
