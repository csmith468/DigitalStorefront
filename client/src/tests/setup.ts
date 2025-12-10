import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import { mockAnimationsApi } from 'jsdom-testing-mocks';

mockAnimationsApi();

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useBlocker: vi.fn(() => ({
      state: 'unblocked',
      proceed: vi.fn(),
      reset: vi.fn(),
    })),
    useSearchParams: vi.fn(() => [new URLSearchParams(), vi.fn()]),
  };
});

afterEach(() => {
  cleanup();
  vi.clearAllMocks();
});