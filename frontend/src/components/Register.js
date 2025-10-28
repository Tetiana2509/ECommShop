import React, { useState } from 'react';
import './css/Auth.css';

const Register = ({ onRegister, navigateTo }) => {
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
    phone: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
    setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!formData.firstName || !formData.lastName || !formData.email || !formData.password) {
      setError('Bitte füllen Sie alle Pflichtfelder aus');
      return;
    }

    if (formData.password !== formData.confirmPassword) {
      setError('Passwörter stimmen nicht überein');
      return;
    }

    if (formData.password.length < 6) {
      setError('Passwort muss mindestens 6 Zeichen lang sein');
      return;
    }

    try {
      setLoading(true);
      setError('');
      const { confirmPassword, ...registerData } = formData;
      const result = await onRegister(registerData);
      if (!result.success) {
        setError(result.error);
      }
    } catch (error) {
      setError('Registrierungsfehler');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2>Registrieren</h2>
        
        {error && <div className="error-message">{error}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Vorname *</label>
            <input
              type="text"
              name="firstName"
              value={formData.firstName}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>Nachname *</label>
            <input
              type="text"
              name="lastName"
              value={formData.lastName}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>E-Mail *</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>Telefon</label>
            <input
              type="tel"
              name="phone"
              value={formData.phone}
              onChange={handleChange}
            />
          </div>

          <div className="form-group">
            <label>Passwort *</label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>Passwort bestätigen *</label>
            <input
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleChange}
              required
            />
          </div>

          <button type="submit" disabled={loading} className="submit-btn">
            {loading ? 'Registrierung läuft...' : 'Registrieren'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Sie haben bereits ein Konto? <button onClick={() => navigateTo('login')} className="link-btn">Anmelden</button>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Register;
