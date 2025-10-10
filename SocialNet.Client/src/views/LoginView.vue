<script>
import { useLoginStore } from '@/stores/login.js';
import { useRouter } from 'vue-router'
import { useToast } from 'vue-toastification'

export default {
    data() {
        const States = {
            SignIn: "SignIn",
            SignUp: "SignUp"
        };

        return {
            currentBlock: States.SignIn,
            States,
            firstName: '',
            lastName: '',
            email: '',
            password: '',
            signUpSuccessfullResult: false,
            emailToConfirm: ''
        };
    },
    setup() {
		const loginStore = useLoginStore();
		const router = useRouter()
        const toast = useToast();

		return {
			doLogin: loginStore.doLogin,
			doSignUp: loginStore.signUp,
			router,
            toast
		};
	},
    methods: {
        isValidEmail(email) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email || '').trim());
        },
        isValidPassword(password) {
            const p = String(password || '');
            const rules = [
                /.{8,}/,
                /[A-Z]/,
                /\d/,
                /[^A-Za-z0-9]/
            ];
            return rules.every(r => r.test(p));
        },
        isAlphaNumLatin(value) {
            return /^[A-Za-z0-9]+$/.test(String(value || ''));
        },
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
                s = s.replace(/\r/g, '');
                let firstLine = s.split('\n')[0];
                if (firstLine.includes(':')) {
                    const idx = firstLine.lastIndexOf(':');
                    if (idx >= 0 && idx + 1 < firstLine.length) {
                        const tail = firstLine.slice(idx + 1).trim();
                        if (tail) firstLine = tail;
                    }
                }
                if (firstLine.length > 180) firstLine = firstLine.slice(0, 180) + '…';
                return firstLine;
            } catch {
                return fallback;
            }
        },
        validateSignUp() {
            const firstName = (this.firstName || '').trim();
            const lastName = (this.lastName || '').trim();
            const email = (this.email || '').trim();
            const password = this.password || '';

            if (!firstName) { this.toast.error('First name is required'); return false; }
            if (!this.isAlphaNumLatin(firstName)) { this.toast.error('First name may contain only Latin letters and digits'); return false; }
            if (!lastName) { this.toast.error('Last name is required'); return false; }
            if (!this.isAlphaNumLatin(lastName)) { this.toast.error('Last name may contain only Latin letters and digits'); return false; }
            if (!email) { this.toast.error('Email is required'); return false; }
            if (!this.isValidEmail(email)) { this.toast.error('Email is not valid'); return false; }
            if (!password) { this.toast.error('Password is required'); return false; }
            if (!this.isValidPassword(password)) {
                this.toast.error('Password must be 8+ chars, include 1 uppercase, 1 digit and 1 special symbol');
                return false;
            }
            return true;
        },
        async signIn() {
            const result = await this.doLogin({
                "email": this.email,
                "password": this.password
            });
            if (result && result.success) {
                this.clearData();
                this.router.push('/home');
            } else if (result && result.message) {
                this.toast.error(result.message);
            }
        },

        async signUp() {
            if (!this.validateSignUp()) return;
            try {
                const result = await this.doSignUp({
                    "email": this.email,
                    "password": this.password,
                    "firstName": this.firstName,
                    "lastName": this.lastName
                });

                if (result && result.success){
                    this.emailToConfirm = this.email;
                    this.clearData();
                    this.signUpSuccessfullResult = true;
                } else if (result && result.message) {
                    this.toast.error(result.message);
                }
            } catch (e) {
                let payload = e?.response?.data;
                if (payload && typeof payload === 'object' && typeof payload.text === 'function') {
                    try { payload = await payload.text(); } catch {}
                }
                const raw = this._getErrorMessage(payload, e?.message || 'Registration failed');
                const msg = this._prettifyMessage(raw, 'Registration failed');
                this.toast.error(msg);
            }
        },

        clearData() {
            this.email = '';
            this.password = '';
            this.firstName = '';
            this.lastName = '';
        },
        goToSignIn() {
            this.signUpSuccessfullResult = false;
            this.currentBlock = this.States.SignIn;
        }
    }
}
</script>

<template lang='pug'>
  div.login-view
    div.main
              div.container.a-container#a-container(v-if="currentBlock === States.SignIn && !signUpSuccessfullResult")
                  form(id="a-form", class="form" @submit.prevent)
                      h2.form_title.title Sign in to account
                      span.form__span use your email and password
                      input.form__input(type="email", placeholder="Email" v-model="email")
                      input.form__input(type="password", placeholder="Password" v-model="password")
                      button(type='button' @click="signIn").switch__button.button.switch-btn SIGN IN

              div.container.a-container#a-container(v-if="currentBlock !== States.SignIn && !signUpSuccessfullResult")
                  form(id="a-form", class="form" @submit.prevent)
                      h2.form_title.title Create Account
                      span.form__span use email for registration
                      input.form__input(type="text", placeholder="First Name" v-model="firstName")
                      input.form__input(type="text", placeholder="Last Name" v-model="lastName")
                      input.form__input(type="email", placeholder="Email" v-model="email")
                      input.form__input(type="password", placeholder="Password" v-model="password")
                      button(type='button' @click="signUp").switch__button.button.switch-btn SIGN UP
              
              div.container.a-container#a-container(v-if="signUpSuccessfullResult")
                  div.success-card
                      div.success-icon ✔
                      h2.form_title.title Account created!
                      p.description We sent a confirmation link to
                      p.description.email {{ emailToConfirm }}
                      p.description Please check your inbox and confirm your email to continue.
                      button(type='button' @click="goToSignIn").switch__button.button.switch-btn Go to sign in
                      
              div.switch#switch-cnt(v-if="currentBlock !== States.SignIn")
                  div.switch__circle
                  div.switch__circle.switch__circle--t
                  div.switch__container#switch-c1
                      h2.switch__title.title Welcome Back !
                      p.switch__description.description To keep connected with us please login with your personal info
                      button(type='button' @click="currentBlock = States.SignIn").switch__button.button.switch-btn SIGN IN

              div.switch#switch-cnt(v-else)
                  div.switch__circle
                  div.switch__circle.switch__circle--t
                  div.switch__container#switch-c1
                      h2.switch__title.title Welcome Back !
                      p.switch__description.description To keep connected with us please login with your personal info
                      button(type='button' @click="currentBlock = States.SignUp").switch__button.button.switch-btn SIGN UP
</template>
