<template>
  <div class="h-full overflow-auto">
    <div v-if="selectedQuestion" class="pt-3 px-6 pb-6 w-full max-w-[848px]">
      <ResultsChart
        :question="selectedQuestion"
        :questionLabel="selectedQuestion.label || selectedQuestion.qstLabel || ''"
        @analyze-crosstab="handleAnalyzeCrosstab"
        @ai-analysis="handleAIAnalysis"
        @show-info="handleShowInfo"
      />
    </div>
    
    <!-- Empty state for Results -->
    <div v-else class="h-full flex items-center justify-center">
      <div class="text-center">
        <svg class="w-16 h-16 mx-auto mb-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
        </svg>
        <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">No Question Selected</h3>
        <p class="text-sm text-gray-500 dark:text-gray-400">Select a question from the list to view response distribution</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import ResultsChart from './ResultsChart.vue'

// Props
const props = defineProps({
  selectedQuestion: {
    type: Object,
    default: null
  }
})

// Emits
const emit = defineEmits(['analyze-crosstab', 'ai-analysis', 'show-info'])

// Event handlers that forward to parent
function handleAnalyzeCrosstab(question) {
  emit('analyze-crosstab', question)
}

function handleAIAnalysis(question) {
  emit('ai-analysis', question)
}

function handleShowInfo(question) {
  emit('show-info', question)
}
</script>
