<script>
import { usePostStore } from '@/stores/posts.js'
import AddPost from '@/components/AddPost.vue'
import ReplyItem from '@/components/ReplyItem.vue'

export default {
  components: { AddPost, ReplyItem },
  props: {
    profile: { type: Object, default: () => ({}) },
    userId: { type: [Number, String], default: 0 }
  },
  data() {
    return {
      posts: [],
      previewPostItem: null,
      replyingToPostId: null,
      replyPreviewsByParentId: {},
      isLoading: false,
      error: '',
      pageNumber: 1,
      pageSize: 5,
      sortBy: 'CreatedAt',
      sortDirection: 'Desc',
      totalPages: 1,
      totalCount: 0,
      lightboxVisible: false,
      lightboxSrc: ''
    };
  },
  setup() {
    const postStore = usePostStore();

    return {
      storeGetPosts: postStore.getPosts
    };
  },
  mounted() {
    this.loadPosts();
  },
  methods: {
    replyTo(post) {
      const id = post.Id || post.id;
      if (this.replyingToPostId === id) {
        this.replyingToPostId = null;
        this.clearPreview();
      } else {
        this.replyingToPostId = id;
        this.clearPreview();
      }
    },
    async loadPosts() {
      this.isLoading = true;
      this.error = '';
      try {
        const data = await this.storeGetPosts({
          PageNumber: this.pageNumber,
          PageSize: this.pageSize,
          SortBy: this.sortBy,
          SortDirection: this.sortDirection,
        });
        const items = data?.items || [];
        this.posts = Array.isArray(items) ? items : [];
        this.totalPages = Math.ceil((data?.totalCount || 0) / this.pageSize) || 1;
        this.totalCount = data?.totalCount || this.posts.length;
      } catch (e) {
        this.error = 'Failed to load posts';
        this.posts = [];
      } finally {
        this.isLoading = false;
      }
    },
    addPreview(preview) {
      this.previewPostItem = preview;
    },
    togglePreview(preview) {
      if (this.previewPostItem && this.isSamePreview(this.previewPostItem, preview)) {
        this.clearPreview();
      } else {
        this.addPreview(preview);
      }
    },
    clearPreview() {
      this.previewPostItem = null;
    },
    addReplyPreview(parentId, preview) {
      this.replyPreviewsByParentId[parentId] = preview;
    },
    toggleReplyPreview(parentId, preview) {
      const current = this.replyPreviewsByParentId[parentId];
      if (current && this.isSamePreview(current, preview)) {
        delete this.replyPreviewsByParentId[parentId];
      } else {
        this.replyPreviewsByParentId[parentId] = preview;
      }
    },
    clearReplyPreview(parentId) {
      if (this.replyPreviewsByParentId[parentId]) {
        delete this.replyPreviewsByParentId[parentId];
      }
    },
    isSamePreview(a, b) {
      if (!a || !b) return false;
      const aName = (a.userName || '').trim();
      const bName = (b.userName || '').trim();
      const aEmail = (a.userEmail || '').trim();
      const bEmail = (b.userEmail || '').trim();
      const aText = (a.text || '').trim();
      const bText = (b.text || '').trim();
      const aFile = a.attachment?.fileName || '';
      const bFile = b.attachment?.fileName || '';
      const aType = a.attachment?.contentType || '';
      const bType = b.attachment?.contentType || '';
      const aSize = a.attachment?.fileSize || 0;
      const bSize = b.attachment?.fileSize || 0;
      return aName === bName && aEmail === bEmail && aText === bText && aFile === bFile && aType === bType && aSize === bSize;
    },
    switchSort(field) {
      if (this.sortBy.toLowerCase() === field.toLowerCase()) {
        this.sortDirection = this.sortDirection === 'Asc' ? 'Desc' : 'Asc';
      } else {
        this.sortBy = field;
        this.sortDirection = 'Desc';
      }
      this.pageNumber = 1;
      this.loadPosts();
    },
    prevPage() {
      if (this.pageNumber > 1) {
        this.pageNumber -= 1;
        this.loadPosts();
      }
    },
    nextPage() {
      if (this.pageNumber < this.totalPages) {
        this.pageNumber += 1;
        this.loadPosts();
      }
    },
    contentFromAttachment(attachment) {
      if (!attachment || !attachment.data) return null;
      const mime = attachment.contentType || 'application/octet-stream';
      try {
        const base64 = Array.isArray(attachment.data) ? this.byteArrayToBase64(attachment.data) : attachment.data;
        return `data:${mime};base64,${base64}`;
      } catch {
        return null;
      }
    },
    byteArrayToBase64(byteArray) {
      const binString = String.fromCharCode.apply(null, byteArray);
      return btoa(binString);
    },
    openImage(src) {
      if (!src) return;
      this.lightboxSrc = src;
      this.lightboxVisible = true;
    },
    closeImage() {
      this.lightboxVisible = false;
      this.lightboxSrc = '';
    },
    getHomeUrl(p) {
      return p?.homePage || p?.homepage || p?.homepageUrl || p?.homePageUrl || p?.HomePage || p?.Homepage || p?.HomepageUrl || p?.HomePageUrl || '';
    }
  }
};
</script>

