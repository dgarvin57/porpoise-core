<template>
  <div class="h-full flex flex-col bg-white dark:bg-gray-800">
    <!-- Tabs for Question/Block -->
    <div class="flex-shrink-0 border-b border-gray-200 dark:border-gray-700">
      <div class="flex">
        <button
          @click="activeTab = 'question'"
          :class="[
            'flex items-center gap-2 px-4 py-1 text-xs font-medium border-b-2 transition-colors',
            activeTab === 'question'
              ? 'border-blue-500 text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20'
              : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
          ]"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          Question
        </button>
        <button
          @click="hasBlockStem ? (activeTab = 'block') : null"
          :disabled="!hasBlockStem"
          :class="[
            'flex items-center gap-2 px-4 py-1 text-xs font-medium border-b-2 transition-colors',
            activeTab === 'block'
              ? 'border-blue-500 text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20'
              : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600',
            !hasBlockStem && 'opacity-50 cursor-not-allowed hover:text-gray-500 dark:hover:text-gray-400 hover:border-transparent'
          ]"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
          </svg>
          Block
        </button>
      </div>
    </div>

    <!-- Content Area -->
    <div class="flex-1 overflow-y-auto p-4 min-h-0">
      <!-- Question Tab -->
      <div v-if="activeTab === 'question'" class="prose dark:prose-invert max-w-none">
        <p class="text-xs text-gray-900 dark:text-white leading-relaxed whitespace-pre-wrap">
          {{ question.text || question.qstLabel || question.label }}
        </p>
      </div>

      <!-- Block Tab -->
      <div v-else-if="activeTab === 'block'" class="prose dark:prose-invert max-w-none">
        <p v-if="blockStemText" class="text-xs text-gray-900 dark:text-white leading-relaxed whitespace-pre-wrap">
          {{ blockStemText }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  question: {
    type: Object,
    required: true
  },
  surveyId: {
    type: String,
    required: true
  }
})

// Use the same localStorage key pattern as ResultsTable
const STORAGE_KEY = computed(() => `porpoise_survey_${props.surveyId}_stem_tab`)
const activeTab = ref(localStorage.getItem(STORAGE_KEY.value) || 'question')

// Check for block stem using multiple possible property names
const hasBlockStem = computed(() => {
  return !!(props.question?.blockStem || 
           props.question?.blkStem || 
           props.question?.block?.stem)
})

// Persist tab selection to localStorage
watch(activeTab, (newTab) => {
  localStorage.setItem(STORAGE_KEY.value, newTab)
})

// Auto-switch to Question tab if Block is selected but question has no block stem
watch(() => props.question, (newQuestion) => {
  if (activeTab.value === 'block' && !hasBlockStem.value) {
    activeTab.value = 'question'
  }
}, { immediate: true })

const blockStemText = computed(() => {
  return props.question?.blockStem || 
         props.question?.blkStem || 
         props.question?.block?.stem || 
         ''
})
</script>

<style scoped>
/* Custom scrollbar styling */
.overflow-y-auto::-webkit-scrollbar {
  width: 8px;
}

.overflow-y-auto::-webkit-scrollbar-track {
  @apply bg-gray-100 dark:bg-gray-800;
}

.overflow-y-auto::-webkit-scrollbar-thumb {
  @apply bg-gray-300 dark:bg-gray-600 rounded-full;
}

.overflow-y-auto::-webkit-scrollbar-thumb:hover {
  @apply bg-gray-400 dark:bg-gray-500;
}
</style>
