<template>
  <div class="space-y-4">
    <!-- Header with question label and buttons -->
    <div class="flex items-end justify-between mb-2 pb-2">
      <div>
        <h3 class="text-base font-semibold text-gray-900 dark:text-white">
          {{ questionLabel }}
        </h3>
        <div class="text-[10px] font-medium text-blue-600 dark:text-blue-400 uppercase tracking-wide">
          Frequency Distribution
        </div>
      </div>
      <div class="flex items-center gap-3">
        <button 
          @click="handleAnalyzeInCrosstab"
          class="inline-flex items-center gap-2 px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white text-xs font-medium rounded-lg transition-colors"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
          </svg>
          Analyze in Crosstab
        </button>
        <button 
          @click="handleShowInfo"
          class="p-1.5 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors rounded-full hover:bg-gray-100 dark:hover:bg-gray-700"
          title="Understanding Results & Metrics"
        >
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Frequency Distribution Chart Component -->
    <FrequencyDistributionChart 
      :question="question"
      class="pt-5"
    />
  </div>
</template>

<script setup>
import FrequencyDistributionChart from './FrequencyDistributionChart.vue'

const props = defineProps({
  question: {
    type: Object,
    required: true
  },
  questionLabel: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['analyze-crosstab', 'show-info'])

const handleAnalyzeInCrosstab = () => {
  emit('analyze-crosstab', props.question)
}

const handleShowInfo = () => {
  emit('show-info', props.question)
}
</script>
