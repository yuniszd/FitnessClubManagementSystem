namespace FCMS.Application.Responses;

public class PagedResponse<T> : BaseResponse<IEnumerable<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}