import { defineStore } from 'pinia';
import { doLogin as apiLogin, signUp as apiSignUp } from '@/api/login.js';
import { jwtDecode } from 'jwt-decode';

export const useLoginStore = defineStore('login', {
	state: () => ({
		user: localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')) : null,
		isAuthenticated: !!localStorage.getItem('token') && !!localStorage.getItem('user')
	}),
	getters: {
		userData: (state) => state.user
	},
	actions: {
		async doLogin(data) {
			try {
				let response = await apiLogin(data);
				if (response.data.status == 200) {
					let token = response.data.data;
					const decoded = jwtDecode(token);
					const user = {
						id: decoded.sub,
						email: decoded.email,
						username: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
						firstName: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname"],
						lastName: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname"],
						phone: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone"],
						description: decoded.description,
						age: decoded.age
					}

					localStorage.setItem('token', token);
					localStorage.setItem('user', JSON.stringify(user));

					this.user = user;
					this.isAuthenticated = true;

					return true;
				}
				return false;
			} catch (error) {
				console.error('Login store: Login error:', error);
				throw error;
			}
		},
		async signUp(signUpData) {
			let response = await apiSignUp(signUpData);

			if (response.data.status == 200) return true;
			else return false;
		},
		logout() {
			this.user = {};
			this.isAuthenticated = false;
			localStorage.removeItem('token');
			localStorage.removeItem('user');
		}
	}
});