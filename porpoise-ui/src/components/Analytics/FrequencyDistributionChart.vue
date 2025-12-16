<template>
  <div class="space-y-4">
    <!-- Horizontal Bar Chart -->
    <div class="space-y-2">
      <div
        v-for="(response, index) in sortedResponses"
        :key="index"
        class="flex items-center gap-3"
      >
        <div class="w-32 text-right text-sm text-gray-700 dark:text-gray-300 flex-shrink-0">
          {{ response.label }}
        </div>
        <div class="flex-1 flex items-center gap-2">
          <div class="flex-1 bg-gray-200 dark:bg-gray-700 rounded-full h-6 relative overflow-hidden">
            <div
              class="h-full rounded-full bg-blue-600 dark:bg-blue-500 flex items-center justify-end pr-2"
              :style="{ width: `${response.percentage}%` }"
            >
              <span class="text-xs font-semibold text-white">
                {{ response.percentage.toFixed(1) }}%
              </span>
            </div>
          </div>
          <div class="w-16 text-right text-sm text-gray-600 dark:text-gray-400">
            {{ response.count }}
          </div>
        </div>
      </div>
    </div>

    <!-- X-axis scale -->
    <div class="flex items-center gap-3">
      <div class="w-32 flex-shrink-0"></div>
      <div class="flex-1 flex justify-between text-xs text-gray-500 dark:text-gray-400 px-2">
        <span>0</span>
        <span>25</span>
        <span>50</span>
        <span>75</span>
        <span>100</span>
      </div>
      <div class="w-16"></div>
    </div>

    <!-- Summary Statistics -->
    <div class="pt-4 border-t border-gray-200 dark:border-gray-700">
      <div class="flex items-end justify-center gap-6">
        <!-- Total Responses -->
        <div class="text-center">
          <div class="text-lg font-semibold text-purple-600 dark:text-purple-400/70">
            {{ totalResponses }}
          </div>
          <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Total Responses (N)
          </div>
        </div>

        <!-- Top Response -->
        <div class="text-center">
          <div class="text-lg font-semibold text-orange-600 dark:text-orange-400/70">
            {{ topResponsePercentage }}%
          </div>
          <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Top Response
          </div>
        </div>

        <!-- Answer Options -->
        <div class="text-center">
          <div class="text-lg font-semibold text-pink-600 dark:text-pink-400/70">
            {{ answerOptions }}
          </div>
          <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Answer Options
          </div>
        </div>

        <!-- Bullet Separator -->
        <div class="text-gray-400 dark:text-gray-600 text-xl leading-none pb-5">•</div>

        <!-- Index -->
        <div class="text-center">
          <div class="text-lg font-semibold text-blue-600 dark:text-blue-400/70">
            {{ index }}
          </div>
          <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Index
          </div>
        </div>

        <!-- CI -->
        <div class="text-center">
          <div class="text-lg font-semibold text-green-600 dark:text-green-400/70">
            {{ confidenceInterval }}
          </div>
          <div class="text-xs text-gray-500 dark:text-gray-400 mt-1">
            CI
          </div>
        </div>
        
        <!-- Info Icon -->
        <button 
          @click="$emit('show-info')"
          class="flex-shrink-0 p-1.5 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors rounded-full hover:bg-gray-100 dark:hover:bg-gray-700 mb-5 ml-2"
          title="Understanding Results & Metrics"
        >
          <svg class="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  question: {
    type: Object,
    required: true
  }
})

defineEmits(['show-info'])

// Sort responses by percentage (descending)
const sortedResponses = computed(() => {
  if (!props.question?.responses) return []
  return [...props.question.responses].sort((a, b) => b.percentage - a.percentage)
})

// Calculate summary statistics
const totalResponses = computed(() => {
  if (!props.question?.responses) return 0
  return props.question.responses.reduce((sum, r) => sum + r.count, 0)
})

const topResponsePercentage = computed(() => {
  if (!sortedResponses.value.length) return 0
  return sortedResponses.value[0].percentage.toFixed(1)
})

const answerOptions = computed(() => {
  return props.question?.responses?.length || 0
})

const index = computed(() => {
  return props.question?.index || props.question?.idx || 0
})

const confidenceInterval = computed(() => {
  const ci = props.question?.samplingError || props.question?.confidenceInterval || props.question?.ci || 0
  return ci ? `±${ci.toFixed(1)}` : '±0.0'
})
</script>
