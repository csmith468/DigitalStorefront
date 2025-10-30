import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithRouter } from '../../../tests/test-utils';
import { AdminProductList } from '../AdminProductList';
import * as useProductsHooks from '../../../hooks/queries/useProducts';
import * as useMetadataHooks from '../../../hooks/queries/useMetadata';
import * as useUserHook from '../../../contexts/useUser';

vi.mock('../../../hooks/queries/useProducts');
vi.mock('../../../hooks/queries/useMetadata');
vi.mock('../../../contexts/useUser');

function createMockProduct(id: number, isDemoProduct: boolean = false) {
  return {
    productId: id,
    name: isDemoProduct ? 'Demo Pet' : 'Regular Pet',
    slug: isDemoProduct ? 'demo-pet' : `regular-pet-${id}`,
    productTypeId: 1,
    price: 1000,
    premiumPrice: 800,
    priceIcon: 'KC',
    isDemoProduct,
    primaryImage: null,
  };
}

const mockProductTypes = [
  { productTypeId: 1, typeName: 'Pet' },
];

function mockUseUser(roles: string[] = []) {
  const hasRole = (roleName: string) => roles.includes(roleName);
  const isAdmin = () => hasRole('Admin');
  const canManageProducts = () => hasRole('Admin') || hasRole('ProductWriter');

  vi.mocked(useUserHook.useUser).mockReturnValue({
    user: roles.length > 0 ? { userId: 1, username: 'testuser' } : null,
    isAuthenticated: roles.length > 0,
    roles,
    isLoading: false,
    login: vi.fn(),
    register: vi.fn(),
    logout: vi.fn(),
    openAuthModal: vi.fn(),
    closeAuthModal: vi.fn(),
    hasRole,
    isLoggedIn: () => roles.length > 0,
    isAdmin,
    canManageProducts,
    canManageImages: () => hasRole('Admin') || hasRole('ImageManager'),
  } as any);
}

describe('AdminProductList - RBAC (Authorization) Tests', () => {
  beforeEach(() => {
    vi.mocked(useProductsHooks.useProducts).mockReturnValue({
      data: {
        items: [
          createMockProduct(1, true),   // Demo product
          createMockProduct(2, false),  // Regular product
        ],
        totalCount: 2,
        totalPages: 1,
        page: 1,
        pageSize: 10,
      },
      isLoading: false,
      error: null,
    } as any);

    vi.mocked(useMetadataHooks.useProductTypes).mockReturnValue({
      data: mockProductTypes,
      isLoading: false,
    } as any);

    vi.mocked(useProductsHooks.useDeleteProduct).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any);
  });

  describe('when user is not logged in or has no roles', () => {
    it('shows only view icon for all products', () => {
      mockUseUser([]);

      renderWithRouter(<AdminProductList />);

      const viewButtons = screen.getAllByLabelText('View Product');
      expect(viewButtons).toHaveLength(2);

      expect(screen.queryByLabelText('Edit Product')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Delete Product')).not.toBeInTheDocument();
    });
  });

  describe('when user is logged in as non-admin (ProductWriter)', () => {
    it('shows view icon for demo products, edit+delete for regular products', () => {
      mockUseUser(['ProductWriter']);

      renderWithRouter(<AdminProductList />);

      const viewButtons = screen.getAllByLabelText('View Product');
      expect(viewButtons).toHaveLength(1);

      const editButtons = screen.getAllByLabelText('Edit Product');
      expect(editButtons).toHaveLength(1);

      const deleteButtons = screen.getAllByLabelText('Delete Product');
      expect(deleteButtons).toHaveLength(1);
    });
  });

  describe('when user is logged in as admin', () => {
    it('shows edit + delete icons for all products including demo', () => {
      mockUseUser(['Admin']);

      renderWithRouter(<AdminProductList />);

      const editButtons = screen.getAllByLabelText('Edit Product');
      expect(editButtons).toHaveLength(2);

      const deleteButtons = screen.getAllByLabelText('Delete Product');
      expect(deleteButtons).toHaveLength(2);

      expect(screen.queryByLabelText('View Product')).not.toBeInTheDocument();
    });
  });

  describe('when user has both Admin and ProductWriter roles', () => {
    it('shows edit + delete for all products including demo', () => {
      mockUseUser(['Admin', 'ProductWriter']);

      renderWithRouter(<AdminProductList />);

      const editButtons = screen.getAllByLabelText('Edit Product');
      expect(editButtons).toHaveLength(2);

      const deleteButtons = screen.getAllByLabelText('Delete Product');
      expect(deleteButtons).toHaveLength(2);

      expect(screen.queryByLabelText('View Product')).not.toBeInTheDocument();
    });
  });
});