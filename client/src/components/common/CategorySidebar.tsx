import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { getCategories } from "../../services/categories";
import { ChevronDownIcon, Bars3Icon, XMarkIcon } from '@heroicons/react/24/outline';
import './CategorySidebar.css'
import type { Category } from '../../types/category';
import { VIEW_ALL_SUBCATEGORY } from '../../types/subcategory';

const CategorySidebar = () => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [expanded, setExpanded] = useState<Set<number>>(new Set());
  const [loading, setLoading] = useState<boolean>(true);
  const [mobileMenuOpen, setMobileMenuOpen] = useState<boolean>(false);

  useEffect(() => {
    const loadCategories = async () => {
      try {
        const categories = await getCategories();
        categories.forEach(cat => {
          cat.subcategories = [VIEW_ALL_SUBCATEGORY, ...(cat.subcategories || [])];
        });
        setCategories(categories);
      } catch (error) {
        console.error("Error fetching categories:", error);
      } finally {
        setLoading(false);
      }
    };
    loadCategories();
  }, []);

  const toggleCategory = (categoryId: number) => {
    setExpanded(prev => {
      const next = new Set(prev);
      if (next.has(categoryId)) next.delete(categoryId);
      else next.add(categoryId);
      return next;
    });
  };

  if (loading) {
    return (
      <aside className="category-sidebar--loading">
        <div className="skeleton skeleton--title"></div>
        <div className="skeleton skeleton--item"></div>
        <div className="skeleton skeleton--item"></div>
        <div className="skeleton skeleton--item"></div>
        <div className="skeleton skeleton--item"></div>
      </aside>
    );
  }

  return (
    <>
      {!mobileMenuOpen && (
        <button
          onClick={() => setMobileMenuOpen(true)}
          className="mobile-menu-button lg:hidden"
          aria-label="Open Menu">
          <Bars3Icon className="w-6 h-6 text-text-primary" />
        </button>
      )}

      {mobileMenuOpen && (
        <div
          className="fixed inset-0 bg-overlay-dark z-30 lg:hidden"
          onClick={() => setMobileMenuOpen(false)}
        />
      )}

      <aside className={`category-sidebar ${
        mobileMenuOpen ? 'category-sidebar--open' : 'category-sidebar--closed'
      }`}>
        <div className="category-sidebar__header">
          <div className="flex items-center justify-between">
            <h2 className="font-semibold text-primary text-sm uppercase tracking-wider">
              Browse Categories
            </h2>
            <button
              onClick={() => setMobileMenuOpen(false)}
              className="lg:hidden p-1 hover:bg-hover-bg rounded"
              aria-label="Close Menu">
              <XMarkIcon className="w-5 h-5 text-text-primary" />
            </button>
          </div>
        </div>
        
        <nav className="category-sidebar__nav">
          {categories.map(category => {
            const isExpanded = expanded.has(category.categoryId);
            return (
              <div key={category.categoryId} className="mb-1">
                <button 
                  onClick={() => toggleCategory(category.categoryId)} 
                  className="category-item__button">
                  <span className="font-medium text-text-primary">{category.name}</span>
                  <ChevronDownIcon 
                    className={`w-4 h-4 text-text-secondary transition-transform duration-200 flex-shrink-0 ${
                      isExpanded ? 'rotate-180' : ''
                    }`}
                  />
                </button>
                <div 
                  className={`subcategory-list ${
                    isExpanded ? 'subcategory-list--expanded' : 'subcategory-list--collapsed'
                  }`}
                >
                  <div className="subcategory-list__inner">
                    {category.subcategories?.map(sub => (
                      <Link
                        key={sub.subcategoryId}
                        to={`/products/${category.slug}/${sub.slug}`}
                        className="subcategory-link"
                        onClick={() => setMobileMenuOpen(false)}>
                        {sub.name}
                      </Link>
                    ))}
                  </div>
                </div>
              </div>
            );
          })}
        </nav>

        <div className="category-sidebar__footer">
          <a href="https://github.com/csmith468"
            target="_blank"
            rel="noopener noreferrer"
            className="text-text-secondary hover:text-primary transition-colors flex items-center justify-center"
            aria-label="GitHub"
          >
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path fillRule="evenodd" d="M12 2C6.477 2 2 6.484 2 12.017c0 4.425 2.865 8.18 6.839 9.504.5.092.682-.217.682-.483 0-.237-.008-.868-.013-1.703-2.782.605-3.369-1.343-3.369-1.343-.454-1.158-1.11-1.466-1.11-1.466-.908-.62.069-.608.069-.608 1.003.07 1.531 1.032 1.531 1.032.892 1.53 2.341 1.088 2.91.832.092-.647.35-1.088.636-1.338-2.22-.253-4.555-1.113-4.555-4.951 0-1.093.39-1.988 1.029-2.688-.103-.253-.446-1.272.098-2.65 0 0 .84-.27 2.75 1.026A9.564 9.564 0 0112 6.844c.85.004 1.705.115 2.504.337 1.909-1.296 2.747-1.027 2.747-1.027.546 1.379.202 2.398.1 2.651.64.7 1.028 1.595 1.028 2.688 0 3.848-2.339 4.695-4.566 4.943.359.309.678.92.678 1.855 0 1.338-.012 2.419-.012 2.747 0 .268.18.58.688.482A10.019 10.019 0 0022 12.017C22 6.484 17.522 2 12 2z" clipRule="evenodd" />
            </svg>
          </a>
          <a href="https://www.linkedin.com/in/chapin-smith/"
            target="_blank"
            rel="noopener noreferrer"
            className="text-text-secondary hover:text-primary transition-colors flex items-center justify-center"
            aria-label="LinkedIn"
          >
            <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 24 24" aria-hidden="true">
              <path d="M20.447 20.452h-3.554v-5.569c0-1.328-.027-3.037-1.852-3.037-1.853 0-2.136 1.445-2.136 2.939v5.667H9.351V9h3.414v1.561h.046c.477-.9 1.637-1.85 3.37-1.85 3.601 0 4.267 2.37 4.267 5.455v6.286zM5.337 7.433c-1.144 0-2.063-.926-2.063-2.065 0-1.138.92-2.063 2.063-2.063 1.14 0 2.064.925 2.064 2.063 0 1.139-.925 2.065-2.064 2.065zm1.782 13.019H3.555V9h3.564v11.452zM22.225 0H1.771C.792 0 0 .774 0 1.729v20.542C0 23.227.792 24 1.771 24h20.451C23.2 24 24 23.227 24 22.271V1.729C24 .774 23.2 0 22.222 0h.003z"/>
            </svg>
          </a>
        </div>
      </aside>
    </>
  );
};

export default CategorySidebar;