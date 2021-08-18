using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GbxMapBrowser
{
    public static class MoodManager
    {
        public static List<Tuple<string, string>> MoodLibrary = new List<Tuple<string, string>> //tuple contains: tp id, tp image
    {
        Tuple.Create("Sunrise", "Sunrise"),
        Tuple.Create("Day", "Day"),
        Tuple.Create("Sunset", "Sunset"),
        Tuple.Create("Night", "Night")/*,
        Tuple.Create("64x64Sunrise", "Sunrise"),
        Tuple.Create("64x64Day", "Day"),
        Tuple.Create("64x64Sunset", "Sunset"),
        Tuple.Create("64x64Night", "Night"),
        Tuple.Create("48x48Sunrise", "Sunrise"),
        Tuple.Create("48x48Day", "Day"),
        Tuple.Create("48x48Sunset", "Sunset"),
        Tuple.Create("48x48Night", "Night")*/
    };

        //find titlepack first, then environment
        public static Uri GetMoodImagePath(string moodId)
        {
            var foundMoodTuple = MoodLibrary.FirstOrDefault(tp => moodId.Contains(tp.Item1));
            if (foundMoodTuple != null)
            {
                return new Uri(Environment.CurrentDirectory + "\\Data\\MoodIcons\\" + foundMoodTuple.Item2 + ".png");
            }
            return new Uri(Environment.CurrentDirectory + "\\Data\\UIIcons\\Error.png");

        }
    }
}

