import { defineStore } from 'pinia';
import env from './env.json';
import router from '@/router';
import { jwtDecode } from 'jwt-decode';

function safeParseUser() {
	try {
		const raw = localStorage.getItem('user');
		return raw ? JSON.parse(raw) : null;
	} catch (e) {
		console.warn('Invalid user in localStorage:', e);
		return null;
	}
}

export const useMainStore = defineStore('main', {
	state: () => ({
		baseUrl: env.baseUrl,
		user: safeParseUser(),
		token: localStorage.getItem('token') || '',
	}),
	getters: {
		isAuthenticated: (state) => !!state.token
	},
	actions: {
		isTokenValid() {
			const token = localStorage.getItem('token')
			if (!token) {
				this.removeToken()
				return false
			}

			try {
				const decoded = jwtDecode(token)
				const now = Math.floor(Date.now() / 1000);
				if (!decoded.exp || decoded.exp <= now) {
					this.removeToken()
					return false
				}
				return true
			} catch (err) {
				this.removeToken()
				return false
			}
		},
		setToken(token) {
			const _ = this;
			localStorage.setItem('token', token)
			_.token = token;
		},
		async removeToken() {
			localStorage.removeItem('token')
			this.token = '';
			this.user = null;
			router.push('/login');
		},
		setUser(user) {
			localStorage.setItem('user', JSON.stringify(user))
			this.user = user
		}
	}
});
