import { useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { TabNav } from "../../components/primitives/TabNav";
import { AdminProductList } from "../../components/admin/AdminProductList";
import { AdminOrdersList } from "../../components/admin/AdminOrdersList";
import { Modal } from "../../components/primitives/Modal";
import { PageHeader } from "../../components/primitives/PageHeader";
import { AdminEmailTemplate } from "../../components/admin/AdminEmailTemplate";

export function AdminPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const activeTab = searchParams.get('tab') || 'products';
  const [successOrderId, setSuccessOrderId] = useState<string | null>(null);

  useEffect(() => {
    if (!searchParams.get('tab')) {
      setSearchParams({ tab: 'products' }, { replace: true });
    }
  }, []);

  useEffect(() => {
    const orderId = searchParams.get('orderSuccess');
    if (orderId) {
      setSuccessOrderId(orderId);
      const newParams = new URLSearchParams(searchParams);
      newParams.delete('orderSuccess');
      setSearchParams(newParams, { replace: true });
    }
  }, []);

  const tabs = [
    { id: 'products', label: 'Products' },
    { id: 'orders', label: 'Orders' },
    { id: 'emailTemplate', label: 'Email Template' },
  ];

  return (
    <div className="container mx-auto px-4 pt-4 pb-8">
      <PageHeader title="Admin Console" returnLink="/" returnText="Back to Home" />

      <TabNav
        tabs={tabs}
        activeTab={activeTab}
        onTabChange={(tab) => setSearchParams({ tab })}
      />

      {activeTab === 'products' && <AdminProductList/>}
      {activeTab === 'orders' && <AdminOrdersList recentOrderSuccess={successOrderId !== null} />}
      {activeTab === 'emailTemplate' && <AdminEmailTemplate/>}

      <Modal
        isOpen={successOrderId !== null}
        onClose={() => setSuccessOrderId(null)}
        title="Payment Successful!"
        size="sm"
      >
        <div className="space-y-4">
          <p className="text-gray-600">
            Your order <span className="font-bold">#{successOrderId}</span> has been placed successfully.
          </p>
          <p className="text-sm text-gray-500">
            This was a test transaction using Stripe test mode - no real payment was processed.
          </p>
          <div className="pt-4 border-t">
            <button
              onClick={() => setSuccessOrderId(null)}
              className="w-full px-4 py-2 bg-[var(--color-primary)] text-white rounded-md hover:opacity-90"
            >
              Close
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
