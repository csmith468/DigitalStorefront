import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { AdminPage } from '../AdminPage';
import { useSearchParams } from 'react-router-dom';

vi.mock('../../../components/admin/AdminProductList', () => ({
  AdminProductList: () => <div data-testid="admin-product-list">Product List</div>
}));

vi.mock('../../../components/admin/AdminOrdersList', () => ({
  AdminOrdersList: () => <div data-testid="admin-orders-list">Orders List</div>
}));

const mockSetSearchParams = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useSearchParams: vi.fn(),
  };
});

interface RenderAdminPageOptions {
  tab?: 'products' | 'orders';
  orderSuccess?: number;
}

function renderAdminPage(options: RenderAdminPageOptions = {}) {
  const { tab, orderSuccess } = options;

  const params = new URLSearchParams();
  if (tab) params.set('tab', tab);
  if (orderSuccess) params.set('orderSuccess', String(orderSuccess));

  vi.mocked(useSearchParams).mockReturnValue([params, mockSetSearchParams]);

  return renderWithProviders(<AdminPage />);
}

describe('AdminPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Tab Navigation', () => {
    it('shows products tab by default when no tab param', () => {
      renderAdminPage();

      expect(screen.getByTestId('admin-product-list')).toBeInTheDocument();
      expect(screen.queryByTestId('admin-orders-list')).not.toBeInTheDocument();
    });

    it('sets default tab param on mount if not present', () => {
      renderAdminPage();

      expect(mockSetSearchParams).toHaveBeenCalledWith(
        { tab: 'products' },
        { replace: true }
      );
    });

    it('shows products tab when tab=products', () => {
      renderAdminPage({ tab: 'products' });

      expect(screen.getByTestId('admin-product-list')).toBeInTheDocument();
      expect(screen.queryByTestId('admin-orders-list')).not.toBeInTheDocument();
    });

    it('shows orders tab when tab=orders', () => {
      renderAdminPage({ tab: 'orders' });

      expect(screen.getByTestId('admin-orders-list')).toBeInTheDocument();
      expect(screen.queryByTestId('admin-product-list')).not.toBeInTheDocument();
    });

    it('renders both Products and Orders tab buttons', () => {
      renderAdminPage({ tab: 'products' });

      expect(screen.getByRole('button', { name: /products/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /orders/i })).toBeInTheDocument();
    });

    it('switches to orders tab when Orders button is clicked', async () => {
      const { user } = renderAdminPage({ tab: 'products' });

      await user.click(screen.getByRole('button', { name: /orders/i }));

      expect(mockSetSearchParams).toHaveBeenCalledWith({ tab: 'orders' });
    });

    it('switches to products tab when Products button is clicked', async () => {
      const { user } = renderAdminPage({ tab: 'orders' });

      await user.click(screen.getByRole('button', { name: /products/i }));

      expect(mockSetSearchParams).toHaveBeenCalledWith({ tab: 'products' });
    });
  });

  describe('Success Modal', () => {
    it('shows success modal when orderSuccess param is present', () => {
      renderAdminPage({ tab: 'products', orderSuccess: 123 });

      expect(screen.getByText('Payment Successful!')).toBeInTheDocument();
      expect(screen.getByText(/your order/i)).toBeInTheDocument();
      expect(screen.getByText(/has been placed successfully/i)).toBeInTheDocument();
    });

    it('displays order ID in success modal', () => {
      const orderId = 456;
      renderAdminPage({ tab: 'products', orderSuccess: orderId });

      expect(screen.getByText(`#${orderId}`)).toBeInTheDocument();
    });

    it('shows test transaction message', () => {
      renderAdminPage({ tab: 'products', orderSuccess: 123 });

      expect(screen.getByText(/test transaction.*stripe test mode/i)).toBeInTheDocument();
    });

    it('removes orderSuccess param from URL after showing modal', async () => {
      renderAdminPage({ tab: 'products', orderSuccess: 123 });

      await waitFor(() => {
        expect(mockSetSearchParams).toHaveBeenCalledWith(
          expect.any(URLSearchParams),
          { replace: true }
        );
      });

      const callArgs = mockSetSearchParams.mock.calls.find(
        call => call[0] instanceof URLSearchParams
      );
      if (callArgs) expect(callArgs[0].get('orderSuccess')).toBeNull();
    });

    it('closes success modal when Close button is clicked', async () => {
      const { user } = renderAdminPage({ tab: 'products', orderSuccess: 123 });

      expect(screen.getByText('Payment Successful!')).toBeInTheDocument();

      await user.click(screen.getByRole('button', { name: /close/i }));
      await waitFor(() => {
        expect(screen.queryByText('Payment Successful!')).not.toBeInTheDocument();
      });
    });

    it('does not show success modal when no orderSuccess param', () => {
      renderAdminPage({ tab: 'products' });

      expect(screen.queryByText('Payment Successful!')).not.toBeInTheDocument();
    });
  });
});
