<template>
  <div class="h-full flex">
    <!-- Question List Sidebar -->
    <aside class="w-80 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col">
      <QuestionListSelector
        :surveyId="surveyId"
        selectionMode="crosstab"
        :initialFirstSelection="firstQuestion"
        :initialSecondSelection="secondQuestion"
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
          <Button
            @click="generateCrosstab"
            class="mt-4"
            size="sm"
          >
            Try Again
          </Button>
        </div>
      </div>

      <!-- Crosstab Results -->
      <div v-else-if="crosstabData" class="flex flex-col h-full">
        <!-- Sticky Header -->
        <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-3 sticky top-0 z-10">
          <div class="flex items-center justify-between">
            <div class="flex items-center space-x-3">
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white">
                {{ crosstabData.firstQuestion.label }}&nbsp;&nbsp;<span class="text-gray-400 dark:text-gray-500 font-normal">by</span>&nbsp;&nbsp;{{ crosstabData.secondQuestion.label }}
              </h2>
            </div>
            <div class="flex items-center space-x-3">
              <div class="flex items-center space-x-4 text-sm text-gray-600 dark:text-gray-400">
                <span v-if="crosstabData.significant" :class="crosstabData.pValue < 0.05 ? 'text-green-600 dark:text-green-400 font-medium' : ''">
                  {{ crosstabData.significant }}
                </span>
                <span>•</span>
                <span><span class="font-medium">Total N:</span> {{ crosstabData.totalN }}</span>
              </div>
              <button
                @click="showStatistics = true"
                class="p-1 bg-transparent text-gray-500 hover:text-blue-600 hover:bg-blue-50 dark:text-gray-400 dark:hover:text-blue-400 dark:hover:bg-gray-800 transition-colors border-0"
                title="View statistical measures (Chi-Square, Phi, Cramér's V)"
              >
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" fill="currentColor" />
                </svg>
              </button>
            </div>
          </div>
        </div>

        <!-- Scrollable Content -->
        <div class="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900 p-6">
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
                    {{ col.trim() === '' ? '' : col }}
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
        <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
          <div class="px-6 py-3 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
            <h3 class="text-base font-medium text-gray-900 dark:text-white">
              {{ crosstabData.firstQuestion.label }}
            </h3>
            <div class="flex items-center gap-4">
              <label class="flex items-center cursor-pointer">
                <input
                  type="radio"
                  v-model="graphMode"
                  value="index"
                  class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                />
                <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Graph Index</span>
              </label>
              <label class="flex items-center cursor-pointer">
                <input
                  type="radio"
                  v-model="graphMode"
                  value="posneg"
                  class="w-4 h-4 text-blue-600 border-gray-300 focus:ring-blue-500"
                />
                <span class="ml-2 text-sm text-gray-700 dark:text-gray-300">Graph Pos/Neg Percent</span>
              </label>
            </div>
          </div>
          <div class="p-6">
          
          <!-- Index Mode Chart -->
          <div v-if="graphMode === 'index'" class="space-y-3">
            <div
              v-for="(bar, idx) in chartData"
              :key="idx"
              class="flex items-center gap-3"
            >
              <div class="w-32 text-sm text-gray-700 dark:text-gray-300 text-right truncate">
                {{ bar.label }}
              </div>
              <div class="flex-1 flex items-center gap-2">
                <div class="flex-1 bg-gray-200 dark:bg-gray-700 rounded-full h-8 overflow-hidden">
                  <div
                    :style="{ width: Math.min((bar.value / 200 * 100), 100) + '%', backgroundColor: bar.color }"
                    class="h-full transition-all"
                  ></div>
                </div>
                <div class="w-16 text-sm font-medium text-gray-900 dark:text-white text-right">
                  {{ bar.value }}
                </div>
              </div>
            </div>
          </div>
          
          <!-- Pos/Neg Mode Chart -->
          <div v-else class="space-y-4">
            <div
              v-for="(bar, idx) in chartData"
              :key="idx"
              class="flex items-start gap-3"
            >
              <div class="w-32 text-sm text-gray-700 dark:text-gray-300 text-right pt-2">
                {{ bar.label }}
              </div>
              <div class="flex-1 space-y-1">
                <div
                  v-for="(segment, segIdx) in bar.segments"
                  :key="segIdx"
                  class="flex items-center gap-2"
                >
                  <div class="flex-1 bg-gray-200 dark:bg-gray-700 rounded h-5 overflow-hidden max-w-md">
                    <div
                      :style="{ width: Math.min(segment.value, 100) + '%', backgroundColor: segment.color }"
                      class="h-full transition-all"
                      :title="`${segment.label}: ${segment.value.toFixed(1)}%`"
                    ></div>
                  </div>
                  <div class="w-12 text-xs font-medium text-gray-700 dark:text-gray-300 text-right">
                    {{ segment.value.toFixed(1) }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Legend and AI Explanation -->
          <div class="mt-6 flex items-center justify-between">
            <Button
              @click="showExplanation = true"
              variant="ghost"
              size="sm"
            >
              <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
              </svg>
              <span>Understanding This Analysis</span>
            </Button>
            <div v-if="graphMode === 'posneg'" class="flex flex-wrap gap-4">
              <div class="flex items-center gap-2">
                <div class="w-4 h-4 rounded bg-blue-500"></div>
                <span class="text-sm text-gray-700 dark:text-gray-300">Positive Indexes</span>
              </div>
              <div class="flex items-center gap-2">
                <div class="w-4 h-4 rounded bg-red-500"></div>
                <span class="text-sm text-gray-700 dark:text-gray-300">Negative Indexes</span>
              </div>
            </div>
          </div>
          </div> <!-- Close p-6 div -->
        </div> <!-- Close chart div -->
        </div> <!-- Close scrollable content -->
      </div> <!-- Close crosstab results -->
    </div>
    
    <!-- Explanation Modal -->
    <div v-if="showExplanation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showExplanation = false">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
        <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
          <div class="flex items-center justify-between">
            <div>
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Analysis & Insights</h3>
              <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">
                {{ crosstabData.firstQuestion.label }} <span class="text-gray-400 dark:text-gray-500">by</span> {{ crosstabData.secondQuestion.label }}
              </p>
            </div>
            <CloseButton @click="showExplanation = false" />
          </div>
        </div>
        <div class="px-6 py-4 space-y-4">
          <!-- Quick Tip -->
          <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
            <div class="flex gap-3">
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 flex-shrink-0 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
              </svg>
              <div>
                <h4 class="font-semibold text-gray-900 dark:text-white mb-1 text-sm">Quick Tip</h4>
                <p class="text-sm text-gray-700 dark:text-gray-300">Use the positive/negative graph to quickly compare sentiment across different demographic groups or response categories. Larger positive bars indicate more favorable opinions in that group.</p>
              </div>
            </div>
          </div>
          
          <!-- AI Analysis Section (Collapsible) -->
          <div class="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
            <button
              @click="showAIAnalysis = !showAIAnalysis"
              class="w-full px-4 py-3 flex items-center justify-between bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-900/20 dark:to-indigo-900/20 hover:from-blue-100 hover:to-indigo-100 dark:hover:from-blue-900/30 dark:hover:to-indigo-900/30 transition-colors border-b border-blue-200 dark:border-blue-800"
            >
              <div class="flex items-center gap-2">
                <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                </svg>
                <span class="font-medium text-gray-900 dark:text-white">AI Analysis of Your Data</span>
              </div>
              <svg
                class="w-5 h-5 text-gray-500 transition-transform"
                :class="{ 'rotate-180': showAIAnalysis }"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </button>
            <div v-show="showAIAnalysis" class="px-4 py-4 bg-white dark:bg-gray-800">
              <div v-if="loadingAnalysis" class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                <svg class="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                  <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                  <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <span>Generating analysis...</span>
              </div>
              <div v-else-if="aiAnalysis" class="text-sm text-gray-700 dark:text-gray-300 space-y-3">
                <p v-for="(paragraph, idx) in aiAnalysis.split('\n\n').filter(p => p.trim())" :key="idx">
                  {{ paragraph.trim() }}
                </p>
              </div>
              <Button
                v-else
                @click="generateAIAnalysis"
                :loading="loadingAnalysis"
                size="sm"
              >
                Generate AI Analysis
              </Button>
            </div>
          </div>
          
          <!-- Collapsible General Explanation -->
          <div class="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden">
            <button
              @click="showGeneralInfo = !showGeneralInfo"
              class="w-full px-4 py-3 flex items-center justify-between bg-gray-50 dark:bg-gray-900 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <span class="font-medium text-gray-900 dark:text-white">Understanding Crosstab Analysis</span>
              <svg
                class="w-5 h-5 text-gray-500 transition-transform"
                :class="{ 'rotate-180': showGeneralInfo }"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </button>
            <div v-show="showGeneralInfo" class="px-4 py-4 space-y-4 text-sm text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-800">
              <div>
                <h4 class="font-semibold text-gray-900 dark:text-white mb-2">What is Crosstab Analysis?</h4>
                <p>Crosstab (cross-tabulation) analysis examines the relationship between two categorical variables by displaying their frequencies in a table format. This helps identify patterns and correlations between different survey questions.</p>
              </div>
              
              <div>
                <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Statistical Measures</h4>
                <ul class="list-disc list-inside space-y-1 ml-2">
                  <li><strong>Chi-Square (χ²):</strong> Tests whether the two variables are independent or related</li>
                  <li><strong>Phi (φ):</strong> Measures association strength for 2×2 tables (range: 0-1)</li>
                  <li><strong>Cramér's V:</strong> Measures association strength for larger tables (range: 0-1)</li>
                  <li><strong>Total N:</strong> Total number of valid responses analyzed</li>
                </ul>
              </div>
              
              <div>
                <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Graph Modes</h4>
                <ul class="list-disc list-inside space-y-1 ml-2">
                  <li><strong>Graph Index:</strong> Shows the overall index value (0-200 scale) for each category, where 100 is neutral, above 100 is positive, and below 100 is negative</li>
                  <li><strong>Graph Pos/Neg Percent:</strong> Shows separate bars for positive and negative response percentages within each category, making it easy to see sentiment distribution</li>
                </ul>
              </div>
              
              <div>
                <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Index Values Explained</h4>
                <ul class="list-disc list-inside space-y-1 ml-2">
                  <li><strong>Positive Indexes:</strong> Percentage of favorable/desirable responses (e.g., "Strongly Agree", "Very Satisfied")</li>
                  <li><strong>Negative Indexes:</strong> Percentage of unfavorable/undesirable responses (e.g., "Strongly Disagree", "Very Dissatisfied")</li>
                  <li><strong>Overall Index:</strong> Combined measure where higher values indicate more positive sentiment</li>
                </ul>
              </div>
            </div>
          </div>
        </div>
        <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 flex justify-end border-t border-gray-200 dark:border-gray-700">
          <Button @click="showExplanation = false">
            Close
          </Button>
        </div>
      </div>
    </div>
    
    <!-- Statistics Modal -->
    <CrosstabStatisticsModal :show="showStatistics" :data="crosstabData" @close="showStatistics = false" />
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import axios from 'axios'
import QuestionListSelector from './QuestionListSelector.vue'
import Button from '../common/Button.vue'
import CloseButton from '../common/CloseButton.vue'
import CrosstabStatisticsModal from '../CrosstabStatisticsModal.vue'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  initialFirstQuestion: {
    type: Object,
    default: null
  },
  initialSecondQuestion: {
    type: Object,
    default: null
  }
})

