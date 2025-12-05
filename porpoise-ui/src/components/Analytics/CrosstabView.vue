<template>
  <div class="h-full flex">
    <!-- Question List Sidebar -->
    <aside class="w-80 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden">
      <QuestionListSelector
        :surveyId="surveyId"
        selectionMode="crosstab"
        @crosstab-selection="handleCrosstabSelection"
      />
    </aside>

    <!-- Crosstab Results -->
    <div class="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900">
      <!-- Loading State -->
      <div 
        v-if="loading"
        class="flex items-center justify-center h-full"
      >
        <div class="text-center">
          <svg class="animate-spin h-12 w-12 mx-auto text-blue-500" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
          </svg>
          <p class="mt-4 text-sm text-gray-600 dark:text-gray-400">Generating crosstab...</p>
        </div>
      </div>

      <!-- Empty State -->
      <div 
        v-else-if="!firstQuestion || !secondQuestion"
        class="flex items-center justify-center h-full"
      >
        <div class="text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Crosstab Analysis</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Select two questions to create a crosstab analysis
          </p>
        </div>
      </div>

      <!-- Error State -->
      <div v-else-if="error" class="flex items-center justify-center h-full">
        <div class="text-center p-6">
          <svg class="mx-auto h-12 w-12 text-red-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Error</h3>
          <p class="mt-1 text-sm text-red-600 dark:text-red-400">{{ error }}</p>
          <button
            @click="generateCrosstab"
            class="mt-4 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white text-sm rounded-lg"
          >
            Try Again
          </button>
        </div>
      </div>

      <!-- Crosstab Results -->
      <div v-else-if="crosstabData" class="p-6">
        <!-- Header -->
        <div class="mb-6">
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">
            {{ crosstabData.firstQuestion.label }}
          </h2>
          <p class="text-sm text-gray-600 dark:text-gray-400">
            By {{ crosstabData.secondQuestion.label }}
          </p>
        </div>

        <!-- Statistics -->
        <div class="grid grid-cols-4 gap-4 mb-6">
          <div class="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
            <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Total N</div>
            <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ crosstabData.totalN }}</div>
          </div>
          
          <div class="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
            <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Chi-Square</div>
            <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ formatNumber(crosstabData.chiSquare) }}</div>
            <div v-if="crosstabData.significant" class="text-xs text-green-600 dark:text-green-400 mt-1">
              {{ crosstabData.significant }}
            </div>
          </div>
          
          <div class="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
            <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Phi</div>
            <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ formatNumber(crosstabData.phi) }}</div>
          </div>
          
          <div class="bg-white dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
            <div class="text-xs text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-1">Cramér's V</div>
            <div class="text-2xl font-bold text-gray-900 dark:text-white">{{ formatNumber(crosstabData.cramersV) }}</div>
          </div>
        </div>

        <!-- Table -->
        <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden mb-6">
          <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
              <thead class="bg-gray-50 dark:bg-gray-700">
                <tr>
                  <th 
                    v-for="(col, idx) in tableColumns"
                    :key="idx"
                    class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider"
                  >
                    {{ col }}
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                <tr
                  v-for="(row, rowIdx) in crosstabData.table"
                  :key="rowIdx"
                  class="hover:bg-gray-50 dark:hover:bg-gray-700"
                >
                  <td
                    v-for="(col, colIdx) in tableColumns"
                    :key="colIdx"
                    class="px-4 py-3 text-sm whitespace-nowrap"
                    :class="colIdx === 0 ? 'font-medium text-gray-900 dark:text-white' : 'text-gray-600 dark:text-gray-300'"
                  >
                    {{ formatCellValue(row[col]) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Chart -->
        <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 p-6">
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">
            {{ crosstabData.firstQuestion.label }}
          </h3>
          
          <div class="space-y-3">
            <div
              v-for="(row, idx) in chartData"
              :key="idx"
              class="flex items-center gap-3"
            >
              <div class="w-32 text-sm text-gray-700 dark:text-gray-300 text-right truncate">
                {{ row.label }}
              </div>
              <div class="flex-1 flex items-center gap-2">
                <div class="flex-1 bg-gray-200 dark:bg-gray-700 rounded-full h-8 overflow-hidden">
                  <div
                    v-for="(segment, segIdx) in row.segments"
                    :key="segIdx"
                    :style="{ width: segment.width + '%', backgroundColor: segment.color }"
                    class="h-full inline-block transition-all"
                    :title="`${segment.label}: ${segment.value}`"
                  ></div>
                </div>
                <div class="w-16 text-sm font-medium text-gray-900 dark:text-white text-right">
                  {{ row.total }}
                </div>
              </div>
            </div>
          </div>

          <!-- Legend -->
          <div class="mt-6 flex flex-wrap gap-4">
            <div
              v-for="(legend, idx) in chartLegend"
              :key="idx"
              class="flex items-center gap-2"
            >
              <div
                :style="{ backgroundColor: legend.color }"
                class="w-4 h-4 rounded"
              ></div>
              <span class="text-sm text-gray-700 dark:text-gray-300">{{ legend.label }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import axios from 'axios'
import QuestionListSelector from './QuestionListSelector.vue'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  }
})

