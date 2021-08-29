using System;
using System.Collections.Generic;
using System.Text;

namespace GbxMapBrowser
{
    public static class SortKind
    {
        public enum Kind
        {
            ByNameAscending,
            ByNameDescending,
            ByDateModifiedAscending,
            ByDateModifiedDescending,
            /*ByTitlepackAscending,
            ByTitlepackDescending,*/
            BySizeAscending,
            BySizeDescending,
            ByLendthAscending,
            ByLendthDescending
        }
    }
}