console.log('CrosstabView: Component created with props', { 
  initialFirstQuestion: props.initialFirstQuestion, 
  initialSecondQuestion: props.initialSecondQuestion 
})

const emit = defineEmits(['selections-changed'])

// State
const loading = ref(false)
const error = ref(null)
const firstQuestion = ref(props.initialFirstQuestion)
const secondQuestion = ref(props.initialSecondQuestion)
const crosstabData = ref(null)
const graphMode = ref('index') // 'index' or 'posneg'
const showExplanation = ref(false)
const showStatistics = ref(false)
const aiAnalysis = ref('')
const loadingAnalysis = ref(false)
const showGeneralInfo = ref(false)
const showAIAnalysis = ref(false)

console.log('CrosstabView: Internal refs initialized', { 
  firstQuestion: firstQuestion.value, 
  secondQuestion: secondQuestion.value 
})

// Watch for prop changes and update internal state
watch(() => props.initialFirstQuestion, (newVal, oldVal) => {
  console.log('CrosstabView: initialFirstQuestion prop changed', { old: oldVal, new: newVal })
  firstQuestion.value = newVal
}, { deep: true, immediate: true })

watch(() => props.initialSecondQuestion, (newVal, oldVal) => {
  console.log('CrosstabView: initialSecondQuestion prop changed', { old: oldVal, new: newVal })
  secondQuestion.value = newVal
}, { deep: true, immediate: true })

