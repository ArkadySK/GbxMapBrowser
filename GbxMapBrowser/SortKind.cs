using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace GbxMapBrowser
{
    public static class SortKind
    {
        public enum Kind
        {
            [Description("Name ⬆️")]
            ByNameAscending,

            [Description("Name ⬇️")]             
            ByNameDescending,

            [Description("Date ⬆️")] 
            ByDateModifiedAscending,

            [Description("Date ⬇️")]
            ByDateModifiedDescending,

            /*
            [Description("Titlepack ⬆️")]          
            ByTitlepackAscending,

            [Description("Titlepack ⬇️")]
            ByTitlepackDescending,*/

            [Description("Size ⬆️")]
            BySizeAscending,

            [Description("Size ⬇️")]
            BySizeDescending,

            [Description("Length ⬆️")]
            ByLendthAscending,

            [Description("Length ⬇️")]
            ByLendthDescending
        }
    }
}
