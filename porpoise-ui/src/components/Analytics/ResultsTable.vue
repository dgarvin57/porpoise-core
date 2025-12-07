<template>
  <div>
    <!-- Question Header with Analyze in Crosstab button -->
    <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 py-3">
      <div class="relative flex items-center justify-between px-6">
        <div class="flex items-center space-x-3 min-w-0">
          <button
            @click="isResultsSectionCollapsed = !isResultsSectionCollapsed"
            class="p-1 hover:bg-gray-100 dark:hover:bg-gray-700 rounded transition-colors bg-transparent flex-shrink-0"
            :title="isResultsSectionCollapsed ? 'Show results and question details' : 'Hide results and question details'"
          >
            <svg class="w-5 h-5 text-gray-500 dark:text-gray-400 transition-transform" :class="{ 'rotate-180': isResultsSectionCollapsed }" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
            </svg>
          </button>
          <svg 
            class="w-5 h-5 flex-shrink-0" 
            :class="question.variableType === 1 ? 'text-red-400' : question.variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
            fill="currentColor" 
            viewBox="0 0 20 20"
          >
            <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
          </svg>
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white truncate" style="max-width: 400px;">
            {{ question.label }}
          </h2>
          <span class="text-sm text-gray-400 dark:text-gray-500 flex-shrink-0">
            {{ question.qstNumber }}
          </span>
        </div>
        <!-- Fixed position Analyze in Crosstab button -->
        <button
          v-if="activeSection === 'results'"
          @click="$emit('analyze-crosstab')"
          class="absolute left-[25%] px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors flex items-center gap-2 flex-shrink-0"
          title="Analyze this question in crosstab with another variable"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
          </svg>
          Analyze in Crosstab
        </button>
        <!-- Fixed position RESULTS title -->
        <div class="absolute left-[45%]">
          <h1 class="text-xl font-bold text-blue-600 dark:text-blue-400 text-left">
            RESULTS
          </h1>
        </div>
        <div class="flex items-center space-x-3 flex-shrink-0">
          <div class="flex items-center space-x-4 text-sm text-gray-600 dark:text-gray-400">
            <span><span class="font-medium">Index:</span> {{ question.index || '100' }}</span>
            <span>•</span>
            <span><span class="font-medium">CI:</span> +/- {{ question.samplingError?.toFixed(1) || '0.0' }}</span>
            <span>•</span>
            <span><span class="font-medium">Total N:</span> {{ question.totalCases || 0 }}</span>
          </div>
          <button
            @click="showMetricDefinitions = true"
            class="p-1 bg-transparent text-gray-500 hover:text-blue-600 hover:bg-blue-50 dark:text-gray-400 dark:hover:text-blue-400 dark:hover:bg-gray-800 transition-colors border-0"
            title="View metric definitions"
          >
            <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" fill="currentColor" />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Results Table and Question Info side by side -->
    <div v-if="!isResultsSectionCollapsed" class="flex gap-4 mt-4 px-4">
      <!-- Left: Results Table (45% width) -->
      <div ref="tableContainerRef" class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col" :style="{ width: '45%', flexShrink: 0, height: contentHeight + 'px' }">
        <div class="px-4 py-2 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 flex items-center justify-between flex-shrink-0">
          <div class="flex items-center gap-2">
            <h3 class="text-sm font-medium text-gray-900 dark:text-white">
              Response Results
            </h3>
            <span class="text-xs text-gray-400 dark:text-gray-500">
              {{ question.responses?.length || 0 }} responses
            </span>
          </div>
          <div class="flex items-center gap-2">
            <button
              @click="toggleExpand"
              :disabled="!needsExpand"
              class="text-xs px-2 py-1 rounded border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 transition-colors flex items-center gap-1"
              :class="{
                'hover:bg-gray-50 dark:hover:bg-gray-600 cursor-pointer': needsExpand,
                'opacity-50 cursor-not-allowed': !needsExpand
              }"
              :title="!needsExpand ? 'All content visible' : (isExpanded ? 'Collapse to standard size' : 'Expand to show all content')"
            >
              <svg v-if="!isExpanded" class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
              <svg v-else class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
              </svg>
              {{ isExpanded ? 'Show Less' : 'Show More' }}
            </button>
            <select
              v-model="localColumnMode"
              @change="$emit('column-mode-changed', localColumnMode)"
              class="text-xs border border-gray-300 dark:border-gray-600 rounded px-2 py-1 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 cursor-pointer"
            >
              <option value="totalN">Total N</option>
              <option value="cumulative">Show Cumulative</option>
              <option value="inverseCumulative">Show Inverse Cumulative</option>
              <option value="samplingError">Sampling Error</option>
              <option value="blank">Leave Blank</option>
            </select>
          </div>
        </div>
        <div class="overflow-auto flex-1">
          <table class="min-w-full text-xs">
            <thead class="bg-gray-50 dark:bg-gray-700 sticky top-0">
              <tr>
                <th class="px-3 py-1.5 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  #
                </th>
                <th class="px-3 py-1.5 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Response
                </th>
                <th class="px-3 py-1.5 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  %
                </th>
                <th class="px-3 py-1.5 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  Index
                </th>
                <th v-if="columnModeConfig.showColumn" class="px-3 py-1.5 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                  {{ columnModeConfig.header }}
                </th>
              </tr>
            </thead>
            <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
              <tr v-for="(response, index) in computedResponses" :key="index" class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                <td class="px-3 py-1 whitespace-nowrap text-xs text-center text-gray-500 dark:text-gray-400">
                  {{ index + 1 }}
                </td>
                <td class="px-3 py-1 text-xs text-gray-900 dark:text-white">
                  {{ response.label }}
                </td>
                <td class="px-3 py-1 whitespace-nowrap text-xs text-right text-gray-900 dark:text-white font-medium">
                  {{ response.percentage.toFixed(1) }}
                </td>
                <td class="px-3 py-1 whitespace-nowrap text-xs text-center">
                  <span v-if="response.index" :class="{
                    'text-green-500': response.index > 100,
                    'text-red-500': response.index < 100,
                    'text-gray-500 dark:text-gray-400': response.index === 100
                  }">
                    {{ response.index > 100 ? '▲' : response.index < 100 ? '▼' : '—' }}
                  </span>
                  <span v-else-if="response.indexSymbol" class="text-gray-500 dark:text-gray-400">{{ response.indexSymbol }}</span>
                  <span v-else class="text-gray-500 dark:text-gray-400">—</span>
                </td>
                <td v-if="localColumnMode === 'totalN'" class="px-3 py-1 whitespace-nowrap text-xs text-right text-gray-500 dark:text-gray-400">
                  {{ response.count }}
                </td>
                <td v-else-if="localColumnMode === 'cumulative'" class="px-3 py-1 whitespace-nowrap text-xs text-right text-gray-500 dark:text-gray-400">
                  {{ response.cumulative.toFixed(1) }}
                </td>
                <td v-else-if="localColumnMode === 'inverseCumulative'" class="px-3 py-1 whitespace-nowrap text-xs text-right text-gray-500 dark:text-gray-400">
                  {{ response.inverseCumulative.toFixed(1) }}
                </td>
                <td v-else-if="localColumnMode === 'samplingError'" class="px-3 py-1 whitespace-nowrap text-xs text-right text-gray-500 dark:text-gray-400">
                  {{ response.samplingError.toFixed(1) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Right: Question Information (Always Visible) -->
      <div class="flex-1 bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col" :style="{ height: contentHeight + 'px' }">
        <div class="p-4 flex gap-4 flex-1 min-h-0 overflow-hidden">
          <!-- Left: Buttons -->
          <div class="flex flex-col gap-2 flex-shrink-0">
            <button
              @click="selectTab('question')"
              :class="[
                'px-4 py-2 rounded-lg text-sm font-medium transition-colors text-left flex items-center gap-2 whitespace-nowrap',
                activeStemTab === 'question' 
                  ? 'bg-blue-600 text-white' 
                  : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
              ]"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              Question
            </button>
            <button
              @click="selectTab('block')"
              :class="[
                'px-4 py-2 rounded-lg text-sm font-medium transition-colors text-left flex items-center gap-2 whitespace-nowrap',
                activeStemTab === 'block' 
                  ? 'bg-blue-600 text-white' 
                  : 'bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 hover:bg-gray-200 dark:hover:bg-gray-600'
              ]"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h8m-8 4h5" />
              </svg>
              Block
            </button>
          </div>
          
          <!-- Right: Text Content (Scrollable) -->
          <div ref="textContentRef" class="flex-1 bg-gray-50 dark:bg-gray-900 rounded-lg p-4 border border-gray-200 dark:border-gray-700 overflow-y-auto min-h-0">
            <p v-if="currentStemText" class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">
              {{ currentStemText }}
            </p>
            <p v-else class="text-sm text-gray-400 dark:text-gray-600 italic">
              No {{ activeStemTab }} stem available
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- Separator line -->
    <div class="border-t border-gray-300 dark:border-gray-600 mt-4"></div>

    <!-- Metric Definitions Modal -->
    <MetricDefinitionsModal :show="showMetricDefinitions" @close="showMetricDefinitions = false" />
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted } from 'vue'
import MetricDefinitionsModal from '../MetricDefinitionsModal.vue'

