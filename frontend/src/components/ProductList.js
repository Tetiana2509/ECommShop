import React, { useState, useEffect } from 'react';
import { getProducts, getCategories } from '../services/api';
import ProductCard from '../components/ProductCard';
import './css/ProductList.css';

const ProductList = ({ user, navigateTo }) => {
  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [filters, setFilters] = useState({
    category: '',
    search: '',
    page: 1,
    pageSize: 12,
  });
  const [pagination, setPagination] = useState({
    totalCount: 0,
    totalPages: 0,
  });

  useEffect(() => {
    fetchCategories();
  }, []);

  useEffect(() => {
    fetchProducts();
  }, [filters]);

  const fetchCategories = async () => {
    try {
      const data = await getCategories();
      setCategories(data);
    } catch (error) {
      console.error('Error fetching categories:', error);
    }
  };

  const fetchProducts = async () => {
    try {
      setLoading(true);
      const params = {};
      if (filters.category) params.category = filters.category;
      if (filters.search) params.search = filters.search;
      params.page = filters.page;
      params.pageSize = filters.pageSize;

      const data = await getProducts(params);
      setProducts(data.products || []);
      setPagination({
        totalCount: data.totalCount || 0,
        totalPages: data.totalPages || 0,
      });
    } catch (error) {
      console.error('Error fetching products:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleCategoryChange = (category) => {
    setFilters({ ...filters, category, page: 1 });
  };

  const handleSearchChange = (e) => {
    setFilters({ ...filters, search: e.target.value, page: 1 });
  };

  const handlePageChange = (page) => {
    setFilters({ ...filters, page });
  };

  return (
    <div className="product-list-container">
      <div className="filters-section">
        <div className="search-bar">
          <input
            type="text"
            placeholder="Produkte suchen..."
            value={filters.search}
            onChange={handleSearchChange}
          />
        </div>
        
        <div className="category-filters">
          <button
            className={!filters.category ? 'active' : ''}
            onClick={() => handleCategoryChange('')}
          >
            Alle Kategorien
          </button>
          {categories.map((category) => (
            <button
              key={category}
              className={filters.category === category ? 'active' : ''}
              onClick={() => handleCategoryChange(category)}
            >
              {category}
            </button>
          ))}
        </div>
      </div>

      {loading ? (
        <div className="loading">Laden...</div>
      ) : (
        <>
          <div className="products-grid">
            {products.length === 0 ? (
              <div className="no-products">Keine Produkte gefunden</div>
            ) : (
              products.map((product) => (
                <ProductCard key={product.id} product={product} user={user} navigateTo={navigateTo} />
              ))
            )}
          </div>

          {pagination.totalPages > 1 && (
            <div className="pagination">
              <button
                disabled={filters.page === 1}
                onClick={() => handlePageChange(filters.page - 1)}
              >
                Zurück
              </button>
              <span>
                Seite {filters.page} von {pagination.totalPages}
              </span>
              <button
                disabled={filters.page === pagination.totalPages}
                onClick={() => handlePageChange(filters.page + 1)}
              >
                Weiter
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default ProductList;
