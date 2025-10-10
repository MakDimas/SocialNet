<script>
import { useLoginStore } from '@/stores/login.js';
import { useRouter } from 'vue-router'
import AddPost from '@/components/AddPost.vue'
import ShowPosts from '@/components/ShowPosts.vue'

export default {
  components: { AddPost, ShowPosts },
  data() {
    const raw = localStorage.getItem('user');
    let parsed = {};
    try { parsed = raw ? JSON.parse(raw) : {}; } catch (e) { parsed = {}; }

    const firstName = parsed.firstName || '';
    const lastName = parsed.lastName || '';
    const email = parsed.email || '';
    const phoneNumber = parsed.phoneNumber || '';
    const age = parsed.age || '';
    const description = parsed.description || '';
    const userId = parsed.id || '';

    return {
      profile: { firstName, lastName, email, phoneNumber, age, description },
      userId,
      showPostForm: false,
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
      this.showPostForm = true;
      this.$nextTick(() => {
        if (typeof window !== 'undefined') {
          window.scrollTo({ top: 0, behavior: 'smooth' });
        }
      });
    },
    closePostForm() {
      this.showPostForm = false;
    },
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
        div.left-buttons
          button.add-post(type="btn" @click="openPostForm") Add Post
          button.show-posts(type="btn") Show Posts
        router-link.settings-btn(:to="{ name: 'settings' }") Account Settings
        button.logout-button(type="btn" @click="logout") Logout
      AddPost(
        v-if="showPostForm"
        :profile="profile"
        :userId="userId"
        @created="() => { closePostForm(); $refs.posts && ($refs.posts.pageNumber = 1); $refs.posts && $refs.posts.loadPosts && $refs.posts.loadPosts(); $refs.posts && $refs.posts.clearPreview && $refs.posts.clearPreview() }"
        @close="() => { closePostForm(); $refs.posts && $refs.posts.clearPreview && $refs.posts.clearPreview() }"
        @preview="$refs.posts?.togglePreview ? $refs.posts.togglePreview($event) : $refs.posts?.addPreview($event)"
      )
      ShowPosts(ref="posts" :profile="profile" :user-id="userId")
</template>
