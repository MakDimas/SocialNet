<script>
import { useLoginStore } from '@/stores/login.js';
import { useRouter } from 'vue-router'

export default {
  data() {
    const raw = localStorage.getItem('user');
    let parsed = {};
    try { parsed = raw ? JSON.parse(raw) : {}; } catch (e) { parsed = {}; }

    const firstName = parsed.firstName || parsed.givenName || '';
    const lastName = parsed.lastName || parsed.surname || '';
    const email = parsed.email || '';
    const phoneNumber = parsed.phoneNumber || parsed.phone || '';
    const age = parsed.age || '';
    const description = parsed.description || '';

    return {
      profile: { firstName, lastName, email, phoneNumber, age, description },
      showPostForm: false,
      postForm: {
        firstName,
        lastName,
        email,
        homepage: '',
        captchaInput: '',
        text: ''
      },
      captchaCode: ''
    };
  },
  setup(){
    const loginStore = useLoginStore();
    const router = useRouter()

    return{
      storeLogout: loginStore.logout,
      router
    }
  },
  methods: {
    logout() {
      this.storeLogout();
      this.$nextTick(() => {
        this.$router.push('/login');
      });
    },
    openPostForm() {
      if (this.showPostForm) return;
      this.postForm = {
        firstName: this.profile.firstName || '',
        lastName: this.profile.lastName || '',
        email: this.profile.email || '',
        homepage: '',
        captchaInput: '',
        text: ''
      };
      this.captchaCode = this.generateCaptcha();
      this.showPostForm = true;
    },
    closePostForm() {
      this.showPostForm = false;
    },
    generateCaptcha() {
      const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
      let s = '';
      for (let i = 0; i < 6; i++) s += chars[Math.floor(Math.random() * chars.length)];
      return s;
    },
    isValidUrl(value) {
      if (!value) return true;
      try { new URL(value); return true; } catch { return false; }
    },
    submitPost() {
      const { firstName, lastName, email, homepage, captchaInput, text } = this.postForm;
      if (!firstName || !lastName || !email) {
        alert('User name and email are required');
        return;
      }
      if (!text || !text.trim()) {
        alert('Text is required');
        return;
      }
      if (!this.isValidUrl(homepage)) {
        alert('Home page must be a valid URL');
        return;
      }
      if (!captchaInput || captchaInput.trim().length === 0) {
        alert('CAPTCHA is required');
        return;
      }
      // Stub: emulate success
      console.log('Post created:', { firstName, lastName, email, homepage, text });
      this.closePostForm();
    }
  },
  computed: {
    fullName() {
      const { firstName, lastName } = this.profile;
      return [firstName, lastName].filter(Boolean).join(' ') || 'User';
    },
    initials() {
      const { firstName, lastName } = this.profile;
      const a = (firstName || '').trim().charAt(0).toUpperCase();
      const b = (lastName || '').trim().charAt(0).toUpperCase();
      return (a + b) || 'U';
    }
  }
}
</script>

<template lang='pug'>
  div.main
    div.sidebar
      div.profile-card
        div.avatar {{ initials }}
        h2.title {{ fullName }}
        p.description {{ profile.description || 'No description provided' }}
        div.info
          div.info-row
            span.label Email
            span.value {{ profile.email || '—' }}
          div.info-row
            span.label Phone
            span.value {{ profile.phoneNumber || '—' }}
          div.info-row
            span.label Age
            span.value {{ profile.age || '—' }}
    div.content
      div.up-panel
        button.add-post(type="btn" @click="openPostForm") Add Post
        button.logout-button(type="btn" @click="logout") Logout
      // Post form card
      div.post-form-card(v-if="showPostForm")
        div.post-form-header
          h3.post-form-title Create Post
          button.post-form-close(@click="closePostForm") ✕
        form.post-form(@submit.prevent="submitPost")
          // User Name
          div.form-row
            label.form-label User Name
            input.form-input(type="text" :value="`${postForm.firstName} ${postForm.lastName}`.trim()" disabled required)
          // Email
          div.form-row
            label.form-label Email
            input.form-input(type="email" :value="postForm.email" disabled required)
          // Home page
          div.form-row
            label.form-label Home page (optional)
            input.form-input(type="url" v-model="postForm.homepage" placeholder="https://example.com")
          // CAPTCHA (stub)
          div.form-row.captcha-row
            label.form-label CAPTCHA
            div.captcha-box {{ captchaCode }}
            input.form-input(type="text" v-model="postForm.captchaInput" placeholder="Enter code" required)
          // Text
          div.form-row
            label.form-label Text
            textarea.form-textarea(v-model="postForm.text" rows="5" placeholder="Write your post..." required)
          div.form-actions
            button.primary-button(type="submit") Publish
            button.secondary-button(type="button" @click="closePostForm") Cancel
</template>

<style lang='stylus'>

transition = all .25s ease
neu1 = #eef2f7
neu2 = #d1d9e6
purple = #5b21b6
textGray = #6b7280

*, *::after, *::before
  margin 0
  padding 0
  box-sizing border-box

body
  width 100%
  height 100vh
  display flex
  justify-content center
  align-items center
  font-family "Manrope", sans-serif
  font-size 12px
  background-color neu1
  color textGray

.main
  position relative
  width 100vw
  left 0
  min-height 100vh
  padding 24px
  background-color neu1
  display grid
  grid-template-columns 420px 1fr
  grid-gap 24px

