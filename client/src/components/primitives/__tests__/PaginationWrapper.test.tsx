import { describe, it, expect, vi } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../../../tests/test-utils';
import { PaginationWrapper } from '../PaginationWrapper';

const defaultProps = {
  page: 1,
  totalPages: 5,
  pageSize: 10,
  totalCount: 50,
  onPageChange: vi.fn(),
  onPageSizeChange: vi.fn(),
};

describe('PaginationWrapper', () => {
  describe('Rendering', () => {
    it('renders children', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps}>
          <div data-testid="child-content">Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByTestId('child-content')).toBeInTheDocument();
    });

    it('shows item count when items exist', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByText('Showing 1 - 10 of 50')).toBeInTheDocument();
    });

    it('shows correct range for middle pages', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByText('Showing 21 - 30 of 50')).toBeInTheDocument();
    });

    it('shows correct range for last page with partial items', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={5} totalCount={45}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByText('Showing 41 - 45 of 45')).toBeInTheDocument();
    });

    it('shows "No items found" when totalCount is 0', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} totalCount={0} totalPages={0}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByText('No items found')).toBeInTheDocument();
    });

    it('shows current page indicator', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByText('Page 3 of 5')).toBeInTheDocument();
    });
  });

  describe('Pagination Controls Visibility', () => {
    it('shows pagination buttons when multiple pages exist', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('First Page')).toBeInTheDocument();
      expect(screen.getByLabelText('Previous Page')).toBeInTheDocument();
      expect(screen.getByLabelText('Next Page')).toBeInTheDocument();
      expect(screen.getByLabelText('Last Page')).toBeInTheDocument();
    });

    it('hides pagination buttons when only one page', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} totalPages={1} totalCount={5}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.queryByLabelText('First Page')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Previous Page')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Next Page')).not.toBeInTheDocument();
      expect(screen.queryByLabelText('Last Page')).not.toBeInTheDocument();
    });
  });

  describe('Button States - First Page', () => {
    it('disables First and Previous buttons on first page', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={1}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('First Page')).toBeDisabled();
      expect(screen.getByLabelText('Previous Page')).toBeDisabled();
    });

    it('enables Next and Last buttons on first page', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={1}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('Next Page')).toBeEnabled();
      expect(screen.getByLabelText('Last Page')).toBeEnabled();
    });
  });

  describe('Button States - Last Page', () => {
    it('disables Next and Last buttons on last page', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={5}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('Next Page')).toBeDisabled();
      expect(screen.getByLabelText('Last Page')).toBeDisabled();
    });

    it('enables First and Previous buttons on last page', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={5}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('First Page')).toBeEnabled();
      expect(screen.getByLabelText('Previous Page')).toBeEnabled();
    });
  });

  describe('Button States - Middle Page', () => {
    it('enables all buttons on middle page', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('First Page')).toBeEnabled();
      expect(screen.getByLabelText('Previous Page')).toBeEnabled();
      expect(screen.getByLabelText('Next Page')).toBeEnabled();
      expect(screen.getByLabelText('Last Page')).toBeEnabled();
    });
  });

  describe('Button States - Loading', () => {
    it('disables all buttons when loading', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3} isLoading={true}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('First Page')).toBeDisabled();
      expect(screen.getByLabelText('Previous Page')).toBeDisabled();
      expect(screen.getByLabelText('Next Page')).toBeDisabled();
      expect(screen.getByLabelText('Last Page')).toBeDisabled();
    });
  });

  describe('Navigation', () => {
    it('calls onPageChange with 1 when First Page clicked', async () => {
      const onPageChange = vi.fn();
      const { user } = renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3} onPageChange={onPageChange}>
          <div>Content</div>
        </PaginationWrapper>
      );

      await user.click(screen.getByLabelText('First Page'));

      expect(onPageChange).toHaveBeenCalledWith(1);
    });

    it('calls onPageChange with page-1 when Previous clicked', async () => {
      const onPageChange = vi.fn();
      const { user } = renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3} onPageChange={onPageChange}>
          <div>Content</div>
        </PaginationWrapper>
      );

      await user.click(screen.getByLabelText('Previous Page'));

      expect(onPageChange).toHaveBeenCalledWith(2);
    });

    it('calls onPageChange with page+1 when Next clicked', async () => {
      const onPageChange = vi.fn();
      const { user } = renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3} onPageChange={onPageChange}>
          <div>Content</div>
        </PaginationWrapper>
      );

      await user.click(screen.getByLabelText('Next Page'));

      expect(onPageChange).toHaveBeenCalledWith(4);
    });

    it('calls onPageChange with totalPages when Last Page clicked', async () => {
      const onPageChange = vi.fn();
      const { user } = renderWithProviders(
        <PaginationWrapper {...defaultProps} page={3} onPageChange={onPageChange}>
          <div>Content</div>
        </PaginationWrapper>
      );

      await user.click(screen.getByLabelText('Last Page'));

      expect(onPageChange).toHaveBeenCalledWith(5);
    });
  });

  describe('Page Size', () => {
    it('renders page size selector', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps}>
          <div>Content</div>
        </PaginationWrapper>
      );

      expect(screen.getByLabelText('Items per Page')).toBeInTheDocument();
    });

    it('calls onPageSizeChange when page size is changed', async () => {
      const onPageSizeChange = vi.fn();
      const { user } = renderWithProviders(
        <PaginationWrapper {...defaultProps} onPageSizeChange={onPageSizeChange} pageSizeOptions={[10, 25, 50]}>
          <div>Content</div>
        </PaginationWrapper>
      );

      const select = screen.getByLabelText('Items per Page');
      await user.selectOptions(select, '25');

      expect(onPageSizeChange).toHaveBeenCalledWith(25);
    });

    it('uses custom pageSizeOptions when provided', () => {
      renderWithProviders(
        <PaginationWrapper {...defaultProps} pageSizeOptions={[5, 15, 30]}>
          <div>Content</div>
        </PaginationWrapper>
      );

      const select = screen.getByLabelText('Items per Page');
      expect(select).toHaveTextContent('5');
      expect(select).toHaveTextContent('15');
      expect(select).toHaveTextContent('30');
    });
  });
});
