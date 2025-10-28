import React, { useState, useEffect } from 'react';
import { getCart, updateCartItem, removeFromCart, clearCart as clearCartAPI } from '../services/api';
import './css/Cart.css';

const Cart = ({ user, navigateTo }) => {
  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(true);
  const [updatingItems, setUpdatingItems] = useState({});

  useEffect(() => {
    if (user) {
      fetchCart();
    }
  }, [user]);

  const fetchCart = async () => {
    try {
      setLoading(true);
      const data = await getCart();
      setCart(data);
    } catch (error) {
      console.error('Error fetching cart:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleQuantityChange = async (itemId, newQuantity) => {
    if (newQuantity < 1) return;
    
    try {
      setUpdatingItems({ ...updatingItems, [itemId]: true });
      await updateCartItem(itemId, newQuantity);
      await fetchCart();
    } catch (error) {
      alert(error.response?.data || 'Fehler beim Aktualisieren der Menge');
    } finally {
      setUpdatingItems({ ...updatingItems, [itemId]: false });
    }
  };

  const handleRemoveItem = async (itemId) => {
    if (!window.confirm('Produkt aus dem Warenkorb entfernen?')) return;
    
    try {
      await removeFromCart(itemId);
      await fetchCart();
    } catch (error) {
      alert('Fehler beim Entfernen des Produkts');
    }
  };

  const handleClearCart = async () => {
    if (!window.confirm('Warenkorb leeren?')) return;
    
    try {
      await clearCartAPI();
      await fetchCart();
    } catch (error) {
      alert('Fehler beim Leeren des Warenkorbs');
    }
  };

  const handleCheckout = () => {
    navigateTo('checkout');
  };

  if (loading) {
    return <div className="loading">Warenkorb wird geladen...</div>;
  }

  if (!cart || !cart.items || cart.items.length === 0) {
    return (
      <div className="empty-cart">
        <h2>Warenkorb ist leer</h2>
        <p>Produkte aus dem Katalog hinzufügen</p>
        <button onClick={() => navigateTo('products')}>Zu den Produkten</button>
      </div>
    );
  }

  return (
    <div className="cart-container">
      <div className="cart-header">
        <h1>Warenkorb</h1>
        <button onClick={handleClearCart} className="clear-cart-btn">
          Warenkorb leeren
        </button>
      </div>

      <div className="cart-items">
        {cart.items.map((item) => (
          <div key={item.id} className="cart-item">
            <div className="item-image">
              {item.product.imageUrl ? (
                <img src={item.product.imageUrl} alt={item.product.name} />
              ) : (
                <div className="no-image">Kein Bild</div>
              )}
            </div>

            <div className="item-details">
              <h3>{item.product.name}</h3>
              <p className="item-category">{item.product.category}</p>
              {item.product.brand && <p className="item-brand">{item.product.brand}</p>}
            </div>

            <div className="item-price">
              ${item.product.price.toFixed(2)}
            </div>

            <div className="item-quantity">
              <button
                onClick={() => handleQuantityChange(item.id, item.quantity - 1)}
                disabled={updatingItems[item.id]}
              >
                -
              </button>
              <input
                type="number"
                value={item.quantity}
                onChange={(e) => {
                  const val = parseInt(e.target.value) || 1;
                  handleQuantityChange(item.id, val);
                }}
                disabled={updatingItems[item.id]}
              />
              <button
                onClick={() => handleQuantityChange(item.id, item.quantity + 1)}
                disabled={updatingItems[item.id] || item.quantity >= item.product.stockQuantity}
              >
                +
              </button>
            </div>

            <div className="item-total">
              ${item.totalPrice.toFixed(2)}
            </div>

            <button
              onClick={() => handleRemoveItem(item.id)}
              className="remove-item-btn"
            >
              ✕
            </button>
          </div>
        ))}
      </div>

      <div className="cart-summary">
        <div className="summary-row">
          <span>Gesamtartikel:</span>
          <span>{cart.itemCount}</span>
        </div>
        <div className="summary-row total">
          <span>Gesamt:</span>
          <span>${cart.totalAmount.toFixed(2)}</span>
        </div>
        <button onClick={handleCheckout} className="checkout-btn">
          Zur Kasse
        </button>
      </div>
    </div>
  );
};

export default Cart;
