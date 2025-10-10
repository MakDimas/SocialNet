<script>
import { useRoute, useRouter } from 'vue-router'
import { confirmEmail } from '@/api/login.js'

export default {
  data() {
    return {
      loading: true,
      success: false,
      error: ''
    }
  },
  components: {
  },
  setup() {
    const route = useRoute()
    const router = useRouter()

    return { route, router }
  },
  methods: {
    goToLogin() {
      this.router.push('/login')
    },
  },
  async mounted() {
    try {
        const email = this.route.query.email || '';
        const token = this.route.query.token || '';

        if (!email || !token) {
          this.error = 'Invalid confirmation link. Missing email or token parameters.'
          this.loading = false
          return
        }
        
        await confirmEmail(email, token);
        this.success = true
    } catch (err) {
        this.error = err.response?.data?.message || 'Email confirmation failed. Please try again.'
    } finally {
        this.loading = false
    }
  }
}
</script>

<template lang="pug">
.email-confirmation-page
  .confirmation-container(v-if="loading")
    .loading-spinner
    h2.confirmation-title Confirming your email...
    p.confirmation-subtitle Please wait a moment

  .confirmation-container.success-state(v-if="success")
    .success-icon-container
      div.success-icon ✔
    h1.confirmation-title Email Confirmed Successfully!
    p.confirmation-subtitle 
      | Your email has been verified. You can now access all features of our reservation system.
    button.cta-button(@click="goToLogin") Go to Login

  .confirmation-container.error-state(v-if="error")
    .error-icon-container
      .error-icon ✕
    h2.confirmation-title.error-title Confirmation Failed
    p.confirmation-subtitle.error-subtitle {{ error }}
    button.cta-button.secondary(@click="goToLogin") Back to Login
</template>
