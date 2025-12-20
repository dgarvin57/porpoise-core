<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900">
    <TopNav />
    <main class="pt-10">
      <router-view v-slot="{ Component }">
        <keep-alive>
          <component :is="Component" />
        </keep-alive>
      </router-view>
    </main>
    <ContextualHint />
  </div>
</template>

<script setup lang="ts">
import TopNav from './components/TopNav.vue';
import ContextualHint from './components/ContextualHint.vue';
</script>

