namespace cSharpApiFunko.Models.Dto;

public record PageResponse<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
    public int TotalPages => Size > 0 ? (int)Math.Ceiling((double)TotalCount / Size) : 0;
    public bool HasNexPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}