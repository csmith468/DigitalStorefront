import { describe, it, expect, vi, beforeEach, beforeAll, afterAll } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { FormChipInput, type FormChipInputProps } from '../FormChipInput';

beforeAll(() => {
  // Mocking scrollIntoView since it's not implemented in JSDOM
  Element.prototype.scrollIntoView = vi.fn();
});

afterAll(() => { vi.restoreAllMocks(); });

describe('FormChipInput', () => {
  const defaultProps: FormChipInputProps = {
    id: 'test-tags',
    label: 'Tags',
    value: [],
    suggestions: ['dog', 'cat', 'bunny', 'dolphin'],
    onChange: vi.fn(),
  };

  beforeEach(() => {
    vi.clearAllMocks();
  });

  const setup = (props: Partial<FormChipInputProps> = {}) => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    const mergedProps = { ...defaultProps, onChange, ...props };

    const utils = render(<FormChipInput {...mergedProps} />);
    const input = screen.getByRole('combobox') as HTMLInputElement;

    return { user, onChange, input, ...utils };
  };

  describe('Rendering', () => {
    it('renders with label and input', () => {
      setup();
      expect(screen.getByText('Tags')).toBeInTheDocument();
      expect(screen.getByRole('combobox')).toBeInTheDocument();
    });

    it('renders existing chips', () => {
      setup({value: ['dog', 'cat']});
      expect(screen.getByText('dog')).toBeInTheDocument();
      expect(screen.getByText('cat')).toBeInTheDocument();
    });

    it('renders with placeholder when no existing chips', () => {
      const placeholder = "Type to add...";
      const { input } = setup({ placeholder });

      expect(screen.getByPlaceholderText(placeholder)).toBeInTheDocument();
      expect(input).toHaveAttribute('placeholder', placeholder);
    });

    it('hides placeholder when chips exist', () => {
      const { input } = setup({ placeholder: "Type to add...", value: ['dog'] });
      expect(input).toHaveAttribute('placeholder', '');
    });

    it('shows helper text when provided', () => {
      const helperText = "Select relevant tags";
      setup({ helperText });

      expect(screen.getByText(helperText)).toBeInTheDocument();
    });

    it('shows max items message when limit reached', () => {
      const maxItems = 3;
      setup({ value: ['dog', 'cat', 'bunny'], maxItems });

      expect(screen.getByText(`Maximum ${maxItems} items reached`)).toBeInTheDocument();
    });

    it('shows required indicator when required', () => {
      setup({ required: true });
      expect(screen.getByText('Tags')).toHaveTextContent('*');
    })
  });

  describe('Adding Chips', () => {
    it('adds chip on Enter key press', async () => {
      const { user, input, onChange } = setup();
      await user.type(input, 'dog{Enter}');
      expect(onChange).toHaveBeenCalledWith('test-tags', ['dog']);
    });

    it('adds chip on Tab key press', async () => {
      const { user, input, onChange } = setup();
      await user.type(input, 'cat{Tab}');
      expect(onChange).toHaveBeenCalledWith('test-tags', ['cat']);
    });

    it('adds multiple chips separated by space', async () => {
      const { user, input, onChange } = setup();
      await user.type(input, 'dog cat  bunny{Enter}');
      expect(onChange).toHaveBeenCalledWith('test-tags', ['dog', 'cat', 'bunny']);
    });

    it('converts chips to lowercase', async () => {
      const { user, input, onChange } = setup();
      await user.type(input, 'DoG{Enter}');
      expect(onChange).toHaveBeenCalledWith('test-tags', ['dog']);
    });

    it('trims whitespace from chips', async () => {
      const { user, input, onChange } = setup();
      await user.type(input, '  cat   {Enter}');
      expect(onChange).toHaveBeenCalledWith('test-tags', ['cat']);
    });

    it('does not add empty chips', async () => {
      const { user, input, onChange } = setup();
      await user.type(input, '   {Enter}');
      expect(onChange).not.toHaveBeenCalled();
    });

    it('does not add duplicate chips', async () => {
      const { user, input, onChange } = setup({ value: ['dog'] });
      await user.type(input, 'dog{Enter}');
      expect(onChange).not.toHaveBeenCalled();
    });

    it('does not add chips when maxItems reached', async () => {
      const { user, input, onChange } = setup({ value: ['dog', 'cat'], maxItems: 2 });
      await user.type(input, 'bunny{Enter}');
      expect(onChange).not.toHaveBeenCalled();
    });

    it('clears input after adding chip', async () => {
      const { user, input } = setup();
      await user.type(input, 'dog{Enter}');
      expect(input.value).toBe('');
    });

    it('keeps focus on input after adding chip', async () => {
      const { user, input } = setup();
      await user.type(input, 'dog{Enter}');
      expect(input).toHaveFocus();
    });

    it('handles rapid chip additions', async () => {
      const { user, input, onChange } = setup();
      await user.type(input, 'dog{Enter}cat{Enter}bunny{Enter}');
      expect(onChange).toHaveBeenCalledTimes(3);
    });

    it('allows adding custom chips', async () => {
      const { user, input, onChange } = setup({ suggestions: ['dog', 'cat'] });
      await user.type(input, 'parrot{Enter}');
      expect(onChange).toHaveBeenCalledWith('test-tags', ['parrot']);
    })
  });

  describe('Removing Chips', () => {
    it('removes chip when X button clicked', async () => {
      const { user, onChange } = setup({ value: ['dog', 'cat'] });

      const xButton = screen.getByLabelText("Remove dog");
      await user.click(xButton);

      expect(onChange).toHaveBeenCalledWith('test-tags', ['cat']);
    });

    it('removes last chip on backspace when input is empty', async () => {
      const { user, input, onChange } = setup({ value: ['dog', 'cat'] });

      input.focus();
      await user.keyboard('{Backspace}');

      expect(onChange).toHaveBeenCalledWith('test-tags', ['dog']);
    });

    it('does not remove chip on backspace if input has text', async () => {
      const { user, input, onChange } = setup({ value: ['dog'] });
      await user.type(input, 'cat{Backspace}');
      expect(onChange).not.toHaveBeenCalled();
    });

    it('hides remove button when disabled', () => {
      setup({ value: ['dog'], disabled: true });
      expect(screen.queryByLabelText('Remove dog')).not.toBeInTheDocument();
    });
  });

  describe('Autocomplete Suggestions', () => {
    it('shows filtered suggestions when typing', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        const renderedSuggestions = screen.getAllByRole('option');
        expect(renderedSuggestions).toHaveLength(2);
        expect(renderedSuggestions[0]).toHaveTextContent('dog');
        expect(renderedSuggestions[1]).toHaveTextContent('dolphin');
      });
    });

    it('filters suggestions without case sensitivity', async () => {
      const { user, input } = setup();

      await user.type(input, 'DO');

      await waitFor(() => {
        const renderedSuggestions = screen.getAllByRole('option');
        expect(renderedSuggestions).toHaveLength(2);
        expect(renderedSuggestions[0]).toHaveTextContent('dog');
        expect(renderedSuggestions[1]).toHaveTextContent('dolphin');
      });
    });

    it('hides suggestions that are already selected', async () => {
      const { user, input } = setup({ value: ['dog', 'cat'] });

      await user.type(input, 'do');

      await waitFor(() => {
        const renderedSuggestions = screen.getAllByRole('option');
        expect(renderedSuggestions).toHaveLength(1);
        expect(renderedSuggestions[0]).not.toHaveTextContent('dog');
        expect(renderedSuggestions[0]).toHaveTextContent('dolphin');
      });
    });

    it('limits suggestions to 10 items', async () => {
      const manySuggestions = Array.from({ length: 20 }, (_, i) => `item${i + 1}`);
      const { user, input } = setup({ suggestions: manySuggestions });

      await user.type(input, 'item');

      await waitFor(() => {
        const renderedSuggestions = screen.getAllByRole('option');
        expect(renderedSuggestions.length).toBe(10);
      });
    });

    it('adds chip when suggestion clicked', async () => {
      const { user, input, onChange } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
      });

      await user.click(screen.getByText('dog'));

      expect(onChange).toHaveBeenCalledWith('test-tags', ['dog']);
    });

    it('hides suggestions when none match', async () => {
      const { user, input } = setup();

      await user.type(input, 'xyz');

      await waitFor(() => {
        expect(screen.queryByRole('option')).not.toBeInTheDocument();
      });
    });
    
    it('hides suggestions when clicking outside', async () => {
      const { container } = render(
        <div>
          <FormChipInput {...defaultProps} />
          <button>Outside</button>
        </div>
      );

      const user = userEvent.setup();
      const inputElement = container.querySelector('input[role="combobox"]') as HTMLInputElement;
      await user.type(inputElement, 'bu');

      await waitFor(() => {
        expect(screen.getByText('bunny')).toBeInTheDocument();
      });

      await user.click(screen.getByRole('button', { name: 'Outside' }));

      await waitFor(() => {
        expect(screen.queryByText('bunny')).not.toBeInTheDocument();
      });
    });
  });

  describe('Keyboard Navigation', () => {
    it('highlights first suggestion on ArrowDown', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
      });

      await user.keyboard('{ArrowDown}');

      const firstOption = screen.getByText('dog').closest('button');
      expect(firstOption).toHaveClass('bg-blue-100');
    });

    it('moves highlight down with ArrowDown', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
        expect(screen.getByText('dolphin')).toBeInTheDocument();
      });

      await user.keyboard('{ArrowDown}{ArrowDown}');

      const secondOption = screen.getByText('dolphin');
      expect(secondOption).toHaveAttribute('aria-selected', 'true');
    });

    it('moves highlight up with ArrowUp', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
        expect(screen.getByText('dolphin')).toBeInTheDocument();
      });

      await user.keyboard('{ArrowDown}{ArrowDown}{ArrowUp}');

      const firstOption = screen.getByText('dog');
      expect(firstOption).toHaveAttribute('aria-selected', 'true');
    });

    it('adds highlighted suggestion on Enter', async () => {
      const { user, input, onChange } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
        expect(screen.getByText('dolphin')).toBeInTheDocument();
      });

      await user.keyboard('{ArrowDown}{Enter}');

      expect(onChange).toHaveBeenCalledWith('test-tags', ['dog']);
    });

    it('adds highlighted suggestion on Tab', async () => {
      const { user, input, onChange } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
        expect(screen.getByText('dolphin')).toBeInTheDocument();
      });

      await user.keyboard('{ArrowDown}{Tab}');

      expect(onChange).toHaveBeenCalledWith('test-tags', ['dog']);
    });

    it('closes suggestions on Escape', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
      });

      await user.keyboard('{Escape}');

      await waitFor(() => {
        expect(screen.queryByText('dog')).not.toBeInTheDocument();
      });
    });
  });

  describe('ARIA Attributes', () => {
    it('has correct ARIA role', () => {
      const { input } = setup();
      expect(input).toBeInTheDocument();
    });

    it('has aria-expanded when suggestions are visible', async () => {
      const { user, input } = setup();

      expect(input).toHaveAttribute('aria-expanded', 'false');

      await user.type(input, 'do');

      await waitFor(() => {
        expect(input).toHaveAttribute('aria-expanded', 'true');
      });
    });

    it('has aria-activedescendant for highlighted suggestion', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(screen.getByText('dog')).toBeInTheDocument();
      });

      await user.keyboard('{ArrowDown}');

      const firstOption = screen.getByText('dog').closest('button');
      expect(input).toHaveAttribute('aria-activedescendant', firstOption?.id || '');
    });

    it('has aria-autocomplete attribute', () => {
      const { input } = setup();
      expect(input).toHaveAttribute('aria-autocomplete', 'list');
    });

    it('has aria-controls attribute linking to suggestions list', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        expect(input).toHaveAttribute('aria-controls', 'test-tags-suggestions');
      });
    });

    it('suggestions have correct ARIA roles', async () => {
      const { user, input } = setup();

      await user.type(input, 'do');

      await waitFor(() => {
        const renderedSuggestions = screen.getAllByRole('option');
        expect(renderedSuggestions).toHaveLength(2);
        expect(renderedSuggestions[0]).toHaveTextContent('dog');
        expect(renderedSuggestions[1]).toHaveTextContent('dolphin');
      });
    });
  });

  describe('Disabled State', () => {
    it('disables input when disabled prop is true', () => {
      const { input } = setup({ disabled: true });
      expect(input).toBeDisabled();
    });

    it('does not allow adding chips when disabled', async () => {
      const { user, input, onChange } = setup({ disabled: true });
      await user.type(input, 'dog{Enter}');
      expect(onChange).not.toHaveBeenCalled();
    });

    it('does not allow removing chips when disabled', async () => {
      const { user, onChange } = setup({ value: ['dog'], disabled: true });

      const xButton = screen.queryByLabelText("Remove dog");
      expect(xButton).not.toBeInTheDocument();

      if (xButton) {
        await user.click(xButton);
      }

      expect(onChange).not.toHaveBeenCalled();
    }); 

    it('disables input when max items reached', () => {
      const { input } = setup({ value: ['dog', 'cat'], maxItems: 2 });
      expect(input).toBeDisabled();
    });
  });

  describe('Click to Focus', () => {
    it('focuses input when container clicked', async () => {
      const { user, input } = setup();
      const container = input.closest('.flex');

      expect(input).not.toHaveFocus();
      
      if (container) {
        await user.click(container);
        expect(input).toHaveFocus();
      }
    });
  });
});