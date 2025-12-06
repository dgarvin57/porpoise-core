<template>
  <div class="h-full flex flex-col bg-white dark:bg-gray-800">
    <!-- Header -->
    <div class="flex-shrink-0 px-6 py-4 border-b border-gray-200 dark:border-gray-700">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-xl font-bold text-gray-900 dark:text-white">Raw Survey Data</h2>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            View the imported survey response data
          </p>
        </div>
        <div v-if="data" class="text-sm text-gray-600 dark:text-gray-400">
          <span class="font-semibold">{{ data.dataRows }}</span> cases × 
          <span class="font-semibold">{{ data.headerRow.length }}</span> columns
        </div>
      </div>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <svg class="animate-spin h-10 w-10 text-blue-600 dark:text-blue-400 mx-auto" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Loading survey data...</p>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <svg class="mx-auto h-12 w-12 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Error Loading Data</h3>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">{{ error }}</p>
        <button
          @click="loadData"
          class="mt-4 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
        >
          Retry
        </button>
      </div>
    </div>

    <!-- Data Table -->
    <div v-else-if="data" class="flex-1 overflow-auto">
      <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
        <thead class="bg-gray-50 dark:bg-gray-900 sticky top-0 z-10">
          <tr>
            <th scope="col" class="px-3 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider bg-gray-100 dark:bg-gray-800 sticky left-0 z-20 border-r border-gray-300 dark:border-gray-600">
              Row #
            </th>
            <th 
              v-for="(header, index) in data.headerRow" 
              :key="index"
              scope="col" 
              class="px-3 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider whitespace-nowrap"
            >
              {{ header }}
            </th>
          </tr>
        </thead>
        <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
          <tr 
            v-for="(row, rowIndex) in paginatedData" 
            :key="rowIndex"
            class="hover:bg-gray-50 dark:hover:bg-gray-700"
          >
            <td class="px-3 py-2 text-sm font-medium text-gray-900 dark:text-white bg-gray-50 dark:bg-gray-900 sticky left-0 z-10 border-r border-gray-300 dark:border-gray-600">
              {{ rowIndex + 1 + (currentPage - 1) * rowsPerPage }}
            </td>
            <td 
              v-for="(cell, cellIndex) in row" 
              :key="cellIndex"
              class="px-3 py-2 text-sm text-gray-700 dark:text-gray-300 whitespace-nowrap"
            >
              {{ cell }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Pagination Controls -->
    <div v-if="data && totalPages > 1" class="flex-shrink-0 px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
      <div class="flex items-center justify-between">
        <div class="text-sm text-gray-700 dark:text-gray-300">
          Showing {{ ((currentPage - 1) * rowsPerPage) + 1 }} to {{ Math.min(currentPage * rowsPerPage, data.dataRows) }} of {{ data.dataRows }} cases
        </div>
        <div class="flex items-center space-x-2">
          <button
            @click="currentPage = 1"
            :disabled="currentPage === 1"
            class="px-3 py-1 text-sm border border-gray-300 dark:border-gray-600 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300"
          >
            First
          </button>
          <button
            @click="currentPage--"
            :disabled="currentPage === 1"
            class="px-3 py-1 text-sm border border-gray-300 dark:border-gray-600 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300"
          >
            Previous
          </button>
          <span class="px-3 py-1 text-sm text-gray-700 dark:text-gray-300">
            Page {{ currentPage }} of {{ totalPages }}
          </span>
          <button
            @click="currentPage++"
            :disabled="currentPage === totalPages"
            class="px-3 py-1 text-sm border border-gray-300 dark:border-gray-600 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300"
          >
            Next
          </button>
          <button
            @click="currentPage = totalPages"
            :disabled="currentPage === totalPages"
            class="px-3 py-1 text-sm border border-gray-300 dark:border-gray-600 rounded-md disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300"
          >
            Last
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { API_BASE_URL } from '@/config/api'
import { ref, computed, watch } from 'vue'
import axios from 'axios'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  }
})

const loading = ref(false)
const error = ref(null)
const data = ref(null)
const currentPage = ref(1)
const rowsPerPage = ref(100)

const totalPages = computed(() => {
  if (!data.value) return 0
  return Math.ceil(data.value.dataRows / rowsPerPage.value)
})

const paginatedData = computed(() => {
  if (!data.value || !data.value.dataList) return []
  
  const start = (currentPage.value - 1) * rowsPerPage.value + 1 // +1 to skip header
  const end = Math.min(start + rowsPerPage.value, data.value.dataList.length)
  
  return data.value.dataList.slice(start, end)
})

const loadData = async () => {
  loading.value = true
  error.value = null
  
  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/${props.surveyId}/data`)
    data.value = response.data
  } catch (err) {
    console.error('Error loading survey data:', err)
    error.value = err.response?.data || err.message || 'Failed to load survey data'
  } finally {
    loading.value = false
  }
}

// Load data when component mounts or surveyId changes
watch(() => props.surveyId, () => {
  if (props.surveyId) {
    loadData()
  }
}, { immediate: true })
</script>
