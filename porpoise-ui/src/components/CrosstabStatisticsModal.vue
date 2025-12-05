<template>
  <!-- Modal Overlay -->
  <div v-if="show" @click="close" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
    <div @click.stop class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-2xl w-full mx-4 overflow-hidden">
      <!-- Header -->
      <div class="bg-blue-50 dark:bg-blue-900/30 px-5 py-3 border-b border-gray-200 dark:border-gray-700">
        <div class="flex items-center justify-between">
          <h3 class="text-base font-semibold text-gray-900 dark:text-white">Statistical Measures</h3>
          <CloseButton @click="close" />
        </div>
      </div>

      <!-- Content -->
      <div class="px-5 py-4 space-y-4">
        <!-- Chi-Square -->
        <div>
          <div class="flex items-start gap-4">
            <div class="flex-1">
              <div class="flex items-center space-x-2 mb-2">
                <div class="w-2 h-2 bg-blue-500 rounded-full"></div>
                <h4 class="font-semibold text-gray-900 dark:text-white text-sm">Chi-Square (χ²)</h4>
              </div>
              <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug">
                Tests whether two variables are independent or related. 
                Higher values indicate stronger relationships.
              </p>
            </div>
            <div class="flex-shrink-0 bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4 min-w-[140px] text-center border border-blue-200 dark:border-blue-800">
              <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Value</div>
              <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ formatNumber(data?.chiSquare) }}</div>
              <div v-if="data?.significant" class="text-xs mt-1" :class="data.pValue < 0.05 ? 'text-green-600 dark:text-green-400 font-medium' : 'text-gray-500 dark:text-gray-400'">
                {{ data.significant }}
              </div>
            </div>
          </div>
        </div>

        <!-- Phi -->
        <div>
          <div class="flex items-start gap-4">
            <div class="flex-1">
              <div class="flex items-center space-x-2 mb-2">
                <div class="w-2 h-2 bg-green-500 rounded-full"></div>
                <h4 class="font-semibold text-gray-900 dark:text-white text-sm">Phi (φ)</h4>
              </div>
              <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug">
                Measures the strength of association between two binary variables. 
                Values range from 0 (no association) to 1 (perfect association). 
                <span class="text-gray-500 dark:text-gray-400 italic">e.g., 0.3+ indicates a moderate relationship</span>
              </p>
            </div>
            <div class="flex-shrink-0 bg-green-50 dark:bg-green-900/20 rounded-lg p-4 min-w-[140px] text-center border border-green-200 dark:border-green-800">
              <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Value</div>
              <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ formatNumber(data?.phi) }}</div>
            </div>
          </div>
        </div>

        <!-- Cramér's V -->
        <div>
          <div class="flex items-start gap-4">
            <div class="flex-1">
              <div class="flex items-center space-x-2 mb-2">
                <div class="w-2 h-2 bg-purple-500 rounded-full"></div>
                <h4 class="font-semibold text-gray-900 dark:text-white text-sm">Cramér's V</h4>
              </div>
              <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug">
                Measures association strength for tables of any size (extension of Phi). 
                Values range from 0 (no association) to 1 (perfect association). 
                <span class="text-gray-500 dark:text-gray-400 italic">Guidelines: 0.1 = small, 0.3 = medium, 0.5+ = large effect</span>
              </p>
            </div>
            <div class="flex-shrink-0 bg-purple-50 dark:bg-purple-900/20 rounded-lg p-4 min-w-[140px] text-center border border-purple-200 dark:border-purple-800">
              <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Value</div>
              <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ formatNumber(data?.cramersV) }}</div>
            </div>
          </div>
        </div>

        <!-- Total N -->
        <div>
          <div class="flex items-start gap-4">
            <div class="flex-1">
              <div class="flex items-center space-x-2 mb-2">
                <div class="w-2 h-2 bg-orange-500 rounded-full"></div>
                <h4 class="font-semibold text-gray-900 dark:text-white text-sm">Total N</h4>
              </div>
              <p class="text-sm text-gray-600 dark:text-gray-300 leading-snug">
                The total number of valid responses included in the crosstab analysis. 
                Larger samples provide more reliable statistical measures.
              </p>
            </div>
            <div class="flex-shrink-0 bg-orange-50 dark:bg-orange-900/20 rounded-lg p-4 min-w-[140px] text-center border border-orange-200 dark:border-orange-800">
              <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Value</div>
              <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ data?.totalN || 0 }}</div>
            </div>
          </div>
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
  },
  data: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['close'])

const close = () => {
  emit('close')
}

const formatNumber = (value) => {
  if (value == null) return '—'
  return typeof value === 'number' ? value.toFixed(3) : value
}

const formatPValue = (value) => {
  if (value == null) return '—'
  if (value < 0.001) return '<.001'
  if (value < 0.01) return '<.01'
  if (value < 0.05) return '<.05'
  return value.toFixed(3)
}
</script>