const props = defineProps({
  question: {
    type: Object,
    required: true
  },
  columnMode: {
    type: String,
    default: 'totalN'
  },
  surveyId: {
    type: String,
    required: true
  },
  activeSection: {
    type: String,
    default: 'results'
  }
})

const emit = defineEmits(['column-mode-changed', 'analyze-crosstab'])

// Calculate height dynamically based on preference
const DEFAULT_HEIGHT_KEY = 'porpoise_table_rows_to_show'
// Load preference immediately (not in onMounted)
const savedRows = parseInt(localStorage.getItem(DEFAULT_HEIGHT_KEY) || '3')
const defaultRowsToShow = ref(Math.max(2, Math.min(6, savedRows)))
const localColumnMode = ref(props.columnMode)
const contentHeight = ref(200)
const tableContainerRef = ref(null)
const isExpanded = ref(false) // Track expand/collapse state

// Track collapse of entire results section with localStorage persistence
const COLLAPSE_STATE_KEY = computed(() => `porpoise_survey_${props.surveyId}_results_collapsed`)
const isResultsSectionCollapsed = ref(localStorage.getItem(COLLAPSE_STATE_KEY.value) === 'true')

// Load saved tab preference from localStorage
const STORAGE_KEY = computed(() => `porpoise_survey_${props.surveyId}_stem_tab`)
const activeStemTab = ref(localStorage.getItem(STORAGE_KEY.value) || 'question')

