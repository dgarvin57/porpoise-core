<template>
  <!-- Modal Overlay -->
  <div v-if="show" @click="close" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div @click.stop class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-xl w-full mx-4 overflow-hidden">
      <!-- Header -->
      <div class="bg-blue-50 dark:bg-blue-900/30 px-5 py-3 border-b border-gray-200 dark:border-gray-700">
        <div class="flex items-center justify-between">
          <h3 class="text-base font-semibold text-gray-900 dark:text-white">Key Metric Definitions</h3>
          <CloseButton @click="close" />
        </div>
      </div>

      <!-- Content -->
      <div class="px-5 py-4 space-y-3.5">
        <!-- Index -->
        <div>
          <div class="flex items-center space-x-2 mb-1.5">
            <div class="w-2 h-2 bg-blue-500 rounded-full"></div>
            <h4 class="font-semibold text-gray-900 dark:text-white text-sm">Index</h4>
          </div>
          <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
            Our proprietary sentiment metric that instantly reveals response favorability. 
            Calculated as <span class="font-mono bg-gray-100 dark:bg-gray-700 px-1.5 py-0.5 rounded text-xs">(Positive% - Negative% + 100)</span>. 
            Centered at 100—higher values show positive sentiment, lower show negative. 
            <span class="text-gray-500 dark:text-gray-400 italic">e.g., 155 = 55% net positive</span>
          </p>
        </div>

        <!-- CI (Confidence Interval) -->
        <div>
          <div class="flex items-center space-x-2 mb-1.5">
            <div class="w-2 h-2 bg-green-500 rounded-full"></div>
            <h4 class="font-semibold text-gray-900 dark:text-white text-sm">CI (Confidence Interval)</h4>
          </div>
          <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
            The margin of error for the entire question at 95% confidence. 
            Shows the range where the true population value likely falls. 
            <span class="text-gray-500 dark:text-gray-400 italic">e.g., ±3.5 means results are accurate within 3.5 points</span>
          </p>
        </div>

        <!-- Total N -->
        <div>
          <div class="flex items-center space-x-2 mb-1.5">
            <div class="w-2 h-2 bg-purple-500 rounded-full"></div>
            <h4 class="font-semibold text-gray-900 dark:text-white text-sm">Total N</h4>
          </div>
          <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug ml-4">
            The number of valid responses for this question. 
            Larger samples provide more reliable results and lower margins of error.
          </p>
        </div>
      </div>

      <!-- Footer -->
      <div class="bg-gray-50 dark:bg-gray-900 px-5 py-3 flex justify-end">
        <button
          @click="close"
          class="px-4 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-md transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        >
          Got it
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import CloseButton from './common/CloseButton.vue'

defineProps({
  show: {
    type: Boolean,
    required: true
  }
})

const emit = defineEmits(['close'])

const close = () => {
  emit('close')
}
</script>