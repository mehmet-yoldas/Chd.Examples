import React, { useState, useEffect } from 'react';
import { EntityMetadata } from '../types/metadata';
import { crudService } from '../services/api';

interface DynamicFormProps {
  metadata: EntityMetadata;
  endpoint: string;
  editId?: number;
  onSave?: () => void;
  onCancel?: () => void;
}

export const DynamicForm: React.FC<DynamicFormProps> = ({ metadata, endpoint, editId, onSave, onCancel }) => {
  const [formData, setFormData] = useState<Record<string, any>>({});
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (editId) {
      loadData();
    }
  }, [editId]);

  const loadData = async () => {
    if (!editId) return;
    setLoading(true);
    try {
      const data = await crudService.getById(endpoint, editId);
      const formValues: Record<string, any> = {};
      Object.keys(data).forEach(key => {
        formValues[key.charAt(0).toLowerCase() + key.slice(1)] = (data as any)[key];
      });
      setFormData(formValues);
    } catch (error) {
      console.error('Error loading data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    console.log('📤 Form Data:', formData);
    console.log('📤 Endpoint:', endpoint);

    try {
      if (editId) {
        await crudService.update(endpoint, editId, formData);
      } else {
        await crudService.create(endpoint, formData);
      }
      if (onSave) onSave();
    } catch (error) {
      console.error('Error saving:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (name: string, value: any) => {
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const formFields = metadata.properties
    .filter(p => p.form && !p.form.readOnly)
    .sort((a, b) => (a.form?.order || 0) - (b.form?.order || 0));

  return (
    <div style={{ padding: '20px', maxWidth: '600px' }}>
      <h2>{editId ? 'Edit' : 'Create'} {metadata.title}</h2>
      
      <form onSubmit={handleSubmit} style={{ marginTop: '20px' }}>
        {formFields.map((field) => {
          const fieldName = field.name.charAt(0).toLowerCase() + field.name.slice(1);
          const value = formData[fieldName] || '';
          
          return (
            <div key={field.name} style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '4px', fontWeight: '500' }}>
                {field.form?.label || field.name}
                {field.form?.required && <span style={{ color: 'red' }}>*</span>}
              </label>
              
              {renderField(field, fieldName, value, handleChange)}
            </div>
          );
        })}
        
        <div style={{ marginTop: '24px', display: 'flex', gap: '12px' }}>
          <button 
            type="submit" 
            disabled={loading}
            style={{ 
              padding: '10px 20px', 
              backgroundColor: '#4CAF50', 
              color: 'white', 
              border: 'none', 
              borderRadius: '4px', 
              cursor: loading ? 'not-allowed' : 'pointer' 
            }}
          >
            {loading ? 'Saving...' : 'Save'}
          </button>
          
          {onCancel && (
            <button 
              type="button" 
              onClick={onCancel}
              style={{ 
                padding: '10px 20px', 
                backgroundColor: '#ccc', 
                border: 'none', 
                borderRadius: '4px', 
                cursor: 'pointer' 
              }}
            >
              Cancel
            </button>
          )}
        </div>
      </form>
    </div>
  );
};

function renderField(
  field: any,
  fieldName: string,
  value: any,
  onChange: (name: string, value: any) => void
) {
  const baseStyle = {
    width: '100%',
    padding: '8px',
    border: '1px solid #ddd',
    borderRadius: '4px',
    fontSize: '14px'
  };

  switch (field.form?.type) {
    case 'textarea':
      return (
        <textarea
          value={value}
          onChange={(e) => onChange(fieldName, e.target.value)}
          placeholder={field.form?.placeholder}
          maxLength={field.form?.maxLength}
          required={field.form?.required}
          style={{ ...baseStyle, minHeight: '100px' }}
        />
      );
    
    case 'number':
      return (
        <input
          type="number"
          value={value}
          onChange={(e) => onChange(fieldName, e.target.valueAsNumber)}
          placeholder={field.form?.placeholder}
          required={field.form?.required}
          style={baseStyle}
        />
      );
    
    case 'email':
      return (
        <input
          type="email"
          value={value}
          onChange={(e) => onChange(fieldName, e.target.value)}
          placeholder={field.form?.placeholder}
          maxLength={field.form?.maxLength}
          required={field.form?.required}
          style={baseStyle}
        />
      );
    
    case 'date':
      return (
        <input
          type="date"
          value={value}
          onChange={(e) => onChange(fieldName, e.target.value)}
          required={field.form?.required}
          style={baseStyle}
        />
      );
    
    case 'checkbox':
      return (
        <input
          type="checkbox"
          checked={value}
          onChange={(e) => onChange(fieldName, e.target.checked)}
          style={{ width: 'auto' }}
        />
      );

    case 'dropdown':
      // TODO: Load options from related entity
      return (
        <select
          value={value}
          onChange={(e) => onChange(fieldName, e.target.value ? parseInt(e.target.value) : null)}
          required={field.form?.required}
          style={baseStyle}
        >
          <option value="">-- Select {field.form?.label} --</option>
          {/* Options will be loaded dynamically in future */}
        </select>
      );

    default:
      return (
        <input
          type="text"
          value={value}
          onChange={(e) => onChange(fieldName, e.target.value)}
          placeholder={field.form?.placeholder}
          maxLength={field.form?.maxLength}
          required={field.form?.required}
          style={baseStyle}
        />
      );
  }
}