// Check if expand is needed (content is scrollable)
const textContentRef = ref(null)
const showMetricDefinitions = ref(false)
const needsExpand = computed(() => {
  // Force re-check when refs are available
  if (!tableContainerRef.value || !textContentRef.value) return false
  
  // Check if EITHER the table OR the question text has scrollable content
  let hasScrollableTable = false
  let hasScrollableText = false
  
  // Check table
  const scrollDiv = tableContainerRef.value.querySelector('.overflow-auto')
  if (scrollDiv) {
    hasScrollableTable = scrollDiv.scrollHeight > scrollDiv.clientHeight
  }
  
  // Check question text
  hasScrollableText = textContentRef.value.scrollHeight > textContentRef.value.clientHeight
  
  return hasScrollableTable || hasScrollableText
})

// Function to toggle between fixed and expanded height
function toggleExpand() {
  // Calculate default height based on preference
  const defaultHeight = 45 + 36 + (defaultRowsToShow.value * 32) - 25
  
  if (isExpanded.value) {
    // Collapse back to default height
    contentHeight.value = defaultHeight
    isExpanded.value = false
  } else {
    // Expand to show full content for BOTH table and question text
    let maxHeight = defaultHeight
    
    // Check table content - use exact scrollHeight
    if (tableContainerRef.value) {
      const scrollDiv = tableContainerRef.value.querySelector('.overflow-auto')
      if (scrollDiv) {
        const actualScrollHeight = scrollDiv.scrollHeight
        const headerHeight = 45 // Table header height
        maxHeight = Math.max(maxHeight, actualScrollHeight + headerHeight)
      }
    }
    
    // Check question text content - use exact scrollHeight
    if (textContentRef.value) {
      const textScrollHeight = textContentRef.value.scrollHeight
      const buttonsAndPadding = 80 // Buttons + container padding
      maxHeight = Math.max(maxHeight, textScrollHeight + buttonsAndPadding)
    }
    
    contentHeight.value = maxHeight
    isExpanded.value = true
  }
}

