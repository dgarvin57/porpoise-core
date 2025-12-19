<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 mt-4 max-w-6xl mx-auto">
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
      <!-- Client Filter -->
      <div>
        <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1 ">
          Client
        </label>
        <Select
          v-model="filters.client"
          :options="clientOptions"
          optionLabel="label"
          optionValue="value"
          @change="emitFilters"
          appendTo="self"
          class="w-full"
          size="small"
          :pt="{
            root: { class: 'text-xs border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 w-full' },
            trigger: { class: 'px-2.5 py-1.5' },
            label: { class: 'text-xs' },
            panel: { class: 'bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-md shadow-lg py-1' },
            item: { class: 'text-xs leading-tight text-gray-800 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-750 px-2.5 py-0.5 flex items-center gap-2' },
            itemLabel: { class: 'text-xs' },
            header: { class: 'hidden' }
          }"
        >
          <template #option="slotProps">
            <div class="flex items-center gap-2">
              <svg v-if="slotProps.option.value === ''" class="w-3 h-3 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21H3V3h6V1H3a2 2 0 00-2 2v18a2 2 0 002 2h18a2 2 0 002-2v-6h-2v6z" />
              </svg>
              <span>{{ slotProps.option.label }}</span>
            </div>
          </template>
        </Select>
      </div>

      <!-- Status Filter -->
      <div>
        <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">
          Status
        </label>
        <Select
          v-model="filters.status"
          :options="statusOptions"
          optionLabel="label"
          optionValue="value"
          @change="emitFilters"
          appendTo="self"
          class="w-full"
          size="small"
          :pt="{
            root: { class: 'text-xs border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 w-full' },
            trigger: { class: 'px-2.5 py-1.5' },
            label: { class: 'text-xs' },
            panel: { class: 'bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-md shadow-lg py-1' },
            item: { class: 'text-xs leading-tight text-gray-800 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-750 px-2.5 py-0.5 flex items-center gap-2' },
            itemLabel: { class: 'text-xs' },
            header: { class: 'hidden' }
          }"
        >
          <template #option="slotProps">
            <div class="flex items-center gap-2">
              <svg class="w-2.5 h-2.5 fill-current" :class="getStatusIcon(slotProps.option.value)" viewBox="0 0 24 24">
                <circle cx="12" cy="12" r="10" />
              </svg>
              <span>{{ slotProps.option.label }}</span>
            </div>
          </template>
        </Select>
      </div>

      <!-- Date Range Filter -->
      <div>
        <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">
          Date Range
        </label>
        <Select
          v-model="filters.dateRange"
          :options="dateRangeOptions"
          optionLabel="label"
          optionValue="value"
          @change="emitFilters"
          appendTo="self"
          class="w-full"
          size="small"
          :pt="{
            root: { class: 'text-xs border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 w-full' },
            trigger: { class: 'px-2.5 py-1.5' },
            label: { class: 'text-xs' },
            panel: { class: 'bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-md shadow-lg py-1' },
            item: { class: 'text-xs leading-tight text-gray-800 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-750 px-2.5 py-0.5 flex items-center gap-2' },
            itemLabel: { class: 'text-xs' },
            header: { class: 'hidden' }
          }"
        >
          <template #option="slotProps">
            <div class="flex items-center gap-2">
              <svg class="w-3 h-3 text-gray-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
              <span>{{ slotProps.option.label }}</span>
            </div>
          </template>
        </Select>
      </div>
    </div>

    <!-- Active Filters Summary / Clear Button -->
    <div v-if="hasActiveFilters" class="mt-4 pt-4 border-t border-gray-200 dark:border-gray-700 flex items-center justify-between">
      <div class="flex items-center space-x-2 text-sm text-gray-600 dark:text-gray-400">
        <span class="font-medium">Active filters:</span>
        <span v-if="filters.client" class="inline-flex items-center px-2 py-1 rounded-md bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
          {{ filters.client }}
        </span>
        <span v-if="filters.status" class="inline-flex items-center px-2 py-1 rounded-md bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
          {{ filters.status }}
        </span>
        <span v-if="filters.dateRange" class="inline-flex items-center px-2 py-1 rounded-md bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-200">
          {{ getDateRangeLabel(filters.dateRange) }}
        </span>
      </div>
      <button
        @click="clearFilters"
        class="bg-transparent text-sm text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 font-medium"
      >
        Clear All
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import Select from 'primevue/select'

const props = defineProps({
  clients: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['filter-change'])

const filters = ref({
  client: '',
  status: '',
  dateRange: ''
})

const statusOptions = [
  { label: 'All Statuses', value: '' },
  { label: 'Active', value: 'Active' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Draft', value: 'Draft' },
  { label: 'Archived', value: 'Archived' }
]

const dateRangeOptions = [
  { label: 'All Time', value: '' },
  { label: 'Today', value: 'today' },
  { label: 'Last 7 Days', value: 'week' },
  { label: 'Last 30 Days', value: 'month' },
  { label: 'Last 3 Months', value: 'quarter' },
  { label: 'Last Year', value: 'year' }
]

const clientOptions = computed(() => [
  { label: 'All Clients', value: '' },
  ...props.clients.map(client => ({ label: client, value: client }))
])

const hasActiveFilters = computed(() => {
  return filters.value.client || filters.value.status || filters.value.dateRange
})

function emitFilters() {
  emit('filter-change', { ...filters.value })
}

function clearFilters() {
  filters.value = {
    client: '',
    status: '',
    dateRange: ''
  }
  emitFilters()
}

function getDateRangeLabel(range) {
  const labels = {
    today: 'Today',
    week: 'Last 7 Days',
    month: 'Last 30 Days',
    quarter: 'Last 3 Months',
    year: 'Last Year'
  }
  return labels[range] || range
}

function getStatusIcon(status) {
  const statusClasses = {
    'Active': 'text-green-500',
    'Completed': 'text-blue-500',
    'Draft': 'text-yellow-500',
    'Archived': 'text-gray-500',
    '': 'text-gray-400'
  }
  return statusClasses[status] || statusClasses['']
}
</script>
