import { useQuery } from "@tanstack/react-query";
import { LoadingScreen } from "../primitives/LoadingScreen";
import { usePagination } from "../../hooks/utilities/usePagination";
import { PaginationWrapper } from "../primitives/PaginationWrapper";
import { getOrders } from "../../services/orderService";

export function AdminOrdersList() {
  const pagination = usePagination({
    initialPageSize: 10,
    pageSizeOptions: [10, 25, 50],
  });

  const { data, isLoading, error } = useQuery({
    queryKey: ["orders", pagination.page, pagination.pageSize],
    queryFn: ({ signal }) => getOrders({
      page: pagination.page,
      pageSize: pagination.pageSize
    }, signal),
  });

  if (isLoading && !data) {
    return <LoadingScreen message="Loading Orders..." />;
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="text-danger text-center">
          <p className="text-lg font-semibold mb-2">Error</p>
          <p>Failed to load orders. Please try again.</p>
        </div>
      </div>
    );
  }

  const orders = data?.items || [];

  const formatCents = (cents: number) => {
    return `$${(cents / 100).toFixed(2)}`;
  };

  const formatDate = (dateString: string | null) => {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleString();
  };

  const getStatusBadge = (status: string) => {
    const styles: Record<string, string> = {
      'Pending': 'bg-yellow-100 text-yellow-800',
      'Processing': 'bg-blue-100 text-blue-800',
      'Completed': 'bg-green-100 text-green-800',
      'Failed': 'bg-red-100 text-red-800',
    };
    return styles[status] || 'bg-gray-100 text-gray-800';
  };

  const tableHeaderStyle = "px-6 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider";

  return (
    <div className="mb-8">
      <h2 className="text-3xl font-bold text-text-primary mb-6">Order Management</h2>

      <PaginationWrapper
        {...pagination}
        totalPages={data?.totalPages || 0}
        totalCount={data?.totalCount || 0}
      >
        <div className="w-full overflow-x-auto bg-white rounded-lg shadow">
          <table className="w-full divide-y divide-gray-200 text-center">
            <thead className="bg-gray-50">
              <tr>
                <th className={tableHeaderStyle}>Order ID</th>
                <th className={tableHeaderStyle}>Status</th>
                <th className={tableHeaderStyle}>Items</th>
                <th className={tableHeaderStyle}>Total</th>
                <th className={tableHeaderStyle}>Completed At</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {orders.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-8 text-center text-gray-500">
                    No orders found. Try purchasing a product!
                  </td>
                </tr>
              ) : (
                orders.map((order) => (
                  <tr key={order.orderId} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm font-medium text-gray-900">#{order.orderId}</span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-3 py-1 text-xs font-semibold rounded-full ${getStatusBadge(order.status)}`}>
                        {order.status}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="text-sm text-gray-900">
                        {order.orderItems.map((item, idx) => (
                          <div key={item.orderItemId}>
                            {item.productName} x{item.quantity}
                            {idx < order.orderItems.length - 1 && ', '}
                          </div>
                        ))}
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm font-medium text-gray-900">{formatCents(order.totalCents)}</span>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                      {formatDate(order.paymentCompletedAt)}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </PaginationWrapper>
    </div>
  );
}