.sidebar
  height calc(100vh - 48px)

.content
  background #fff
  border-radius 20px
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff
  min-height calc(100vh - 48px)

.post-form-card
  margin 18px auto
  padding 24px
  width 100%
  max-width 900px
  background #fff
  border-radius 16px
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff

.post-form-header
  display flex
  justify-content space-between
  align-items center
  margin-bottom 12px

.post-form-title
  font-size 20px
  font-weight 800
  color #111827

.post-form-close
  width 40px
  height 40px
  border-radius 12px
  border none
  background neu1
  box-shadow inset 2px 2px 4px neu2, inset -2px -2px 4px #ffffff
  cursor pointer

.post-form
  display flex
  flex-direction column
  gap 16px

.form-row
  display flex
  flex-direction column

.form-label
  font-weight 700
  color #111827
  margin-bottom 6px

.form-input
  width 100%
  height 44px
  padding 0 14px
  font-size 14px
  border none
  outline none
  background-color neu1
  border-radius 10px
  box-shadow inset 2px 2px 4px neu2, inset -2px -2px 4px #ffffff

.form-textarea
  width 100%
  padding 12px 14px
  font-size 14px
  border none
  outline none
  background-color neu1
  border-radius 12px
  box-shadow inset 2px 2px 4px neu2, inset -2px -2px 4px #ffffff
  resize vertical

.captcha-row
  display flex
  flex-direction column
  gap 8px

.captcha-box
  height 44px
  display flex
  align-items center
  justify-content center
  font-weight 800
  letter-spacing 2px
  background neu1
  border-radius 10px
  box-shadow inset 2px 2px 4px neu2, inset -2px -2px 4px #ffffff

.form-actions
  grid-column 1 / -1
  display flex
  justify-content flex-end
  gap 12px
  margin-top 4px

.primary-button
  display inline-flex
  align-items center
  justify-content center
  padding 12px 18px
  height 44px
  min-width 140px
  background purple
  color #fff
  border none
  border-radius 12px
  font-weight 800
  font-size 14px
  letter-spacing .5px
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff
  cursor pointer
  transition transition
  &:hover
    box-shadow 6px 6px 12px neu2, -6px -6px 12px #ffffff
    transform scale(0.985)
  &:active,
  &:focus
    box-shadow 2px 2px 6px neu2, -2px -2px 6px #ffffff
    transform scale(0.97)

.secondary-button
  display inline-flex
  align-items center
  justify-content center
  padding 12px 18px
  height 44px
  min-width 120px
  background #111827
  color #fff
  border none
  border-radius 12px
  font-weight 800
  font-size 14px
  letter-spacing .5px
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff
  cursor pointer
  transition transition
  &:hover
    box-shadow 6px 6px 12px neu2, -6px -6px 12px #ffffff
    transform scale(0.985)
  &:active,
  &:focus
    box-shadow 2px 2px 6px neu2, -2px -2px 6px #ffffff
    transform scale(0.97)

.profile-card
  width 100%
  padding 32px
  border-radius 20px
  background #fff
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff
  display flex
  flex-direction column
  align-items center
  text-align center

.avatar
  width 120px
  height 120px
  border-radius 50%
  background neu1
  box-shadow inset 6px 6px 12px neu2, inset -6px -6px 12px #ffffff
  display flex
  align-items center
  justify-content center
  font-weight 800
  font-size 42px
  color purple
  margin-bottom 18px

.title
  font-size 36px
  font-weight 800
  color #111827
  margin-bottom 8px

.description
  font-size 14px
  color #374151
  margin-bottom 24px

.info
  width 100%
  margin-top 8px

.info-row
  display flex
  justify-content space-between
  align-items center
  background neu1
  box-shadow inset 2px 2px 4px neu2, inset -2px -2px 4px #ffffff
  border-radius 12px
  padding 14px 18px
  margin 8px 0

.label
  font-weight 700
  color #111827

.value
  color #111827

.up-panel
  display flex
  justify-content space-between
  align-items center
  width 95%
  margin 16px auto 0
  padding 16px 18px
  background #fff
  border-radius 16px
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff

danger = #e3363c

.add-post
  display inline-flex
  align-items center
  justify-content center
  padding 12px 18px
  height 44px
  min-width 140px
  background purple
  color #fff
  border none
  border-radius 12px
  font-weight 800
  font-size 14px
  letter-spacing .5px
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff
  cursor pointer
  transition transition
  &:hover
    box-shadow 6px 6px 12px neu2, -6px -6px 12px #ffffff
    transform scale(0.985)
  &:active,
  &:focus
    box-shadow 2px 2px 6px neu2, -2px -2px 6px #ffffff
    transform scale(0.97)

.logout-button
  display inline-flex
  align-items center
  justify-content center
  padding 12px 18px
  height 44px
  min-width 140px
  background danger
  color #fff
  border none
  border-radius 12px
  font-weight 800
  font-size 14px
  letter-spacing .5px
  box-shadow 8px 8px 16px neu2, -8px -8px 16px #ffffff
  cursor pointer
  transition transition
  &:hover
    box-shadow 6px 6px 12px neu2, -6px -6px 12px #ffffff
    transform scale(0.985)
  &:active,
  &:focus
    box-shadow 2px 2px 6px neu2, -2px -2px 6px #ffffff
    transform scale(0.97)

</style>