// Auto-generate crosstab when both questions are selected
watch([firstQuestion, secondQuestion], ([first, second]) => {
  if (first && second) {
    generateCrosstab()
  }
  // Emit changes for parent component to persist
  emit('selections-changed', { 
    firstQuestion: first, 
    secondQuestion: second 
  })
})

// Computed
const tableColumns = computed(() => {
  if (!crosstabData.value || !crosstabData.value.table || crosstabData.value.table.length === 0) {
    return []
  }
  // Include all columns, even those with space as name (first column)
  return Object.keys(crosstabData.value.table[0])
})

const chartData = computed(() => {
  if (!crosstabData.value) return []

  const columns = tableColumns.value.filter(col => col !== '' && !col.toLowerCase().includes('total'))
  
  if (graphMode.value === 'index') {
    // Index mode: Show one bar per DV column with its index value
    return columns.map(col => {
      const indexInfo = crosstabData.value.ivIndexes?.find(idx => idx.label === col)
      return {
        label: col,
        value: indexInfo?.index || 0,
        color: '#3b82f6' // blue
      }
    }).filter(bar => bar.value > 0) // Don't show bars with zero value
  } else {
    // Pos/Neg mode: Show stacked bars with positive and negative index percentages
    return columns.map(col => {
      const indexInfo = crosstabData.value.ivIndexes?.find(idx => idx.label === col)
      const posIndex = indexInfo?.posIndex || 0
      const negIndex = indexInfo?.negIndex || 0
      
      return {
        label: col,
        segments: [
          { label: 'Positive', value: posIndex, color: '#3b82f6', width: posIndex },
          { label: 'Negative', value: negIndex, color: '#ef4444', width: negIndex }
        ].filter(s => s.value > 0)
      }
    }).filter(bar => bar.segments.length > 0) // Don't show bars with no segments
  }
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
  aiAnalysis.value = '' // Reset AI analysis when generating new crosstab

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

async function generateAIAnalysis() {
  if (!crosstabData.value) return
  
  loadingAnalysis.value = true
  
  try {
    // Prepare context for AI
    const context = {
      independentVariable: firstQuestion.value.label,
      dependentVariable: secondQuestion.value.label,
      totalN: crosstabData.value.totalN,
      chiSquare: crosstabData.value.chiSquare,
      chiSquareSignificant: crosstabData.value.chiSquareSignificant,
      phi: crosstabData.value.phi,
      cramersV: crosstabData.value.cramersV,
      tableData: crosstabData.value.tableData,
      indexes: crosstabData.value.ivIndexes
    }
    
    const response = await axios.post(
      `http://localhost:5107/api/survey-analysis/${props.surveyId}/analyze-crosstab`,
      context
    )
    
    aiAnalysis.value = response.data.analysis
  } catch (err) {
    console.error('Error generating AI analysis:', err)
    aiAnalysis.value = 'Unable to generate analysis at this time. The statistical results above show the key findings from your crosstab.'
  } finally {
    loadingAnalysis.value = false
  }
}

function formatNumber(value) {
  if (typeof value !== 'number') return value
  return value.toFixed(3)
}

function formatPValue(value) {
  if (value == null) return '—'
  if (value < 0.001) return '<.001'
  if (value < 0.01) return '<.01'
  if (value < 0.05) return '<.05'
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
