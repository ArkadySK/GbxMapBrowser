﻿using System;
using System.Linq;

namespace GbxMapBrowser
{
    public static class MoodManager
    {
        public readonly static Tuple<string, string>[] MoodLibrary =
        //tuple contains: tp id, tp image
        [
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
        ];

        //find titlepack first, then environment
        public static Uri GetMoodImagePath(string moodId)
        {
            var foundMoodTuple = MoodLibrary.FirstOrDefault(tp => moodId.Contains(tp.Item1));
            return foundMoodTuple != null
                ? new Uri(Environment.CurrentDirectory + "\\Data\\MoodIcons\\" + foundMoodTuple.Item2 + ".png")
                : new Uri(Environment.CurrentDirectory + "\\Data\\Moods\\Unknown.png");
        }
    }
}

