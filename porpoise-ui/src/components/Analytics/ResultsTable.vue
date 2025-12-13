<template>
  <div>
    <!-- Question Header with Analyze in Crosstab button - STICKY -->
    <div class="sticky top-0 z-30 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 py-1">
      <div class="relative flex items-center justify-between px-4">
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
          <h2 class="text-base font-semibold text-gray-900 dark:text-white truncate" style="max-width: 400px;">
            {{ question.label }}
          </h2>
          <span class="text-sm text-gray-400 dark:text-gray-500 flex-shrink-0">
            {{ question.qstNumber }}
          </span>
        </div>
        <!-- Fixed position RESULTS title -->
        <div class="absolute left-[45%]">
          <span class="text-base font-semibold text-blue-600 dark:text-blue-400 text-left uppercase tracking-wider">
            RESULTS
          </span>
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
    <div v-if="!isResultsSectionCollapsed" :style="{ height: containerHeight + 'px' }">
      <Splitter layout="horizontal" class="h-full">
        <!-- Left: Results Table with Header -->
        <SplitterPanel :size="45" :minSize="30" :maxSize="60">
          <div ref="tableContainerRef" class="h-full bg-white dark:bg-gray-800 overflow-y-auto">
            <!-- Responses Header -->
            <div class="sticky top-0 z-10 px-4 py-1 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 flex items-center justify-between">
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
          <table class="min-w-full text-xs">
            <thead class="bg-gray-50 dark:bg-gray-700">
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
        </SplitterPanel>

        <!-- Right: Question/Block with Tabs -->
        <SplitterPanel :size="55" :minSize="40" :maxSize="70">
          <div class="h-full bg-white dark:bg-gray-800 overflow-y-auto">
            <!-- Sticky Tabs Header -->
            <div class="sticky top-0 z-10 primevue-tabs-wrapper border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
              <Tabs :value="activeStemTab" @update:value="selectTab">
                <TabList>
                  <Tab value="question">
                    <div class="flex items-center gap-1.5">
                      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                      <span>Question</span>
                    </div>
                  </Tab>
                  <Tab value="block">
                    <div class="flex items-center gap-1.5">
                      <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
                      </svg>
                      <span>Block</span>
                    </div>
                  </Tab>
                </TabList>
              </Tabs>
            </div>
            <!-- Text Content -->
            <div class="p-4 bg-gray-50 dark:bg-gray-900">
              <div ref="textContentRef">
                <p v-if="currentStemText" class="text-xs text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">
                  {{ currentStemText }}
                </p>
                <p v-else class="text-xs text-gray-400 dark:text-gray-600 italic">
                  No {{ activeStemTab }} stem available
                </p>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </div>

    <!-- Separator line -->
    <div class="border-t border-gray-300 dark:border-gray-600 mt-4"></div>

    <!-- Metric Definitions Modal -->
    <MetricDefinitionsModal :show="showMetricDefinitions" @close="showMetricDefinitions = false" />
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted } from 'vue'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import Splitter from 'primevue/splitter'
import SplitterPanel from 'primevue/splitterpanel'
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

const localColumnMode = ref(props.columnMode)
const tableContainerRef = ref(null)

// Track collapse of entire results section with localStorage persistence
const COLLAPSE_STATE_KEY = computed(() => `porpoise_survey_${props.surveyId}_results_collapsed`)
const isResultsSectionCollapsed = ref(localStorage.getItem(COLLAPSE_STATE_KEY.value) === 'true')

// Load saved tab preference from localStorage
const STORAGE_KEY = computed(() => `porpoise_survey_${props.surveyId}_stem_tab`)
const activeStemTab = ref(localStorage.getItem(STORAGE_KEY.value) || 'question')

const textContentRef = ref(null)
const showMetricDefinitions = ref(false)

// Calculate height based on content - considers both table rows and text length
const containerHeight = computed(() => {
  const numRows = props.question?.responses?.length || 0
  // Header: 33px, Each row: ~28px, Cap at 8 rows visible
  const maxRows = Math.min(numRows, 8)
  const tableHeight = 33 + (maxRows * 28)
  
  // Estimate text height: ~16px line-height for text-xs, ~80 chars per line at typical width
  const stemText = currentStemText.value || ''
  const estimatedLines = Math.ceil(stemText.length / 80)
  const textHeight = 33 + (estimatedLines * 16) + 32 // tabs (33px) + text lines + padding (32px)
  
  // Use the larger of the two, ensuring minimum height
  const contentHeight = Math.max(tableHeight, textHeight)
  return Math.max(contentHeight + 50, 200)
})

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

<style scoped>
/* Match the styling of the bottom analysis tabs */
.primevue-tabs-wrapper :deep(button[role="tab"]) {
  font-weight: 300 !important;
  font-size: 0.75rem !important;
  padding-top: 0.375rem !important;
  padding-bottom: 0.25rem !important;
}

/* Light mode: dark text for active tab */
:root .primevue-tabs-wrapper :deep(button[role="tab"][data-p-active="true"]) {
  font-weight: 600 !important;
  font-size: 0.875rem !important;
  color: rgb(24 24 27) !important;
}

/* Dark mode: white text for active tab */
.dark .primevue-tabs-wrapper :deep(button[role="tab"][data-p-active="true"]) {
  font-weight: 600 !important;
  font-size: 0.875rem !important;
  color: rgb(255 255 255) !important;
}

/* Splitter gutter hover effect */
:deep(.p-splitter-gutter) {
  transition: background-color 0.2s ease;
}

:deep(.p-splitter-gutter:hover) {
  background-color: rgb(59 130 246) !important;
}

:deep(.dark .p-splitter-gutter:hover) {
  background-color: rgb(96 165 250) !important;
}
</style>
