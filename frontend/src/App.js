import React, { useState, useEffect } from 'react';
import { login as loginAPI, register as registerAPI } from './services/api';
import ProductList from './components/ProductList';
import ProductDetail from './components/ProductDetail';
import Cart from './components/Cart';
import Checkout from './components/Checkout';
import Login from './components/Login';
import Register from './components/Register';
import Profile from './components/Profile';
import Orders from './components/Orders';
import AdminOrders from './components/AdminOrders';
import AdminProducts from './components/AdminProducts';
import Navbar from './components/Navbar';
import './App.css';

function App() {
  const [currentPage, setCurrentPage] = useState('products');
  const [selectedProductId, setSelectedProductId] = useState(null);
  const [user, setUser] = useState(null);
  const [cart, setCart] = useState(null);

  useEffect(() => {
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
  }, []);

  const handleLogin = async (email, password) => {
    try {
      const userData = await loginAPI(email, password);
      console.log('Login response:', userData);
      setUser(userData);
      localStorage.setItem('user', JSON.stringify(userData));
      setCurrentPage('products');
      return { success: true };
    } catch (error) {
      return { success: false, error: error.response?.data || 'Anmeldefehler' };
    }
  };

  const handleRegister = async (formData) => {
    try {
      const userData = await registerAPI(formData);
      setUser(userData);
      localStorage.setItem('user', JSON.stringify(userData));
      setCurrentPage('products');
      return { success: true };
    } catch (error) {
      return { success: false, error: error.response?.data || 'Registrierungsfehler' };
    }
  };

  const handleLogout = () => {
    setUser(null);
    localStorage.removeItem('user');
    setCurrentPage('products');
  };

  const navigateTo = (page, productId = null) => {
    setCurrentPage(page);
    if (productId) {
      setSelectedProductId(productId);
    }
  };

  const renderPage = () => {
    switch (currentPage) {
      case 'products':
        return <ProductList user={user} navigateTo={navigateTo} />;
      case 'productDetail':
        return <ProductDetail productId={selectedProductId} user={user} navigateTo={navigateTo} />;
      case 'cart':
        return <Cart user={user} navigateTo={navigateTo} />;
      case 'checkout':
        return <Checkout user={user} navigateTo={navigateTo} />;
      case 'login':
        return <Login onLogin={handleLogin} navigateTo={navigateTo} />;
      case 'register':
        return <Register onRegister={handleRegister} navigateTo={navigateTo} />;
      case 'profile':
        return <Profile user={user} setUser={setUser} navigateTo={navigateTo} />;
      case 'orders':
        return <Orders user={user} navigateTo={navigateTo} />;
      case 'adminOrders':
        return <AdminOrders user={user} navigateTo={navigateTo} />;
      case 'adminProducts':
        return <AdminProducts user={user} navigateTo={navigateTo} />;
      default:
        return <ProductList user={user} navigateTo={navigateTo} />;
    }
  };

  return (
    <div className="App">
      <Navbar 
        user={user} 
        onLogout={handleLogout} 
        navigateTo={navigateTo}
        currentPage={currentPage}
      />
      <main className="main-content">
        {renderPage()}
      </main>
    </div>
  );
}

export default App;
