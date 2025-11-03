namespace SYJBD.Models
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = [];
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalItems { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalItems / PageSize);
        public string? Query { get; set; }  // para mantener el q del buscador

        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
