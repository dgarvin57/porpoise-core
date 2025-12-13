<template>
  <div class="h-full flex items-center justify-center bg-gray-50 dark:bg-gray-900">
    <div class="text-center">
      <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Questions</h3>
      <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Coming soon...</p>
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'

defineProps({
  surveyId: {
    type: String,
    required: true
  }
})
</script>
