import type { CreatePaymentIntentRequest, PaymentIntentResponse } from "../types/paymentIntent";
import { fetchers } from "./fetchers";

export const createPaymentIntent = (paymentIntent: CreatePaymentIntentRequest): Promise<PaymentIntentResponse> =>
  fetchers.post<PaymentIntentResponse>('/orders/payment-intent', paymentIntent);
