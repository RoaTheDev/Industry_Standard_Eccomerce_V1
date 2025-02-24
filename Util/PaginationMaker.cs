namespace Ecommerce_site.Util;

public class PaginationMaker
{
    public IQueryable<T> ApplyPagination<T>(IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public void ValidatePaginationCheck(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be at least 1.");

        if (pageSize < 1) throw new ArgumentException("Page size must be between 1 and 100.");
    }
}