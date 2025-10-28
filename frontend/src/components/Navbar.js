import React from 'react';
import './css/Navbar.css';

const Navbar = ({ user, onLogout, navigateTo, currentPage }) => {
  const isAuthenticated = !!user;

  return (
    <nav className="navbar">
      <div className="navbar-container">
        <div className="navbar-logo" onClick={() => navigateTo('products')}>
          ECommerce Shop
        </div>

        <div className="navbar-menu">
          <button 
            onClick={() => navigateTo('products')} 
            className={`navbar-link ${currentPage === 'products' ? 'active' : ''}`}
          >
            Produkte
          </button>

          {isAuthenticated ? (
            <>
              <button 
                onClick={() => navigateTo('cart')} 
                className={`navbar-link cart-link ${currentPage === 'cart' ? 'active' : ''}`}
              >
                Warenkorb
              </button>
              
              <button 
                onClick={() => navigateTo('orders')} 
                className={`navbar-link ${currentPage === 'orders' ? 'active' : ''}`}
              >
                Bestellungen
              </button>
              
              {(user?.IsAdmin || user?.isAdmin) && (
                <>
                  <button 
                    onClick={() => navigateTo('adminProducts')} 
                    className={`navbar-link admin-link ${currentPage === 'adminProducts' ? 'active' : ''}`}
                  >
                    Admin Produkte
                  </button>
                  <button 
                    onClick={() => navigateTo('adminOrders')} 
                    className={`navbar-link admin-link ${currentPage === 'adminOrders' ? 'active' : ''}`}
                  >
                    Admin Bestellungen
                  </button>
                </>
              )}
              
              <button 
                onClick={() => navigateTo('profile')} 
                className={`navbar-link ${currentPage === 'profile' ? 'active' : ''}`}
              >
                Profil
              </button>

              <button onClick={onLogout} className="navbar-link logout-btn">
                Abmelden
              </button>

              <span className="user-greeting">
                {user?.firstName || user?.FirstName || user?.email}
              </span>
            </>
          ) : (
            <>
              <button 
                onClick={() => navigateTo('login')} 
                className={`navbar-link ${currentPage === 'login' ? 'active' : ''}`}
              >
                Anmelden
              </button>
              <button 
                onClick={() => navigateTo('register')} 
                className={`navbar-link register-btn ${currentPage === 'register' ? 'active' : ''}`}
              >
                Registrieren
              </button>
            </>
          )}
        </div>
      </div>
    </nav>
  );
};

export default Navbar;
