import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { BuyNowButton } from '../BuyNowButton';
import { useNavigate } from 'react-router-dom';

const mockOrderId = 123;

vi.mock('../PaymentModal', () => ({
  PaymentModal: ({ isOpen, onClose, product, onSuccess }: any) =>
    isOpen ? (
      <div data-testid="payment-modal">
        <span data-testid="modal-product-name">{product.name}</span>
        <button onClick={onClose}>Close</button>
        <button onClick={() => onSuccess(mockOrderId)}>Simulate Success</button>
      </div>
    ) : null
}));

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: vi.fn(),
  };
});

const mockProduct = {
  productId: 1,
  name: 'Test Product',
  price: 5000,
  priceIcon: '★'
};

describe('BuyNowButton', () => {
  const mockNavigate = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(useNavigate).mockReturnValue(mockNavigate);
  });

  it('renders Buy Now button', () => {
    renderWithProviders(
      <BuyNowButton product={mockProduct} className="test-class" />
    );

    expect(screen.getByRole('button', { name: /buy now/i })).toBeInTheDocument();
  });

  it('applies custom className to button', () => {
    renderWithProviders(
      <BuyNowButton product={mockProduct} className="custom-class" />
    );

    expect(screen.getByRole('button', { name: /buy now/i })).toHaveClass('custom-class');
  });

  it('can be disabled', () => {
    renderWithProviders(
      <BuyNowButton product={mockProduct} disabled className="test-class" />
    );

    expect(screen.getByRole('button', { name: /buy now/i })).toBeDisabled();
  });

  it('opens PaymentModal when clicked', async () => {
    const { user } = renderWithProviders(
      <BuyNowButton product={mockProduct} className="test-class" />
    );

    expect(screen.queryByTestId('payment-modal')).not.toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /buy now/i }));

    expect(screen.getByTestId('payment-modal')).toBeInTheDocument();
  });

  it('passes product to PaymentModal', async () => {
    const { user } = renderWithProviders(
      <BuyNowButton product={mockProduct} className="test-class" />
    );

    await user.click(screen.getByRole('button', { name: /buy now/i }));

    expect(screen.getByTestId('modal-product-name')).toHaveTextContent(mockProduct.name);
  });

  it('closes modal when close is triggered', async () => {
    const { user } = renderWithProviders(
      <BuyNowButton product={mockProduct} className="test-class" />
    );

    await user.click(screen.getByRole('button', { name: /buy now/i }));
    expect(screen.getByTestId('payment-modal')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /close/i }));
    expect(screen.queryByTestId('payment-modal')).not.toBeInTheDocument();
  });

  it('navigates to admin page with orderSuccess param on success', async () => {
    const { user } = renderWithProviders(
      <BuyNowButton product={mockProduct} className="test-class" />
    );

    await user.click(screen.getByRole('button', { name: /buy now/i }));

    await user.click(screen.getByRole('button', { name: /simulate success/i }));

    expect(mockNavigate).toHaveBeenCalledWith(`/admin?orderSuccess=${mockOrderId}&tab=orders`);
  });
});
