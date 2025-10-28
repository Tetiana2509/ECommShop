import React, { useState, useEffect } from 'react';
import { getCart, createOrder, clearCart as clearCartAPI } from '../services/api';
import './css/Checkout.css';

const Checkout = ({ user, navigateTo }) => {
  const [cart, setCart] = useState(null);
  const [shippingAddress, setShippingAddress] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchCart();
  }, []);

  const fetchCart = async () => {
    try {
      const data = await getCart();
      setCart(data);
    } catch (error) {
      console.error('Error fetching cart:', error);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!shippingAddress.trim()) {
      setError('Bitte geben Sie eine Lieferadresse an');
      return;
    }

    try {
      setLoading(true);
      setError('');
      await createOrder(user.id, shippingAddress);
      await clearCartAPI();
      alert('Bestellung erfolgreich aufgegeben!');
      navigateTo('orders');
    } catch (error) {
      setError(error.response?.data?.message || 'Fehler beim Aufgeben der Bestellung');
    } finally {
      setLoading(false);
    }
  };

  if (!cart || !cart.items || cart.items.length === 0) {
    return (
      <div className="empty-checkout">
        <h2>Warenkorb ist leer</h2>
        <p>Produkte hinzufügen, um eine Bestellung aufzugeben</p>
        <button onClick={() => navigateTo('products')}>Zu den Produkten</button>
      </div>
    );
  }

  return (
    <div className="checkout-container">
      <h1>Bestellung aufgeben</h1>

      <div className="checkout-content">
        <div className="order-summary">
          <h2>Ihre Bestellung</h2>
          <div className="summary-items">
            {cart.items.map((item) => (
              <div key={item.id} className="summary-item">
                <span>{item.product.name} x {item.quantity}</span>
                <span>${item.totalPrice.toFixed(2)}</span>
              </div>
            ))}
          </div>
          <div className="summary-total">
            <strong>Gesamt:</strong>
            <strong>${cart.totalAmount.toFixed(2)}</strong>
          </div>
        </div>

        <div className="shipping-form">
          <h2>Lieferadresse</h2>
          
          {error && <div className="error-message">{error}</div>}
          
          <form onSubmit={handleSubmit}>
            <textarea
              placeholder="Lieferadresse eingeben..."
              value={shippingAddress}
              onChange={(e) => setShippingAddress(e.target.value)}
              rows="4"
              required
            />
            
            <div className="form-actions">
              <button
                type="button"
                onClick={() => navigateTo('cart')}
                className="back-btn"
              >
                Zurück zum Warenkorb
              </button>
              <button
                type="submit"
                disabled={loading}
                className="submit-btn"
              >
                {loading ? 'Wird aufgegeben...' : 'Bestellung aufgeben'}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default Checkout;
