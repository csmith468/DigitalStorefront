import { useMutation, useQueryClient, type QueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import { logger } from "../../utils/logger";

interface MutationWithToastOptions<TData, TVariables> {
  mutationFn: (variables: TVariables) => Promise<TData>;
  successMessage: string;
  errorMessage?: string;
  onSuccess?: (data: TData, variables: TVariables, queryClient: QueryClient) => void;
}

export function useMutationWithToast<TData = unknown, TVariables = unknown>({
  mutationFn,
  successMessage,
  errorMessage = 'An error occurred. Please try again.',
  onSuccess,
}: MutationWithToastOptions<TData, TVariables>) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn,
    onSuccess: (data, variables) => {
      toast.success(successMessage);
      onSuccess?.(data, variables, queryClient);
    },
    onError: (error) => {
      logger.error('Mutation error: ', error);
      toast.error(errorMessage);
    },
  });
}