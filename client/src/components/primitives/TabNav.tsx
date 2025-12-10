interface Tab {
  id: string;
  label: string;
  badge?: number;
}

interface TabNavProps {
  tabs: Tab[];
  activeTab: string;
  onTabChange: (tabId: string) => void;
}

export function TabNav({ tabs, activeTab, onTabChange }: TabNavProps) {
  return (
    <nav className="flex gap-1 border-b-2 border-gray-200 mb-6">
      {tabs.map((tab) => (
        <button
          key={tab.id}
          onClick={() => onTabChange(tab.id)}
          className={`px-6 py-3 font-medium text-sm rounded-t-lg transition-all flex items-center gap-2 relative ${
            activeTab === tab.id
              ? 'bg-gradient-to-br from-[var(--color-primary)] to-[var(--color-accent)] text-white shadow-lg -mb-0.5 border-b-2 border-transparent'
              : 'bg-gray-200 text-gray-700 hover:bg-gray-300 hover:text-gray-900 border border-gray-300 border-b-0'
          }`}
        >
          <span>{tab.label}</span>
          {tab.badge !== undefined && (
            <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${
              activeTab === tab.id
                ? 'bg-white/20 text-white border border-white/30'
                : 'bg-gray-400 text-gray-800'
            }`}>
              {tab.badge}
            </span>
          )}
        </button>
      ))}
    </nav>
  );
}
