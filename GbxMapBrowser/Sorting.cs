using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GbxMapBrowser
{
    public static class Sorting
    {
        public enum Kind
        {
            ByNameAscending,
            ByNameDescending,

            ByDateModifiedAscending,
            ByDateModifiedDescending,

            ByTitlepackAscending,
            ByTitlepackDescending,

            BySizeAscending,
            BySizeDescending,

            ByLendthAscending,
            ByLendthDescending
        }

        public static string[] Kinds = new string[] { "Name ⬆️", "Name ⬇️", "Date ⬆️", "Date ⬇️", "Titlepack ⬆️", "Titlepack ⬇️", "Size ⬆️", "Size ⬇️", "Length ⬆️", "Length ⬇️" };
}
}
