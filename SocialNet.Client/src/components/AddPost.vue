<script>
import { generateCaptcha, validateCaptcha } from '@/api/captcha.js'
import { usePostStore } from '@/stores/posts.js'
import { useToast } from 'vue-toastification'

export default {
  props: {
    profile: { type: Object, required: true },
    userId: { type: [Number], default: 0 },
    parentPostId: { type: [Number, String, null], default: null }
  },
  data() {
    return {
      postForm: {
        firstName: '',
        lastName: '',
        email: '',
        homepage: '',
        captchaInput: '',
        text: ''
      },
      captchaId: '',
      captchaImage: '',
      attachmentType: '',
      attachmentUrl: '',
      attachmentName: '',
      attachmentSize: 0,
      attachmentError: '',
      attachmentBlob: null,
      attachmentContentType: '',
      isItalicActive: false,
      isBoldActive: false,
      activeFormat: null,
      isCodeActive: false,
      showLinkPanel: false,
      isLinkModeActive: false,
      linkUrl: '',
      savedSelectionRange: null,
      linkError: ''
    };
  },
  setup() {
    const postStore = usePostStore();
    const toast = useToast();

    return {
      storeCreatePost: postStore.createPost,
      toast
    };
  },
  mounted() {
    document.addEventListener('selectionchange', this.updateActiveFormats);
    this.initFromProfile();
    this.refreshCaptcha();
  },
  beforeUnmount() {
    document.removeEventListener('selectionchange', this.updateActiveFormats);
  },
  methods: {
    initFromProfile() {
      let firstName = this.profile?.firstName || '';
      let lastName = this.profile?.lastName || '';
      let email = this.profile?.email || '';

      if (!firstName || !lastName || !email) {
          const raw = localStorage.getItem('user');
          const parsed = raw ? JSON.parse(raw) : {};
          firstName = firstName || parsed.firstName || '';
          lastName = lastName || parsed.lastName || '';
          email = email || parsed.email || '';
      }

      this.postForm.firstName = firstName;
      this.postForm.lastName = lastName;
      this.postForm.email = email;
      this.postForm.homepage = '';
      this.postForm.captchaInput = '';
      this.postForm.text = '';
    },
    close() {
      this.$emit('close');
    },
    async refreshCaptcha() {
      try {
        const resp = await generateCaptcha();
        const id = resp?.captchaId ?? resp?.CaptchaId;
        const img = resp?.imageBase64 ?? resp?.ImageBase64;
        this.captchaId = id || '';
        this.captchaImage = img ? `data:image/png;base64,${img}` : '';
      } catch (e) {
        console.error('Failed to load captcha', e);
      }
    },
    isValidUrl(value) {
      if (!value) return true;
      try { new URL(value); return true; } catch { return false; }
    },
    async createPost() {
      const { firstName, lastName, email, homepage, captchaInput, text } = this.postForm;
      if (!firstName || !lastName || !email) {
        this.toast.error('User name and email are required');
        return;
      }
      if (!text || !text.trim()) {
        this.toast.error('Text is required');
        return;
      }
      if (!this.isValidUrl(homepage)) {
        this.toast.error('Home page must be a valid URL');
        return;
      }
      if (!captchaInput || captchaInput.trim().length === 0) {
        this.toast.error('CAPTCHA is required');
        return;
      }

      const captcha = await validateCaptcha({ captchaId: this.captchaId, input: captchaInput });
      const isValid = !!(captcha?.valid ?? captcha?.Valid);
      if (!isValid) {
        this.toast.error('CAPTCHA is incorrect');
        this.refreshCaptcha();
        this.postForm.captchaInput = '';
        return;
      }

      let attachmentDto = null;
      if (this.attachmentBlob && this.attachmentName) {
        const base64 = await this.blobToBase64(this.attachmentBlob);
        attachmentDto = {
          type: this.attachmentType || '',
          fileName: this.attachmentName,
          contentType: this.attachmentContentType || '',
          fileSize: this.attachmentSize || 0,
          data: base64,
        };
      }

      const postToCreate = {
        userName: `${this.postForm.firstName} ${this.postForm.lastName}`.trim(),
        userEmail: this.postForm.email,
        homepageUrl: this.postForm.homepage ? this.postForm.homepage : null,
        text: this.postForm.text,
        userId: Number(this.userId),
        parentPostId: this.parentPostId ?? null,
        attachmentDto
      };

      let isSuccessResult = await this.storeCreatePost(postToCreate);
      if (isSuccessResult) {
        this.toast.success('Post created successfully');
        this.$emit('created');
      }
      else {
        this.toast.error('Post creation error');
      }

      this.close();
    },
    async previewPost() {
      let attachment = null;
      if (this.attachmentBlob && this.attachmentName) {
        const base64 = await this.blobToBase64(this.attachmentBlob);
        attachment = {
          fileName: this.attachmentName,
          contentType: this.attachmentContentType || 'application/octet-stream',
          fileSize: this.attachmentSize || 0,
          data: base64
        };
      }

      const preview = {
        Id: -1,
        userName: `${this.postForm.firstName} ${this.postForm.lastName}`.trim() || 'User',
        userEmail: this.postForm.email,
        homepage: this.postForm.homepage || '',
        text: this.$refs?.editor?.innerHTML || this.postForm.text || '',
        attachment
      };
      this.$emit('preview', preview);
    },
    useMyUrl() {
      const base = 'https://socialnetclient-cxggdqdrfwg6h0bd.northeurope-01.azurewebsites.net';
      let firstName = (this.profile?.firstName || '').trim();
      let lastName = (this.profile?.lastName || '').trim();
      let id = this.userId;
      if (!firstName || !lastName || !id) {
        try {
          const raw = localStorage.getItem('user');
          const parsed = raw ? JSON.parse(raw) : {};
          firstName = firstName || (parsed.firstName || '').trim();
          lastName = lastName || (parsed.lastName || '').trim();
          id = parsed.id || 'me';
          this.userId = parsed.id;
        } catch {
          id = id || 'me';
        }
      }
      const namePart = `${firstName}.${lastName}`
        .replace(/\s+/g, '.')
        .replace(/\.+/g, '.')
        .replace(/^\.|\.$/g, '')
        .toLowerCase();
      const safeName = encodeURIComponent(namePart);
      this.postForm.homepage = `${base}/user/${safeName}/${id}`;
    },
    triggerFileSelect() {
      const input = this.$refs.fileInput;
      if (input) input.click();
    },
    async onAttachmentChange(e) {
      this.attachmentError = '';
      const file = e?.target?.files?.[0];
      if (!file) return;

      const isImage = /^(image\/jpeg|image\/png|image\/gif)$/i.test(file.type);
      const isText = /^text\/plain$/i.test(file.type) || /\.txt$/i.test(file.name);

      try {
        if (isImage) {
          const { blob, url, name } = await this.processImageFile(file);
          this.attachmentType = 'image';
          this.attachmentUrl = url;
          this.attachmentName = name;
          this.attachmentSize = blob.size;
          this.attachmentBlob = blob;
          this.attachmentContentType = blob.type || 'image/png';
        } else if (isText) {
          const { blob, name } = await this.processTextFile(file);
          this.attachmentType = 'text';
          this.attachmentUrl = '';
          this.attachmentName = name;
          this.attachmentSize = blob.size;
          this.attachmentBlob = blob;
          this.attachmentContentType = blob.type || 'text/plain';
        } else {
          this.attachmentError = 'Only JPG, PNG, GIF images or TXT files are allowed';
        }
      } catch (err) {
        this.attachmentError = 'Failed to process file';
        this.toast.error('File must be up to 100 kB');
      } finally {
        if (this.$refs.fileInput) this.$refs.fileInput.value = '';
      }
    },
    async processImageFile(file) {
      const { blob, url } = await this.resizeImageToMax(file, 320, 240);
      const name = file.name;
      return { blob, url, name };
    },
    resizeImageToMax(file, maxW, maxH) {
      return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => {
          let { width, height } = img;
          const ratio = Math.min(maxW / width, maxH / height, 1);
          const targetW = Math.round(width * ratio);
          const targetH = Math.round(height * ratio);

          const canvas = document.createElement('canvas');
          canvas.width = targetW;
          canvas.height = targetH;
          const ctx = canvas.getContext('2d');
          ctx.drawImage(img, 0, 0, targetW, targetH);

          const outType = file.type && /image\/(png|jpeg)/.test(file.type) ? file.type : 'image/png';
          canvas.toBlob((blob) => {
            if (!blob) return reject(new Error('toBlob failed'));
            const url = URL.createObjectURL(blob);
            resolve({ blob, url });
          }, outType, 0.92);
        };
        img.onerror = reject;
        img.src = URL.createObjectURL(file);
      });
    },
    async processTextFile(file) {
      const maxBytes = 100 * 1024;
      if (file.size > maxBytes) {
        throw new Error('TXT file must be <= 100KB');
      }
      return { blob: file, name: file.name };
    },
    removeAttachment() {
      if (this.attachmentUrl) URL.revokeObjectURL(this.attachmentUrl);
      this.attachmentType = '';
      this.attachmentUrl = '';
      this.attachmentName = '';
      this.attachmentSize = 0;
      this.attachmentError = '';
      this.attachmentBlob = null;
      this.attachmentContentType = '';
    },
    blobToBase64(blob) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => {
          const result = reader.result || '';
          const commaIdx = String(result).indexOf(',');
          const base64 = commaIdx >= 0 ? String(result).slice(commaIdx + 1) : String(result);
          resolve(base64);
        };
        reader.onerror = reject;
        reader.readAsDataURL(blob);
      });
    },
    updateContent() {
      this.postForm.text = this.$refs.editor.innerHTML;
    },
    saveSelectionIfInsideEditor() {
      const editor = this.$refs?.editor;
      const sel = window.getSelection();
      if (!editor || !sel || sel.rangeCount === 0) return;
      const range = sel.getRangeAt(0);
      const container = range.commonAncestorContainer;
      const node = container.nodeType === 3 ? container.parentNode : container;
      if (editor.contains(node)) {
        this.savedSelectionRange = range.cloneRange();
      }
    },
    restoreSavedSelection() {
      if (!this.savedSelectionRange) return false;
      const sel = window.getSelection();
      sel.removeAllRanges();
      sel.addRange(this.savedSelectionRange);
      return true;
    },
    updateActiveFormats() {
      try {
        const editor = this.$refs?.editor;
        const sel = window.getSelection();
        if (!editor || !sel || sel.rangeCount === 0) {
          this.isItalicActive = false;
          this.isBoldActive = false;
          return;
        }
        const range = sel.getRangeAt(0);
        const container = range.commonAncestorContainer;
        const node = container.nodeType === 3 ? container.parentNode : container;
        const inside = editor.contains(node);
        if (!inside) {
          this.isItalicActive = false;
          this.isBoldActive = false;
          return;
        }
        this.isItalicActive = !!document.queryCommandState('italic');
        this.isBoldActive = !!document.queryCommandState('bold');
      } catch (e) {
        this.isItalicActive = false;
        this.isBoldActive = false;
      }
    },
    toggleFormat(command) {
      const editor = this.$refs.editor;
      editor.focus();

      const selection = window.getSelection();
      if (!selection.rangeCount) return;
      const range = selection.getRangeAt(0);

      if (command === 'code') {
        const currentCode = selection.anchorNode?.parentElement?.closest('code');

        if (currentCode && this.activeFormat === 'code') {
          const spacer = document.createTextNode('\u200B');
          if (currentCode.parentNode) {
            currentCode.parentNode.insertBefore(spacer, currentCode.nextSibling);
          }
          const afterRange = document.createRange();
          afterRange.setStart(spacer, 1);
          afterRange.collapse(true);
          selection.removeAllRanges();
          selection.addRange(afterRange);
          this.activeFormat = null;
          this.isCodeActive = false;
          this.updateActiveFormats();
          this.updateContent();
          return;
        }

        this.activeFormat = 'code';

        const codeEl = document.createElement('code');
        codeEl.classList.add('inline-code');

        if (selection.isCollapsed) {
          codeEl.textContent = '\u200B';
          range.insertNode(codeEl);

          const txt = codeEl.firstChild;
          const newRange = document.createRange();
          newRange.setStart(txt, 1);
          newRange.collapse(true);
          selection.removeAllRanges();
          selection.addRange(newRange);
          this.isCodeActive = true;
        } else {
          const fragment = range.extractContents();
          codeEl.appendChild(fragment);
          range.insertNode(codeEl);
          range.setStartAfter(codeEl);
          range.collapse(true);
          selection.removeAllRanges();
          selection.addRange(range);
          this.activeFormat = null;
          this.isCodeActive = false;
        }

        this.updateActiveFormats();
        this.updateContent();
        return;
      }

      document.execCommand(command);
      this.updateActiveFormats();
      this.updateContent();
    },
    onEditorKeydown(e) {
      if (e.key === 'Enter') {
        e.preventDefault();
        document.execCommand('insertLineBreak');
        this.updateContent();
      }
    },
    onEditorClick(e) {
      const target = e.target;
      const anchor = target && typeof target.closest === 'function' ? target.closest('a') : null;
      if (!anchor) return;
      const href = anchor.getAttribute('href');
      if (!href) return;
      e.preventDefault();
      window.open(href, '_blank', 'noopener');
    },
    toggleLinkMode() {
      if (this.isLinkModeActive) {
        this.isLinkModeActive = false;
        this.showLinkPanel = false;
        this.linkUrl = '';
        this.savedSelectionRange = null;
        return;
      }
      this.saveSelectionIfInsideEditor();
      this.isLinkModeActive = true;
      this.showLinkPanel = true;
      this.linkError = '';
      this.$nextTick(() => {
        const input = this.$el.querySelector('.link-input');
        if (input) input.focus();
      });
    },
    cancelLinkMode() {
      this.isLinkModeActive = false;
      this.showLinkPanel = false;
      this.linkUrl = '';
      this.linkError = '';
      this.savedSelectionRange = null;
    },
    confirmInsertLink() {
      let url = (this.linkUrl || '').trim();
      if (!url) {
        this.linkError = 'Specify URL';
        return;
      }
      if (!/^https?:\/\//i.test(url)) {
        url = `https://${url}`;
      }
      if (!this.isValidUrl(url)) {
        this.linkError = 'Incorrect URL';
        return;
      }
      this.linkError = '';

      const editor = this.$refs.editor;
      editor.focus();

      const restored = this.restoreSavedSelection();
      const selection = window.getSelection();
      if (!selection || selection.rangeCount === 0) return;
      const range = selection.getRangeAt(0);

      const a = document.createElement('a');
      a.href = url;
      a.target = '_blank';
      a.rel = 'noopener noreferrer';

      if (!selection.isCollapsed) {
        try {
          range.surroundContents(a);
        } catch (_) {
          const frag = range.extractContents();
          a.appendChild(frag);
          range.insertNode(a);
        }
      } else {
        a.textContent = url;
        range.insertNode(a);
      }

      const spacer = document.createTextNode('\u200B');
      if (a.parentNode) a.parentNode.insertBefore(spacer, a.nextSibling);
      const afterRange = document.createRange();
      afterRange.setStart(spacer, 1);
      afterRange.collapse(true);
      selection.removeAllRanges();
      selection.addRange(afterRange);

      this.updateContent();
      this.updateActiveFormats();

      this.isLinkModeActive = false;
      this.showLinkPanel = false;
      this.linkUrl = '';
      this.savedSelectionRange = null;
    },
  }
}
</script>

