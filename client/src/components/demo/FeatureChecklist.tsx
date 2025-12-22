import { useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { CheckIcon, ChevronDownIcon } from '@heroicons/react/24/outline';
import { features, STORAGE_KEY, type FeatureItem } from './featureChecklistData';
import { useLocalStorage } from '../../hooks/utilities/useLocalStorage';


export function FeatureChecklist() {
  // Don't render during Playwright tests
  if (import.meta.env.VITE_PLAYWRIGHT === 'true')
    return null;

  const navigate = useNavigate();
  const [isExpanded, setIsExpanded] = useState(true);
  const [completedItems, setCompletedItems] = useLocalStorage<string[]>(STORAGE_KEY, []);
  const [justCompleted, setJustCompleted] = useState(false);
  const location = useLocation();

  const addToCompletedItems = (id: string) => {
    setCompletedItems(prev => {
      if (prev.includes(id)) return prev;
      setJustCompleted(true);
      setTimeout(() => setJustCompleted(false), 1000);
      return [...prev, id];
    });
  };

  useEffect(() => {
    const path = location.pathname;
    const params = new URLSearchParams(location.search);

    features.forEach((feature) => {
      if (feature.completionPaths?.includes(path)) {
        addToCompletedItems(feature.id);
      }
    });

    if (params.has('orderSuccess'))
      addToCompletedItems('payment');
  }, [location])

  const handleItemClick = (feature: FeatureItem) => {
    if (feature.externalUrl) {
      addToCompletedItems(feature.id);
      window.open(feature.externalUrl, '_blank', 'noopener,noreferrer');
    } else if (feature.path) {
      navigate(feature.path);
    }
  };

  return (
    <div className="fixed bottom-4 right-4 z-50">
      {!isExpanded && (
        <button
          onClick={() => setIsExpanded(true)}
          className={`flex items-center gap-2 px-4 py-3 bg-white rounded-lg shadow-lg border-2 border-[var(--color-border)] hover:shadow-xl 
                    ${ justCompleted ? 'animate-flash-purple' : '' }`}
        >
          <svg className="w-5 h-5 text-[var(--color-primary)]" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
          </svg>
          <span className="font-medium text-gray-700">Features to Explore</span>
        </button>
      )}

      {isExpanded && (
        <div className="w-80 bg-white rounded-lg border-2 border-[var(--color-border)] overflow-hidden" style={{ boxShadow: 'var(--shadow-xl)' }}>
          <div className="flex items-center justify-between px-4 py-3 bg-[var(--color-hover-bg)] border-b border-[var(--color-border)]">
            <div className="flex items-center gap-2">
              <svg className="w-5 h-5 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
              </svg>
              <span className="font-semibold text-gray-800">Technical Highlights</span>
            </div>
            <button
              onClick={() => setIsExpanded(false)}
              className="p-1 hover:bg-gray-200 rounded transition-colors"
              aria-label="Minimize checklist"
            >
              <ChevronDownIcon className="w-4 h-4 text-gray-500" />
            </button>
          </div>

          <div className="divide-y divide-gray-100">
            {features.map((item) => (
              <button
                key={item.id}
                onClick={() => handleItemClick(item)}
                className="w-full px-4 py-3 flex items-start gap-3 hover:bg-gray-50 transition-colors text-left"
              >
                {/* Checkbox */}
                <div className={`mt-0.5 flex-shrink-0 w-5 h-5 rounded-full border-2 flex items-center justify-center transition-all duration-300 ${
                  completedItems.includes(item.id)
                    ? 'bg-green-500 border-green-500 scale-110'
                    : 'border-gray-300 scale-100'
                }`}>
                  {completedItems.includes(item.id) && (
                    <CheckIcon className="w-3 h-3 text-white" strokeWidth={3} />
                  )}
                </div>

                <div className="flex-1 min-w-0">
                  <div className="font-medium text-gray-700">
                    {item.title}
                  </div>
                  <p className="text-xs text-gray-500 mt-0.5">{item.techNote}</p>
                </div>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}