import axios from 'axios';
import { EntityMetadata } from '../types/metadata';

const API_BASE = 'http://localhost:5218/api';

export const metadataService = {
  async getAllMetadata(): Promise<EntityMetadata[]> {
    const response = await axios.get(`${API_BASE}/metadata`);
    return response.data;
  },

  async getMetadata(entityName: string): Promise<EntityMetadata> {
    const response = await axios.get(`${API_BASE}/metadata/${entityName}`);
    return response.data;
  }
};

export const crudService = {
  async getAll<T>(endpoint: string): Promise<T[]> {
    const response = await axios.get(`${API_BASE}/${endpoint}`);
    return response.data;
  },

  async getById<T>(endpoint: string, id: number): Promise<T> {
    const response = await axios.get(`${API_BASE}/${endpoint}/${id}`);
    return response.data;
  },

  async create<T>(endpoint: string, data: Partial<T>): Promise<T> {
    const response = await axios.post(`${API_BASE}/${endpoint}`, data);
    return response.data;
  },

  async update<T>(endpoint: string, id: number, data: Partial<T>): Promise<void> {
    await axios.put(`${API_BASE}/${endpoint}/${id}`, data);
  },

  async delete(endpoint: string, id: number): Promise<void> {
    await axios.delete(`${API_BASE}/${endpoint}/${id}`);
  }
};
