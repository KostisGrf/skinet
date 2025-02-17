using Core.Entities;
using Core.Specifications;

namespace Core.Interfaces;

public class BrandListSpecification:BaseSpecification<Product,string>
{
    public BrandListSpecification()
    {
        AddSelect(x=>x.Brand);
        ApplyDistinct();
    }
}
