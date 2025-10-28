import React, { useState } from 'react';
import { updateUser } from '../services/api';
import './css/Profile.css';

const Profile = ({ user, setUser, navigateTo }) => {
  const [editing, setEditing] = useState(false);
  const [formData, setFormData] = useState({
    firstName: user?.FirstName || user?.firstName || '',
    lastName: user?.LastName || user?.lastName || '',
    email: user?.Email || user?.email || '',
    phone: user?.Phone || user?.phone || '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({ ...formData, [name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    try {
      setLoading(true);
      setError('');
      const userId = user.Id || user.id;
      await updateUser(userId, formData);
      // Обновляем пользователя с учетом правильных полей
      const updatedUser = {
        ...user,
        FirstName: formData.firstName,
        LastName: formData.lastName,
        Email: formData.email,
        Phone: formData.phone,
      };
      setUser(updatedUser);
      localStorage.setItem('user', JSON.stringify(updatedUser));
      setEditing(false);
      alert('Profile updated successfully!');
    } catch (error) {
      setError(error.response?.data || 'Error updating profile');
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    setFormData({
      firstName: user?.FirstName || user?.firstName || '',
      lastName: user?.LastName || user?.lastName || '',
      email: user?.Email || user?.email || '',
      phone: user?.Phone || user?.phone || '',
    });
    setEditing(false);
    setError('');
  };

  if (!user) {
    return (
      <div className="profile-container">
        <h2>Please log in</h2>
        <button onClick={() => navigateTo('login')}>Login</button>
      </div>
    );
  }

  return (
    <div className="profile-container">
      <h1>User Profile</h1>

      {error && <div className="error-message">{error}</div>}

      {editing ? (
        <form onSubmit={handleSubmit} className="profile-form">
          <div className="form-group">
            <label>First Name:</label>
            <input
              type="text"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>Last Name:</label>
            <input
              type="text"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>Email:</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>Phone:</label>
            <input
              type="tel"
              name="phone"
              value={formData.phone}
              onChange={handleChange}
            />
          </div>

          <div className="form-actions">
            <button type="button" onClick={handleCancel} className="cancel-btn">
              Cancel
            </button>
            <button type="submit" disabled={loading} className="save-btn">
              {loading ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      ) : (
        <div className="profile-info">
          <div className="info-row">
            <strong>Name:</strong>
            <span>
              {((user.FirstName || user.firstName) || (user.LastName || user.lastName))
                ? `${user.FirstName || user.firstName || ''} ${user.LastName || user.lastName || ''}`.trim()
                : 'Not specified'}
            </span>
          </div>
          <div className="info-row">
            <strong>Email:</strong>
            <span>{user.Email || user.email}</span>
          </div>
          {(user.Phone || user.phone) && (
            <div className="info-row">
              <strong>Phone:</strong>
              <span>{user.Phone || user.phone}</span>
            </div>
          )}
          
          <button onClick={() => setEditing(true)} className="edit-btn">
            Edit
          </button>
        </div>
      )}

      <div className="profile-actions">
        <button onClick={() => navigateTo('orders')} className="orders-btn">
          My Orders
        </button>
      </div>
    </div>
  );
};

export default Profile;