<template lang='pug'>
  div.posts-wrapper
    div.posts-toolbar
      div.sort-group
        span.sort-label Sort by:
        button.sort-button(type="button" @click="switchSort('UserName')" :class="{active: sortBy==='UserName'}") User Name
        button.sort-button(type="button" @click="switchSort('UserEmail')" :class="{active: sortBy==='UserEmail'}") User Email
        button.sort-button(type="button" @click="switchSort('CreatedAt')" :class="{active: sortBy==='CreatedAt'}") Created At
      div.page-group
        button.page-button(type="button" @click="prevPage" :disabled="pageNumber===1") ◀
        span.page-info Page {{ pageNumber }} / {{ totalPages }}
        button.page-button(type="button" @click="nextPage" :disabled="pageNumber===totalPages") ▶

    div.posts-list
      template(v-if="previewPostItem")
        div.post-card.preview
          div.post-header
            div.post-avatar {{ (previewPostItem.userName ? previewPostItem.userName.split(' ').map(w => w[0]).join('') : 'U').toUpperCase() }}
            div.post-author
              div.author-name {{ previewPostItem.userName }}
              div.author-email {{ previewPostItem.userEmail }}
              div.author-home(v-if="getHomeUrl(previewPostItem)")
                a(:href="getHomeUrl(previewPostItem)" target="_blank" rel="noopener") {{ getHomeUrl(previewPostItem) }}
          div.post-body
            div.post-text(v-html="previewPostItem.text")
            template(v-if="previewPostItem.attachment")
              template(v-if="(previewPostItem.attachment.contentType||'').startsWith('image/')")
                img.post-image(:src="contentFromAttachment(previewPostItem.attachment)" @click="openImage(contentFromAttachment(previewPostItem.attachment))")
              template(v-else)
                a.post-file(:href="contentFromAttachment(previewPostItem.attachment)" :download="previewPostItem.attachment.fileName") {{ previewPostItem.attachment.fileName }} ({{ (previewPostItem.attachment.fileSize/1024).toFixed(1) }} KB)
      div.loader(v-if="isLoading") Loading...
      div.error(v-else-if="error") {{ error }}
      template(v-else)
        div.post-card(v-for="post in posts" :key="post.Id")
          div.post-header
            div.post-avatar {{ (post.userName ? post.userName.split(' ').map(w => w[0]).join('') : 'U').toUpperCase() }}
            div.post-author
              div.author-name {{ post.userName }}
              div.author-email {{ post.userEmail }}
              div.author-home(v-if="getHomeUrl(post)")
                a(:href="getHomeUrl(post)" target="_blank" rel="noopener") {{ getHomeUrl(post) }}
            div.post-date-top(v-if="!post.parentPostId && !post.parentId && (post.createdAt || post.CreatedAt)") {{ (new Date(post.createdAt || post.CreatedAt)).toLocaleDateString('ru-RU', {day:'2-digit',month:'2-digit',year:'numeric'}) }} at {{ (new Date(post.createdAt || post.CreatedAt)).toLocaleTimeString('ru-RU', {hour:'2-digit',minute:'2-digit'}) }}
            button.reply-button-top(type="button" v-if="!post.parentPostId && !post.parentId" @click="replyTo(post)") ↩ Reply
          div.post-body
            div.post-text(v-html="post.text")
            template(v-if="post.attachment")
              template(v-if="(post.attachment.contentType||'').startsWith('image/')")
                img.post-image(:src="contentFromAttachment(post.attachment)" :alt="post.attachment.fileName" @click="openImage(contentFromAttachment(post.attachment))")
              template(v-else)
                a.post-file(:href="contentFromAttachment(post.attachment)" :download="post.attachment.fileName") {{ post.attachment.fileName }} ({{ (post.attachment.fileSize/1024).toFixed(1) }} KB)

            // Replies
            div.replies-thread(v-if="post.replies?.length || post.Replies?.length || replyPreviewsByParentId[post.Id || post.id]")
              template(v-if="replyPreviewsByParentId[post.Id || post.id]")
                div.post-card.preview.reply-card.reply-preview-item(style="margin-left: 24px")
                  div.post-header
                    div.post-avatar {{ ((replyPreviewsByParentId[post.Id || post.id].userName || 'U').split(' ').map(w => w[0]).join('')).toUpperCase() }}
                    div.post-author
                      div.author-name {{ replyPreviewsByParentId[post.Id || post.id].userName }}
                      div.author-email {{ replyPreviewsByParentId[post.Id || post.id].userEmail }}
                      div.author-home(v-if="getHomeUrl(replyPreviewsByParentId[post.Id || post.id])")
                        a(:href="getHomeUrl(replyPreviewsByParentId[post.Id || post.id])" target="_blank" rel="noopener") {{ getHomeUrl(replyPreviewsByParentId[post.Id || post.id]) }}
                  div.post-body
                    div.post-text(v-html="replyPreviewsByParentId[post.Id || post.id].text")
                    template(v-if="replyPreviewsByParentId[post.Id || post.id].attachment")
                      template(v-if="(replyPreviewsByParentId[post.Id || post.id].attachment.contentType||'').startsWith('image/')")
                        img.post-image(:src="contentFromAttachment(replyPreviewsByParentId[post.Id || post.id].attachment)" @click="openImage(contentFromAttachment(replyPreviewsByParentId[post.Id || post.id].attachment))")
                      template(v-else)
                        a.post-file(:href="contentFromAttachment(replyPreviewsByParentId[post.Id || post.id].attachment)" :download="replyPreviewsByParentId[post.Id || post.id].attachment.fileName") {{ replyPreviewsByParentId[post.Id || post.id].attachment.fileName }} ({{ (replyPreviewsByParentId[post.Id || post.id].attachment.fileSize/1024).toFixed(1) }} KB)
              ReplyItem(v-for="rep in (post.replies || post.Replies)"
                        :key="rep.Id || rep.id"
                        :reply="rep"
                        :level="1"
                        :content-from-attachment="contentFromAttachment"
                        :open-image="openImage")
            div.reply-form-wrapper(v-if="replyingToPostId === (post.Id || post.id)")
              AddPost(
                :profile="profile"
                :user-id="Number(userId) || 0"
                :parent-post-id="post.Id || post.id"
                @created="async () => { replyingToPostId = null; clearReplyPreview(post.Id || post.id); pageNumber = 1; await loadPosts(); }"
                @close="() => { replyingToPostId = null; clearReplyPreview(post.Id || post.id) }"
                @preview="(p) => toggleReplyPreview(post.Id || post.id, p)"
              )

    transition(name="fade-zoom")
      div.lightbox-overlay(v-if="lightboxVisible" @click="closeImage")
        img.lightbox-image(:src="lightboxSrc" @click.stop)

    div.posts-footer
      div.page-group
        button.page-button(type="button" @click="prevPage" :disabled="pageNumber===1") ◀ Prev
        span.page-info Page {{ pageNumber }} / {{ totalPages }} ({{ totalCount }} total)
        button.page-button(type="button" @click="nextPage" :disabled="pageNumber===totalPages") Next ▶
