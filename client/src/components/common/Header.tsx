import { useState } from 'react';
  import { useUser } from '../../hooks/useUser';
  import { useNavigate } from 'react-router-dom';
  import { AuthModal } from '../auth/AuthModal';
  import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/react';
  import { ChevronDownIcon, UserCircleIcon, MagnifyingGlassIcon, XMarkIcon, ArrowRightOnRectangleIcon, UserPlusIcon } from '@heroicons/react/24/outline';
  import './Header.css';

  export const Header: React.FC = () => {
    const { user, logout } = useUser();
    const [authModalOpen, setAuthModalOpen] = useState(false);
    const [authMode, setAuthMode] = useState<'login' | 'register'>('login');
    const [mobileSearchOpen, setMobileSearchOpen] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');
    const navigate = useNavigate();

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

    return (
      <>
        <header className="app-header">
          <div className="flex justify-between items-center pl-16 lg:pl-0">
            <h1 className="header-h1">
              Digital Collectibles
            </h1>
            <div className="flex items-center gap-1 md:gap-3">
              <form onSubmit={handleSearch} className="hidden md:flex gap-2">
                <input type="text" placeholder='Search Products...' value={searchQuery} 
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-[var(--color-primary)]"
                />
                <button type="submit" aria-label="Search" className="px-4 py-2 bg-[var(--color-primary)] text-white rounded-md hover:opacity-90">
                  <MagnifyingGlassIcon className="h-5 w-5" />
                </button>
              </form>

              <button onClick={() => setMobileSearchOpen(!mobileSearchOpen)}
                aria-label={mobileSearchOpen ? "Close search" : "Open search"}
                className="md:hidden px-3 py-2 text-white hover:bg-white/10 rounded-md transition-colors"
              >
                {mobileSearchOpen ? (
                  <XMarkIcon className="h-6 w-6" />
                ) : (
                  <MagnifyingGlassIcon className="h-6 w-6" />
                )}
              </button>

              {user ? (
                <Menu as="div" className="relative">
                  <MenuButton className="flex items-center gap-2 px-3 py-2 text-white hover:bg-white/10 rounded-md transition-colors">
                    <UserCircleIcon className="h-6 w-6" />
                    <span className="text-sm font-medium hidden md:inline">{user.username}</span>
                    <ChevronDownIcon className="h-4 w-4" />
                  </MenuButton>

                <MenuItems className="absolute right-0 mt-2 w-48 origin-top-right bg-white rounded-md shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none">
                    <div className="py-1">
                      <MenuItem>{({ focus }) => (
                        <button disabled className={`${focus ? 'bg-gray-100' : ''} w-full text-left px-4 py-2 text-sm text-gray-400 cursor-not-allowed`}>
                            Admin Console (Coming Soon)
                          </button>
                        )}
                      </MenuItem>
                      <MenuItem>{({ focus }) => (
                        <button onClick={logout} className={`${focus ? 'bg-gray-100' : ''} w-full text-left px-4 py-2 text-sm text-gray-700 hover:text-gray-900`}>
                            Logout
                          </button>
                        )}
                      </MenuItem>
                    </div>
                  </MenuItems>
                </Menu>
              ) : (
                <>
                  <button onClick={openLogin} className="px-3 md:px-4 py-2 text-sm font-medium text-white hover:bg-white/10 rounded-md transition-colors flex items-center gap-2">
                    <ArrowRightOnRectangleIcon className="h-6 w-6" />
                    <span className="hidden md:inline">Login</span>
                  </button>
                  <button onClick={openRegister} className="px-3 md:px-4 py-2 text-sm font-medium text-white bg-white/20 rounded-md hover:bg-white/30 transition-colors border border-white/30 flex items-center gap-2">
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
                <input type="text" placeholder="Search Products..." value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)} autoFocus
                  className="flex-1 px-4 py-2 border border-white/30 bg-white/90 rounded-md focus:outline-none focus:ring-2 focus:ring-white"
                />
                <button type="submit" className="px-4 py-2 bg-white text-[var(--color-primary)] rounded-md hover:bg-white/90 font-medium">
                  Search
                </button>
              </form>
            </div>
          )}
        </header>
      <AuthModal isOpen={authModalOpen} onClose={() => setAuthModalOpen(false)} initialMode={authMode} />
      </>
    );
  };