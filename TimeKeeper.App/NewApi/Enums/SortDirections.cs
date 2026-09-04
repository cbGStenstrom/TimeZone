namespace TimeKeeper.App.Api.Enums;

/// <summary>
///  Specifies the direction in which a collection is sorted.
/// </summary>
/// <remarks>
///  Use this enumeration to indicate whether sorting should be performed in ascending or descending
///  order, or if no sort direction is specified. This is commonly used in sorting operations for lists, 
///  tables, or query results.
/// </remarks>
public enum SortDirections
{
    Unset = 0,
    Ascending = 1,
    Descending = 2
}
