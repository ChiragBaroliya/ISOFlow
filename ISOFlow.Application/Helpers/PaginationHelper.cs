using ISOFlow.Application.DTOs;

namespace ISOFlow.Application.Helpers;

public static class PaginationHelper
{
    public static PagedResponse<T> CreatePagedResponse<T>(
        IEnumerable<T> source,
        int pageNumber,
        int pageSize,
        Func<T, bool>? filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        var query = source.AsQueryable();
        if (filter != null)
        {
            query = query.Where(filter).AsQueryable();
        }

        var totalCount = query.Count();

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        var pagedItems = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResponse<T>(pagedItems, totalCount, pageNumber, pageSize);
    }
}
