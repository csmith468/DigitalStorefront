import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { PaymentModal } from "./PaymentModal";

interface BuyNowButtonProps {
  product: {
    productId: number;
    name: string;
    price: number;
    priceIcon: string;
  };
  disabled?: boolean;
  className: string;
}

export function BuyNowButton({ product, disabled, className }: BuyNowButtonProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const navigate = useNavigate();

  const handleSuccess = (orderId: number) => {
    navigate(`/admin?orderSuccess=${orderId}&tab=orders`);
  };

  return (
    <>
      <button 
        onClick={() => setIsModalOpen(true)}
        disabled={disabled}
        className={className}
        >Buy Now
      </button>

      <PaymentModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        product={product}
        onSuccess={handleSuccess}
      />
    </>
  )
}