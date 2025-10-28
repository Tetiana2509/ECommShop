import React, { useState, useEffect } from 'react';
import { getProduct, addToCart as addToCartAPI } from '../services/api';
import './css/ProductDetail.css';

const ProductDetail = ({ productId, user, navigateTo }) => {
  const [product, setProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [adding, setAdding] = useState(false);

  useEffect(() => {
    fetchProduct();
  }, [productId]);

  const fetchProduct = async () => {
    try {
      setLoading(true);
      const data = await getProduct(productId);
      setProduct(data);
    } catch (error) {
      console.error('Error fetching product:', error);
      alert('Produkt nicht gefunden');
      navigateTo('products');
    } finally {
      setLoading(false);
    }
  };

  const handleAddToCart = async () => {
    if (!user) {
      alert('Bitte melden Sie sich an');
      navigateTo('login');
      return;
    }

    try {
      setAdding(true);
      await addToCartAPI(product.id, quantity);
      alert('Produkt zum Warenkorb hinzugefügt!');
    } catch (error) {
      alert(error.response?.data || 'Fehler beim Hinzufügen zum Warenkorb');
    } finally {
      setAdding(false);
    }
  };

  if (loading) {
    return <div className="loading">Laden...</div>;
  }

  if (!product) {
    return <div className="error">Produkt nicht gefunden</div>;
  }

  return (
    <div className="product-detail-container">
      <button onClick={() => navigateTo('products')} className="back-btn">
        ← Zurück
      </button>

      <div className="product-detail">
        <div className="product-detail-image">
          {product.imageUrl ? (
            <img src={product.imageUrl} alt={product.name} />
          ) : (
            <div className="no-image-large">Kein Bild</div>
          )}
        </div>

        <div className="product-detail-info">
          <h1>{product.name}</h1>
          
          <div className="product-meta">
            <span className="category">{product.category}</span>
            {product.brand && <span className="brand">{product.brand}</span>}
          </div>

          <div className="product-price-large">
            ${product.price.toFixed(2)}
          </div>

          <div className="product-stock">
            {product.stockQuantity > 0 ? (
              <span className="in-stock">
                Auf Lager: {product.stockQuantity} Stk.
              </span>
            ) : (
              <span className="out-of-stock">Nicht auf Lager</span>
            )}
          </div>

          {product.description && (
            <div className="product-description">
              <h3>Beschreibung</h3>
              <p>{product.description}</p>
            </div>
          )}

          {product.stockQuantity > 0 && (
            <div className="add-to-cart-section-large">
              <div className="quantity-selector">
                <label>Menge:</label>
                <input
                  type="number"
                  min="1"
                  max={product.stockQuantity}
                  value={quantity}
                  onChange={(e) =>
                    setQuantity(
                      Math.max(1, Math.min(product.stockQuantity, parseInt(e.target.value) || 1))
                    )
                  }
                />
              </div>
              <button
                onClick={handleAddToCart}
                disabled={adding}
                className="add-to-cart-btn-large"
              >
                {adding ? 'Wird hinzugefügt...' : 'In den Warenkorb'}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductDetail;
