import { useEffect, useState } from "react";

// NOTE: only update after delay, reset delay when value changes
export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    return () => clearTimeout(timer); 
  }, [value, delay]);

  return debouncedValue;
}