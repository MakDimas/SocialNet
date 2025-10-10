<script>
export default {
  props: {
    reply: { type: Object, required: true },
    level: { type: Number, default: 1 },
    contentFromAttachment: { type: Function, required: true },
    openImage: { type: Function, required: true }
  },
  methods: {
    openHome(url) {
      if (!url) return;
      try {
        const u = new URL(url, window.location.origin);
        if (u.origin === window.location.origin) {
          const path = u.pathname + u.search + u.hash;
          this.$router.push(path);
        } else {
          window.location.href = url;
        }
      } catch {
        window.location.href = url;
      }
    },
    getAttachment(obj) {
      return obj?.attachment || obj?.Attachment || null;
    },
    getFileName(att) {
      return att?.fileName || att?.FileName || '';
    },
    getFileSize(att) {
      return att?.fileSize || att?.FileSize || 0;
    },
    formatDate(dateString) {
      if (!dateString) return '';
      try {
        const date = new Date(dateString);
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        const hours = String(date.getHours()).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        return `${day}.${month}.${year} at ${hours}:${minutes}`;
      } catch {
        return '';
      }
    },
    getHomeUrl(p) {
      return p?.homePage || p?.homepage || p?.homepageUrl || p?.homePageUrl || p?.HomePage || p?.Homepage || p?.HomepageUrl || p?.HomePageUrl || '';
    }
  }
}
</script>

<template lang='pug'>
  div.post-card.reply-card(:style="{ marginLeft: `${Math.min(level, 6) * 24}px`, marginBottom: '8px' }")
    div.post-header
      div.post-avatar {{ (reply.userName ? reply.userName.split(' ').map(w => w[0]).join('') : 'U').toUpperCase() }}
      div.post-author
        div.author-name {{ reply.userName }}
        div.author-email {{ reply.userEmail }}
        div.author-home(v-if="getHomeUrl(reply)")
          a(:href="getHomeUrl(reply)" @click.prevent="openHome(getHomeUrl(reply))") {{ getHomeUrl(reply) }}
      div.post-date-top(v-if="reply.createdAt || reply.CreatedAt") {{ formatDate(reply.createdAt || reply.CreatedAt) }}
    div.post-body
      div.post-text(v-html="reply.text")
      template(v-if="getAttachment(reply)")
        template(v-if="(getAttachment(reply).contentType || getAttachment(reply).ContentType || '').startsWith('image/')")
          img.post-image(:src="contentFromAttachment(getAttachment(reply))" :alt="getFileName(getAttachment(reply))" @click="openImage(contentFromAttachment(getAttachment(reply)))")
        template(v-else)
          a.post-file(:href="contentFromAttachment(getAttachment(reply))" :download="getFileName(getAttachment(reply))") {{ getFileName(getAttachment(reply)) }} ({{ (getFileSize(getAttachment(reply))/1024).toFixed(1) }} KB)

    div.replies-container(v-if="reply.replies?.length || reply.Replies?.length")
      ReplyItem(v-for="child in (reply.replies || reply.Replies)"
                :key="child.Id || child.id"
                :reply="child"
                :level="level + 1"
                :content-from-attachment="contentFromAttachment"
                :open-image="openImage")
</template>

<style lang='stylus'>
.reply-card
  background #f4f7fb
  border-left 3px solid #d1d9e6

.reply-card .post-header
  position relative

.reply-card .post-date-top
  position absolute
  right 8px
  top 8px
  font-size 12px
  color #6b7280
  font-weight 600
  white-space nowrap

.author-home
  font-size 12px
  a
    color #2563eb
    text-decoration underline
</style>
