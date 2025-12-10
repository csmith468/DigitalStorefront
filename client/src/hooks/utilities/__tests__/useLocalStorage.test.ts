import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useLocalStorage } from '../useLocalStorage';

describe('useLocalStorage', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('returns initial value when nothing stored', () => {
    const { result } = renderHook(() => useLocalStorage('test-key', 'initial'));
    expect(result.current[0]).toBe('initial');
  });

  it('returns initial value for arrays', () => {
    const { result } = renderHook(() => useLocalStorage<string[]>('test-key', []));
    expect(result.current[0]).toEqual([]);
  });

  it('persists value to localStorage', () => {
    const { result } = renderHook(() => useLocalStorage('test-key', 'initial'));
    act(() => {
      result.current[1]('updated');
    });

    expect(result.current[0]).toBe('updated');
    expect(localStorage.getItem('test-key')).toBe('"updated"');
  });

  it('persists array values to localStorage', () => {
    const { result } = renderHook(() => useLocalStorage<string[]>('test-key', []));

    act(() => {
      result.current[1](['item1', 'item2']);
    });

    expect(result.current[0]).toEqual(['item1', 'item2']);
    expect(localStorage.getItem('test-key')).toBe('["item1","item2"]');
  });

  it('loads existing value from localStorage', () => {
    localStorage.setItem('test-key', '"existing-value"');
    const { result } = renderHook(() => useLocalStorage('test-key', 'initial'));
    expect(result.current[0]).toBe('existing-value');
  });

  it('loads existing array from localStorage', () => {
    localStorage.setItem('test-key', '["a","b","c"]');
    const { result } = renderHook(() => useLocalStorage<string[]>('test-key', []));
    expect(result.current[0]).toEqual(['a', 'b', 'c']);
  });

  it('handles invalid JSON', () => {
    localStorage.setItem('test-key', 'not-valid-json');
    const { result } = renderHook(() => useLocalStorage('test-key', 'fallback'));
    expect(result.current[0]).toBe('fallback');
  });

  it('supports functional updates', () => {
    const { result } = renderHook(() => useLocalStorage<number>('test-key', 0));

    act(() => {
      result.current[1](prev => prev + 1);
    });

    expect(result.current[0]).toBe(1);

    act(() => {
      result.current[1](prev => prev + 5);
    });

    expect(result.current[0]).toBe(6);
  });

  it('uses different keys independently', () => {
    const { result: result1 } = renderHook(() => useLocalStorage('key1', 'value1'));
    const { result: result2 } = renderHook(() => useLocalStorage('key2', 'value2'));

    expect(result1.current[0]).toBe('value1');
    expect(result2.current[0]).toBe('value2');

    act(() => {
      result1.current[1]('updated1');
    });

    expect(result1.current[0]).toBe('updated1');
    expect(result2.current[0]).toBe('value2');
  });
});
