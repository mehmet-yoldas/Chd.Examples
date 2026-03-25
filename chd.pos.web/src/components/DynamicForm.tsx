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
  const [dropdownOptions, setDropdownOptions] = useState<Record<string, any[]>>({});

  useEffect(() => {
    if (editId) {
      loadData();
    }
    loadDropdownOptions();
  }, [editId]);

  const loadData = async () => {
    if (!editId) return;
    setLoading(true);
    try {
      const data: any = await crudService.getById(endpoint, editId);
      const formValues: Record<string, any> = {};
      Object.keys(data).forEach(key => {
        formValues[key.charAt(0).toLowerCase() + key.slice(1)] = data[key];
      });
      setFormData(formValues);
    } catch (error) {
      console.error('Error loading data:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadDropdownOptions = async () => {
    const formFields = metadata.properties.filter(p => p.form && !p.form.readOnly);
    const optionsToLoad: Record<string, any[]> = {};

    for (const field of formFields) {
      if ((field.form?.type === 'dropdown' || field.form?.type === 'multiselect') && field.form?.relatedEntity) {
        try {
          const relatedData = await crudService.getAll(field.form.relatedEntity);
          console.log(`🔽 Loaded options for ${field.name} from ${field.form.relatedEntity}:`, relatedData);
          console.log(`   Display: ${field.form.displayProperty}, Value: ${field.form.valueProperty}`);
          if (relatedData && relatedData.length > 0) {
            console.log(`   Sample item:`, relatedData[0]);
          }
          optionsToLoad[field.name] = relatedData;
        } catch (error) {
          console.error(`Error loading options for ${field.name}:`, error);
          optionsToLoad[field.name] = [];
        }
      }
    }

    setDropdownOptions(optionsToLoad);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    console.log('📤 Form Data being sent:', formData);
    console.log('📤 Endpoint:', endpoint);
    console.log('📤 Edit ID:', editId);

    try {
      if (editId) {
        console.log('🔄 Updating existing record...');
        await crudService.update(endpoint, editId, formData);
      } else {
        console.log('➕ Creating new record...');
        await crudService.create(endpoint, formData);
      }
      console.log('✅ Save successful!');
      if (onSave) onSave();
    } catch (error: any) {
      console.error('❌ Error saving:', error);
      console.error('❌ Response:', error.response?.data);
      console.error('❌ Status:', error.response?.status);
      alert(`Error: ${error.response?.data?.message || error.message || 'Failed to save'}`);
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
          const value = formData[fieldName] !== undefined ? formData[fieldName] : '';

          return (
            <div key={field.name} style={{ marginBottom: '16px' }}>
              <label style={{ display: 'block', marginBottom: '4px', fontWeight: '500' }}>
                {field.form?.label || field.name}
                {field.form?.required && <span style={{ color: 'red' }}>*</span>}
              </label>

              {renderField(field, fieldName, value, handleChange, dropdownOptions)}
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
  onChange: (name: string, value: any) => void,
  dropdownOptions: Record<string, any[]>
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
          value={value || ''}
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
          value={value || ''}
          onChange={(e) => onChange(fieldName, e.target.valueAsNumber || null)}
          placeholder={field.form?.placeholder}
          required={field.form?.required}
          style={baseStyle}
        />
      );

    case 'email':
      return (
        <input
          type="email"
          value={value || ''}
          onChange={(e) => onChange(fieldName, e.target.value)}
          placeholder={field.form?.placeholder}
          maxLength={field.form?.maxLength}
          required={field.form?.required}
          style={baseStyle}
        />
      );

    case 'password':
      return (
        <input
          type="password"
          value={value || ''}
          onChange={(e) => onChange(fieldName, e.target.value)}
          placeholder={field.form?.placeholder}
          maxLength={field.form?.maxLength}
          required={field.form?.required}
          style={baseStyle}
        />
      );

    case 'date':
      // Convert ISO string or date to YYYY-MM-DD format
      let dateValue = '';
      if (value) {
        try {
          const date = new Date(value);
          if (!isNaN(date.getTime())) {
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            dateValue = `${year}-${month}-${day}`;
          }
        } catch (e) {
          // If parsing fails, try to use value as-is if it's already in YYYY-MM-DD format
          if (typeof value === 'string' && value.match(/^\d{4}-\d{2}-\d{2}/)) {
            dateValue = value.split('T')[0];
          }
        }
      }

      return (
        <input
          type="date"
          value={dateValue}
          onChange={(e) => onChange(fieldName, e.target.value)}
          required={field.form?.required}
          style={baseStyle}
        />
      );

    case 'datetime':
      // Convert ISO string to datetime-local format (YYYY-MM-DDTHH:mm)
      let datetimeValue = '';
      if (value) {
        try {
          const date = new Date(value);
          if (!isNaN(date.getTime())) {
            // Format: YYYY-MM-DDTHH:mm
            const year = date.getFullYear();
            const month = String(date.getMonth() + 1).padStart(2, '0');
            const day = String(date.getDate()).padStart(2, '0');
            const hours = String(date.getHours()).padStart(2, '0');
            const minutes = String(date.getMinutes()).padStart(2, '0');
            datetimeValue = `${year}-${month}-${day}T${hours}:${minutes}`;
            console.log(`📅 DateTime field "${fieldName}": ${value} → ${datetimeValue}`);
          }
        } catch (e) {
          console.error(`❌ Error parsing datetime for "${fieldName}":`, e);
        }
      }

      return (
        <input
          type="datetime-local"
          value={datetimeValue}
          onChange={(e) => {
            // Convert to ISO string for backend
            const inputValue = e.target.value;
            console.log(`📅 DateTime changed "${fieldName}":`, inputValue);
            if (inputValue) {
              const isoString = new Date(inputValue).toISOString();
              console.log(`📅 ISO String:`, isoString);
              onChange(fieldName, isoString);
            } else {
              onChange(fieldName, null);
            }
          }}
          required={field.form?.required}
          style={baseStyle}
        />
      );

    case 'checkbox':
      return (
        <input
          type="checkbox"
          checked={!!value}
          onChange={(e) => onChange(fieldName, e.target.checked)}
          style={{ width: 'auto' }}
        />
      );

    case 'dropdown':
      const options = field.form?.relatedEntity 
        ? dropdownOptions[field.name] || []
        : field.form?.options || [];

      // Helper function to get property value case-insensitively
      const getPropValue = (obj: any, propName: string) => {
        if (!obj || !propName) return '';
        // Try exact match first
        if (obj[propName] !== undefined) return obj[propName];
        // Try lowercase
        const lowerProp = propName.toLowerCase();
        const foundKey = Object.keys(obj).find(k => k.toLowerCase() === lowerProp);
        return foundKey ? obj[foundKey] : '';
      };

      return (
        <select
          value={value || ''}
          onChange={(e) => {
            const newValue = e.target.value;
            onChange(fieldName, field.form?.relatedEntity ? (newValue ? parseInt(newValue) : null) : newValue);
          }}
          required={field.form?.required}
          style={baseStyle}
        >
          <option value="">-- Select {field.form?.label} --</option>
          {field.form?.relatedEntity ? (
            options.map((opt: any, index: number) => {
              const valueKey = field.form?.valueProperty || 'Id';
              const displayKey = field.form?.displayProperty || 'Name';
              const optValue = getPropValue(opt, valueKey);
              const optLabel = getPropValue(opt, displayKey);

              return (
                <option key={optValue || index} value={optValue}>
                  {optLabel || `Item ${index + 1}`}
                </option>
              );
            })
          ) : (
            options.map((opt: any, index: number) => (
              <option key={opt.value || index} value={opt.value}>
                {opt.label}
              </option>
            ))
          )}
        </select>
      );

    case 'multiselect':
      // Handle both string arrays and OptionMetadata objects
      const rawMultiOptions = field.form?.options || [];
      console.log(`🔲 MultiSelect field "${fieldName}":`, {
        rawOptions: rawMultiOptions,
        rawOptionsType: typeof rawMultiOptions,
        isArray: Array.isArray(rawMultiOptions),
        length: rawMultiOptions.length
      });

      const multiOptions = rawMultiOptions.map((opt: any) => {
        if (typeof opt === 'string') {
          return { label: opt, value: opt };
        }
        return opt; // Already an object with label/value
      });

      console.log(`🔲 Processed options:`, multiOptions);

      const selectedValues = value ? value.split(',').map((v: string) => v.trim()) : [];

      if (multiOptions.length === 0) {
        return (
          <div style={{ ...baseStyle, padding: '12px', color: '#999' }}>
            ⚠️ No options available for {field.form?.label}
          </div>
        );
      }

      return (
        <div style={{ 
          ...baseStyle, 
          padding: '12px', 
          maxHeight: '200px', 
          overflowY: 'auto',
          backgroundColor: '#f9f9f9'
        }}>
          <div style={{ marginBottom: '8px', fontWeight: 'bold', fontSize: '12px', color: '#666' }}>
            {field.form?.label || fieldName} ({multiOptions.length} options)
          </div>
          {multiOptions.map((opt: any, index: number) => {
            const optValue = opt.value;
            const optLabel = opt.label;

            return (
              <div key={optValue || index} style={{ marginBottom: '8px' }}>
                <label style={{ 
                  display: 'flex', 
                  alignItems: 'center', 
                  fontWeight: 'normal', 
                  cursor: 'pointer',
                  padding: '4px',
                  borderRadius: '4px',
                  transition: 'background-color 0.2s'
                }}>
                  <input
                    type="checkbox"
                    checked={selectedValues.includes(optValue)}
                    onChange={(e) => {
                      let newValues = [...selectedValues];
                      if (e.target.checked) {
                        newValues.push(optValue);
                      } else {
                        newValues = newValues.filter(v => v !== optValue);
                      }
                      console.log(`✅ MultiSelect changed: ${newValues.join(', ')}`);
                      onChange(fieldName, newValues.join(', '));
                    }}
                    style={{ marginRight: '8px', cursor: 'pointer' }}
                  />
                  <span>{optLabel}</span>
                </label>
              </div>
            );
          })}
          {selectedValues.length > 0 && (
            <div style={{ 
              marginTop: '12px', 
              paddingTop: '8px', 
              borderTop: '1px solid #ddd',
              fontSize: '12px',
              color: '#666'
            }}>
              ✅ Selected: {selectedValues.join(', ')}
            </div>
          )}
        </div>
      );

    case 'radio':
      const radioOptions = field.form?.options || [];

      return (
        <div>
          {radioOptions.map((opt: any) => {
            const optValue = typeof opt === 'string' ? opt : opt.value;
            const optLabel = typeof opt === 'string' ? opt : opt.label;

            return (
              <div key={optValue} style={{ marginBottom: '8px' }}>
                <label style={{ display: 'flex', alignItems: 'center', cursor: 'pointer' }}>
                  <input
                    type="radio"
                    name={fieldName}
                    value={optValue}
                    checked={value === optValue}
                    onChange={(e) => onChange(fieldName, e.target.value)}
                    required={field.form?.required}
                    style={{ marginRight: '8px' }}
                  />
                  {optLabel}
                </label>
              </div>
            );
          })}
        </div>
      );

    case 'file':
      return (
        <div>
          <input
            type="file"
            accept={field.form?.accept}
            multiple={field.form?.multiple}
            onChange={(e) => {
              const file = e.target.files?.[0];
              if (file) {
                // For now, just store the filename
                // In production, you'd upload to server and get URL
                onChange(fieldName, file.name);
                console.log('📁 File selected:', file);
              }
            }}
            style={{ ...baseStyle, padding: '6px' }}
          />
          {value && (
            <div style={{ marginTop: '8px', fontSize: '12px', color: '#666' }}>
              Current: {value}
            </div>
          )}
        </div>
      );

    default:
      return (
        <input
          type="text"
          value={value || ''}
          onChange={(e) => onChange(fieldName, e.target.value)}
          placeholder={field.form?.placeholder}
          maxLength={field.form?.maxLength}
          required={field.form?.required}
          style={baseStyle}
        />
      );
  }
}
