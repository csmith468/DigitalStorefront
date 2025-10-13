import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { categoryService } from "../services/categories";
import { ChevronDownIcon } from '@heroicons/react/24/outline';
import './CategorySidebar.css'
import type { Category } from '../types/category';

const CategorySidebar = () => {
  const [categories, setCategories] = useState<Category[]>([]); // set categories to []
  const [expanded, setExpanded] = useState(new Set()); // set expanded to empty set (none are expanded)
  const [loading, setLoading] = useState<boolean>(true); // set loading to true initially

  // same as mounted/created, ending [] means run once
  useEffect(() => {
    const loadCategories = async () => {
      try {
        const categories = await categoryService.getMenu();
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
    const newExpanded = new Set(expanded);
    if (newExpanded.has(categoryId))
      newExpanded.delete(categoryId);
    else
      newExpanded.add(categoryId);
    setExpanded(newExpanded);
  }

  if (loading) {
    return (
      <aside className="category-sidebar--loading">
        <div className="skeleton skeleton--title"></div>
        <div className="skeleton skeleton--item"></div>
      </aside>
    )
  }

  // template
  return (
    <aside className="category-sidebar">
      <div className="category-sidebar__header">
        <h2 className="category-sidebar__title">Browse Categories</h2>
      </div>
      <nav className="category-sidebar__nav">
        {/* similar to v-for */}
        { categories.map(category => {
            const isExpanded = expanded.has(category.categoryId);
            return (
              <div key={category.categoryId} className="category-item">
                <button onClick={() => toggleCategory(category.categoryId)} className="category-item__button">
                  <span className="category-item__name">{ category.name }</span>
                  <ChevronDownIcon className={`category-item__icon ${ isExpanded ? 'category-item__icon--expanded' : '' }`}/>
                </button>
                <div className={`subcategory-list ${isExpanded ? 'subcategory-list--expanded' : 'subcategory-list--collapsed'}`}>
                  <div className="subcategory-list__inner">
                    { category.subcategories?.map(sub => (
                      <Link key={sub.subcategoryId} to={`/subcategory/${sub.slug}`} className="subcategory-link">{sub.name}</Link>
                    ))}
                  </div>
                </div>
              </div>
            );
          })
        }
      </nav>
    </aside>
  );
}

export default CategorySidebar;