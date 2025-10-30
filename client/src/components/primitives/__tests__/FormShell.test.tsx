import { describe, it, expect, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { FormShell } from '../FormShell';
import { renderWithRouter } from '../../../tests/test-utils';

describe('FormShell', () => {
  it('renders form with submit & cancel buttons', () => {
    renderWithRouter(
      <FormShell
        initial={{ name: '' }}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      >
        {({ data, updateField }) => (
          <input
            data-testid="name-input"
            value={data.name}
            onChange={(e) => updateField('name', e.target.value)}
          />
        )}
      </FormShell>
    );

    expect(screen.getByRole('button', { name: /save/i})).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /cancel/i})).toBeInTheDocument();
  });

  it('shows validation error when validation fails on submit', async () => {
    const validate = (data: { name: string }) => !data.name ? 'Name is required' : null;

    const { user } = renderWithRouter(
      <FormShell
        initial={{ name: '' }}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
        validate={validate}
      >
        {({ data, updateField }) => (
          <input
            data-testid="name-input"
            value={data.name}
            onChange={(e) => updateField('name', e.target.value)}
          />
        )}
      </FormShell>
    );

    const submitButton = screen.getByRole('button', { name: /save/i });
    await user.click(submitButton);

    expect(await screen.findByText('Name is required')).toBeInTheDocument();
  });

  it('calls onSubmit when validation passes', async () => {
    const onSubmit = vi.fn();

    const { user } = renderWithRouter(
      <FormShell
        initial={{ name: 'Test Product' }}
        validate={(data) => (data.name ? null : 'Required')}
        onSubmit={onSubmit}
        onCancel={vi.fn()}
      >
        {({data}) => <div>{data.name}</div>}
      </FormShell>
    );

    const submitButton = screen.getByRole('button', { name: /save/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalledWith({ name: 'Test Product' });
    });
  });

  it('calls onCancel when cancel button is clicked', async () => {
    const onCancel = vi.fn();

    const { user } = renderWithRouter(
      <FormShell
        initial={{ name: '' }}
        onSubmit={vi.fn()}
        onCancel={onCancel}
      >
        {() => <div>Form Content</div>}
      </FormShell>
    );

    const cancelButton = screen.getByRole('button', { name: /cancel/i });
    await user.click(cancelButton);

    expect(onCancel).toHaveBeenCalled();
  });

  it('updates field values when updateField is called', async () => {
    const { user } = renderWithRouter(
      <FormShell
        initial={{ name: '', email: '' }}
        onSubmit={vi.fn()}
        onCancel={vi.fn()}
      >
        {({ data, updateField }) => (
          <>
            <input
              data-testid="name-input"
              value={data.name}
              onChange={(e) => updateField('name', e.target.value)}
            />
            <span data-testid="name-display">{data.name}</span>
          </>
        )}
      </FormShell>        
    );

    const input = screen.getByTestId('name-input');
    await user.type(input, 'New Name');

    expect(screen.getByTestId('name-display')).toHaveTextContent('New Name');
  });
});