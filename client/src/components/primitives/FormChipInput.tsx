import { useState, useRef, useEffect } from 'react';
import { XMarkIcon } from '@heroicons/react/20/solid';
import { FormLabel } from './FormLabel';
import { formStyles } from './primitive-constants';

export interface FormChipInputProps {
  id: string;
  label: string;
  required?: boolean;
  value: string[];
  onChange: (field: string, value: string[]) => void;
  suggestions?: string[];
  placeholder?: string;
  disabled?: boolean;
  maxItems?: number;
  helperText?: string;
}

export function FormChipInput({
  id,
  label,
  required = false,
  value,
  onChange,
  suggestions = [],
  placeholder = "Type to add items...",
  disabled = false,
  maxItems = 10,
  helperText,
}: FormChipInputProps) {
  const [inputValue, setInputValue] = useState('');
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [highlightedIndex, setHighlightedIndex] = useState(-1);
  const inputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);
  const suggestionRefs = useRef<(HTMLButtonElement | null)[]>([]);

  const filteredSuggestions = inputValue.trim()
    ? suggestions.filter(item => item.toLowerCase().includes(inputValue.toLowerCase()) && !value.includes(item)).slice(0, 10)
    : [];

  useEffect(() => {
    setHighlightedIndex(-1);
  }, [filteredSuggestions.length, inputValue]);

  useEffect(() => {
    if (highlightedIndex >= 0 && suggestionRefs.current[highlightedIndex]) {
      suggestionRefs.current[highlightedIndex]?.scrollIntoView({
        block: 'nearest',
        behavior: 'smooth'
      });
    }
  }, [highlightedIndex]);

  const addItems = (input: string) => {
    const newItems = input
      .split(' ')
      .map(item => item.trim().toLowerCase())
      .filter(item => item && !value.includes(item));

    if (newItems.length > 0) {
      const updatedItems = [...value, ...newItems].slice(0, maxItems);
      onChange(id, updatedItems);
      setInputValue('');
      setShowSuggestions(false);
      setHighlightedIndex(-1);
    }
  };

  const removeItem = (itemToRemove: string) => {
    onChange(id, value.filter(item => item !== itemToRemove));
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'ArrowDown') {
      e.preventDefault();
      setShowSuggestions(true);
      setHighlightedIndex(prev =>
        prev < filteredSuggestions.length - 1 ? prev + 1 : prev
      );
    }
    else if (e.key === 'ArrowUp') {
      e.preventDefault();
      setHighlightedIndex(prev => prev > 0 ? prev - 1 : -1);
    }
    else if (e.key === 'Enter' || e.key === 'Tab') {
      e.preventDefault();
      if (highlightedIndex >= 0 && filteredSuggestions[highlightedIndex]) {
        addItems(filteredSuggestions[highlightedIndex]);
      } else if (inputValue.trim()) {
        addItems(inputValue);
      }
    }
    else if (e.key === 'Escape') {
      setShowSuggestions(false);
      setHighlightedIndex(-1);
    }
    else if (e.key === 'Backspace' && !inputValue && value.length > 0) {
      removeItem(value[value.length - 1]);
    }
  };

  const selectSuggestion = (item: string) => {
    addItems(item);
    inputRef.current?.focus();
  };

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setShowSuggestions(false);
        setHighlightedIndex(-1);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div>
      <FormLabel htmlFor={id} label={label} required={required} />

      <div ref={containerRef} className="relative">
        <div 
          className={`${formStyles.input} flex flex-wrap gap-2 min-h-[42px] cursor-text`}
          onClick={() => inputRef.current?.focus()}
        >
          {value.map(item => (
            <span
              key={item}
              className="inline-flex items-center gap-1 px-2 py-1 bg-blue-100 text-blue-800 text-sm rounded-md whitespace-nowrap"
            >
              {item}
              {!disabled && 
                <button
                  type="button"
                  onClick={() => removeItem(item)}
                  className="hover:text-blue-600 focus:outline-none"
                  aria-label={`Remove ${item}`}
                >
                  <XMarkIcon className="w-4 h-4" />
                </button>
              }
            </span>
          ))}

          <input
            ref={inputRef}
            id={id}
            type="text"
            value={inputValue}
            onChange={(e) => {
              setInputValue(e.target.value);
              setShowSuggestions(true);
            }}
            onKeyDown={handleKeyDown}
            onFocus={() => setShowSuggestions(true)}
            placeholder={value.length === 0 ? placeholder : ''}
            className="flex-1 min-w-[120px] outline-none bg-transparent"
            disabled={disabled || value.length >= maxItems}
            role="combobox"
            aria-expanded={showSuggestions && filteredSuggestions.length > 0}
            aria-autocomplete="list"
            aria-controls={`${id}-suggestions`}
            aria-activedescendant={highlightedIndex >= 0 ? `${id}-option-${highlightedIndex}` : undefined}
          />
        </div>

        {showSuggestions && filteredSuggestions.length > 0 && (
          <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto">
            {filteredSuggestions.map((item, index) => (
              <button
                key={item}
                id={`${id}-option-${index}`}
                ref={el => { suggestionRefs.current[index] = el; }}
                type="button"
                onClick={() => selectSuggestion(item)}
                role="option"
                aria-selected={highlightedIndex === index}
                className={`w-full text-left px-4 py-2 text-sm focus:outline-none ${
                  highlightedIndex === index
                    ? 'bg-blue-100 text-blue-900'
                    : 'hover:bg-gray-100'
                }`}
              >
                {item}
              </button>
            ))}
          </div>
        )}
      </div>

      {helperText && <p className={formStyles.helperText}>{helperText}</p>}

      {value.length >= maxItems && (
        <p className={formStyles.helperText}>
          Maximum {maxItems} items reached
        </p>
      )}
    </div>
  );
}