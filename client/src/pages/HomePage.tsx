import { useNavigate } from 'react-router-dom';
import { useUser } from '../contexts/useUser';
import type { ReactNode } from 'react';
import { PageHeader } from '../components/primitives/PageHeader';

interface FeatureCardProps {
  title: string;
  description: string;
  features: string;
  children?: ReactNode;
}

export function HomePage() {
  const navigate = useNavigate();
  const { user, openAuthModal } = useUser();

  const FeatureCard: React.FC<FeatureCardProps> = ({ title, description, features, children }) => {
    return (
      <div className="bg-white rounded-lg shadow-md p-4 border border-gray-200">
        <h2 className="text-xl font-semibold text-text-primary mb-2">{title}</h2>
        <p className="text-text-secondary mb-2 text-sm">{description}</p>
        <div className="text-xs text-gray-500">Features: {features}</div>
        {children && <div className="mt-3">{children}</div>}
      </div>
    );
  };

  const adminButtonStyle = "px-4 py-2 bg-[var(--color-primary)] text-white rounded-md hover:opacity-90 font-medium text-sm";

  return (
    <div className="container mx-auto px-4 py-6 max-w-4xl">
      <PageHeader
        title="Welcome to Digital Collectibles"
        subtitle="Inspired by virtual pet game economies - demonstrating admin console architecture, complex product management, and full-stack integration beyond basic e-commerce"
      />

      <div className="grid md:grid-cols-2 gap-4 mb-6">
        <FeatureCard
          title='Browse Products'
          description='Explore our product catalog organized by categories. Click any category in the sidebar to start browsing.'
          features='Server-side pagination, filtering, dynamic routing'
        />
        <FeatureCard
          title='Search Products'
          description='Use the search bar in the header to find products by name, category, or subcategory with relevance-based sorting.'
          features='Smart search, SQL relevance ranking, React Query caching'
        />
      </div>

      <div className="md:col-span-2">
        <FeatureCard
          title='Admin Console'
          description={user
            ? "Access the admin console to create and manage products with image uploads, category assignments, and more. You can create up to 3 products with 5 images each (5MB max per image). Demo products are protected from editing."
            : "Register for a free account to access the admin console where you can create products, upload images, and manage inventory. No email required! Demo accounts can create up to 3 products with 5 images each to protect server resources."}
          features='CRUD operations, form validation, protected routes, image management'
        >
        {user ? (
          <button
            onClick={() => navigate('/admin/products')}
            className={adminButtonStyle}
          >Go to Admin Console →</button>
        ) : (
          <button
            onClick={() => openAuthModal('register')}
            className={adminButtonStyle}
          >Register to Try Admin Features</button>
        )}
        </FeatureCard>
      </div>

      <div className="bg-gradient-to-r from-blue-50 to-purple-50 rounded-lg p-4 border border-blue-200 mb-4 mt-5">
        <h3 className="text-base font-semibold text-text-primary mb-2">
          Built With
        </h3>
        <div className="grid grid-cols-2 md:grid-cols-4 gap-3 text-xs">
          <div>
            <div className="font-medium text-gray-700">Frontend</div>
            <div className="text-gray-600">React 18, TypeScript, Tailwind</div>
          </div>
          <div>
            <div className="font-medium text-gray-700">Backend</div>
            <div className="text-gray-600">.NET Core 8, C#, Dapper, SQL</div>
          </div>
          <div>
            <div className="font-medium text-gray-700">State</div>
            <div className="text-gray-600">React Query, Context API</div>
          </div>
          <div>
            <div className="font-medium text-gray-700">Cloud</div>
            <div className="text-gray-600">Azure</div>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-md p-5 border border-gray-200">
        <div className="text-center">
          <h3 className="text-xl font-semibold text-text-primary mb-2">Connect With Me</h3>
          <p className="text-text-secondary mb-4 text-sm">
            This project is part of my portfolio. Check out more of my work or get in touch!
          </p>
          <div className="flex justify-center gap-4">
            <a
              href="https://github.com/csmith468"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-2 px-5 py-2 bg-gray-800 text-white rounded-md hover:bg-gray-700 transition-colors font-medium text-sm"
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path fillRule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 
                  0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032
                    1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 
                  1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 
                  2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 
                  2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" clipRule="evenodd" />
              </svg>
              GitHub
            </a>

            <a
              href="https://www.linkedin.com/in/chapin-smith/"
              target="_blank"
              rel="noopener noreferrer"
              className="flex items-center gap-2 px-5 py-2 bg-[#0077B5] text-white rounded-md hover:bg-[#006399] transition-colors font-medium text-sm"
            >
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
                <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 
                  2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 
                  0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 
                  13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 
                  .774 23.2 0 22.222 0h.003z"/>
              </svg>
              LinkedIn
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}