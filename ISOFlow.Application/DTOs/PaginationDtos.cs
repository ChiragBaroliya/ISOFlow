using System.ComponentModel.DataAnnotations;

namespace ISOFlow.Application.DTOs;

public class PagedRequestDto
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than or equal to 1.")]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : (value < 1 ? 10 : value);
    }

    [StringLength(100, ErrorMessage = "SearchTerm cannot exceed 100 characters.")]
    public string? SearchTerm { get; set; }

    [StringLength(50, ErrorMessage = "SortColumn cannot exceed 50 characters.")]
    public string? SortColumn { get; set; }

    [RegularExpression("^(ASC|DESC|asc|desc)$", ErrorMessage = "SortDirection must be ASC or DESC.")]
    public string SortDirection { get; set; } = "ASC";

    public string? StatusFilter { get; set; }
    public string? CategoryFilter { get; set; }
}

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResponse() { }

    public PagedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
