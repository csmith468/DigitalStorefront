import { useState } from "react";
import { CardElement, useStripe, useElements } from '@stripe/react-stripe-js';
import { ClipboardDocumentIcon } from "@heroicons/react/24/outline";
import { Modal } from "../primitives/Modal";
import { formatPrice } from "../../utils/formatters";
import { usePaymentIntent } from "../../hooks/queries/useCheckout";
import { FormInput } from "../primitives/FormInput";
import { FormLabel } from "../primitives/FormLabel";

interface PaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
  product: {
    productId: number;
    name: string;
    price: number;
    priceIcon: string;
  };
  onSuccess: (orderId: number) => void;
}

const COIN_TO_USD = 0.001;

export function PaymentModal({ isOpen, onClose, product, onSuccess }: PaymentModalProps) {
  const stripe = useStripe();
  const elements = useElements();
  const [error, setError] = useState<string | null>(null);
  const [processing, setProcessing] = useState(false);
  const [copied, setCopied] = useState(false);
  const [email, setEmail] = useState('');

  const handleCopy = (e: React.MouseEvent<HTMLButtonElement>) => {
    navigator.clipboard.writeText('4242424242424242');
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
    e.currentTarget.blur();
  };

  const isCoins = product.priceIcon === '★';
  const usdAmount = isCoins ? product.price * COIN_TO_USD : product.price;

  const paymentIntentMutation = usePaymentIntent();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!stripe || !elements) return;

    setProcessing(true);
    setError(null);

    try {
      const { clientSecret, orderId } = await paymentIntentMutation.mutateAsync({
        productId: product.productId, 
        quantity: 1,
        email: email || undefined
      });

      const cardElement = elements.getElement(CardElement);
      if (!cardElement) throw new Error('Card element not found');

      const { error: stripeError, paymentIntent } = await stripe.confirmCardPayment(
        clientSecret, { payment_method: { card: cardElement } }
      );

      if (stripeError) {
        setError(stripeError.message || 'Payment failed');
        return;
      }

      if (paymentIntent?.status === 'succeeded') {
        onSuccess(orderId);
        onClose();
      }
    } catch (err: any) {
      setError(err.response?.data?.message || err.message || 'Something went wrong');
    } finally {
      setProcessing(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Complete Purchase" size="sm">
      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Product Summary */}
        <div className="bg-gray-50 p-4 rounded-lg">
          <div className="flex justify-between">
            <span className="font-medium">{product.name}</span>
            <span className="font-bold">{formatPrice(product.price, product.priceIcon)}</span>
          </div>
          {isCoins && (
            <div className="mt-2 pt-2 border-t border-gray-200 text-sm text-gray-600">
              <div className="flex justify-between">
                <span>Conversion</span>
                <span className="font-semibold text-green-700">${usdAmount.toFixed(2)} USD</span>
              </div>
            </div>
          )}
        </div>

        <div className="bg-blue-600 text-white rounded-lg p-4">
          <p className="font-semibold">Test Mode</p>
          <p className="mt-2 flex items-center gap-2">
            <span>Use card:</span>
            <span className="bg-white/20 rounded font-mono flex items-center overflow-hidden">
              <code className="px-2 py-1">4242 4242 4242 4242</code>
              <span className="w-px self-stretch bg-white/30" />
              <button
                type="button"
                onClick={handleCopy}
                aria-label="Copy card number"
                className="hover:bg-white/10 px-2 py-1 transition-colors focus:outline-none focus:bg-white/10"
              >
                <ClipboardDocumentIcon className="h-4 w-4" />
              </button>
            </span>
            {copied && <span className="text-sm text-blue-200">Copied!</span>}
          </p>
          <p className="text-md mt-1 text-blue-100">
            Any Future Date, 3-digit CVC, & ZIP
          </p>
        </div>

        <div>
          <FormLabel label="Card Details" />
          <div className="border border-gray-300 rounded-lg p-3">
            <CardElement
              options={{
                disableLink: true,
                style: {
                  base: {
                    fontSize: '16px',
                    color: '#424770',
                    '::placeholder': { color: '#aab7c4' }
                  },
                  invalid: { color: '#9e2146' }
                }
              }}
            />
          </div>
        </div>
        <div>
          <FormInput 
            id="email" 
            label="Email (optional to test SendGrid integration)"
            type="email"
            value={email}
            onChange={(_, v) => setEmail(v?.toString() || '')}
          />
          <p className="text-xs text-gray-500 mt-1">
            This will send you a sample order confirmation email.
          </p>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-3">
            <p className="text-sm text-red-600">{error}</p>
          </div>
        )}

        <div className="flex gap-3 pt-4 border-t">
          <button
            type="button"
            onClick={onClose}
            disabled={processing}
            className="flex-1 px-4 py-2 border border-gray-300 rounded-md hover:bg-gray-50 disabled:opacity-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={!stripe || processing}
            className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 disabled:opacity-50"
          >
            {processing ? 'Processing...' : `Pay $${usdAmount.toFixed(2)}`}
          </button>
        </div>
      </form>
    </Modal>
  );
}