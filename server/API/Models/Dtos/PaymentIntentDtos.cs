namespace API.Models.Dtos;

public class CreatePaymentIntentRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class PaymentIntentResponse
{
    public string ClientSecret { get; set; } = "";
    public int OrderId { get; set; }
}