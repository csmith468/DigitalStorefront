import { ErrorMessages, SuccessMessages } from "../../constants/messages";
import { createPaymentIntent } from "../../services/checkoutService";
import type { CreatePaymentIntentRequest } from "../../types/paymentIntent";
import { useMutationWithToast } from "../utilities/useMutationWithToast";

export const usePaymentIntent = () => {
  return useMutationWithToast({
    mutationFn: (paymentIntent: CreatePaymentIntentRequest) => createPaymentIntent(paymentIntent),
    onSuccess: (_data, _, queryClient) => {
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
    successMessage: SuccessMessages.Checkout.created,
    errorMessage: ErrorMessages.Checkout.createFailed,
  })
}