import axios from 'axios';

const API_BASE_URL = 'http://localhost:5140/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

//ProductsAPI
export const getProducts = async (params = {}) => {
  const response = await api.get('/products', { params });
  return response.data;
};

export const getProduct = async (id) => {
  const response = await api.get(`/products/${id}`);
  return response.data;
};

export const getCategories = async () => {
  const response = await api.get('/products/categories');
  return response.data;
};

export const createProduct = async (product) => {
  const response = await api.post('/products', product);
  return response.data;
};

export const updateProduct = async (id, product) => {
  const response = await api.put(`/products/${id}`, product);
  return response.data;
};

export const deleteProduct = async (id) => {
  const response = await api.delete(`/products/${id}`);
  return response.data;
};

// Cart API
export const getCart = async () => {
  const user = JSON.parse(localStorage.getItem('user'));
  const response = await api.get(`/cart/${user.id}`);
  return response.data;
};

export const addToCart = async (productId, quantity) => {
  const user = JSON.parse(localStorage.getItem('user'));
  const response = await api.post(`/cart`, { userId: user.id, productId, quantity });
  return response.data;
};

export const updateCartItem = async (itemId, quantity) => {
  const response = await api.put(`/cart/${itemId}`, { quantity });
  return response.data;
};

export const removeFromCart = async (itemId) => {
  const response = await api.delete(`/cart/${itemId}`);
  return response.data;
};

export const clearCart = async () => {
  const user = JSON.parse(localStorage.getItem('user'));
  const response = await api.delete(`/cart/clear/${user.id}`);
  return response.data;
};

//Orders API
export const getUserOrders = async (userId) => {
  const response = await api.get(`/orders/user/${userId}`);
  return response.data;
};

export const getOrder = async (orderId) => {
  const response = await api.get(`/orders/${orderId}`);
  return response.data;
};

export const createOrder = async (userId, shippingAddress) => {
  const response = await api.post('/orders', { userId, shippingAddress });
  return response.data;
};

export const getAllOrders = async (params = {}) => {
  const response = await api.get('/orders', { params });
  return response.data;
};

export const updateOrderStatus = async (orderId, status) => {
  const response = await api.put(`/orders/${orderId}/status`, { status });
  return response.data;
};

// Users API
export const getUser = async (userId) => {
  const response = await api.get(`/users/${userId}`);
  return response.data;
};

export const register = async (userData) => {
  const response = await api.post('/users/register', userData);
  return response.data;
};

export const login = async (email, password) => {
  const response = await api.post('/users/login', { email, password });
  return response.data;
};

export const updateUser = async (userId, userData) => {
  const response = await api.put(`/users/${userId}`, userData);
  return response.data;
};

export const getUsers = async (params = {}) => {
  const response = await api.get('/users', { params });
  return response.data;
};

export const toggleAdminStatus = async (userId) => {
  const response = await api.put(`/users/${userId}/admin`);
  return response.data;
};

export default api;
