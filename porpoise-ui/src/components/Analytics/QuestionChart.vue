<template>
  <div class="space-y-6">
    <!-- Horizontal Bar Chart -->
    <div class="space-y-3">
      <div
        v-for="(response, index) in sortedResponses"
        :key="index"
        class="flex items-center space-x-4"
      >
        <!-- Response Label -->
        <div class="w-32 text-sm text-right text-gray-700 dark:text-gray-300 truncate">
          {{ response.label }}
        </div>

        <!-- Bar -->
        <div class="flex-1 relative">
          <div class="bg-gray-200 dark:bg-gray-700 rounded-full h-8 overflow-hidden">
            <div
              :style="{ width: response.percentage + '%' }"
              class="h-full bg-blue-600 dark:bg-blue-500 transition-all duration-500 ease-out flex items-center justify-end pr-3"
            >
              <span class="text-xs font-semibold text-white">
                {{ response.percentage.toFixed(1) }}%
              </span>
            </div>
          </div>
        </div>

        <!-- Count -->
        <div class="w-16 text-sm text-gray-600 dark:text-gray-400 text-right">
          {{ response.count }}
        </div>
      </div>
    </div>

    <!-- X-axis scale labels -->
    <div class="flex justify-between text-xs text-gray-500 dark:text-gray-400 pl-36">
      <span>0</span>
      <span>25</span>
      <span>50</span>
      <span>75</span>
      <span>100</span>
    </div>

    <!-- Summary Stats -->
    <div class="pt-4 border-t border-gray-200 dark:border-gray-700 grid grid-cols-3 gap-4">
      <div class="text-center">
        <div class="text-2xl font-bold text-gray-900 dark:text-white">
          {{ totalResponses }}
        </div>
        <div class="text-sm text-gray-600 dark:text-gray-400">
          Total Responses
        </div>
      </div>
      <div class="text-center">
        <div class="text-2xl font-bold text-gray-900 dark:text-white">
          {{ topResponse?.percentage.toFixed(1) }}%
        </div>
        <div class="text-sm text-gray-600 dark:text-gray-400">
          Top Response
        </div>
      </div>
      <div class="text-center">
        <div class="text-2xl font-bold text-gray-900 dark:text-white">
          {{ question.responses?.length || 0 }}
        </div>
        <div class="text-sm text-gray-600 dark:text-gray-400">
          Answer Options
        </div>
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

const sortedResponses = computed(() => {
  if (!props.question.responses) return []
  return [...props.question.responses].sort((a, b) => b.percentage - a.percentage)
})

const totalResponses = computed(() => {
  if (!props.question.responses) return 0
  return props.question.responses.reduce((sum, r) => sum + r.count, 0)
})

const topResponse = computed(() => {
  return sortedResponses.value[0] || null
})
</script>
