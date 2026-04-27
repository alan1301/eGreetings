namespace EGreetings.Shared.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<FieldError>? Errors { get; set; }
    public PaginationMeta? Meta { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Thành công")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> OkPaged(T data, PaginationMeta meta, string message = "Thành công")
        => new() { Success = true, Message = message, Data = data, Meta = meta };

    public static ApiResponse<T> Fail(string message, List<FieldError>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };

    public static ApiResponse<T> Created(T data, string message = "Tạo thành công")
        => new() { Success = true, Message = message, Data = data };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Thành công")
        => new() { Success = true, Message = message };

    public static new ApiResponse Fail(string message, List<FieldError>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public class FieldError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMeta Meta { get; set; } = new();
}
