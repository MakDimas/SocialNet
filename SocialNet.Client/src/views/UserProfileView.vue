<script>
import { getUserByFullNameAndId } from '@/api/user.js'

export default {
  props: {
    fullName: { type: String, required: true },
    id: { type: Number, required: true }
  },
  data() {
    return {
      isLoading: false,
      error: '',
      user: null,
    };
  },
  async mounted() {
    await this.loadUser();
  },
  methods: {
    async loadUser() {
      this.isLoading = true;
      this.error = '';
      try {
        const resp = await getUserByFullNameAndId({ fullName: this.fullName, id: this.id });
        const data = resp?.data?.data || {};
        this.user = {
          id: data.id,
          userName: data.userName,
          firstName: data.firstName,
          lastName: data.lastName,
          email: data.email,
          phoneNumber: data.phoneNumber ?? '',
          description: data.description ?? '',
          age: data.age ?? null,
        };
      } catch (e) {
        this.error = 'Failed to load user';
        this.user = null;
      } finally {
        this.isLoading = false;
      }
    },
    goBack() {
      try {
        if (window.history.length > 1) {
          this.$router.back();
        } else {
          this.$router.push({ name: 'home' });
        }
      } catch {
        this.$router.push({ name: 'home' });
      }
    }
  }
}
</script>

<template lang='pug'>
div.main
  div.sidebar
    div.profile-card
      div.avatar {{ (user?.firstName?.[0] || 'U') + (user?.lastName?.[0] || '') }}
      h2.title {{ user?.firstName }} {{ user?.lastName }}
      p.description {{ user?.description || 'No description provided' }}
      div.info
        div.info-row
          span.label Email
          span.value {{ user?.email || '—' }}
        div.info-row
          span.label Phone
          span.value {{ user?.phoneNumber || '—' }}
        div.info-row
          span.label Age
          span.value {{ user?.age ?? '—' }}
  div.content
    div.up-panel
      div.left-buttons
        button.back-button(type="button" @click="goBack") Back
    div.post-form-card
      div(v-if="isLoading") Loading...
      div.error(v-else-if="error") {{ error }}
      div(v-else-if="user")
        h3.post-form-title User Profile
        ul.user-details
          li ID: {{ user.id }}
          li UserName: {{ user.userName || (user.firstName + ' ' + user.lastName).trim() }}
          li Email: {{ user.email }}
          li Phone: {{ user.phoneNumber || '—' }}
          li Age: {{ user.age ?? '—' }}
          li Description: {{ user.description || '—' }}
</template>
