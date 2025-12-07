<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 shadow-sm">
    <!-- Compact Header -->
    <div class="bg-gray-50 dark:bg-gray-900 px-3 py-1.5 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
      <div class="flex items-center space-x-2">
        <svg 
          class="w-4 h-4" 
          :class="question?.variableType === 1 ? 'text-red-400' : question?.variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
          fill="currentColor" 
          viewBox="0 0 20 20"
        >
          <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
        </svg>
        <h3 class="text-sm font-semibold text-gray-900 dark:text-white">
          {{ question?.label || 'Select a question' }}
        </h3>
        <span class="text-xs text-gray-400 dark:text-gray-500">
          {{ question?.qstNumber }}
        </span>
      </div>
      <div class="flex items-center space-x-3 text-xs text-gray-600 dark:text-gray-400">
        <span>N: {{ question?.totalCases || 0 }}</span>
        <span>•</span>
        <span>CI: +/- {{ question?.samplingError?.toFixed(1) || '0.0' }}</span>
      </div>
    </div>
    
    <!-- Condensed Table -->
    <div class="overflow-x-auto max-h-48 overflow-y-auto">
      <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700 text-xs">
        <thead class="bg-gray-50 dark:bg-gray-700 sticky top-0">
          <tr>
            <th class="px-3 py-1.5 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Response
            </th>
            <th class="px-3 py-1.5 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              N
            </th>
            <th class="px-3 py-1.5 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              %
            </th>
            <th class="px-3 py-1.5 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
              Index
            </th>
          </tr>
        </thead>
        <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
          <tr 
            v-for="(response, idx) in question?.responses || []"
            :key="idx"
            class="hover:bg-gray-50 dark:hover:bg-gray-700"
          >
            <td class="px-3 py-1 text-gray-900 dark:text-white font-medium">
              {{ response.label }}
            </td>
            <td class="px-3 py-1 text-right text-gray-600 dark:text-gray-300">
              {{ response.count }}
            </td>
            <td class="px-3 py-1 text-right text-gray-600 dark:text-gray-300">
              {{ response.percentage?.toFixed(1) }}%
            </td>
            <td class="px-3 py-1 text-right text-gray-600 dark:text-gray-300">
              {{ response.index || 100 }}
            </td>
          </tr>
          <tr v-if="!question || !question.responses || question.responses.length === 0">
            <td colspan="4" class="px-3 py-2 text-center text-gray-500 dark:text-gray-400 italic">
              No data available
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { defineProps } from 'vue'

const props = defineProps({
  question: {
    type: Object,
    default: null
  }
})
</script>