// State
const loading = ref(false)
const error = ref(null)
const firstQuestion = ref(null)
const secondQuestion = ref(null)
const crosstabData = ref(null)

// Colors for chart segments
const chartColors = [
  '#3b82f6', // blue
  '#10b981', // green
  '#f59e0b', // amber
  '#ef4444', // red
  '#8b5cf6', // purple
  '#ec4899', // pink
  '#14b8a6', // teal
  '#f97316', // orange
]

// Auto-generate crosstab when both questions are selected
watch([firstQuestion, secondQuestion], ([first, second]) => {
  if (first && second) {
    generateCrosstab()
  }
})

// Computed
const tableColumns = computed(() => {
  if (!crosstabData.value || !crosstabData.value.table || crosstabData.value.table.length === 0) {
    return []
  }
  return Object.keys(crosstabData.value.table[0])
})

const chartData = computed(() => {
  if (!crosstabData.value || !crosstabData.value.table) return []

  const data = []
  const columns = tableColumns.value.filter(col => col !== '' && !col.toLowerCase().includes('total'))
  
  for (const row of crosstabData.value.table) {
    const label = row[''] || row[Object.keys(row)[0]] || ''
    if (label.toLowerCase().includes('total')) continue

    const segments = []
    let total = 0

    columns.forEach((col, idx) => {
      const value = parseFloat(row[col]) || 0
      if (value > 0) {
        segments.push({
          label: col,
          value: value,
          width: 0, // Will be calculated after getting total
          color: chartColors[idx % chartColors.length]
        })
        total += value
      }
    })

    // Calculate widths as percentages
    segments.forEach(seg => {
      seg.width = (seg.value / total) * 100
    })

    data.push({
      label,
      segments,
      total
    })
  }

  return data
})

const chartLegend = computed(() => {
  if (!crosstabData.value || !crosstabData.value.table) return []

  const columns = tableColumns.value.filter(col => col !== '' && !col.toLowerCase().includes('total'))
  
  return columns.map((col, idx) => ({
    label: col,
    color: chartColors[idx % chartColors.length]
  }))
})

// Methods
function handleCrosstabSelection({ first, second }) {
  firstQuestion.value = first
  secondQuestion.value = second
  crosstabData.value = null // Reset crosstab data when selection changes
}

async function generateCrosstab() {
  if (!firstQuestion.value || !secondQuestion.value) {
    console.log('Cannot generate crosstab: missing questions', { firstQuestion: firstQuestion.value, secondQuestion: secondQuestion.value })
    return
  }

  console.log('Generating crosstab with:', {
    surveyId: props.surveyId,
    firstQuestionId: firstQuestion.value.id,
    secondQuestionId: secondQuestion.value.id
  })

  loading.value = true
  error.value = null

  try {
    const response = await axios.post(
      `http://localhost:5107/api/survey-analysis/${props.surveyId}/crosstab`,
      {
        firstQuestionId: firstQuestion.value.id,
        secondQuestionId: secondQuestion.value.id
      }
    )

    console.log('Crosstab response:', response.data)
    crosstabData.value = response.data
  } catch (err) {
    console.error('Failed to generate crosstab:', err)
    console.error('Error details:', err.response?.data)
    error.value = err.response?.data?.title || err.response?.data || 'Failed to generate crosstab. Please try again.'
  } finally {
    loading.value = false
  }
}

function formatNumber(value) {
  if (typeof value !== 'number') return value
  return value.toFixed(3)
}

function formatCellValue(value) {
  if (value === null || value === undefined) return '-'
  if (typeof value === 'number') {
    return value % 1 === 0 ? value : value.toFixed(1)
  }
  return value
}
</script>
