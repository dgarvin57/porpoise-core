<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <TopNav />
    <main class="pt-10">
      <router-view />
    </main>
  </div>
</template>

<script setup lang="ts">
import TopNav from './components/TopNav.vue';
</script>

