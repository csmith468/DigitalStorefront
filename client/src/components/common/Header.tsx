import { useState } from 'react';
import { useUser } from '../../contexts/UserContext';
import { AuthModal } from '../auth/AuthModal';
import { Menu, MenuButton, MenuItem, MenuItems } from '@headlessui/react';
import { ChevronDownIcon, UserCircleIcon } from '@heroicons/react/24/outline';
import './Header.css';

export const Header: React.FC = () => {
  const { user, logout } = useUser();
  const [authModalOpen, setAuthModalOpen] = useState(false);
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');

  const openLogin = () => {
    setAuthMode('login');
    setAuthModalOpen(true);
  };

  const openRegister = () => {
    setAuthMode('register');
    setAuthModalOpen(true);
  };

  return (
    <>
      <header className="app-header">
        <div className="flex justify-between items-center pl-16 lg:pl-0">
          <h1 className="header-h1">
            Digital Collectibles Marketplace
          </h1>
          <div className="flex items-center gap-3">
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
                <button onClick={openLogin} className="px-4 py-2 text-sm font-medium text-white hover:bg-white/10 rounded-md transition-colors">
                  Login
                </button>
                <button onClick={openRegister} className="px-4 py-2 text-sm font-medium text-white bg-white/20 rounded-md hover:bg-white/30 transition-colors border border-white/30">
                  Register
                </button>
              </>
            )}
          </div>
        </div>
      </header>
      <AuthModal isOpen={authModalOpen} onClose={() => setAuthModalOpen(false)} initialMode={authMode} />
    </>
  );
};