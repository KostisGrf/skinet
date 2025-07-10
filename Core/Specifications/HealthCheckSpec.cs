using Core.Entities;

namespace Core.Specifications;

public class HealthCheckSpec :BaseSpecification<Product>
{
    public HealthCheckSpec() : base()
    {
        ApplyPaging(0, 1);
    }
}
