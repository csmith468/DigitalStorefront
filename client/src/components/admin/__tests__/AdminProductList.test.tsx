import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { AdminProductList } from '../AdminProductList';
import * as useProductsHooks from '../../../hooks/queries/useProducts';
import * as useMetadataHooks from '../../../hooks/queries/useMetadata';
import { createMockUseUserReturn, mockProductTypes, ProductDetailBuilder } from '../../../tests/fixtures';
import { useUser } from '../../../contexts/useUser';

vi.mock('../../../hooks/queries/useProducts');
vi.mock('../../../hooks/queries/useMetadata');
vi.mock('../../../contexts/useUser', () => ({
  useUser: vi.fn(),
}));

function mockUseUser(roles: string[] = []) {
  vi.mocked(useUser).mockReturnValue(
    createMockUseUserReturn(roles) as any
  );
}

describe('AdminProductList - RBAC (Authorization) Tests', () => {
  beforeEach(() => {
    vi.mocked(useProductsHooks.useProducts).mockReturnValue({
      data: {
        items: [
          new ProductDetailBuilder().withId(1).asDemo().build(), // Demo product
          new ProductDetailBuilder().withId(2).build(), // Regular product
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

      renderWithProviders(<AdminProductList />);

      const viewButtons = screen.getAllByLabelText('View Product');
      expect(viewButtons).toHaveLength(2);

      expect(screen.queryByLabelText('Edit Product')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Delete Product')).not.toBeInTheDocument();
    });
  });

  describe('when user is logged in as non-admin (ProductWriter)', () => {
    it('shows view icon for demo products, edit+delete for regular products', () => {
      mockUseUser(['ProductWriter']);

      renderWithProviders(<AdminProductList />);

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

      renderWithProviders(<AdminProductList />);

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

      renderWithProviders(<AdminProductList />);

      const editButtons = screen.getAllByLabelText('Edit Product');
      expect(editButtons).toHaveLength(2);

      const deleteButtons = screen.getAllByLabelText('Delete Product');
      expect(deleteButtons).toHaveLength(2);

      expect(screen.queryByLabelText('View Product')).not.toBeInTheDocument();
    });
  });
});