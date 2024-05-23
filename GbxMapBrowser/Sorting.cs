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

            ByLengthAscending,
            ByLengthDescending
        }

        public static string[] Kinds { get; } = ["Name ⬆️", "Name ⬇️", "Date ⬆️", "Date ⬇️", "Titlepack ⬆️", "Titlepack ⬇️", "Size ⬆️", "Size ⬇️", "Length ⬆️", "Length ⬇️"];
        public static string[] KindsShort { get; } = ["NA", "ND", "DMA", "DMD", "TPA", "TPD", "SA", "SD", "LA", "LD"];

    }
}
