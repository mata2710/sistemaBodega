using System;
using System.Collections.Generic;

namespace SistemaBodega.Models
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 25;
        public int TotalItems { get; init; } = 0;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
