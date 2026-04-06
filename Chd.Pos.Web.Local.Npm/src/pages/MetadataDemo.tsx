import React, { useEffect, useState } from 'react';
import axios from 'axios';

function FieldRenderer({ field }: { field: any }) {
  const t = field.form?.type || field.type;
  const label = field.form?.label || field.name;
  const [treeOptions, setTreeOptions] = React.useState<any[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);

  React.useEffect(() => {
    if (t === 'treeselect') {
      const entity = field.form?.treeEntity || field.form?.relatedEntity || 'categories';
      setIsLoading(true);
      // Try server-provided tree first
      axios.get(`/api/${entity}/tree`)
        .then(res => {
          const data = res.data || [];
          const hasChildren = Array.isArray(data) && data.some((n: any) => n.children && n.children.length > 0);
          if (hasChildren) {
            setTreeOptions(data);
            setIsLoading(false);
            return;
          }
          // Fallback: build tree from flat list
          return axios.get(`/api/${entity}`).then(r2 => {
            const flat = r2.data || [];
            // build lookup
            const lookup: any = {};
            flat.forEach((it: any) => {
              lookup[it.id] = { id: it.id, label: it.name || it.label || it.Name || it.Label, parentId: it.parentCategoryId ?? it.ParentCategoryId ?? it.ParentId ?? null, children: [] };
            });
            flat.forEach((it: any) => {
              const node = lookup[it.id];
              const p = node.parentId;
              if (p && lookup[p]) lookup[p].children.push(node);
            });
            const roots = Object.values(lookup).filter((n: any) => !n.parentId);
            setTreeOptions(roots as any[]);
            setIsLoading(false);
          });
        })
        .catch(() => { setTreeOptions([]); setIsLoading(false); });
    }
  }, [t, field]);

  switch (t) {
    case 'text':
    case 'string':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="text" style={{ display: 'block', width: '100%', padding: 8 }} />
        </div>
      );
    case 'number':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="number" style={{ display: 'block', width: '100%', padding: 8 }} />
        </div>
      );
    case 'date':
    case 'datetime':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="date" style={{ display: 'block', width: '100%', padding: 8 }} />
        </div>
      );
    case 'checkbox':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>
            <input type="checkbox" /> {label}
          </label>
        </div>
      );
    case 'file':
    case 'imagepreview':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="file" accept={field.form?.accept || '*/*'} />
        </div>
      );
    case 'multiselect':
    case 'select':
    case 'dropdown':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <select style={{ display: 'block', width: '100%', padding: 8 }} multiple={t === 'multiselect'}>
            {(field.form?.options || []).map((o: any) => (
              <option key={o.value} value={o.value}>{o.label}</option>
            ))}
          </select>
        </div>
      );
    case 'autocomplete':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="text" style={{ display: 'block', width: '100%', padding: 8 }} placeholder={`Search (min ${field.form?.autocompleteMinChars || 2})`} />
        </div>
      );
    case 'richtexteditor':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <textarea style={{ display: 'block', width: '100%', padding: 8, minHeight: 120 }} />
        </div>
      );
    case 'taginput':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="text" style={{ display: 'block', width: '100%', padding: 8 }} placeholder="Add tags separated by comma" />
        </div>
      );
    case 'treeselect':
      // build flat option list with indentation
      const buildOptions = (node: any, prefix = ''): JSX.Element[] => {
        const opts: JSX.Element[] = [];
        opts.push(<option key={node.id} value={node.id}>{prefix + node.label}</option>);
        if (node.children && node.children.length) {
          node.children.forEach((c: any) => {
            buildOptions(c, prefix + '— ').forEach(o => opts.push(o));
          });
        }
        return opts;
      };

      const options = treeOptions.flatMap((n: any) => buildOptions(n));

      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <select style={{ display: 'block', width: '100%', padding: 8 }}>
            {isLoading ? (
              <option>Loading...</option>
            ) : options.length === 0 ? (
              <option>No categories</option>
            ) : (
              options
            )}
          </select>
        </div>
      );
    case 'colorpicker':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="color" />
        </div>
      );
    case 'daterangepicker':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="date" /> - <input type="date" />
        </div>
      );
    case 'stepper':
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <div style={{ padding: 12, border: '1px dashed #ccc' }}>Stepper placeholder</div>
        </div>
      );
    default:
      return (
        <div style={{ marginBottom: 12 }}>
          <label>{label}</label>
          <input type="text" style={{ display: 'block', width: '100%', padding: 8 }} />
        </div>
      );
  }
}

export default function MetadataDemo() {
  const [fields, setFields] = useState<any[]>([]);

  useEffect(() => {
    axios.get('/api/metadata/ProductDto')
      .then(res => {
        const props = res.data.properties || res.data.properties || [];
        setFields(props);
      })
      .catch(err => console.error(err));
  }, []);

  return (
    <div style={{ padding: 24 }}>
      <h2>ProductDto Fields Demo</h2>
      <div>
        {fields.map(f => (
          <FieldRenderer key={f.name} field={f} />
        ))}
      </div>
    </div>
  );
}
