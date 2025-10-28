import React, { useState, useEffect } from 'react';
import { getUserOrders, getOrder } from '../services/api';
import OrderDetailsModal from './OrderDetailsModal';
import './css/Orders.css';

const Orders = ({ user, navigateTo }) => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [orderDetails, setOrderDetails] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);

  useEffect(() => {
    if (user) {
      fetchOrders();
    }
    // eslint-disable-next-line
  }, [user]);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      const userId = user.Id || user.id;
      const data = await getUserOrders(userId);
      console.log('User orders:', data);
      setOrders(data);
    } catch (error) {
      console.error('Error fetching orders:', error);
      alert('Error fetching orders');
    } finally {
      setLoading(false);
    }
  };

  const handleViewDetails = async (orderId) => {
    try {
      setDetailsLoading(true);
      const details = await getOrder(orderId);
      console.log('Order details:', details);
      setOrderDetails(details);
      setSelectedOrder(orderId);
    } catch (error) {
      alert('Error loading order details');
      console.error('Error:', error);
    } finally {
      setDetailsLoading(false);
    }
  };

  const closeDetails = () => {
    setSelectedOrder(null);
    setOrderDetails(null);
  };

  const getStatusText = (status) => {
    const statusMap = {
      0: 'Pending',
      1: 'Processing',
      2: 'Shipped',
      3: 'Delivered',
      4: 'Cancelled'
    };
    return statusMap[status] || 'Unknown';
  };

  const getStatusColor = (status) => {
    const colorMap = {
      0: '#FFA500', // Pending - orange
      1: '#007bff', // Processing - blue
      2: '#17a2b8', // Shipped - cyan
      3: '#28a745', // Delivered - green
      4: '#dc3545'  // Cancelled - red
    };
    return colorMap[status] || '#6c757d';
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  if (!user) {
    return (
      <div className="orders-container">
        <h2>Please login</h2>
        <button onClick={() => navigateTo('login')}>Login</button>
      </div>
    );
  }

  if (loading) {
    return <div className="loading">Loading orders...</div>;
  }

  if (orders.length === 0) {
    return (
      <div className="empty-orders">
        <h2>You don't have any orders yet</h2>
        <button onClick={() => navigateTo('products')}>Go Shopping</button>
      </div>
    );
  }

  return (
    <div className="orders-container">
      <h1>My Orders</h1>

      <div className="orders-list">
        {orders.map((order) => {
          const orderId = order.Id || order.id;
          const status = order.Status !== undefined ? order.Status : order.status;
          const totalAmount = order.TotalAmount || order.totalAmount;
          const createdAt = order.CreatedAt || order.createdAt;
          const shippingAddress = order.ShippingAddress || order.shippingAddress;

          return (
            <div key={orderId} className="order-card">
              <div className="order-header">
                <div className="order-info">
                  <h3>Order #{orderId}</h3>
                  <p className="order-date">{new Date(createdAt).toLocaleDateString()}</p>
                </div>
                <div className="order-status">
                  <span 
                    className="status-badge"
                    style={{ backgroundColor: getStatusColor(status) }}
                  >
                    {getStatusText(status)}
                  </span>
                </div>
              </div>

              <div className="order-details">
                {shippingAddress && (
                  <p className="shipping-address">
                    <strong>Shipping Address:</strong> {shippingAddress}
                  </p>
                )}

                <div className="order-summary">
                  <span><strong>Total Amount:</strong></span>
                  <span className="total-amount">${totalAmount?.toFixed(2)}</span>
                </div>

                <button 
                  onClick={() => handleViewDetails(orderId)}
                  className="view-details-btn"
                >
                  View Details
                </button>
              </div>
            </div>
          );
        })}
      </div>

      {selectedOrder && orderDetails && (
        <OrderDetailsModal
          order={orderDetails}
          onClose={closeDetails}
          detailsLoading={detailsLoading}
        />
      )}
    </div>
  );
};

export default Orders;
