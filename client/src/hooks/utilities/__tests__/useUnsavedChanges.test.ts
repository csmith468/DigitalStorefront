import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useUnsavedChanges } from '../useUnsavedChanges';
import type { Blocker } from 'react-router-dom';

type BlockerState = Blocker['state']; // 'unblocked' | 'blocked' | 'proceeding'

const mockProceed = vi.fn();
const mockReset = vi.fn();
let mockBlockerState: BlockerState = 'unblocked';

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useBlocker: vi.fn(() => ({
      state: mockBlockerState,
      proceed: mockProceed,
      reset: mockReset
    })),
  };
});

describe('useUnsavedChanges', () => {
  beforeEach(() => {
    mockBlockerState = 'unblocked';
    mockProceed.mockClear();
    mockReset.mockClear();

    vi.spyOn(window, 'addEventListener');
    vi.spyOn(window, 'removeEventListener');
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('Router Navigation Blocking', () => {
    it('returns showPrompt as false when blocker is not blocked', () => {
      mockBlockerState = 'unblocked';
      const { result } = renderHook(() => useUnsavedChanges({ isDirty: true }));
      expect(result.current.showPrompt).toBe(false);
    });

    it('returns showPrompt as true when blocker is blocked', () => {
      mockBlockerState = 'blocked';
      const { result } = renderHook(() => useUnsavedChanges({ isDirty: true }));
      expect(result.current.showPrompt).toBe(true);
    });

    it('calls blocker.proceed when proceed is called', () => {
      mockBlockerState = 'blocked';
      const { result } = renderHook(() => useUnsavedChanges({ isDirty: true }));

      result.current.proceed();
      
      expect(mockProceed).toHaveBeenCalledOnce();
    });

    it('calls blocker.reset when reset is called', () => {
      mockBlockerState = 'blocked';
      const { result } = renderHook(() => useUnsavedChanges({ isDirty: true }));

      result.current.reset();

      expect(mockReset).toHaveBeenCalledOnce();
    });

    it('handles proceed when blocker.proceed is undefined', () => {
      mockBlockerState = 'blocked';
      const { result } = renderHook(() => useUnsavedChanges({ isDirty: true }));

      mockProceed.mockReturnValue(undefined);

      expect(() => result.current.proceed()).not.toThrow();
    });

    it('handles reset when blocker.reset is undefined', () => {
      mockBlockerState = 'blocked';
      const { result } = renderHook(() => useUnsavedChanges({ isDirty: true }));

      mockReset.mockReturnValue(undefined);

      expect(() => result.current.reset()).not.toThrow();
    });
  });

  describe('Browser beforeunload Event', () => {
    const beforeunload = 'beforeunload';

    it('adds beforeunload event listener on mount', () => {
      renderHook(() => useUnsavedChanges({ isDirty: true }));

      expect(window.addEventListener).toHaveBeenCalledWith(
        beforeunload, expect.any(Function)
      );
    });

    it('removes beforeunload event listener on unmount', () => {
      const { unmount } = renderHook(() => useUnsavedChanges({ isDirty: true }));

      unmount();

      expect(window.removeEventListener).toHaveBeenCalledWith(
        beforeunload,
        expect.any(Function)
      );
    });

    it('prevents default and sets returnValue when isDirty is true', () => {
      renderHook(() => useUnsavedChanges({ isDirty: true }));

      const addEventListenerCalls = vi.mocked(window.addEventListener).mock.calls;
      const beforeUnloadCall = addEventListenerCalls.find(call => call[0] === beforeunload);
      const beforeUnloadHandler = beforeUnloadCall?.[1] as (e: BeforeUnloadEvent) => void;

      const mockEvent = {
        preventDefault: vi.fn(),
        returnValue: true,
      } as unknown as BeforeUnloadEvent;

      beforeUnloadHandler(mockEvent);

      expect(mockEvent.preventDefault).toHaveBeenCalled();
      expect(mockEvent.returnValue).toBe('');
    });

    it('does not prevent default when isDirty is false', () => {
      renderHook(() => useUnsavedChanges({ isDirty: false }));

      const addEventListenerCalls = vi.mocked(window.addEventListener).mock.calls;
      const beforeUnloadCall = addEventListenerCalls.find(call => call[0] === beforeunload);
      const beforeUnloadHandler = beforeUnloadCall?.[1] as (e: BeforeUnloadEvent) => void;

      const mockEvent = {
        preventDefault: vi.fn(),
        returnValue: true,
      } as unknown as BeforeUnloadEvent;

      beforeUnloadHandler(mockEvent);

      expect(mockEvent.preventDefault).not.toHaveBeenCalled();
      // returnValue is deprecated but required for beforeunload
      expect(mockEvent.returnValue).toBe(true);
    });

    it('updates event listener when isDirty changes', () => {
      const { rerender } = renderHook(
        ({ isDirty }) => useUnsavedChanges({ isDirty }),
        { initialProps: { isDirty: false } }
      );

      const initialCalls = vi.mocked(window.addEventListener).mock.calls;
      const initialHandler = initialCalls.find(call => call[0] === beforeunload)?.[1];

      rerender({ isDirty: true });

      expect(window.removeEventListener).toHaveBeenCalledWith(
        beforeunload,
        initialHandler
      );
    });
  });

  describe('Edge Cases', () => {
    it('handles multiple mounts and unmounts without errors', () => {
      const { unmount: unmount1 } = renderHook(() => useUnsavedChanges({ isDirty: true }));
      const { unmount: unmount2 } = renderHook(() => useUnsavedChanges({ isDirty: true }));

      expect(() => {
        unmount1();
        unmount2();
      }).not.toThrow();
    });

    it('returns all expected functions and properties', () => {
      const { result } = renderHook(() => useUnsavedChanges({ isDirty: true }));

      expect(result.current).toHaveProperty('showPrompt');
      expect(result.current).toHaveProperty('proceed');
      expect(result.current).toHaveProperty('reset');
      expect(typeof result.current.proceed).toBe('function');
      expect(typeof result.current.reset).toBe('function');
    });

    it('handles rapid isDirty changes', () => {
      const { rerender } = renderHook(
        ({ isDirty }) => useUnsavedChanges({ isDirty }),
        { initialProps: { isDirty: false } }
      );

      rerender({ isDirty: true });
      rerender({ isDirty: false });
      rerender({ isDirty: true });
      rerender({ isDirty: false });

      expect(() => rerender({ isDirty: true })).not.toThrow();
    });
  });
});