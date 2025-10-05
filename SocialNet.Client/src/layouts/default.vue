<script>
  export default {
    components: {
    },
    layout: 'default'
  }
</script>
<template lang="pug">
	main.inner
			slot

</template>

<style lang='stylus'>
transition = all .25s ease
neu1 = #eef2f7
textGray = #6b7280

*, *::after, *::before
  margin 0
  padding 0
  box-sizing border-box

.inner
  min-height 100vh
  width 100vw
  display flex
  justify-content center
  align-items center
  background-color neu1
  color textGray
  font-family "Manrope", sans-serif
  font-size 12px
</style>