<template>
  <div class="h-full flex flex-col bg-white dark:bg-gray-800">
    <!-- Header -->
    <div class="flex-shrink-0 px-6 -mb-1 border-b border-gray-200 dark:border-gray-700">
      <div class="flex items-center justify-between py-0.5">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Raw Survey Data</h2>
        <div v-if="tableData.length > 0" class="text-sm text-gray-600 dark:text-gray-400">
          <span class="font-semibold">{{ tableData.length }}</span> cases × 
          <span class="font-semibold">{{ columns.length - 1 }}</span> columns
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

    <!-- PrimeVue DataTable with Virtual Scrolling -->
    <div v-else-if="tableData.length > 0" class="flex-1 overflow-hidden px-6 py-1 pr-1">
      <DataTable 
        :value="tableData" 
        :virtualScrollerOptions="{ itemSize: 30 }"
        scrollable 
        scrollHeight="flex"
        class="text-xs data-view-table"
        :rowHover="true"
      >
        <!-- Row Number Column (Frozen) -->
        <Column 
          field="rowNum" 
          header="Row #" 
          frozen
          :style="{ width: '70px', minWidth: '70px' }"
          class="font-medium"
        >
          <template #body="{ data }">
            <span class="text-gray-900 dark:text-white font-medium">{{ data.rowNum }}</span>
          </template>
        </Column>

        <!-- Dynamic Data Columns (auto-sized with minimum width to prevent wrapping) -->
        <Column 
          v-for="(col, index) in columns.filter(c => c.field !== 'rowNum')" 
          :key="col.field"
          :field="col.field" 
          :header="col.header"
          :style="{ minWidth: index === columns.length - 2 ? '140px' : '50px' }"
        >
          <template #body="{ data }">
            <span class="text-gray-700 dark:text-gray-300">{{ data[col.field] }}</span>
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<script setup>
import { API_BASE_URL } from '@/config/api'
import { ref, computed, watch } from 'vue'
import axios from 'axios'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  }
})

const loading = ref(false)
const error = ref(null)
const data = ref(null)

// Transform data for PrimeVue DataTable
const columns = computed(() => {
  if (!data.value || !data.value.headerRow) return []
  
  // First column is row number
  const cols = [{ field: 'rowNum', header: 'Row #' }]
  
  // Add data columns
  data.value.headerRow.forEach((header, index) => {
    cols.push({
      field: `col${index}`,
      header: header
    })
  })
  
  return cols
})

const tableData = computed(() => {
  if (!data.value || !data.value.dataList) return []
  
  // Transform rows into objects for DataTable
  return data.value.dataList.slice(1).map((row, rowIndex) => {
    const rowData = { rowNum: rowIndex + 1 }
    
    row.forEach((cell, cellIndex) => {
      rowData[`col${cellIndex}`] = cell
    })
    
    return rowData
  })
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

<style scoped>
/* Override PrimeVue DataTable styles to match your theme */
.data-view-table :deep(.p-datatable-thead) {
  @apply bg-blue-50 dark:bg-gray-800;
}

.data-view-table :deep(.p-datatable-thead > tr > th) {
  @apply text-xs font-semibold text-gray-800 dark:text-gray-300 uppercase tracking-wider px-2 py-1.5 border-b-2 border-blue-200 dark:border-gray-600;
}

.data-view-table :deep(.p-datatable-tbody > tr) {
  @apply bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700;
}

.data-view-table :deep(.p-datatable-tbody > tr:nth-child(even)) {
  @apply bg-gray-50 dark:bg-gray-900;
}

.data-view-table :deep(.p-datatable-tbody > tr:hover) {
  @apply bg-blue-50 dark:bg-gray-700;
}

.data-view-table :deep(.p-datatable-tbody > tr > td) {
  @apply px-2 py-1 text-xs;
}

/* Frozen column styling */
.data-view-table :deep(.p-frozen-column) {
  @apply bg-gray-100 dark:bg-gray-800 border-r-2 border-gray-300 dark:border-gray-600 font-semibold;
}

/* Virtual scroller */
.data-view-table :deep(.p-virtualscroller) {
  @apply h-full;
}
</style>
