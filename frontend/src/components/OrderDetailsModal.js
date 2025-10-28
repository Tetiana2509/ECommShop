import React from 'react';
import './css/OrderDetailsModal.css';

const OrderDetailsModal = ({ order, onClose, detailsLoading }) => {
  if (!order) return null;

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

  const orderId = order.Id || order.id;
  const status = order.Status !== undefined ? order.Status : order.status;
  const totalAmount = order.TotalAmount || order.totalAmount;
  const createdAt = order.CreatedAt || order.createdAt;
  const shippingAddress = order.ShippingAddress || order.shippingAddress;
  const user = order.User || order.user;
  const items = order.Items || order.items || [];

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Order Details - #{orderId}</h2>
          <button onClick={onClose} className="close-btn">&times;</button>
        </div>

        <div className="modal-body">
          <div className="order-info-section">
            <h3>Order Information</h3>
            <div className="info-grid">
              <div className="info-item">
                <strong>Order ID:</strong>
                <span>#{orderId}</span>
              </div>
              <div className="info-item">
                <strong>Status:</strong>
                <span 
                  className="status-badge"
                  style={{ backgroundColor: getStatusColor(status) }}
                >
                  {getStatusText(status)}
                </span>
              </div>
              <div className="info-item">
                <strong>Total Amount:</strong>
                <span>${totalAmount?.toFixed(2)}</span>
              </div>
              <div className="info-item">
                <strong>Order Date:</strong>
                <span>{new Date(createdAt).toLocaleString()}</span>
              </div>
            </div>

            {user && (
              <div className="customer-info">
                <h4>Customer Information</h4>
                <p><strong>Name:</strong> {user.FirstName || user.firstName} {user.LastName || user.lastName}</p>
                <p><strong>Email:</strong> {user.Email || user.email}</p>
                {(user.Phone || user.phone) && (
                  <p><strong>Phone:</strong> {user.Phone || user.phone}</p>
                )}
              </div>
            )}

            {shippingAddress && (
              <div className="shipping-info">
                <h4>Shipping Address</h4>
                <p>{shippingAddress}</p>
              </div>
            )}
          </div>

          <div className="order-items-section">
            <h3>Order Items</h3>
            {detailsLoading ? (
              <p>Loading items...</p>
            ) : (
              <table className="items-table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Quantity</th>
                    <th>Unit Price</th>
                    <th>Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((item, index) => {
                    const product = item.Product || item.product;
                    const quantity = item.Quantity || item.quantity;
                    const unitPrice = item.UnitPrice || item.unitPrice;
                    
                    return (
                      <tr key={index}>
                        <td>
                          {product?.Name || product?.name || 'Product'}
                          <br />
                          <small>{product?.Brand || product?.brand}</small>
                        </td>
                        <td>{quantity}</td>
                        <td>${unitPrice?.toFixed(2)}</td>
                        <td>${(quantity * unitPrice)?.toFixed(2)}</td>
                      </tr>
                    );
                  })}
                </tbody>
                <tfoot>
                  <tr>
                    <td colSpan="3"><strong>Total:</strong></td>
                    <td><strong>${totalAmount?.toFixed(2)}</strong></td>
                  </tr>
                </tfoot>
              </table>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default OrderDetailsModal;
