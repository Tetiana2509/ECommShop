import React, { useState } from 'react';
import { addToCart as addToCartAPI } from '../services/api';
import './css/ProductCard.css';

const ProductCard = ({ product, user, navigateTo }) => {
  const [quantity, setQuantity] = useState(1);
  const [adding, setAdding] = useState(false);

  const handleAddToCart = async (e) => {
    e.stopPropagation();
    
    if (!user) {
      alert('Bitte melden Sie sich an, um Artikel in den Warenkorb zu legen');
      navigateTo('login');
      return;
    }

    try {
      setAdding(true);
      await addToCartAPI(product.id, quantity);
      alert('Produkt zum Warenkorb hinzugefügt!');
      setQuantity(1);
    } catch (error) {
      alert(error.response?.data || 'Fehler beim Hinzufügen zum Warenkorb');
    } finally {
      setAdding(false);
    }
  };

  const handleCardClick = () => {
    navigateTo('productDetail', product.id);
  };

  return (
    <div className="product-card" onClick={handleCardClick}>
      <div className="product-image">
        {product.imageUrl ? (
          <img src={product.imageUrl} alt={product.name} />
        ) : (
          <div className="no-image">Kein Bild</div>
        )}
      </div>
      
      <div className="product-info">
        <h3 className="product-name">{product.name}</h3>
        <p className="product-category">{product.category}</p>
        {product.brand && <p className="product-brand">{product.brand}</p>}
        
        <div className="product-footer">
          <div className="product-price">${product.price.toFixed(2)}</div>
          <div className="product-stock">
            {product.stockQuantity > 0 ? (
              <span className="in-stock">Auf Lager: {product.stockQuantity}</span>
            ) : (
              <span className="out-of-stock">Nicht auf Lager</span>
            )}
          </div>
        </div>

        {product.stockQuantity > 0 && (
          <div className="add-to-cart-section" onClick={(e) => e.stopPropagation()}>
            <input
              type="number"
              min="1"
              max={product.stockQuantity}
              value={quantity}
              onChange={(e) => setQuantity(Math.max(1, Math.min(product.stockQuantity, parseInt(e.target.value) || 1)))}
            />
            <button
              onClick={handleAddToCart}
              disabled={adding}
              className="add-to-cart-btn"
            >
              {adding ? 'Wird hinzugefügt...' : 'In den Warenkorb'}
            </button>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProductCard;