// Save collapse state to localStorage whenever it changes
watch(isResultsSectionCollapsed, (newValue) => {
  localStorage.setItem(COLLAPSE_STATE_KEY.value, newValue.toString())
})

// Auto-expand when returning to results tab
watch(() => props.activeSection, (newSection) => {
  if (newSection === 'results') {
    isResultsSectionCollapsed.value = false
  }
})

// Reset to collapsed state when question changes
watch(() => props.question?.id, async () => {
  // Always show fixed height for preference number of rows
  // Header (45px) + table header row (36px) + (rows * 32px each) - 25px adjustment
  const calculatedHeight = 45 + 36 + (defaultRowsToShow.value * 32) - 25
  
  contentHeight.value = calculatedHeight
  isExpanded.value = false
  
  // Wait for DOM to update then trigger needsExpand recomputation
  await nextTick()
  // Force recomputation by accessing the computed property
  needsExpand.value
}, { immediate: true })

// Watch for prop changes
watch(() => props.columnMode, (newVal) => {
  localColumnMode.value = newVal
})

const blockStem = computed(() => {
  // Try to get block stem from various possible locations in the question object
  // API returns blockStem (camelCase), but also check deprecated blkStem and block.stem
  return props.question?.blockStem || 
         props.question?.blkStem ||
         props.question?.block?.stem || 
         props.question?.block?.text ||
         null
})

const currentStemText = computed(() => {
  if (activeStemTab.value === 'question') {
    return props.question?.text || null
  } else {
    return blockStem.value
  }
})

function selectTab(tab) {
  activeStemTab.value = tab
  localStorage.setItem(STORAGE_KEY.value, tab)
}

const columnModeConfig = computed(() => {
  const configs = {
    totalN: { showColumn: true, header: 'N' },
    cumulative: { showColumn: true, header: 'Cumulative %' },
    inverseCumulative: { showColumn: true, header: 'Inverse Cum %' },
    samplingError: { showColumn: true, header: 'SE' },
    blank: { showColumn: false, header: '' }
  }
  return configs[localColumnMode.value] || configs.totalN
})

const computedResponses = computed(() => {
  if (!props.question?.responses) return []
  
  let cumSum = 0
  let invCumSum = 100
  
  return props.question.responses.map((r, idx) => {
    cumSum += r.percentage
    const current = {
      ...r,
      cumulative: cumSum,
      inverseCumulative: invCumSum,
      samplingError: r.samplingError || 0,
      indexSymbol: r.index ? (r.index > 100 ? '▲' : r.index < 100 ? '▼' : '─') : '─'
    }
    invCumSum -= r.percentage
    return current
  })
})
</script>
