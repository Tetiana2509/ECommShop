import React, { useState, useEffect } from 'react';
import { getProducts, createProduct, updateProduct, deleteProduct } from '../services/api';
import './css/AdminProducts.css';

const AdminProducts = ({ user, navigateTo }) => {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showModal, setShowModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: '',
    stockQuantity: '',
    category: '',
    brand: '',
    imageUrl: ''
  });

  useEffect(() => {
    if (!user || !(user.IsAdmin || user.isAdmin)) {
      console.log('User is not admin, redirecting to products');
      navigateTo('products');
      return;
    }
    fetchProducts();
  }, []);

  const fetchProducts = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await getProducts();
      console.log('Products:', data);
      setProducts(data.Products || data.products || []);
    } catch (error) {
      setError('Error loading products');
      console.error('Error fetching products:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleOpenModal = (product = null) => {
    if (product) {
      setEditingProduct(product);
      setFormData({
        name: product.Name || product.name || '',
        description: product.Description || product.description || '',
        price: product.Price || product.price || '',
        stockQuantity: product.StockQuantity || product.stockQuantity || '',
        category: product.Category || product.category || '',
        brand: product.Brand || product.brand || '',
        imageUrl: product.ImageUrl || product.imageUrl || ''
      });
    } else {
      setEditingProduct(null);
      setFormData({
        name: '',
        description: '',
        price: '',
        stockQuantity: '',
        category: '',
        brand: '',
        imageUrl: ''
      });
    }
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setEditingProduct(null);
    setFormData({
      name: '',
      description: '',
      price: '',
      stockQuantity: '',
      category: '',
      brand: '',
      imageUrl: ''
    });
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      const productData = {
        name: formData.name,
        description: formData.description,
        price: parseFloat(formData.price),
        stockQuantity: parseInt(formData.stockQuantity),
        category: formData.category,
        brand: formData.brand,
        imageUrl: formData.imageUrl,
        isActive: true
      };

      if (editingProduct) {
        const productId = editingProduct.Id || editingProduct.id;
        productData.id = productId;
        
        await updateProduct(productId, productData);
        alert('Product updated successfully!');
      } else {
        await createProduct(productData);
        alert('Product created successfully!');
      }
      
      handleCloseModal();
      fetchProducts();
    } catch (error) {
      alert('Error saving product: ' + (error.response?.data || error.message));
      console.error('Error:', error);
    }
  };

  const handleDelete = async (productId) => {
    if (!window.confirm('Are you sure you want to delete this product?')) {
      return;
    }

    try {
      await deleteProduct(productId);
      alert('Product deleted successfully!');
      fetchProducts();
    } catch (error) {
      alert('Error deleting product');
      console.error('Error:', error);
    }
  };

  if (!user || !(user.IsAdmin || user.isAdmin)) {
    return (
      <div className="admin-products-container">
        <h2>Access Denied</h2>
        <p>You must be an admin to access this page.</p>
        <button onClick={() => navigateTo('products')} className="back-btn">
          Back to Products
        </button>
      </div>
    );
  }

  if (loading) {
    return <div className="loading">Loading products...</div>;
  }

  return (
    <div className="admin-products-container">
      <div className="admin-header">
        <h1>Admin - Product Management</h1>
        <div className="header-actions">
          <button onClick={() => handleOpenModal()} className="add-btn">
            + Add New Product
          </button>
          <button onClick={() => navigateTo('products')} className="back-btn">
            Back to Products
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}

      {products.length === 0 ? (
        <div className="empty-state">
          <p>No products found</p>
          <button onClick={() => handleOpenModal()} className="add-btn">
            Add First Product
          </button>
        </div>
      ) : (
        <div className="products-table-container">
          <table className="products-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Image</th>
                <th>Name</th>
                <th>Category</th>
                <th>Brand</th>
                <th>Price</th>
                <th>Stock</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => {
                const productId = product.Id || product.id;
                const name = product.Name || product.name;
                const category = product.Category || product.category;
                const brand = product.Brand || product.brand;
                const price = product.Price || product.price;
                const stock = product.StockQuantity || product.stockQuantity;
                const imageUrl = product.ImageUrl || product.imageUrl;

                return (
                  <tr key={productId}>
                    <td>#{productId}</td>
                    <td>
                      {imageUrl ? (
                        <img src={imageUrl} alt={name} className="product-thumbnail" />
                      ) : (
                        <div className="no-image">No image</div>
                      )}
                    </td>
                    <td>{name}</td>
                    <td>{category}</td>
                    <td>{brand || '-'}</td>
                    <td>${price?.toFixed(2)}</td>
                    <td>
                      <span className={stock > 0 ? 'in-stock' : 'out-of-stock'}>
                        {stock} pcs
                      </span>
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button
                          onClick={() => handleOpenModal(product)}
                          className="edit-btn"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(productId)}
                          className="delete-btn"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}

      {showModal && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>{editingProduct ? 'Edit Product' : 'Add New Product'}</h2>
              <button onClick={handleCloseModal} className="close-btn">&times;</button>
            </div>

            <div className="modal-body">
              <form onSubmit={handleSubmit}>
                <div className="form-row">
                  <div className="form-group">
                    <label>Product Name *</label>
                    <input
                      type="text"
                      name="name"
                      value={formData.name}
                      onChange={handleChange}
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label>Category *</label>
                    <input
                      type="text"
                      name="category"
                      value={formData.category}
                      onChange={handleChange}
                      required
                    />
                  </div>
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label>Brand</label>
                    <input
                      type="text"
                      name="brand"
                      value={formData.brand}
                      onChange={handleChange}
                    />
                  </div>

                  <div className="form-group">
                    <label>Price *</label>
                    <input
                      type="number"
                      name="price"
                      value={formData.price}
                      onChange={handleChange}
                      step="0.01"
                      min="0"
                      required
                    />
                  </div>
                </div>

                <div className="form-row">
                  <div className="form-group">
                    <label>Stock Quantity *</label>
                    <input
                      type="number"
                      name="stockQuantity"
                      value={formData.stockQuantity}
                      onChange={handleChange}
                      min="0"
                      required
                    />
                  </div>

                  <div className="form-group">
                    <label>Image URL</label>
                    <input
                      type="text"
                      name="imageUrl"
                      value={formData.imageUrl}
                      onChange={handleChange}
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Description</label>
                  <textarea
                    name="description"
                    value={formData.description}
                    onChange={handleChange}
                    rows="4"
                  />
                </div>

                <div className="form-actions">
                  <button type="button" onClick={handleCloseModal} className="cancel-btn">
                    Cancel
                  </button>
                  <button type="submit" className="submit-btn">
                    {editingProduct ? 'Update Product' : 'Create Product'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminProducts;