</template>

<style lang='stylus'>
.posts-wrapper
  width 95%
  margin 16px auto
  padding 16px 18px
  background #fff
  border-radius 16px
  box-shadow 8px 8px 16px #d1d9e6, -8px -8px 16px #ffffff
  overflow-y auto
  overflow-x hidden

.posts-toolbar
  display flex
  justify-content space-between
  align-items center
  margin-bottom 12px

.sort-group, .page-group
  display flex
  align-items center
  gap 8px

.sort-label
  font-weight 700
  color #111827

.sort-button, .page-button
  height 32px
  padding 0 12px
  border none
  border-radius 10px
  background #111827
  color #fff
  font-weight 700
  cursor pointer
  box-shadow 8px 8px 16px #d1d9e6, -8px -8px 16px #ffffff

.sort-button.active
  background #5b21b6

.posts-list
  display flex
  flex-direction column
  gap 12px

.post-card
  padding 16px
  background #eef2f7
  border-radius 12px
  box-shadow inset 2px 2px 4px #d1d9e6, inset -2px -2px 4px #ffffff

.post-card.preview
  border 2px dashed #5b21b6

.replies-thread
  margin-top 8px

.reply-preview-item
  margin-bottom 8px

.post-header
  display flex
  gap 12px
  align-items center
  margin-bottom 8px
  position relative