<template lang='pug'>
  div.post-form-card
    div.post-form-header
      h3.post-form-title Create Post
      button.post-form-close(@click="close") ✕
    form.post-form
      // User Name
      div.form-row
        label.form-label User Name*
        input.form-input(type="text" :value="`${postForm.firstName} ${postForm.lastName}`.trim()" disabled required)
      // Email
      div.form-row
        label.form-label Email*
        input.form-input(type="email" :value="postForm.email" disabled required)
      // Home page
      div.form-row.home-url-row
        label.form-label Home page
        div.home-url-group
          input.form-input(type="url" v-model="postForm.homepage" placeholder="https://example.com")
          button.use-url-button(type="button" @click="useMyUrl") Use my URL
      // CAPTCHA
      div.form-row.captcha-row
        label.form-label CAPTCHA*
        div.captcha-visual
          img.captcha-img(:src="captchaImage" alt="captcha")
          button.refresh-captcha(type="button" title="Refresh" @click="refreshCaptcha") ↻
        input.form-input.captcha-input(type="text" v-model="postForm.captchaInput" placeholder="Enter code" required)
      // Attachment
      div.form-row.attachment-row
        label.form-label Attachment (JPG, PNG, GIF; TXT up to 100KB)
        div.attachment-controls
          input(type="file" ref="fileInput" accept="image/jpeg,image/png,image/gif,.txt" @change="onAttachmentChange" hidden)
          button.attach-button(type="button" @click="triggerFileSelect") Attach file
          button.remove-attachment(type="button" v-if="attachmentType" @click="removeAttachment") Remove
          span.attachment-error(v-if="attachmentError") {{ attachmentError }}
        div.attachment-preview(v-if="attachmentType === 'image'")
          img.attachment-img(:src="attachmentUrl" alt="preview")
        div.attachment-preview(v-else-if="attachmentType === 'text'")
          span.attachment-file {{ attachmentName }} ({{ (attachmentSize/1024).toFixed(1) }} KB)
      // Text
      div.form-row
        label.form-label Text*
        div
          div.editor-toolbar
            button.toolbar-button(:class="{active: isItalicActive}" type="button" @mousedown.prevent @click="toggleFormat('italic')") [i]
            button.toolbar-button(:class="{active: isBoldActive}" type="button" @mousedown.prevent @click="toggleFormat('bold')") [strong]
            button.toolbar-button(:class="{active: isCodeActive}" type="button" @mousedown.prevent @click="toggleFormat('code')") [code]
            div.link-tool
              button.toolbar-button(:class="{active: isLinkModeActive}" type="button" @mousedown.prevent @click="toggleLinkMode") [a]
              transition(name="slide-left")
                div.link-panel(v-if="showLinkPanel")
                  input.link-input(type="url" v-model="linkUrl" placeholder="https://..." @keydown.enter.prevent="confirmInsertLink")
                  button.link-apply(type="button" @click="confirmInsertLink") OK
                  button.link-cancel(type="button" @click="cancelLinkMode") ✕
                  span.link-error(v-if="linkError") {{ linkError }}
          div.form-textarea(
            ref="editor"
            class="editor"
            contenteditable="true"
            @input="updateContent"
            @keydown="onEditorKeydown"
            @click="onEditorClick"
            placeholder="Write your post..."
          )
      div.form-actions
        button.primary-button(type="button" @click="createPost") Publish
        button.secondary-button(type="button" @click="previewPost") Preview
        button.secondary-button(type="button" @click="close") Cancel
</template>
