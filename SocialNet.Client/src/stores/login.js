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
		_getErrorMessage(payload, fallback = 'Request failed') {
			try {
				if (!payload) return fallback;
				if (typeof payload === 'string') return payload;
				const msg = payload.message || payload.Message || payload.error || payload.Error || payload.detail || payload.Detail || payload.title || payload.Title;
				if (msg) return String(msg);
				const errs = payload.errors || payload.Errors || payload.errorMessages || payload.ErrorMessages;
				if (Array.isArray(errs) && errs.length) return errs.map(e => (typeof e === 'string' ? e : JSON.stringify(e))).join('\n');
				if (errs && typeof errs === 'object') {
					const firstKey = Object.keys(errs)[0];
					if (firstKey) {
						const val = errs[firstKey];
						if (Array.isArray(val)) return val.join('\n');
						return String(val);
					}
				}
				return fallback;
			} catch {
				return fallback;
			}
		},
		_prettifyMessage(message, fallback = 'Request failed') {
			try {
				let s = String(message || '').trim();
				if (!s) return fallback;
				// normalize and strip basic HTML
				s = s.replace(/\r/g, '').replace(/<[^>]*>/g, ' ');
				// Known backend pattern
				let m = s.match(/Error during user registration:\s*([^\n<]+)/i);
				if (m && m[1]) return m[1].trim();
				m = s.match(/System\.[A-Za-z]+Exception:\s*([^\n<]+)/i);
				if (m && m[1]) {
					let t = m[1].trim();
					if (t.includes(':')) t = t.slice(t.lastIndexOf(':') + 1).trim();
					if (t) return t;
				}
				// Fallback: first non-empty line, then take text after last colon
				let firstLine = (s.split('\n').find(l => l && l.trim().length) || '').trim();
				if (firstLine.includes(':')) {
					const idx = firstLine.lastIndexOf(':');
					if (idx >= 0 && idx + 1 < firstLine.length) {
						const tail = firstLine.slice(idx + 1).trim();
						if (tail) firstLine = tail;
					}
				}
				if (firstLine.length > 180) firstLine = firstLine.slice(0, 180) + '…';
				return firstLine || fallback;
			} catch {
				return fallback;
			}
		},
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

					return { success: true };
				}
				let payload = response?.data;
				if (payload && typeof payload === 'object' && typeof payload.text === 'function') {
					try { payload = await payload.text(); } catch {}
				}
				const message = this._prettifyMessage(this._getErrorMessage(payload, 'Login failed'), 'Login failed');
				return { success: false, message };
			} catch (error) {
				console.error('Login store: Login error:', error);
				let payload = error?.response?.data;
				if (payload && typeof payload === 'object' && typeof payload.text === 'function') {
					try { payload = await payload.text(); } catch {}
				}
				const message = this._prettifyMessage(this._getErrorMessage(payload, error?.message || 'Login failed'), 'Login failed');
				return { success: false, message };
			}
		},
		async signUp(signUpData) {
			try {
				let response = await apiSignUp(signUpData);
				if (response?.data?.status == 200) {
					return { success: true };
				}
				let payload = response?.data;
				if (payload && typeof payload === 'object' && typeof payload.text === 'function') {
					try { payload = await payload.text(); } catch {}
				}
				const message = this._prettifyMessage(this._getErrorMessage(payload, 'Registration failed'), 'Registration failed');
				return { success: false, message };
			} catch (error) {
				let payload = error?.response?.data;
				if (payload && typeof payload === 'object' && typeof payload.text === 'function') {
					try { payload = await payload.text(); } catch {}
				}
				const message = this._prettifyMessage(this._getErrorMessage(payload, error?.message || 'Registration failed'), 'Registration failed');
				return { success: false, message };
			}
		},
		logout() {
			this.user = {};
			this.isAuthenticated = false;
			localStorage.removeItem('token');
			localStorage.removeItem('user');
		}
	}
});