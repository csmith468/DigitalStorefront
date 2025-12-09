import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { PaymentModal } from '../PaymentModal';

const mockConfirmCardPayment = vi.fn();
const mockGetElement = vi.fn();

vi.mock('@stripe/react-stripe-js', () => ({
  useStripe: () => ({
    confirmCardPayment: mockConfirmCardPayment,
  }),
  useElements: () => ({
    getElement: mockGetElement,
  }),
  CardElement: ({ options }: any) => (
    <div data-testid="stripe-card-element" data-options={JSON.stringify(options)}>
      Mock Card Element
    </div>
  ),
}));

const mockMutateAsync = vi.fn();
vi.mock('../../../hooks/queries/useCheckout', () => ({
  usePaymentIntent: () => ({
    mutateAsync: mockMutateAsync,
    isPending: false,
  }),
}));


const mockProduct = {
  productId: 1,
  name: 'Test Product',
  price: 5000,
  priceIcon: '★', // Coins
};

const mockUsdProduct = {
  productId: 2,
  name: 'USD Product',
  price: 10,
  priceIcon: '$',
};

const defaultProps = {
  isOpen: true,
  onClose: vi.fn(),
  product: mockProduct,
  onSuccess: vi.fn(),
};

describe('PaymentModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockGetElement.mockReturnValue({ /* mock card element */ });
    mockMutateAsync.mockResolvedValue({
      clientSecret: 'pi_test_secret',
      orderId: 123,
    });
    mockConfirmCardPayment.mockResolvedValue({
      paymentIntent: { status: 'succeeded' },
    });
  });

  describe('Rendering', () => {
    it('renders modal with title', () => {
      renderWithProviders(<PaymentModal {...defaultProps} />);

      expect(screen.getByText('Complete Purchase')).toBeInTheDocument();
    });

    it('does not render when isOpen is false', () => {
      renderWithProviders(<PaymentModal {...defaultProps} isOpen={false} />);

      expect(screen.queryByText('Complete Purchase')).not.toBeInTheDocument();
    });

    it('displays product name', () => {
      renderWithProviders(<PaymentModal {...defaultProps} />);

      expect(screen.getByText(mockProduct.name)).toBeInTheDocument();
    });

    it('displays Stripe card element', () => {
      renderWithProviders(<PaymentModal {...defaultProps} />);

      expect(screen.getByTestId('stripe-card-element')).toBeInTheDocument();
    });

    it('renders Pay and Cancel buttons', () => {
      renderWithProviders(<PaymentModal {...defaultProps} />);

      expect(screen.getByRole('button', { name: /pay/i })).toBeInTheDocument();
      expect(screen.getByRole('button', { name: /cancel/i })).toBeInTheDocument();
    });
  });

  describe('Coin to USD Conversion', () => {
    it('shows conversion for coin-priced products', () => {
      renderWithProviders(<PaymentModal {...defaultProps} product={mockProduct} />);

      const expectedUsd = `$${(mockProduct.price * 0.001).toFixed(2)} USD`;
      expect(screen.getByText('Conversion')).toBeInTheDocument();
      expect(screen.getByText(expectedUsd)).toBeInTheDocument();
    });

    it('does not show conversion for USD-priced products', () => {
      renderWithProviders(<PaymentModal {...defaultProps} product={mockUsdProduct} />);

      expect(screen.queryByText('Conversion')).not.toBeInTheDocument();
    });

    it('shows correct amount on Pay button for coins', () => {
      renderWithProviders(<PaymentModal {...defaultProps} product={mockProduct} />);

      expect(screen.getByRole('button', { name: /pay \$5\.00/i })).toBeInTheDocument();
    });

    it('shows correct amount on Pay button for USD', () => {
      renderWithProviders(<PaymentModal {...defaultProps} product={mockUsdProduct} />);

      expect(screen.getByRole('button', { name: /pay \$10\.00/i })).toBeInTheDocument();
    });
  });

  describe('Test Mode Banner', () => {
    it('displays test mode information', () => {
      renderWithProviders(<PaymentModal {...defaultProps} />);

      expect(screen.getByText('Test Mode')).toBeInTheDocument();
      expect(screen.getByText(/4242 4242 4242 4242/)).toBeInTheDocument();
    });

    it('shows "Copied!" feedback after clicking copy button', async () => {
      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      const copyButton = screen.getByLabelText('Copy card number');
      await user.click(copyButton);

      expect(screen.getByText('Copied!')).toBeInTheDocument();
    });
  });

  describe('Form Submission', () => {
    it('calls payment intent mutation on submit', async () => {
      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      expect(mockMutateAsync).toHaveBeenCalledWith({
        productId: 1,
        quantity: 1,
      });
    });

    it('calls stripe confirmCardPayment with client secret', async () => {
      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      await waitFor(() => {
        expect(mockConfirmCardPayment).toHaveBeenCalledWith(
          'pi_test_secret',
          expect.objectContaining({
            payment_method: expect.any(Object),
          })
        );
      });
    });

    it('calls onSuccess with orderId on successful payment', async () => {
      const onSuccess = vi.fn();
      const { user } = renderWithProviders(
        <PaymentModal {...defaultProps} onSuccess={onSuccess} />
      );

      await user.click(screen.getByRole('button', { name: /pay/i }));

      await waitFor(() => {
        expect(onSuccess).toHaveBeenCalledWith(123);
      });
    });

    it('calls onClose after successful payment', async () => {
      const onClose = vi.fn();
      const { user } = renderWithProviders(
        <PaymentModal {...defaultProps} onClose={onClose} />
      );

      await user.click(screen.getByRole('button', { name: /pay/i }));

      await waitFor(() => {
        expect(onClose).toHaveBeenCalled();
      });
    });
  });

  describe('Error Handling', () => {
    it('displays Stripe error message', async () => {
      mockConfirmCardPayment.mockResolvedValue({
        error: { message: 'Your card was declined.' },
      });

      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      expect(await screen.findByText('Your card was declined.')).toBeInTheDocument();
    });

    it('displays generic error when Stripe error has no message', async () => {
      mockConfirmCardPayment.mockResolvedValue({
        error: {},
      });

      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      expect(await screen.findByText('Payment failed')).toBeInTheDocument();
    });

    it('displays error when payment intent mutation fails', async () => {
      mockMutateAsync.mockRejectedValue({
        response: { data: { message: 'Product not found' } },
      });

      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      expect(await screen.findByText('Product not found')).toBeInTheDocument();
    });

    it('does not call onSuccess when payment fails', async () => {
      mockConfirmCardPayment.mockResolvedValue({
        error: { message: 'Declined' },
      });
      const onSuccess = vi.fn();

      const { user } = renderWithProviders(
        <PaymentModal {...defaultProps} onSuccess={onSuccess} />
      );

      await user.click(screen.getByRole('button', { name: /pay/i }));

      await waitFor(() => {
        expect(screen.getByText('Declined')).toBeInTheDocument();
      });

      expect(onSuccess).not.toHaveBeenCalled();
    });
  });

  describe('Processing State', () => {
    it('shows "Processing..." while payment is in progress', async () => {
      // Make the mutation hang
      mockMutateAsync.mockImplementation(() => new Promise(() => {}));

      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      expect(await screen.findByText('Processing...')).toBeInTheDocument();
    });

    it('disables Pay button while processing', async () => {
      mockMutateAsync.mockImplementation(() => new Promise(() => {}));

      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /processing/i })).toBeDisabled();
      });
    });

    it('disables Cancel button while processing', async () => {
      mockMutateAsync.mockImplementation(() => new Promise(() => {}));

      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      await waitFor(() => {
        expect(screen.getByRole('button', { name: /cancel/i })).toBeDisabled();
      });
    });
  });

  describe('Cancel Button', () => {
    it('calls onClose when Cancel is clicked', async () => {
      const onClose = vi.fn();
      const { user } = renderWithProviders(
        <PaymentModal {...defaultProps} onClose={onClose} />
      );

      await user.click(screen.getByRole('button', { name: /cancel/i }));

      expect(onClose).toHaveBeenCalled();
    });
  });

  describe('Card Element Not Found', () => {
    it('handles missing card element gracefully', async () => {
      mockGetElement.mockReturnValue(null);

      const { user } = renderWithProviders(<PaymentModal {...defaultProps} />);

      await user.click(screen.getByRole('button', { name: /pay/i }));

      expect(await screen.findByText('Card element not found')).toBeInTheDocument();
    });
  });
});
