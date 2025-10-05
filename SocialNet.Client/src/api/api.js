import axios from 'axios';
import { useMainStore } from '@/stores/main.js';
import router from '../router'

const api = axios.create();

api.interceptors.request.use(config => {
    const store = useMainStore();
    config.baseURL = store.baseUrl;
    
    if (config.skipAuth) return config;

    if (!store.isAuthenticated) return config;

    if (!store.isTokenValid()) {
        store.removeToken();
        return Promise.reject(new axios.Cancel('Token expired'));
    }

	const token = localStorage.getItem('token');
	if (token) {
		config.headers.Authorization = `Bearer ${token}`;
	}

    return config;
}, error => {
    const store = useMainStore();
    return Promise.reject(error);
});

api.interceptors.response.use(
    response =>  response,
    error => {
        const mainStore = useMainStore();

        if (error.response && error.response.status === 401) {
            mainStore.removeToken();
            router.push('/login');
        } else {
        }

        return Promise.reject(error);
    }
);

export default api;
