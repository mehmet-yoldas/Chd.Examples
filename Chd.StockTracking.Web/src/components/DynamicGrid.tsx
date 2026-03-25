import React, { useState, useEffect } from 'react';
import { EntityMetadata, PropertyMetadata } from '../types/metadata';
import { crudService } from '../services/api';

interface DynamicGridProps {
  metadata: EntityMetadata;
  endpoint: string;
  onEdit?: (id: number) => void;
  onDelete?: (id: number) => void;
}

export const DynamicGrid: React.FC<DynamicGridProps> = ({ metadata, endpoint, onEdit, onDelete }) => {
  const [data, setData] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, [endpoint]);

  const loadData = async () => {
    setLoading(true);
    try {
      const result = await crudService.getAll(endpoint);
      setData(result);
    } catch (error) {
      console.error('Error loading data:', error);
    } finally {
      setLoading(false);
    }
  };

  const visibleColumns = metadata.properties.filter(p => p.grid && !p.grid.hidden);

  const handleDelete = async (id: number) => {
    if (!confirm('Are you sure you want to delete this item?')) return;
    
    try {
      await crudService.delete(endpoint, id);
      await loadData();
      if (onDelete) onDelete(id);
    } catch (error) {
      console.error('Error deleting:', error);
    }
  };

  if (loading) return <div style={{ padding: '20px' }}>Loading...</div>;

  return (
    <div style={{ padding: '20px' }}>
      <h2>{metadata.title}</h2>
      <p>{metadata.description}</p>
      
      <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '20px' }}>
        <thead>
          <tr style={{ backgroundColor: '#f5f5f5', borderBottom: '2px solid #ddd' }}>
            {visibleColumns.map((col) => (
              <th key={col.name} style={{ padding: '12px', textAlign: 'left', width: col.grid?.width }}>
                {col.form?.label || col.name}
              </th>
            ))}
            <th style={{ padding: '12px', textAlign: 'right' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {data.map((row, idx) => (
            <tr key={row.id || idx} style={{ borderBottom: '1px solid #eee' }}>
              {visibleColumns.map((col) => (
                <td key={col.name} style={{ padding: '12px' }}>
                  {formatValue(row[col.name.charAt(0).toLowerCase() + col.name.slice(1)], col)}
                </td>
              ))}
              <td style={{ padding: '12px', textAlign: 'right' }}>
                {onEdit && (
                  <button 
                    onClick={() => onEdit(row.id)} 
                    style={{ marginRight: '8px', padding: '6px 12px', cursor: 'pointer' }}
                  >
                    Edit
                  </button>
                )}
                {onDelete && (
                  <button 
                    onClick={() => handleDelete(row.id)} 
                    style={{ padding: '6px 12px', cursor: 'pointer', backgroundColor: '#ff4444', color: 'white', border: 'none', borderRadius: '4px' }}
                  >
                    Delete
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      
      {data.length === 0 && (
        <div style={{ padding: '20px', textAlign: 'center', color: '#888' }}>
          No data found
        </div>
      )}
    </div>
  );
};

function formatValue(value: any, col: PropertyMetadata): string {
  if (value == null) return '';
  
  if (col.grid?.format === 'currency') {
    return `$${Number(value).toFixed(2)}`;
  }
  
  if (col.type === 'date' && value) {
    return new Date(value).toLocaleDateString();
  }
  
  return String(value);
}