.post-date-top
  position absolute
  right 100px
  top 15px
  font-size 12px
  color #6b7280
  font-weight 600
  white-space nowrap

.reply-button-top
  position absolute
  right 8px
  top 8px
  height 32px
  padding 0 12px
  border none
  border-radius 10px
  background #111827
  color #fff
  font-weight 700
  cursor pointer
  box-shadow 8px 8px 16px #d1d9e6, -8px -8px 16px #ffffff

.post-avatar
  width 40px
  height 40px
  border-radius 50%
  background #fff
  box-shadow inset 2px 2px 4px #d1d9e6, inset -2px -2px 4px #ffffff
  display flex
  align-items center
  justify-content center
  font-weight 800
  color #5b21b6

.author-name
  font-weight 800
  color #111827

.author-email
  color #374151
  font-size 12px

.author-home
  font-size 12px
  a
    color #2563eb
    text-decoration underline

.post-text
  color #111827
  margin 8px 0

.post-image
  max-width 320px
  max-height 240px
  border-radius 12px
  background #eef2f7
  box-shadow inset 2px 2px 4px #d1d9e6, inset -2px -2px 4px #ffffff
  cursor zoom-in
.lightbox-overlay
  position fixed
  inset 0
  background rgba(0,0,0,0.6)
  display flex
  align-items center
  justify-content center
  z-index 1000

.lightbox-image
  max-width 90vw
  max-height 90vh
  border-radius 12px
  background #fff
  animation zoomIn .18s ease forwards

@keyframes zoomIn
  from
    transform scale(0.98)
    opacity .98
  to
    transform scale(1)
    opacity 1

.fade-zoom-enter-active,
.fade-zoom-leave-active
  transition opacity .18s ease

.fade-zoom-enter-from,
.fade-zoom-leave-to
  opacity 0

.post-file
  font-weight 700
  color #2563eb
  text-decoration underline

.reply-form-wrapper
  margin-top 12px

.posts-footer
  margin-top 12px
  display flex
  justify-content center
  align-items center
  gap 12px

.loader, .error
  padding 12px
  text-align center
  font-weight 700
</style>