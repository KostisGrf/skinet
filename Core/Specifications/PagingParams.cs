using System;

namespace Core.Specifications;

public class PagingParams
{
    private const int MaxPagesize=50;
    public int PageIndex{get;set;}=1;

    private int _pageSize=6;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize=(value>MaxPagesize)? MaxPagesize:value;
    }
}
