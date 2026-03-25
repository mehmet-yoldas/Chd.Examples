import React, { useState, useEffect } from 'react';
import { DynamicGrid } from './components/DynamicGrid';
import { DynamicForm } from './components/DynamicForm';
import { metadataService } from './services/api';
import { EntityMetadata } from './types/metadata';

function App() {
  const [entities, setEntities] = useState<EntityMetadata[]>([]);
  const [selectedEntity, setSelectedEntity] = useState<EntityMetadata | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<number | undefined>();

  useEffect(() => {
    loadMetadata();
  }, []);

  const loadMetadata = async () => {
    try {
      const data = await metadataService.getAllMetadata();
      setEntities(data);
      if (data.length > 0) {
        setSelectedEntity(data[0]);
      }
    } catch (error) {
      console.error('Error loading metadata:', error);
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

  return (
    <div style={{ display: 'flex', height: '100vh', fontFamily: 'Arial, sans-serif' }}>
      {/* Sidebar */}
      <div style={{ width: '250px', backgroundColor: '#2c3e50', color: 'white', padding: '20px' }}>
        <h1 style={{ fontSize: '20px', marginBottom: '30px' }}>CHD AutoUI Stock</h1>
        <nav>
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
        {selectedEntity && (
          <>
            {!showForm ? (
              <>
                <div style={{ padding: '20px', borderBottom: '1px solid #ddd', backgroundColor: 'white' }}>
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
                </div>
                <DynamicGrid
                  metadata={selectedEntity}
                  endpoint={selectedEntity.route.replace('/', '')}
                  onEdit={handleEdit}
                  onDelete={() => {}}
                />
              </>
            ) : (
              <DynamicForm
                metadata={selectedEntity}
                endpoint={selectedEntity.route.replace('/', '')}
                editId={editId}
                onSave={handleSave}
                onCancel={handleCancel}
              />
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default App;
