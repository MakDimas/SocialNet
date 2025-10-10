<script>
import { updateUser } from '@/api/user.js'
import { useToast } from 'vue-toastification'

export default {
  data() {
    const raw = localStorage.getItem('user');
    let parsed = {};
    try { parsed = raw ? JSON.parse(raw) : {}; } catch { parsed = {}; }
    const user = {
      id: parsed.id || parsed.userId || parsed.userID || parsed.sub || 0,
      firstName: parsed.firstName || '',
      lastName: parsed.lastName || '',
      email: parsed.email || '',
      phoneNumber: parsed.phoneNumber || '',
      description: parsed.description || '',
      age: parsed.age ?? ''
    };
    return {
      form: { ...user },
      original: { ...user },
      isSaving: false,
      errors: {},
      toast: useToast()
    };
  },
  computed: {
    userName() {
      return `${(this.form.firstName || '').trim()} ${(this.form.lastName || '').trim()}`.trim();
    },
    isDirty() {
      return JSON.stringify(this.form) !== JSON.stringify(this.original);
    }
  },
  methods: {
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
        s = s.replace(/\r/g, '').replace(/<[^>]*>/g, ' ');
        let m = s.match(/Error during user update:\s*([^\n<]+)/i);
        if (m && m[1]) return m[1].trim();
        m = s.match(/System\.[A-Za-z]+Exception:\s*([^\n<]+)/i);
        if (m && m[1]) {
          let t = m[1].trim();
          if (t.includes(':')) t = t.slice(t.lastIndexOf(':') + 1).trim();
          if (t) return t;
        }
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
    validate() {
      const errors = {};
      if (!this.form.firstName || !this.form.firstName.trim()) errors.firstName = 'First name is required';
      if (!this.form.lastName || !this.form.lastName.trim()) errors.lastName = 'Last name is required';
      if (!this.form.email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.form.email)) errors.email = 'Valid email is required';
      if (this.form.phoneNumber && /\D/.test(this.form.phoneNumber)) errors.phoneNumber = 'Phone must contain digits only';
      if (this.form.age !== '' && this.form.age !== null) {
        const n = Number(this.form.age);
        if (!Number.isFinite(n) || n <= 0 || n >= 100) errors.age = 'Age must be between 1 and 99';
      }
      this.errors = errors;
      if (Object.keys(errors).length) {
        this.toast.error('Please correct the errors in the form');
        return false;
      }
      return true;
    },
    async save() {
      if (!this.validate()) return;
      this.isSaving = true;
      try {
        const payload = {
          Id: Number(this.form.id),
          FirstName: this.form.firstName.trim(),
          LastName: this.form.lastName.trim(),
          Email: this.form.email.trim(),
          PhoneNumber: this.form.phoneNumber ? String(this.form.phoneNumber).trim() : null,
          Description: this.form.description ? this.form.description.trim() : null,
          Age: this.form.age === '' ? null : Number(this.form.age)
        };
        const resp = await updateUser(payload);
        const data = resp?.data?.data || resp?.data || {};
        const updated = {
          id: data.id ?? this.form.id,
          firstName: data.firstName ?? this.form.firstName,
          lastName: data.lastName ?? this.form.lastName,
          email: data.email ?? this.form.email,
          phoneNumber: data.phoneNumber ?? this.form.phoneNumber,
          description: data.description ?? this.form.description,
          age: data.age ?? this.form.age
        };
        localStorage.setItem('user', JSON.stringify(updated));
        this.original = { ...updated };
        this.form = { ...updated };
        this.toast.success('Profile updated');
      } catch (e) {
        let payload = e?.response?.data;
        if (payload && typeof payload === 'object' && typeof payload.text === 'function') {
          try { payload = await payload.text(); } catch {}
        }
        const raw = this._getErrorMessage(payload, e?.message || 'Failed to update profile');
        const msg = this._prettifyMessage(raw, 'Failed to update profile');
        this.toast.error(msg);
      } finally {
        this.isSaving = false;
      }
    },
    cancel() {
      this.form = { ...this.original };
    }
  }
}
</script>

<template lang='pug'>
div.settings-main
  div.settings-content
    div.settings-up-panel
      div.left-buttons
        router-link.settings-back-link(:to="{ name: 'home' }") Back
    div.settings-card
      h3.settings-title Account Settings
      form.settings-form(@submit.prevent="save")
        .settings-form-row
          label.settings-form-label User Name
          input.settings-form-input(type="text" :value="userName" disabled)
        .settings-form-row
          label.settings-form-label Email*
          input.settings-form-input(type="email" v-model.trim="form.email" :class="{ invalid: errors.email }" placeholder="user@example.com")
          span.settings-input-error(v-if="errors.email") {{ errors.email }}
        .settings-form-row
          label.settings-form-label First name*
          input.settings-form-input(type="text" v-model.trim="form.firstName" :class="{ invalid: errors.firstName }")
          span.settings-input-error(v-if="errors.firstName") {{ errors.firstName }}
        .settings-form-row
          label.settings-form-label Last name*
          input.settings-form-input(type="text" v-model.trim="form.lastName" :class="{ invalid: errors.lastName }")
          span.settings-input-error(v-if="errors.lastName") {{ errors.lastName }}
        .settings-form-row
          label.settings-form-label Phone
          input.settings-form-input(type="text" v-model.trim="form.phoneNumber" :class="{ invalid: errors.phoneNumber }" placeholder="digits only")
          span.settings-input-error(v-if="errors.phoneNumber") {{ errors.phoneNumber }}
        .settings-form-row
          label.settings-form-label Age
          input.settings-form-input(type="number" v-model.number="form.age" min="1" max="99" :class="{ invalid: errors.age }" placeholder="1..99")
          span.settings-input-error(v-if="errors.age") {{ errors.age }}
        .settings-form-row
          label.settings-form-label Description
          textarea.settings-form-textarea(v-model.trim="form.description" rows="4")
        .settings-form-actions
          button.settings-primary-button(type="submit" :disabled="isSaving || !isDirty") {{ isSaving ? 'Saving...' : 'Save changes' }}
          button.settings-secondary-button(type="button" @click="cancel" :disabled="isSaving || !isDirty") Discard
</template>
