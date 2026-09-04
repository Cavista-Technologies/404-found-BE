using Microsoft.EntityFrameworkCore;

namespace Cavista.CTRecruita.Utilities.Extension
{
    public static class PaginationExtension
    {
        public static async Task<PagedResult<T>> PaginateAsync<T>(this IQueryable<T>? queryable, int page, int pageLength = 10, PaginationMetadata metadata = null) where T : class
        {

            page = page < 1 ? 1 : page;
            pageLength = pageLength < 1 ? 10 : pageLength;
            var items = new List<T>();
            if (queryable == null)
            {
                return new PagedResult<T>();
            }
            var count = await queryable.CountAsync();
            if (count > 0)
            {
                items = await queryable.Page(page, pageLength, metadata).ToListAsync();
            }
            var pageCount = (int)Math.Ceiling(count / (double)pageLength);

            return new PagedResult<T>
            {
                Items = items,
                CurrentPage = page,
                ItemCount = count,
                PageCount = pageCount,
                PageLength = pageLength
            };
        }

        public static IQueryable<T> Page<T>(this IQueryable<T> queryable, int pageIndex, int pageLength, PaginationMetadata metadata = null)
        {
            var zeroBase = metadata?.ZeroBase ?? false;
            if (!zeroBase)
            {
                pageIndex -= 1;
            }

            var itemsToSkip = Math.Max(pageIndex * pageLength, 0);
            var skipOffset = metadata?.SkipOffset ?? 0;
            var takeOffset = metadata?.TakeOffset ?? 0;

            return queryable.Skip(itemsToSkip - skipOffset).Take(pageLength - takeOffset);
        }
    }
}
