
export interface CreatePaymentIntentRequest {
  productId: number;
  quantity: number;
}

export interface PaymentIntentResponse {
  clientSecret: string;
  orderId: number;
}