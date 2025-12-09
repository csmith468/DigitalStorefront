import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { AdminOrdersList } from '../AdminOrdersList';
import * as orderService from '../../../services/orderService';
import type { Order } from '../../../types/order';
import type { PaginatedResponse } from '../../../types/pagination';

vi.mock('../../../services/orderService');

const mockCompletedOrder: Order = {
  orderId: 1,
  userId: null,
  status: 'Completed',
  totalCents: 500,
  paymentCompletedAt: '2025-12-08T12:00:00Z',
  orderItems: [
    {
      orderItemId: 1,
      productId: 1,
      productName: 'Test Product',
      unitPriceCents: 500,
      quantity: 1,
      totalCents: 500
    }
  ]
};

const mockPendingOrder: Order = {
  orderId: 2,
  userId: null,
  status: 'Pending',
  totalCents: 1000,
  paymentCompletedAt: null,
  orderItems: [
    {
      orderItemId: 2,
      productId: 2,
      productName: 'Another Product',
      unitPriceCents: 500,
      quantity: 2,
      totalCents: 1000
    }
  ]
};

const mockProcessingOrder: Order = {
  ...mockCompletedOrder,
  orderId: 3,
  status: 'Processing',
  paymentCompletedAt: null
};

const mockFailedOrder: Order = {
  ...mockCompletedOrder,
  orderId: 4,
  status: 'Failed',
  paymentCompletedAt: null
};

function createMockOrdersResponse(orders: Order[]): PaginatedResponse<Order> {
  return {
    items: orders,
    totalCount: orders.length,
    page: 1,
    pageSize: 10,
    totalPages: 1,
    hasPrevious: false,
    hasNext: false
  };
}

describe('AdminOrdersList', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('Loading State', () => {
    it('shows loading screen while fetching orders', () => {
      vi.mocked(orderService.getOrders).mockImplementation(
        () => new Promise(() => {}) // Never resolves
      );

      renderWithProviders(<AdminOrdersList />);

      expect(screen.getByText('Loading Orders...')).toBeInTheDocument();
    });
  });

  describe('Error State', () => {
    it('shows error message when fetch fails', async () => {
      vi.mocked(orderService.getOrders).mockRejectedValue(new Error('Network error'));

      renderWithProviders(<AdminOrdersList />);

      expect(await screen.findByText('Failed to load orders. Please try again.')).toBeInTheDocument();
    });
  });

  describe('Empty State', () => {
    it('shows empty message when no orders exist', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(createMockOrdersResponse([]));

      renderWithProviders(<AdminOrdersList />);

      expect(await screen.findByText('No orders found. Try purchasing a product!')).toBeInTheDocument();
    });
  });

  describe('Data Display', () => {
    it('renders order table with correct headers', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockCompletedOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      expect(await screen.findByText('Order ID')).toBeInTheDocument();
      expect(screen.getByText('Status')).toBeInTheDocument();
      expect(screen.getByText('Items')).toBeInTheDocument();
      expect(screen.getByText('Total')).toBeInTheDocument();
      expect(screen.getByText('Completed At')).toBeInTheDocument();
    });

    it('displays order ID with hash prefix', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockCompletedOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      expect(await screen.findByText(`#${mockCompletedOrder.orderId}`)).toBeInTheDocument();
    });

    it('displays product name and quantity', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockPendingOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      const item = mockPendingOrder.orderItems[0];
      expect(await screen.findByText(new RegExp(`${item.productName} x${item.quantity}`))).toBeInTheDocument();
    });

    it('formats total cents as dollars', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockCompletedOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      const expectedTotal = `$${(mockCompletedOrder.totalCents / 100).toFixed(2)}`;
      expect(await screen.findByText(expectedTotal)).toBeInTheDocument();
    });

    it('displays completed date for completed orders', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockCompletedOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      await screen.findByText(`#${mockCompletedOrder.orderId}`);
      const cells = screen.getAllByRole('cell');
      const lastCell = cells[cells.length - 1];
      expect(lastCell.textContent).not.toBe('-'); // Completed orders should show a date, not a dash
    });

    it('displays dash for incomplete orders', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockPendingOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      await screen.findByText(`#${mockPendingOrder.orderId}`);
      expect(screen.getByText('-')).toBeInTheDocument();
    });
  });

  describe('Status Badges', () => {
    it('displays Completed status with green badge', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockCompletedOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      const badge = await screen.findByText('Completed');
      expect(badge).toHaveClass('bg-green-100', 'text-green-800');
    });

    it('displays Pending status with yellow badge', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockPendingOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      const badge = await screen.findByText('Pending');
      expect(badge).toHaveClass('bg-yellow-100', 'text-yellow-800');
    });

    it('displays Processing status with blue badge', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockProcessingOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      const badge = await screen.findByText('Processing');
      expect(badge).toHaveClass('bg-blue-100', 'text-blue-800');
    });

    it('displays Failed status with red badge', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockFailedOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      const badge = await screen.findByText('Failed');
      expect(badge).toHaveClass('bg-red-100', 'text-red-800');
    });
  });

  describe('Multiple Orders', () => {
    it('renders all orders in the list', async () => {
      vi.mocked(orderService.getOrders).mockResolvedValue(
        createMockOrdersResponse([mockCompletedOrder, mockPendingOrder, mockProcessingOrder])
      );

      renderWithProviders(<AdminOrdersList />);

      expect(await screen.findByText(`#${mockCompletedOrder.orderId}`)).toBeInTheDocument();
      expect(screen.getByText(`#${mockPendingOrder.orderId}`)).toBeInTheDocument();
      expect(screen.getByText(`#${mockProcessingOrder.orderId}`)).toBeInTheDocument();
    });
  });
});
