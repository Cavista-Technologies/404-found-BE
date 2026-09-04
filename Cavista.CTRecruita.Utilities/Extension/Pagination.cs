namespace Cavista.CTRecruita.Utilities.Extension
{
    public class PagedResult<T>
    {
        public int ItemCount { get; set; }

        public int PageLength { get; set; }

        public int CurrentPage { get; set; }

        public int PageCount { get; set; }

        public IList<T> Items { get; set; }
    }

    public class PaginationMetadata
    {
        public bool ZeroBase { get; set; }
        public int SkipOffset { get; set; }
        public int TakeOffset { get; set; }
    }
}
