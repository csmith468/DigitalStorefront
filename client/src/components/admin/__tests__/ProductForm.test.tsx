import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithRouter } from '../../../tests/test-utils';
import { ProductForm } from '../ProductForm';
import * as useProductsHooks from '../../../hooks/queries/useProducts';
import * as useMetadataHooks from '../../../hooks/queries/useMetadata';

vi.mock('../../../hooks/queries/useProducts');
vi.mock('../../../hooks/queries/useMetadata');

const mockCategories = [
  {
    categoryId: 1,
    name: 'Pets',
    subcategories: [
      { subcategoryId: 1, name: 'Dogs', slug: 'dogs', categoryId: 1 },
      { subcategoryId: 2, name: 'Cats', slug: 'cats', categoryId: 1 },
    ],
  },
];

const mockProductTypes = [
  { productTypeId: 1, typeName: 'Pet' },
  { productTypeId: 2, typeName: 'Furniture' },
];

const mockPriceTypes = [
  { priceTypeId: 1, priceTypeName: 'Coins', icon: '★' },
  { priceTypeId: 2, priceTypeName: 'Dollars', icon: '$' },
];

const mockTags = [
  { tagId: 1, name: 'red' },
  { tagId: 2, name: 'blue' },
];

describe('ProductForm', () => {
  beforeEach(() => {
    vi.mocked(useMetadataHooks.useCategories).mockReturnValue({
      data: mockCategories,
      isLoading: false,
    } as any);

    vi.mocked(useMetadataHooks.useProductTypes).mockReturnValue({
      data: mockProductTypes,
      isLoading: false,
    } as any);

    vi.mocked(useMetadataHooks.usePriceTypes).mockReturnValue({
      data: mockPriceTypes,
      isLoading: false,
    } as any);

    vi.mocked(useMetadataHooks.useTags).mockReturnValue({
      data: mockTags,
      isLoading: false,
    } as any);

    vi.mocked(useProductsHooks.useCreateProduct).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any);

    vi.mocked(useProductsHooks.useUpdateProduct).mockReturnValue({
      mutateAsync: vi.fn(),
      isPending: false,
    } as any);
  });

  it('renders form in edit mode with submit button visible', () => {
    renderWithRouter(
      <ProductForm
        mode="edit"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // hideSubmit = false
    expect(screen.getByRole('button', { name: /create product/i })).toBeInTheDocument();

    // Fields should not be disabled
    const nameInput = screen.getByLabelText(/^name/i);
    expect(nameInput).not.toBeDisabled();
  });

  it('renders form in view mode with submit button hidden and fields disabled', () => {
    renderWithRouter(
      <ProductForm
        mode="view"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // hideSubmit = true
    expect(screen.queryByRole('button', { name: /create product/i })).not.toBeInTheDocument();

    // Fields should be disabled
    const nameInput = screen.getByLabelText(/^name/i);
    expect(nameInput).toBeDisabled();
  });

  it('renders form in try mode with fields enabled but submit button hidden', () => {
    renderWithRouter(
      <ProductForm
        mode="try"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // hideSubmit = false
    expect(screen.queryByRole('button', { name: /create product/i })).not.toBeInTheDocument();

    // Fields should not be disabled
    const nameInput = screen.getByLabelText(/^name/i);
    expect(nameInput).not.toBeDisabled();
  });

  it('calls createProduct mutation when creating new product', async () => {
    const mockCreateMutation = vi.fn().mockResolvedValue({ productId: 1, name: 'Test Pet' });
    const onSuccess = vi.fn();

    vi.mocked(useProductsHooks.useCreateProduct).mockReturnValue({
      mutateAsync: mockCreateMutation,
      isPending: false,
    } as any);

    const { user } = renderWithRouter(
      <ProductForm
        mode="edit"
        onSuccess={onSuccess}
        onCancel={vi.fn()}
      />
    );

    // Fill out required fields
    await user.type(screen.getByLabelText(/^name/i), 'Test Pet');
    await user.type(screen.getByLabelText(/slug/i), 'test-pet');

    const productTypeSelect = screen.getByLabelText(/product type/i);
    await user.selectOptions(productTypeSelect, '1');

    const priceTypeSelect = screen.getByLabelText(/price type/i);
    await user.selectOptions(priceTypeSelect, '1');

    const priceInput = screen.getByRole('spinbutton', { name: /^price/i });
    await user.clear(priceInput);
    await user.type(priceInput, '1000');
    await user.clear(screen.getByLabelText(/^premium price/i));
    await user.type(screen.getByLabelText(/^premium price/i), '500');

    const dogCheckbox = screen.getByLabelText(/dogs/i);
    await user.click(dogCheckbox);

    // Submit and assert
    const submitButton = screen.getByRole('button', { name: /create product/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockCreateMutation).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'Test Pet',
          slug: 'test-pet',
          productTypeId: 1,
          priceTypeId: 1,
          price: 1000,
          premiumPrice: 500,
          subcategoryIds: expect.arrayContaining([1]),
        })
      );
    });

    expect(onSuccess).toHaveBeenCalled();
  });

  it('calls updateProduct mutation when editing existing product', async () => {
    const mockUpdateMutation = vi.fn().mockResolvedValue({ productId: 123, name: 'Updated Pet' });
    const onSuccess = vi.fn();

    vi.mocked(useProductsHooks.useUpdateProduct).mockReturnValue({
      mutateAsync: mockUpdateMutation,
      isPending: false,
    } as any);

    const existingProduct = {
      productId: 123,
      name: 'Existing Product',
      slug: 'existing-product',
      productTypeId: 1,
      priceTypeId: 1,
      price: 500,
      premiumPrice: 400,
      subcategories: [{ subcategoryId: 1, name: 'Dogs', slug: 'dogs', categoryId: 1 }],
      tags: [],
      description: '',
      isTradeable: false,
      isNew: true,
      isPromotional: false,
      isExclusive: false,
      images: [], 
      isDemoProduct: false, 
      sku: 'ABC123', 
      priceIcon: '★', 
      primaryImage: null
    };

    const { user } = renderWithRouter(
      <ProductForm
        mode="edit"
        existingProduct={existingProduct}
        onSuccess={onSuccess}
        onCancel={vi.fn()}
      />
    );

    const nameInput = screen.getByLabelText(/^name/i);
    await user.clear(nameInput);
    await user.type(nameInput, 'Updated Product');

    const submitButton = screen.getByRole('button', { name: /update product/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(mockUpdateMutation).toHaveBeenCalledWith({
        productId: 123,
        product: expect.objectContaining({
          name: 'Updated Product',
        }),
      });
    });

    expect(onSuccess).toHaveBeenCalled();
  });

  it('shows custom validation error after HTML5 validation passes', async () => {
    const { user } = renderWithRouter(
      <ProductForm
        mode="edit"
        onSuccess={vi.fn()}
        onCancel={vi.fn()}
      />
    );

    // Fill required fields
    await user.type(screen.getByLabelText(/^name/i), 'Test Pet');
    await user.type(screen.getByLabelText(/slug/i), 'test-pet');

    const productTypeSelect = screen.getByLabelText(/product type/i);
    await user.selectOptions(productTypeSelect, '1');

    const priceTypeSelect = screen.getByLabelText(/price type/i);
    await user.selectOptions(priceTypeSelect, '1');

    const priceInput = screen.getByRole('spinbutton', { name: /^price/i });
    await user.clear(priceInput);
    await user.type(priceInput, '500');
    await user.clear(screen.getByLabelText(/^premium price/i));
    await user.type(screen.getByLabelText(/^premium price/i), '600');

    const dogCheckbox = screen.getByLabelText(/dogs/i);
    await user.click(dogCheckbox);

    // Submit and assert
    const submitButton = screen.getByRole('button', { name: /create product/i });
    await user.click(submitButton);

    expect(await screen.findByText(/premium price cannot exceed regular price/i)).toBeInTheDocument();
  });
});