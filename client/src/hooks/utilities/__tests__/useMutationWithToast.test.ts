import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import toast from "react-hot-toast";
import { useMutationWithToast } from "../useMutationWithToast";
import { createQueryWrapper } from "../../../tests/test-utils";
import { logger } from '../../../utils/logger';


vi.mock('react-hot-toast', () => ({
  default: {
    success: vi.fn(),
    error: vi.fn(),
  },
}));

vi.mock('../../../utils/logger', () => ({
  logger: {
    error: vi.fn(),
    log: vi.fn(),
  },
}));

describe('useMutationWithToast', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('shows success toast when mutation is successful', async () => {
    const mutationFn = vi.fn().mockResolvedValue({ id: 1, name: 'Test' });

    const { result } = renderHook(
        () =>
          useMutationWithToast({
            mutationFn,
            successMessage: 'Success!',
          }),
        { wrapper: createQueryWrapper() }
      );

      result.current.mutate(undefined);

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true);
      });

      expect(toast.success).toHaveBeenCalledWith('Success!');
      expect(mutationFn).toHaveBeenCalled();
  });

  it('shows error toast when mutation fails', async () => {
    const mutationFn = vi.fn().mockRejectedValue(new Error('Mutation failed'));

    const { result } = renderHook(
      () =>
        useMutationWithToast({
          mutationFn,
          successMessage: 'Success!',
        }),
      { wrapper: createQueryWrapper() }
    );

    result.current.mutate(undefined);

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalledWith('An error occurred. Please try again.');
    expect(logger.error).toHaveBeenCalledWith('Mutation error: ', expect.any(Error));
  });

  it('shows custom error message when provided', async () => {
    const mutationFn = vi.fn().mockRejectedValue(new Error('Mutation failed'));

    const { result } = renderHook(
      () =>
        useMutationWithToast({
          mutationFn,
          successMessage: 'Success!',
          errorMessage: 'Custom error message.',
        }),
      { wrapper: createQueryWrapper() }
    );

    result.current.mutate(undefined);
    
    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(toast.error).toHaveBeenCalledWith('Custom error message.');
  });

  it('calls onSuccess callback with data, variables, and queryClient', async () => {
    const mutationFn = vi.fn().mockResolvedValue({ id: 1, name: 'Test' });
    const onSuccess = vi.fn();

    const {result } = renderHook(
      () =>
        useMutationWithToast({
          mutationFn,
          successMessage: 'Success!',
          onSuccess,
        }),
      { wrapper: createQueryWrapper() }
    );

    const variables = { name: 'Test' };
    result.current.mutate(variables);

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
    
    expect(onSuccess).toHaveBeenCalledWith(
      { id: 1, name: 'Test' },
      variables,
      expect.any(Object) // queryClient
    );
  });

  it('does not call onSuccess when mutation fails', async () => {
    const mutationFn = vi.fn().mockRejectedValue(new Error('Mutation failed'));
    const onSuccess = vi.fn();

    const {result } = renderHook(
      () =>
        useMutationWithToast({
          mutationFn,
          successMessage: 'Success!',
          onSuccess,
        }),
      { wrapper: createQueryWrapper() }
    );

    result.current.mutate(undefined);

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(onSuccess).not.toHaveBeenCalled();
  });
});