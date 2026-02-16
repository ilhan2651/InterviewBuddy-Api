// src/services/api.js
import axios from 'axios';

// Base API URL - Backend'in çalıştığı adres
const API_URL = 'http://localhost:5000/api';

// Axios instance with authentication
const apiClient = axios.create({
    baseURL: API_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Request interceptor - JWT token ekleme
apiClient.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor - Hata yakalama
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

// === AUTH ENDPOINTS ===
export const login = (email, password) =>
    apiClient.post('/auth/login', { email, password });

export const register = (userData) =>
    apiClient.post('/auth/register', userData);

export const logout = () => {
    localStorage.removeItem('token');
    window.location.href = '/login';
};

// === INTERVIEW ENDPOINTS ===
export const startInterview = (data) =>
    apiClient.post('/interview/start', data);

export const getCurrentQuestion = (sessionId) =>
    apiClient.get(`/interview/${sessionId}/current-question`);

export const submitAnswer = (formData) =>
    apiClient.post('/interview/submit-answer', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
    });

export const getInterviewReport = (sessionId) =>
    apiClient.get(`/interview/${sessionId}/report`);

// === USER ENDPOINTS ===
export const getUserStats = () =>
    apiClient.get('/user/stats');

export const getRecentInterviews = () =>
    apiClient.get('/user/recent-interviews');

export default apiClient;
