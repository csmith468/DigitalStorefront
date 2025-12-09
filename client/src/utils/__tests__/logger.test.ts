import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';

const mockEnv = { DEV: true };

vi.mock('import.meta', () => ({
  env: mockEnv,
}));

describe('logger', () => {
  let consoleSpy: {
    error: ReturnType<typeof vi.spyOn>;
    warn: ReturnType<typeof vi.spyOn>;
    info: ReturnType<typeof vi.spyOn>;
    debug: ReturnType<typeof vi.spyOn>;
  };

  beforeEach(() => {
    consoleSpy = {
      error: vi.spyOn(console, 'error').mockImplementation(() => {}),
      warn: vi.spyOn(console, 'warn').mockImplementation(() => {}),
      info: vi.spyOn(console, 'info').mockImplementation(() => {}),
      debug: vi.spyOn(console, 'debug').mockImplementation(() => {}),
    };
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('in development mode', () => {
    it('logger.error calls console.error with message and error', async () => {
      const { logger } = await import('../logger');
      const testError = new Error('test error');

      logger.error('Error message', testError);

      expect(consoleSpy.error).toHaveBeenCalledWith('Error message', testError);
    });

    it('logger.error works with just message', async () => {
      const { logger } = await import('../logger');

      logger.error('Error message');

      expect(consoleSpy.error).toHaveBeenCalledWith('Error message', undefined);
    });

    it('logger.warn calls console.warn', async () => {
      const { logger } = await import('../logger');

      logger.warn('Warning message', { details: 'some data' });

      expect(consoleSpy.warn).toHaveBeenCalledWith('Warning message', { details: 'some data' });
    });

    it('logger.info calls console.info', async () => {
      const { logger } = await import('../logger');

      logger.info('Info message', [1, 2, 3]);

      expect(consoleSpy.info).toHaveBeenCalledWith('Info message', [1, 2, 3]);
    });

    it('logger.debug calls console.debug', async () => {
      const { logger } = await import('../logger');

      logger.debug('Debug message', { debug: true });

      expect(consoleSpy.debug).toHaveBeenCalledWith('Debug message', { debug: true });
    });
  });

  describe('logger methods exist', () => {
    it('has error, warn, info, and debug methods', async () => {
      const { logger } = await import('../logger');

      expect(typeof logger.error).toBe('function');
      expect(typeof logger.warn).toBe('function');
      expect(typeof logger.info).toBe('function');
      expect(typeof logger.debug).toBe('function');
    });
  });
});
