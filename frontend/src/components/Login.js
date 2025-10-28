import React, { useState } from 'react';
import './css/Auth.css';

const Login = ({ onLogin, navigateTo }) => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
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
    
    if (!formData.email || !formData.password) {
      setError('Bitte füllen Sie alle Felder aus');
      return;
    }

    try {
      setLoading(true);
      setError('');
      const result = await onLogin(formData.email, formData.password);
      if (!result.success) {
        setError(result.error);
      }
    } catch (error) {
      setError('Anmeldefehler');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2>Anmelden</h2>
        
        {error && <div className="error-message">{error}</div>}
        
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label>Passwort</label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleChange}
              required
            />
          </div>

          <button type="submit" disabled={loading} className="submit-btn">
            {loading ? 'Anmeldung läuft...' : 'Anmelden'}
          </button>
        </form>

        <div className="auth-footer">
          <p>
            Sie haben noch kein Konto? <button onClick={() => navigateTo('register')} className="link-btn">Registrieren</button>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Login;
