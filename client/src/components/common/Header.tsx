import { useState } from 'react';
import { useUser } from '../../contexts/useUser';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { AuthModal } from '../auth/AuthModal';
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/react';
import { 
  ChevronDownIcon, UserCircleIcon, MagnifyingGlassIcon, XMarkIcon, UserPlusIcon, ArrowRightEndOnRectangleIcon,
  Bars3Icon
} from '@heroicons/react/24/outline';
import './Header.css';

interface HeaderProps {
  mobileMenuOpen: boolean;
  setMobileMenuOpen: (open: boolean) => void;
}

export function Header({ mobileMenuOpen, setMobileMenuOpen }: HeaderProps) {
  const { user, logout, roles } = useUser();
  const [authModalOpen, setAuthModalOpen] = useState(false);
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
  const [mobileSearchOpen, setMobileSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const navigate = useNavigate();
  const location = useLocation();

  const isAdminPage = location.pathname.startsWith('/admin');

  const openLogin = () => {
    setAuthMode('login');
    setAuthModalOpen(true);
  };

  const openRegister = () => {
    setAuthMode('register');
    setAuthModalOpen(true);
  };

    const handleSearch = (e: React.FormEvent) => {
      e.preventDefault();
      if (searchQuery.trim()) {
        navigate(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
        setMobileSearchOpen(false);
        setSearchQuery('');
      }
    };

    const dropdownStyle = "w-full text-left px-4 py-2 text-sm text-gray-700 hover:text-gray-900";

  return (
    <>
      <header className="app-header">
        <div className="flex justify-between items-center">
          <div className="flex items-center gap-3">
            {!mobileMenuOpen ? (
              <button
                onClick={() => setMobileMenuOpen(true)}
                className="lg:hidden p-2 text-white hover:bg-white/10 rounded-md transition-colors"
                aria-label="Open Menu"
              ><Bars3Icon className="w-6 h-6" />
              </button>
            ) : (
              <button
                onClick={() => setMobileMenuOpen(false)}
                className="lg:hidden p-2 text-white hover:bg-white/10 rounded-md transition-colors"
                aria-label="Close Menu"
              ><XMarkIcon className="w-6 h-6" />
              </button>
            )}

            <Link to="/" className="header-h1 cursor-pointer">
              Digital Collectibles
            </Link>
          </div>

          <div className="hidden lg:block">
            <Link
              to="/admin/products"
              className="px-4 py-2 text-white/90 hover:text-white hover:bg-white/10 rounded-md transition-colors text-sm font-medium flex items-center gap-2"
            >
              Admin Console
              <span className="text-xs bg-white/20 px-2 py-0.5 rounded-full">Try It</span>
            </Link>
          </div>

          <div className="flex items-center gap-1 md:gap-3">
            {!isAdminPage && (
              <>
                <form onSubmit={handleSearch} className="hidden md:flex gap-2">
                  <input
                    type="text"
                    placeholder="Search Products..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
                  />
                  <button
                    type="submit"
                    aria-label="Search"
                    className="px-4 py-2 bg-[var(--color-primary)] text-white rounded-md hover:opacity-90">
                    <MagnifyingGlassIcon className="h-5 w-5" />
                  </button>
                </form>

                <button
                  onClick={() => setMobileSearchOpen(!mobileSearchOpen)}
                  aria-label={mobileSearchOpen ? "Close search" : "Open search"}
                  className="md:hidden px-3 py-2 text-white hover:bg-white/10 rounded-md transition-colors">
                  {mobileSearchOpen ? (
                    <XMarkIcon className="h-6 w-6" />
                  ) : (
                    <MagnifyingGlassIcon className="h-6 w-6" />
                  )}
                </button>
              </>
            )}

            {user ? (
              <Menu as="div" className="relative">
                <MenuButton className="flex items-center gap-2 px-3 py-2 text-white hover:bg-white/10 rounded-md transition-colors">
                  <UserCircleIcon className="h-6 w-6" />
                  <span className="text-sm font-medium hidden md:inline">
                    {user.username}
                  </span>
                  <ChevronDownIcon className="h-4 w-4" />
                </MenuButton>

                <MenuItems className="absolute right-0 mt-2 w-48 origin-top-right bg-white rounded-md shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none">
                  <div className="px-4 py-3 border-b border-gray-200">
                    <p className="text-sm font-medium text-gray-900">{user.username}</p>

                    <div className="mt-2">
                      <p className="text-xs text-gray-500 font-semibold mb-1">Your Roles:</p>
                      {roles.length > 0 ? (
                        <ul className="text-xs text-gray-600 space-y-1">
                          {roles.map((role) => (<li key={role}>• {role}</li>))}
                        </ul>
                      ) : (
                        <p className="text-xs text-gray-400 italic">No roles assigned</p>
                      )}
                    </div>
                  </div>
                    <div className="py-1">
                      <MenuItem>{({ focus }) => (
                        <button onClick={() => navigate('/admin/products')} 
                          className={`${focus ? 'bg-gray-100' : ''} ${dropdownStyle}`}>
                          Admin Console
                        </button>
                      )}
                      </MenuItem>
                      <MenuItem>{({ focus }) => (
                        <button onClick={logout}
                          className={`${focus ? 'bg-gray-100' : ''} ${dropdownStyle}`}>
                          Logout
                        </button>
                      )}
                      </MenuItem>
                    </div>
                  </MenuItems>
                </Menu>
              ) : (
                <>
                <button
                  onClick={openLogin}
                  className="px-3 md:px-4 py-2 text-sm font-medium text-white hover:bg-white/10 rounded-md transition-colors flex items-center gap-2">
                    <ArrowRightEndOnRectangleIcon className="h-6 w-6" />
                    <span className="hidden md:inline">Login</span>
                  </button>
                <button
                  onClick={openRegister}
                  className="px-3 md:px-4 py-2 text-sm font-medium text-white bg-white/20 rounded-md hover:bg-white/30 transition-colors border border-white/30 flex items-center gap-2">
                    <UserPlusIcon className="h-6 w-6" />
                    <span className="hidden md:inline">Register</span>
                  </button>
                </>
              )}
            </div>
          </div>

        {mobileSearchOpen && (
          <div className="md:hidden mt-4 pl-16">
            <form onSubmit={handleSearch} className="flex gap-2">
              <input
                type="text"
                placeholder="Search Products..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                autoFocus
                className="flex-1 px-4 py-2 border border-white/30 bg-white/90 rounded-md focus:outline-none focus:ring-2 focus:ring-white"
              />
              <button
                type="submit"
                className="px-4 py-2 bg-white text-[var(--color-primary)] rounded-md hover:bg-white/90 font-medium">
                Search
              </button>
            </form>
          </div>
        )}
      </header>
      <AuthModal
        isOpen={authModalOpen}
        onClose={() => setAuthModalOpen(false)}
        initialMode={authMode}
      />
    </>
  );
};
