export interface EntityMetadata {
  entityName: string;
  title: string;
  icon: string;
  route: string;
  description: string;
  properties: PropertyMetadata[];
}

export interface PropertyMetadata {
  name: string;
  type: string;
  grid?: GridMetadata;
  form?: FormMetadata;
}

export interface GridMetadata {
  order: number;
  width: number;
  sortable: boolean;
  filterable: boolean;
  format?: string;
  hidden: boolean;
}

export interface FormMetadata {
  label?: string;
  type: string;
  required: boolean;
  maxLength?: number;
  placeholder?: string;
  order: number;
  readOnly: boolean;
  validationPattern?: string;

  // For Dropdown, Radio, MultiSelect
  relatedEntity?: string;
  displayProperty?: string;
  valueProperty?: string;
  options?: OptionMetadata[];

  // For File upload
  accept?: string;
  multiple?: boolean;
}

export interface OptionMetadata {
  label: string;
  value: string;
}
