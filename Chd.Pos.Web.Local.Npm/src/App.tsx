import React, { useState, useEffect } from 'react';
import { DynamicForm, DynamicGrid, Login, authService } from '@mehmetyoldas/chd-auto-ui-react';
import type { EntityMetadata, CurrentUser } from '@mehmetyoldas/chd-auto-ui-react';
import axios from 'axios';
import MetadataDemo from './pages/MetadataDemo';

const API_BASE = process.env.NODE_ENV === 'production' 
  ? 'http://localhost:5218/api' 
  : '/api'; // Use proxy in development

function App() {
  const [entities, setEntities] = useState<EntityMetadata[]>([]);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false);
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null);
  const [selectedEntity, setSelectedEntity] = useState<EntityMetadata | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<number | undefined>();

  useEffect(() => {
    const token = localStorage.getItem('chd_token');
    setIsAuthenticated(!!token);
    if (token) {
      loadCurrentUser();
    }
    // Load metadata regardless of authentication status
    loadMetadata();
  }, []);

  const loadCurrentUser = async () => {
    try {
      const u = await authService.getCurrentUser();
      setCurrentUser(u);
    } catch (err) {
      console.error('Error loading current user:', err);
    }
  };

  // Helper to check permissions
  const can = (entity: EntityMetadata | null, action: 'create' | 'read' | 'update' | 'delete') => {
    if (!entity) return false;
    const perms = entity.permissions?.[action] || [];
    if (perms.length === 0) return true;
    if (perms.includes('*')) return true;
    if (!currentUser?.roles) return false;
    const userRolesLower = currentUser.roles.map(r => r.toLowerCase());
    return perms.some(role => userRolesLower.includes(role.toLowerCase()));
  };

  const loadMetadata = async () => {
    try {
      console.log('Loading metadata from:', `${API_BASE}/metadata`);
      const response = await axios.get(`${API_BASE}/metadata`, {
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json'
        },
        timeout: 10000
      });
      console.log('Metadata response status:', response.status);
      console.log('Metadata loaded:', response.data);
      setEntities(response.data);
      if (response.data.length > 0) {
        setSelectedEntity(response.data[0]);
      }
    } catch (error: any) {
      console.error('Error loading metadata:', error);
      console.error('Error details:', {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status,
        url: error.config?.url
      });
      alert(`Failed to load metadata: ${error.message}\nCheck console for details.`);
    }
  };

  const handleEdit = (id: number) => {
    setEditId(id);
    setShowForm(true);
  };

  const handleSave = () => {
    setShowForm(false);
    setEditId(undefined);
  };

  const handleCancel = () => {
    setShowForm(false);
    setEditId(undefined);
  };

  const handleCreate = () => {
    setEditId(undefined);
    setShowForm(true);
  };

  const handleLogin = () => {
    const token = localStorage.getItem('chd_token');
    setIsAuthenticated(!!token);
    if (token) {
        loadCurrentUser();
        loadMetadata();
    }
  };

  const handleLogout = () => {
    localStorage.removeItem('chd_token');
    setIsAuthenticated(false);
    setCurrentUser(null);
    setEntities([]);
    setSelectedEntity(null);
    setShowForm(false);
  };

  return (
    <div style={{ display: 'flex', height: '100vh', fontFamily: 'Arial, sans-serif' }}>
      {/* Sidebar */}
      <div style={{ width: '250px', backgroundColor: '#2c3e50', color: 'white', padding: '20px' }}>
        <h1 style={{ fontSize: '20px', marginBottom: '30px' }}>CHD AutoUI POS</h1>
        <div style={{ fontSize: '12px', marginBottom: '20px', color: '#95a5a6' }}>
          {isAuthenticated ? `Entities: ${entities.length}` : 'Not logged in'}
        </div>
        <nav>
          <button
            onClick={() => {
              setSelectedEntity(null);
              setShowForm(false);
            }}
            style={{
              display: 'block',
              width: '100%',
              padding: '12px',
              marginBottom: '8px',
              backgroundColor: '#34495e',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              textAlign: 'left',
              cursor: 'pointer',
              fontSize: '14px'
            }}
          >
            Metadata Demo
          </button>
          {entities.map((entity) => (
            <button
              key={entity.entityName}
              onClick={() => {
                setSelectedEntity(entity);
                setShowForm(false);
              }}
              style={{
                display: 'block',
                width: '100%',
                padding: '12px',
                marginBottom: '8px',
                backgroundColor: selectedEntity?.entityName === entity.entityName ? '#34495e' : 'transparent',
                color: 'white',
                border: 'none',
                borderRadius: '4px',
                textAlign: 'left',
                cursor: 'pointer',
                fontSize: '14px'
              }}
            >
              {entity.icon} {entity.title}
            </button>
          ))}
        </nav>
      </div>

      {/* Main Content */}
      <div style={{ flex: 1, overflow: 'auto', backgroundColor: '#f5f5f5' }}>
        {!isAuthenticated ? (
          <div style={{ padding: 24 }}>
            <h2>Please login to continue</h2>
            <Login
              logo={<img src="https://via.placeholder.com/150x50?text=LOGO" alt="logo" />}
              onLogin={handleLogin}
            >
              <small style={{ color: '#888' }}>Test: children çalışıyor mu?</small>
            </Login>
          </div>
        ) : selectedEntity ? (
          <>
            {!showForm ? (
              <>
                <div style={{ padding: '20px', borderBottom: '1px solid #ddd', backgroundColor: 'white' }}>
                  {can(selectedEntity, 'create') && (
                    <button
                      onClick={handleCreate}
                      style={{
                        padding: '10px 20px',
                        backgroundColor: '#4CAF50',
                        color: 'white',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: 'pointer',
                        fontSize: '14px'
                      }}
                    >
                      + Create New {selectedEntity.title}
                    </button>
                  )}
                </div>
                <DynamicGrid
                  metadata={selectedEntity}
                  endpoint={selectedEntity.route.replace('/', '')}
                  onEdit={handleEdit}
                  onDelete={() => {}}
                  renderActions={(row) => (
                    <button
                      onClick={() => alert(`Test renderActions: id=${row.id}`)}
                      style={{ marginLeft: 8, padding: '4px 10px', cursor: 'pointer' }}
                    >
                      🔍
                    </button>
                  )}
                  renderCell={(colName, value) => {
                    if (colName === 'Status') {
                      return (
                        <span style={{ color: value === 'Active' ? 'green' : 'red', fontWeight: 'bold' }}>
                          {value}
                        </span>
                      );
                    }
                    return null;
                  }}
                />
              </>
            ) : (
              <DynamicForm
                metadata={selectedEntity}
                endpoint={selectedEntity.route.replace('/', '')}
                editId={editId}
                onSave={handleSave}
                onCancel={handleCancel}
              >
                <small style={{ color: '#888' }}>Test: DynamicForm children çalışıyor mu?</small>
              </DynamicForm>
            )}
          </>
        ) : (
          <MetadataDemo />
        )}
      </div>
      <div style={{ position: 'fixed', right: 16, top: 16 }}>
        {!isAuthenticated ? null : (
          <button onClick={handleLogout} style={{ padding: '8px 12px' }}>Logout</button>
        )}
      </div>
    </div>
  );
}

export default App;
