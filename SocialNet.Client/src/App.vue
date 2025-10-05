<script>
import DefaultLayout from './layouts/default.vue'
import { watch } from 'vue';
import { useRoute } from 'vue-router';

export default {
  components: {
    DefaultLayout,
  },
  computed: {
    layout() {
      return 'DefaultLayout'
    }
  },
  setup() {
    const route = useRoute();
    watch(route, () => {}, { immediate: true });
  },
    methods:{
    }
}
</script>

<template>
  <component :is="layout">
    <RouterView />
  </component>
</template>
