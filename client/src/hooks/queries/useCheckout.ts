import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createPaymentIntent } from "../../services/checkoutService";
import type { CreatePaymentIntentRequest } from "../../types/paymentIntent";

export const usePaymentIntent = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (paymentIntent: CreatePaymentIntentRequest) => createPaymentIntent(paymentIntent),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
}