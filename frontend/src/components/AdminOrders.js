import React, { useState, useEffect } from 'react';
import { getAllOrders, updateOrderStatus, getOrder } from '../services/api';
import OrderDetailsModal from './OrderDetailsModal';
import './css/AdminOrders.css';

const AdminOrders = ({ user, navigateTo }) => {
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [orderDetails, setOrderDetails] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);

  useEffect(() => {
    if (!user || !(user.IsAdmin || user.isAdmin)) {
      console.log('User is not admin, redirecting to products');
      navigateTo('products');
      return;
    }
    console.log('Fetching orders for admin user:', user);
    fetchOrders();
  }, [page]);

  const fetchOrders = async () => {
    try {
      setLoading(true);
      setError('');
      const data = await getAllOrders({ page, pageSize: 10 });
      console.log('Orders data:', data);
      setOrders(data.Orders || data.orders || []);
      setTotalPages(data.TotalPages || data.totalPages || 1);
    } catch (error) {
      setError('Error loading orders');
      console.error('Error fetching orders:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleStatusChange = async (orderId, newStatus) => {
    try {
      await updateOrderStatus(orderId, newStatus);
      
      setOrders(prevOrders => prevOrders.map(order => {
        const currentId = order.Id || order.id;
        if (currentId === orderId) {
          return { ...order, Status: newStatus, status: newStatus };
        }
        return order;
      }));
      alert('Order status updated successfully!');
    } catch (error) {
      alert('Error updating order status');
      console.error('Error:', error);
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

  if (!user || !(user.IsAdmin || user.isAdmin)) {
    return (
      <div className="admin-orders-container">
        <h2>Access Denied</h2>
        <p>You must be an admin to access this page.</p>
        <button onClick={() => navigateTo('products')} className="back-btn">
          Back to Products
        </button>
      </div>
    );
  }

  if (loading) {
    return <div className="loading">Loading orders...</div>;
  }

  return (
    <div className="admin-orders-container">
      <div className="admin-header">
        <h1>Admin - Order Management</h1>
        <button onClick={() => navigateTo('products')} className="back-btn">
          Back to Products
        </button>
      </div>

      {error && <div className="error-message">{error}</div>}

      {orders.length === 0 ? (
        <div className="empty-state">
          <p>No orders found</p>
        </div>
      ) : (
        <>
          <div className="orders-table-container">
            <table className="orders-table">
              <thead>
                <tr>
                  <th>Order ID</th>
                  <th>Customer</th>
                  <th>Total Amount</th>
                  <th>Status</th>
                  <th>Date</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((order) => (
                  <tr key={order.Id || order.id}>
                    <td>#{order.Id || order.id}</td>
                    <td>
                      {(order.User?.FirstName || order.user?.firstName)} {(order.User?.LastName || order.user?.lastName)}
                      <br />
                      <small>{order.User?.Email || order.user?.email}</small>
                    </td>
                    <td>${(order.TotalAmount || order.totalAmount)?.toFixed(2)}</td>
                    <td>
                      <span 
                        className="status-badge"
                        style={{ backgroundColor: getStatusColor(order.Status || order.status) }}
                      >
                        {getStatusText(order.Status || order.status)}
                      </span>
                    </td>
                    <td>{new Date(order.CreatedAt || order.createdAt).toLocaleDateString()}</td>
                    <td>
                      <div className="action-buttons">
                        <button
                          onClick={() => handleViewDetails(order.Id || order.id)}
                          className="view-btn"
                        >
                          View Details
                        </button>
                        <select
                          value={order.Status || order.status}
                          onChange={(e) => handleStatusChange(order.Id || order.id, parseInt(e.target.value))}
                          className="status-select"
                        >
                          <option value={0}>Pending</option>
                          <option value={1}>Processing</option>
                          <option value={2}>Shipped</option>
                          <option value={3}>Delivered</option>
                          <option value={4}>Cancelled</option>
                        </select>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {totalPages > 1 && (
            <div className="pagination">
              <button
                onClick={() => setPage(page - 1)}
                disabled={page === 1}
                className="pagination-btn"
              >
                Previous
              </button>
              <span className="pagination-info">
                Page {page} of {totalPages}
              </span>
              <button
                onClick={() => setPage(page + 1)}
                disabled={page === totalPages}
                className="pagination-btn"
              >
                Next
              </button>
            </div>
          )}
        </>
      )}

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

export default AdminOrders;
