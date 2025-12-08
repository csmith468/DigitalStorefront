export interface Order {
  orderId: number;
  userId: number | null;
  status: string;
  totalCents: number;
  paymentCompletedAt: string | null;
  orderItems: OrderItem[];
}

export interface OrderItem {
  orderItemId: number;
  productId: number;
  productName: string;
  unitPriceCents: number;
  quantity: number;
  totalCents: number;
}
