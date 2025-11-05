import { describe, it, expect } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { usePagination } from '../usePagination';

describe('usePagination', () => {
  it('initializes with default values', () => {
    const { result } = renderHook(() => usePagination());

    expect(result.current.page).toBe(1);
    expect(result.current.pageSize).toBe(12);
    expect(result.current.pageSizeOptions).toEqual([12, 24, 48]);
  });

  it('initializes with custom values', () => {
    const { result } = renderHook(() =>
      usePagination({
        initialPageSize: 24,
        pageSizeOptions: [24, 48, 96]
      })
    );

    expect(result.current.page).toBe(1);
    expect(result.current.pageSize).toBe(24);
    expect(result.current.pageSizeOptions).toEqual([24, 48, 96]);
  });

  it('changes page number', () => {
    const { result } = renderHook(() => usePagination());

    act(() => {
      result.current.onPageChange(2);
    });
    
    expect(result.current.page).toBe(2);
  });

  it('changes pages size and resets to page 1', () => {
    const { result } = renderHook(() => usePagination());

    act(() => {
      result.current.onPageChange(2);
    });
    expect(result.current.page).toBe(2);

    act(() => {
      result.current.onPageSizeChange(24);
    });
    expect(result.current.pageSize).toBe(24);
    expect(result.current.page).toBe(1);
  });

  it('resets to first page', () => {
    const { result } = renderHook(() => usePagination());
    
    act(() => {
      result.current.onPageChange(2);
    });
    expect(result.current.page).toBe(2);
    
    act(() => {
      result.current.resetToFirstPage();
    });
    expect(result.current.page).toBe(1);
  })
})