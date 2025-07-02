using Core.Entities.OrderAggregate;

namespace Core.Specifications;

public class OrderdSpecification : BaseSpecification<Order>
{
    public OrderdSpecification(string email) : base(x => x.buyerEmail == email)
    {
        AddInclude(x => x.OrderItems);
        AddInclude(x => x.DeliveryMethod);
        AddOrderByDescending(x => x.OrderDate);
    }

    public OrderdSpecification(string email, int id) : base(x => x.buyerEmail == email && x.Id == id)
    {
        AddInclude("OrderItems");
        AddInclude("DeliveryMethod");
    }

    public OrderdSpecification(string paymentIntentId,bool isPaymentIntent):base(x=>x.PaymentIntentId==paymentIntentId)
    {
        AddInclude("OrderItems");
        AddInclude("DeliveryMethod");
    }
}
