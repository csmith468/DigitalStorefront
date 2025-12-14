
export interface CreatePaymentIntentRequest {
  productId: number;
  quantity: number;
  email?: string;
}

export interface PaymentIntentResponse {
  clientSecret: string;
  orderId: number;
}