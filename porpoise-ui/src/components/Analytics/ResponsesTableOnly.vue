<template>
  <div class="h-full flex flex-col bg-white dark:bg-gray-800">
    <!-- Responses Header -->
    <div class="flex-shrink-0 pl-4 pr-1 pb-1 pt-0.5 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 flex items-center justify-between">
      <div class="flex items-center gap-2">
        <h3 class="text-sm font-medium text-gray-900 dark:text-white">
          Responses
        </h3>
        <span class="text-xs text-gray-400 dark:text-gray-500">
          ({{ question.responses?.length || 0 }})
        </span>
      </div>
      <div class="flex items-center gap-2">
        <select
          v-model="localColumnMode"
          @change="$emit('column-mode-changed', localColumnMode)"
          class="text-xs border border-gray-300 dark:border-gray-600 rounded px-2 py-1 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 cursor-pointer"
          style="width: 120px;"
        >
          <option value="totalN">Total N</option>
          <option value="cumulative">Cumulative</option>
          <option value="inverseCumulative">Inv Cumulative</option>
          <option value="samplingError">Samp Error</option>
          <option value="blank">Leave Blank</option>
        </select>
      </div>
    </div>

    <!-- Table Content -->
    <div class="flex-1 overflow-y-auto min-h-0">
      <table class="min-w-full text-xs">
        <thead class="bg-gray-50 dark:bg-gray-700 sticky top-0 z-10">
          <tr>
            <th class="px-2 py-1 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              #
            </th>
            <th class="px-2 py-1 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Response
            </th>
            <th class="px-2 py-1 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              %
            </th>
            <th class="px-2 py-1 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Index
            </th>
            <th v-if="localColumnMode !== 'blank'" class="px-2 py-1 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              {{ columnModeLabel }}
            </th>
          </tr>
        </thead>
        <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
          <tr 
            v-for="(response, index) in computedResponses" 
            :key="index"
            class="hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          >
            <td class="px-2 py-1 text-center text-gray-500 dark:text-gray-400">
              {{ index + 1 }}
            </td>
            <td class="px-2 py-1 text-gray-900 dark:text-white">
              {{ response.label }}
            </td>
            <td class="px-2 py-1 text-right font-medium text-gray-900 dark:text-white">
              {{ response.percentage?.toFixed(1) || '0.0' }}
            </td>
            <td class="px-2 py-1 text-center">
              <span v-if="response.index !== null && response.index !== undefined">
                <span 
                  v-if="response.index > 100" 
                  class="inline-flex items-center text-green-600 dark:text-green-400"
                >
                  <svg class="w-3 h-3 mr-0.5" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M5.293 9.707a1 1 0 010-1.414l4-4a1 1 0 011.414 0l4 4a1 1 0 01-1.414 1.414L11 7.414V15a1 1 0 11-2 0V7.414L6.707 9.707a1 1 0 01-1.414 0z" clip-rule="evenodd" />
                  </svg>
                </span>
                <span 
                  v-else-if="response.index < 100" 
                  class="inline-flex items-center text-red-600 dark:text-red-400"
                >
                  <svg class="w-3 h-3 mr-0.5" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M14.707 10.293a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 111.414-1.414L9 12.586V5a1 1 0 012 0v7.586l2.293-2.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                  </svg>
                </span>
                <span v-else class="text-gray-400 dark:text-gray-500">—</span>
              </span>
              <span v-else class="text-gray-400 dark:text-gray-500">—</span>
            </td>
            <td v-if="localColumnMode !== 'blank'" class="px-2 py-1 text-right text-gray-600 dark:text-gray-400">
              {{ getColumnValue(response) }}
            </td>
          </tr>
        </tbody>
      </table>
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
  columnMode: {
    type: String,
    default: 'totalN'
  }
})

const emit = defineEmits(['column-mode-changed'])

const localColumnMode = ref(props.columnMode)

watch(() => props.columnMode, (newVal) => {
  localColumnMode.value = newVal
})

// Compute cumulative values like ResultsTable does
const computedResponses = computed(() => {
  if (!props.question?.responses) return []
  
  let cumSum = 0
  let invCumSum = 100
  
  return props.question.responses.map((r) => {
    cumSum += r.percentage
    const current = {
      ...r,
      cumulative: cumSum,
      inverseCumulative: invCumSum,
      samplingError: r.samplingError || 0
    }
    invCumSum -= r.percentage
    return current
  })
})

const columnModeLabel = computed(() => {
  switch (localColumnMode.value) {
    case 'totalN':
      return 'N'
    case 'cumulative':
      return 'Cumulative %'
    case 'inverseCumulative':
      return 'Inverse Cum %'
    case 'samplingError':
      return 'SE'
    default:
      return ''
  }
})

function getColumnValue(response) {
  switch (localColumnMode.value) {
    case 'totalN':
      return response.count || 0
    case 'cumulative':
      return response.cumulative?.toFixed(1) || '0.0'
    case 'inverseCumulative':
      return response.inverseCumulative?.toFixed(1) || '0.0'
    case 'samplingError':
      return response.samplingError?.toFixed(1) || '0.0'
    default:
      return ''
  }
}
</script>

<style scoped>
/* Ensure sticky header works properly */
thead {
  position: sticky;
  top: 0;
  z-index: 10;
}
</style>
