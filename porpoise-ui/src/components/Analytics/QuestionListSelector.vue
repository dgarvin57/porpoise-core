<template>
  <div class="h-full flex flex-col">
    <!-- Crosstab Selection Panel (only show in non-radio mode) -->
    <div 
      v-if="selectionMode === 'crosstab' && !USE_RADIO_BUTTON_MODE" 
      class="px-3 pt-3 pb-2 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
    >
      <!-- DV Selection -->
      <div class="flex items-center space-x-2 mb-1.5">
        <div class="flex items-center space-x-1.5 flex-1 min-w-0">
          <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-blue-600 text-white text-xs font-semibold flex-shrink-0">1</span>
          <span class="text-xs font-medium text-gray-700 dark:text-gray-300 flex-shrink-0">DV:</span>
          <span 
            v-if="firstSelection" 
            class="text-xs text-gray-900 dark:text-gray-100 truncate"
            :title="firstSelection.label"
          >
            {{ firstSelection.label }}
          </span>
          <span v-else class="text-xs text-gray-400 dark:text-gray-500 italic">None selected</span>
        </div>
        <button
          v-if="firstSelection"
          @click="enterReplaceMode(1)"
          :class="replaceMode === 1 ? 'bg-blue-600 text-white' : 'bg-white dark:bg-gray-700 text-blue-600 dark:text-blue-400 border border-blue-300 dark:border-blue-600'"
          class="px-2 py-0.5 rounded text-xs font-medium hover:opacity-80 transition-all flex-shrink-0"
        >
          {{ replaceMode === 1 ? 'Select to Replace' : 'Change' }}
        </button>
      </div>
      
      <!-- IV Selection -->
      <div class="flex items-center space-x-2">
        <div class="flex items-center space-x-1.5 flex-1 min-w-0">
          <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-green-600 text-white text-xs font-semibold flex-shrink-0">2</span>
          <span class="text-xs font-medium text-gray-700 dark:text-gray-300 flex-shrink-0">IV:</span>
          <span 
            v-if="secondSelection" 
            class="text-xs text-gray-900 dark:text-gray-100 truncate"
            :title="secondSelection.label"
          >
            {{ secondSelection.label }}
          </span>
          <span v-else class="text-xs text-gray-400 dark:text-gray-500 italic">None selected</span>
        </div>
        <button
          v-if="secondSelection"
          @click="enterReplaceMode(2)"
          :class="replaceMode === 2 ? 'bg-green-600 text-white' : 'bg-white dark:bg-gray-700 text-green-600 dark:text-green-400 border border-green-300 dark:border-green-600'"
          class="px-2 py-0.5 rounded text-xs font-medium hover:opacity-80 transition-all flex-shrink-0"
        >
          {{ replaceMode === 2 ? 'Select to Replace' : 'Change' }}
        </button>
      </div>
    </div>
    
    <!-- Search Bar -->
    <div class="p-2 border-b border-gray-200 dark:border-gray-700">
      <div class="relative">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Find question"
          autocomplete="off"
          data-1p-ignore
          data-lpignore="true"
          class="w-full px-3 py-1 pl-9 pr-9 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <svg class="absolute left-3 top-2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
        <button
          v-if="searchQuery"
          @click="clearSearch()"
          class="absolute right-2 top-2 p-0.5 rounded bg-transparent text-gray-500 hover:text-gray-900 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-white dark:hover:bg-gray-700 transition-colors"
          title="Clear search"
        >
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>
      <div class="flex items-center justify-between text-xs mt-0.5">
        <div class="flex items-center space-x-1.5">
          <a
            @click="expandAll"
            class="text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 cursor-pointer hover:underline"
          >
            Expand All
          </a>
          <span class="text-gray-400">|</span>
          <a
            @click="collapseAll"
            class="text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 cursor-pointer hover:underline"
          >
            Collapse All
          </a>
        </div>
        <a
          v-if="selectionMode === 'crosstab'"
          @click="clearSelections"
          :class="[
            'cursor-pointer hover:underline',
            (firstSelection || secondSelection) 
              ? 'text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300'
              : 'text-gray-400 dark:text-gray-600 cursor-not-allowed opacity-50'
          ]"
        >
          Clear Selection
        </a>
      </div>
    </div>

    <!-- Question List -->
    <div class="flex-1 overflow-y-auto [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-gray-100 dark:[&::-webkit-scrollbar-track]:bg-gray-800 [&::-webkit-scrollbar-thumb]:bg-gray-300 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb:hover]:bg-gray-400 dark:[&::-webkit-scrollbar-thumb:hover]:bg-gray-500">
      <div v-if="loading" class="flex items-center justify-center py-8">
        <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
      </div>
      
      <div v-else-if="error" class="p-4 text-sm text-red-600 dark:text-red-400">
        {{ error }}
      </div>

      <div v-else class="p-2">
        <div v-for="(item, index) in filteredQuestions" :key="index" class="mb-1">
          <!-- Block -->
          <div v-if="item.type === 'block'">
            <!-- Block Header -->
            <button
              @click="toggleBlock(item)"
              class="w-full flex items-center space-x-2 px-2 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 rounded transition-colors text-left"
            >
              <svg 
                class="w-3 h-3 text-gray-500 dark:text-gray-400 transition-transform flex-shrink-0"
                :class="{ 'rotate-90': item.isExpanded }"
                fill="currentColor" 
                viewBox="0 0 20 20"
              >
                <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
              </svg>
              <svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                <path d="M2 6a2 2 0 012-2h5l2 2h5a2 2 0 012 2v6a2 2 0 01-2 2H4a2 2 0 01-2-2V6z" />
              </svg>
              <div class="flex-1 text-left min-w-0">
                <span class="text-xs font-medium text-gray-800 dark:text-white truncate">
                  {{ item.label }}
                </span>
              </div>
              <!-- Selected indicator for single selection mode -->
              <svg
                v-if="selectionMode === 'single' && item.questions.some(q => (selectedQuestionId && selectedQuestionId === q.id) || (singleSelection?.id === q.id))"
                class="w-4 h-4 text-blue-500 dark:text-blue-400 flex-shrink-0"
                fill="currentColor"
                viewBox="0 0 20 20"
                title="Contains selected question"
              >
                <circle cx="10" cy="10" r="4" />
              </svg>
              <!-- Selected indicator for crosstab mode -->
              <svg
                v-if="selectionMode === 'crosstab' && item.questions.some(q => (firstSelection?.id === q.id) || (secondSelection?.id === q.id))"
                class="w-4 h-4 text-blue-500 dark:text-blue-400 flex-shrink-0"
                fill="currentColor"
                viewBox="0 0 20 20"
                title="Contains selected variable"
              >
                <circle cx="10" cy="10" r="4" />
              </svg>
              <span class="text-xs text-gray-400 dark:text-gray-500 flex-shrink-0">
                {{ item.questions.length }}
              </span>
            </button>
            
            <!-- Block Questions -->
            <div v-show="item.isExpanded" class="mt-0.5 space-y-0">
              <div
                v-for="question in item.questions"
                :key="question.id"
                :class="getQuestionClasses(question)"
                class="w-full flex items-center px-2 py-0.5 rounded cursor-pointer transition-all border-l"
                :data-question-id="question.id"
              >
                <!-- Left hover zone for radio button -->
                <div class="group/radio flex items-center flex-shrink-0">
                  <!-- Radio button for single selection mode (Results/StatSig) -->
                  <input
                    v-if="selectionMode === 'single'"
                    type="radio"
                    name="single-selection"
                    :checked="singleSelection?.id === question.id || selectedQuestionId === question.id"
                    @click.stop="handleRadioSingleClick(question)"
                    :disabled="props.activeTab === 'statsig' && question.variableType === 1"
                    :class="[
                      'w-5 h-5 transition-all duration-200 ease-in-out flex-shrink-0 mr-2 cursor-pointer',
                      'border-2 rounded-full',
                      'accent-blue-600 focus:ring-2 focus:ring-blue-500 focus:ring-offset-0',
                      (singleSelection?.id === question.id || selectedQuestionId === question.id) ? 'opacity-100 scale-110' : 'opacity-40 group-hover/radio:opacity-100 group-hover/radio:scale-105',
                      props.activeTab === 'statsig' && question.variableType === 1 ? 'opacity-20 cursor-not-allowed' : ''
                    ]"
                  />
                  <!-- Radio button for DV selection (crosstab radio mode only) -->
                  <input
                    v-if="selectionMode === 'crosstab' && USE_RADIO_BUTTON_MODE"
                    type="radio"
                    name="dv-selection"
                    :checked="firstSelection?.id === question.id"
                    @click.stop="handleRadioDVClick(question)"
                    :disabled="props.activeTab === 'statsig' && question.variableType === 1"
                    :class="[
                      'w-5 h-5 transition-all duration-200 ease-in-out flex-shrink-0 mr-2 cursor-pointer',
                      'border-2 rounded-full',
                      'accent-blue-600 focus:ring-2 focus:ring-blue-500 focus:ring-offset-0',
                      firstSelection?.id === question.id ? 'opacity-100 scale-110' : 'opacity-40 group-hover/radio:opacity-100 group-hover/radio:scale-105',
                      props.activeTab === 'statsig' && question.variableType === 1 ? 'opacity-20 cursor-not-allowed' : ''
                    ]"
                  />
                </div>
                
                <!-- Wrapper for indentation (always indent block questions) -->
                <div class="flex items-center space-x-2 flex-1 min-w-0 ml-6">
                  <!-- Variable Type Icon (always show) -->
                  <svg 
                    class="w-3.5 h-3.5 flex-shrink-0" 
                    :class="question.variableType === 1 ? 'text-red-400' : question.variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
                    fill="currentColor" 
                    viewBox="0 0 20 20"
                    @click="handleQuestionClick(question)"
                  >
                    <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                  </svg>
                  
                  <div class="flex-1 min-w-0 leading-none" @click="handleQuestionClick(question)">
                    <span class="text-xs text-gray-700 dark:text-gray-300">{{ question.label }}</span>
                    <span v-if="showQuestionNumbers" class="text-xs text-gray-400 dark:text-gray-500 ml-2">{{ question.qstNumber }}</span>
                  </div>
                </div>
                
                <!-- Selection Badge (right side) -->
                <span 
                  v-if="getSelectionNumber(question)"
                  :class="getSelectionBadgeClasses(question)"
                  class="inline-flex items-center justify-center w-4 h-4 rounded font-semibold text-white text-xs flex-shrink-0"
                >
                  {{ getSelectionNumber(question) }}
                </span>
              </div>
            </div>
          </div>

          <!-- Standalone Question -->
          <div
            v-else
            :class="getQuestionClasses(item.question || item)"
            class="w-full flex items-center space-x-2 px-2 py-0.5 rounded cursor-pointer transition-all border-l-2"
            :data-question-id="(item.question || item).id"
          >
            <!-- Left hover zone for radio button -->
            <div class="group/radio flex items-center flex-shrink-0">
              <!-- Radio button for single selection mode (Results/StatSig) -->
              <input
                v-if="selectionMode === 'single'"
                type="radio"
                name="single-selection"
                :checked="singleSelection?.id === (item.question || item).id || selectedQuestionId === (item.question || item).id"
                @click.stop="handleRadioSingleClick(item.question || item)"
                :disabled="props.activeTab === 'statsig' && (item.question || item).variableType === 1"
                :class="[
                  'w-5 h-5 transition-all duration-200 ease-in-out flex-shrink-0 cursor-pointer',
                  'border-2 rounded-full',
                  'accent-blue-600 focus:ring-2 focus:ring-blue-500 focus:ring-offset-0',
                  (singleSelection?.id === (item.question || item).id || selectedQuestionId === (item.question || item).id) ? 'opacity-100 scale-110' : 'opacity-40 group-hover/radio:opacity-100 group-hover/radio:scale-105',
                  props.activeTab === 'statsig' && (item.question || item).variableType === 1 ? 'opacity-20 cursor-not-allowed' : ''
                ]"
              />
              <!-- Radio button for DV selection (crosstab radio mode only) -->
              <input
                v-if="selectionMode === 'crosstab' && USE_RADIO_BUTTON_MODE"
                type="radio"
                name="dv-selection"
                :checked="firstSelection?.id === (item.question || item).id"
                @click.stop="handleRadioDVClick(item.question || item)"
                :disabled="props.activeTab === 'statsig' && (item.question || item).variableType === 1"
                :class="[
                  'w-5 h-5 transition-all duration-200 ease-in-out flex-shrink-0 cursor-pointer',
                  'border-2 rounded-full',
                  'accent-blue-600 focus:ring-2 focus:ring-blue-500 focus:ring-offset-0',
                  firstSelection?.id === (item.question || item).id ? 'opacity-100 scale-110' : 'opacity-40 group-hover/radio:opacity-100 group-hover/radio:scale-105',
                  props.activeTab === 'statsig' && (item.question || item).variableType === 1 ? 'opacity-20 cursor-not-allowed' : ''
                ]"
              />
            </div>
            
            <!-- Icon + Label Container (for tour spotlight targeting) -->
            <div class="flex items-center space-x-2 flex-1 min-w-0" @click="handleQuestionClick(item.question || item)">
              <!-- Variable Type Icon (always show) -->
              <svg 
                class="w-3.5 h-3.5 flex-shrink-0" 
                :class="(item.question || item).variableType === 1 ? 'text-red-400' : (item.question || item).variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
                fill="currentColor" 
                viewBox="0 0 20 20"
              >
                <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
              </svg>
              
              <div class="flex-1 min-w-0 leading-none">
                <span class="text-xs text-gray-700 dark:text-gray-300">{{ (item.question || item).label }}</span>
                <span v-if="showQuestionNumbers" class="text-xs text-gray-400 dark:text-gray-500 ml-2">{{ (item.question || item).qstNumber }}</span>
              </div>
            </div>
            
            <!-- Selection Badge (right side) -->
            <span 
              v-if="getSelectionNumber(item.question || item)"
              :class="getSelectionBadgeClasses(item.question || item)"
              class="inline-flex items-center justify-center w-4 h-4 rounded font-semibold text-white text-xs flex-shrink-0"
            >
              {{ getSelectionNumber(item.question || item) }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { API_BASE_URL } from '@/config/api'
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import axios from 'axios'
import Button from '../common/Button.vue'

// Feature flag: Set to true to use radio button DV selection mode
const USE_RADIO_BUTTON_MODE = true

// LocalStorage key for question numbers preference
const SHOW_QUESTION_NUMBERS_KEY = 'porpoise_show_question_numbers'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  selectionMode: {
    type: String,
    default: 'single', // 'single', 'crosstab', 'multiple'
    validator: (value) => ['single', 'crosstab', 'multiple'].includes(value)
  },
  showNotesIcons: {
    type: Boolean,
    default: false // Show pencil icon for questions with notes
  },
  selectedQuestionId: {
    type: String,
    default: null // For single selection mode highlighting
  },
  initialExpandedBlocks: {
    type: Array,
    default: () => []
  },
  wrapStandaloneQuestions: {
    type: Boolean,
    default: false // If true, wrap standalone questions in {question: {...}} structure for ResultsView
  },
  initialFirstSelection: {
    type: Object,
    default: null // For crosstab mode - initial first question
  },
  initialSecondSelection: {
    type: Object,
    default: null // For crosstab mode - initial second question
  },
  activeTab: {
    type: String,
    default: 'results' // Can be 'results', 'crosstab', 'statsig', etc.
  }
})

const emit = defineEmits(['question-selected', 'crosstab-selection', 'selection-cleared', 'expanded-blocks-changed', 'questions-loaded'])

// State
const loading = ref(false)
const error = ref(null)
const questions = ref([])
const searchQuery = ref('')
const showVariableHelp = ref(false)

// Crosstab selections
const firstSelection = ref(props.initialFirstSelection)
const secondSelection = ref(props.initialSecondSelection)
const replaceMode = ref(null) // null, 1 (replacing DV), or 2 (replacing IV)

// Watch for prop changes to update selections
watch(() => props.initialFirstSelection, (newVal) => {
  firstSelection.value = newVal
  
  // Expand the block containing this question
  if (newVal && questions.value.length > 0) {
    const block = questions.value.find(q => 
      q.type === 'block' && 
      q.questions && 
      q.questions.some(bq => bq.id === newVal.id)
    )
    if (block && !block.isExpanded) {
      block.isExpanded = true
    }
  }
}, { deep: true, immediate: true })

watch(() => props.initialSecondSelection, (newVal) => {
  secondSelection.value = newVal
  
  // Expand the block containing this question
  if (newVal && questions.value.length > 0) {
    const block = questions.value.find(q => 
      q.type === 'block' && 
      q.questions && 
      q.questions.some(bq => bq.id === newVal.id)
    )
    if (block && !block.isExpanded) {
      block.isExpanded = true
    }
  }
}, { deep: true, immediate: true })

// Clear crosstab selections when switching to single selection mode
watch(() => props.selectionMode, (newMode) => {
  if (newMode === 'single') {
    firstSelection.value = null
    secondSelection.value = null
  }
})

// Watch for questions being loaded to expand blocks for existing selections
watch(() => props.questions, (newQuestions) => {
  if (newQuestions && newQuestions.length > 0) {
    // Re-check first selection
    if (firstSelection.value) {
      const block = newQuestions.find(q => 
        q.type === 'block' && 
        q.questions && 
        q.questions.some(bq => bq.id === firstSelection.value.id)
      )
      if (block && !block.isExpanded) {
        block.isExpanded = true
      }
    }
    // Re-check second selection
    if (secondSelection.value) {
      const block = newQuestions.find(q => 
        q.type === 'block' && 
        q.questions && 
        q.questions.some(bq => bq.id === secondSelection.value.id)
      )
      if (block && !block.isExpanded) {
        block.isExpanded = true
      }
    }
  }
}, { deep: true, immediate: true })

// Single selection state
const singleSelection = ref(null)

// Preference for showing question numbers
const showQuestionNumbers = ref(true)

// Check localStorage preference on mount and when storage changes
onMounted(() => {
  const savedPref = localStorage.getItem(SHOW_QUESTION_NUMBERS_KEY)
  showQuestionNumbers.value = savedPref === null ? true : savedPref === 'true'
  
  // Listen for storage changes (e.g., from preferences page)
  const handleStorageChange = () => {
    const savedPref = localStorage.getItem(SHOW_QUESTION_NUMBERS_KEY)
    showQuestionNumbers.value = savedPref === null ? true : savedPref === 'true'
  }
  window.addEventListener('storage', handleStorageChange)
  
  // Also poll for changes in the same tab
  const pollInterval = setInterval(handleStorageChange, 500)
  
  // Cleanup
  onUnmounted(() => {
    window.removeEventListener('storage', handleStorageChange)
    clearInterval(pollInterval)
  })
})

// Computed
const filteredQuestions = computed(() => {
  if (!searchQuery.value.trim()) {
    return questions.value
  }

  const query = searchQuery.value.toLowerCase()
  const filtered = []

  for (const item of questions.value) {
    if (item.type === 'block') {
      // Check if block label matches
      const blockLabelMatches = item.label?.toLowerCase().includes(query)
      
      // Check which questions match
      const matchingQuestions = item.questions.filter(q =>
        q.label?.toLowerCase().includes(query)
      )
      
      // Show block if either block label matches OR any questions match
      if (blockLabelMatches || matchingQuestions.length > 0) {
        filtered.push({
          ...item,
          // If block label matches, show all questions; otherwise show only matching questions
          questions: blockLabelMatches ? item.questions : matchingQuestions,
          isExpanded: true
        })
      }
    } else {
      // Handle both wrapped (item.question.label) and unwrapped (item.label) structures
      const label = (item.question || item).label
      if (label?.toLowerCase().includes(query)) {
        filtered.push(item)
      }
    }
  }

  return filtered
})

// Methods
async function loadQuestions() {
  loading.value = true
  error.value = null

  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/${props.surveyId}/questions-list`)
    
    // Process flat questions into blocks structure
    const flatQuestions = response.data
    // Sort by dataFileCol to match Results view
    flatQuestions.sort((a, b) => (a.dataFileCol || 0) - (b.dataFileCol || 0))
    
    const processed = []
    const blockMap = new Map()

    for (const q of flatQuestions) {
      // Check if this question belongs to a block
      if (q.blkQstStatus === 1) {
        // This is a block header - create the block and add the first question
        if (!blockMap.has(q.blkLabel)) {
          const block = {
            type: 'block',
            label: q.blkLabel,
            stem: q.blkStem,
            questions: [],
            isExpanded: false,
            blockId: q.blkLabel // For initialExpandedBlocks matching
          }
          blockMap.set(q.blkLabel, block)
          processed.push(block)
        }
        // Add the block header question itself to the block
        const block = blockMap.get(q.blkLabel)
        if (block) {
          block.questions.push({
            id: q.id,
            label: q.qstLabel,
            qstNumber: q.qstNumber,
            variableType: q.variableType,
            blkLabel: q.blkLabel
          })
        }
      } else if (q.blkQstStatus === 2 && q.blkLabel) {
        // This is a question within a block
        const block = blockMap.get(q.blkLabel)
        if (block) {
          block.questions.push({
            id: q.id,
            label: q.qstLabel,
            qstNumber: q.qstNumber,
            variableType: q.variableType,
            blkLabel: q.blkLabel
          })
        }
      } else {
        // Standalone question
        const questionData = {
          id: q.id,
          label: q.qstLabel,
          qstNumber: q.qstNumber,
          variableType: q.variableType
        }
        
        if (props.wrapStandaloneQuestions) {
          // For ResultsView compatibility - wrap in question property
          processed.push({
            type: 'question',
            question: questionData
          })
        } else {
          // For CrosstabView - flat structure
          processed.push({
            type: 'question',
            ...questionData
          })
        }
      }
    }

    questions.value = processed
    
    // Restore expanded blocks from prop
    if (props.initialExpandedBlocks && props.initialExpandedBlocks.length > 0) {
      props.initialExpandedBlocks.forEach(blockLabel => {
        const block = questions.value.find(q => q.type === 'block' && q.label === blockLabel)
        if (block) block.isExpanded = true
      })
    }
    
    // Emit questions-loaded event for parent to handle initial selection
    emit('questions-loaded', processed)
  } catch (err) {
    console.error('Failed to load questions:', err)
    error.value = 'Failed to load questions. Please try again.'
  } finally {
    loading.value = false
  }
}

function handleQuestionClick(question) {
  // On statsig tab, don't allow clicking independent variables (type 1)
  // Visual blocking is handled in getQuestionClasses
  if (props.activeTab === 'statsig' && question.variableType === 1) {
    return // Do nothing - IV is disabled on statsig tab
  }
  
  if (props.selectionMode === 'single') {
    // On Results tab (single mode), clicking label = navigate to crosstab with this as IV
    // Emit special event for label click that will trigger crosstab navigation
    emit('label-click-for-crosstab', question)
  } else if (props.selectionMode === 'crosstab') {
    if (USE_RADIO_BUTTON_MODE) {
      handleRadioModeCrosstabSelection(question)
    } else {
      handleCrosstabSelection(question)
    }
  } else if (props.selectionMode === 'multiple') {
    emit('question-selected', question)
  }
}

function handleRadioDVClick(question) {
  // On statsig tab, don't allow selecting IVs as DV
  if (props.activeTab === 'statsig' && question.variableType === 1) {
    return
  }
  
  // Radio button clicked - set as DV
  firstSelection.value = question
  // Keep current IV if it's not the same as the new DV
  if (secondSelection.value?.id === question.id) {
    secondSelection.value = null
  }
  emit('crosstab-selection', { first: question, second: secondSelection.value })
}

function handleRadioSingleClick(question) {
  // On statsig tab, don't allow selecting IVs
  if (props.activeTab === 'statsig' && question.variableType === 1) {
    return
  }
  
  // Radio button clicked in single selection mode
  singleSelection.value = question
  emit('question-selected', question)
}

function handleRadioModeCrosstabSelection(question) {
  // In radio mode, clicking the question label always selects/toggles IV
  // Don't allow selecting the same question as both DV and IV
  if (firstSelection.value?.id === question.id) {
    // Clicking the DV - do nothing or could deselect it
    return
  }
  
  // Set/replace as IV
  secondSelection.value = question
  emit('crosstab-selection', { first: firstSelection.value, second: question })
}

function handleCrosstabSelection(question) {
  // If in replace mode, replace the appropriate variable
  if (replaceMode.value === 1) {
    firstSelection.value = question
    // After replacing DV, automatically go back to IV replace mode
    replaceMode.value = 2
    emit('crosstab-selection', { first: question, second: secondSelection.value })
    return
  }
  
  if (replaceMode.value === 2) {
    // Don't allow selecting the same question as both DV and IV
    if (firstSelection.value?.id === question.id) {
      return // Do nothing
    }
    secondSelection.value = question
    // Stay in replace mode for IV to allow easy comparison
    // replaceMode.value stays as 2
    emit('crosstab-selection', { first: firstSelection.value, second: question })
    return
  }
  
  // If clicking already selected question, deselect it
  if (firstSelection.value?.id === question.id) {
    firstSelection.value = null
    emit('crosstab-selection', { first: null, second: secondSelection.value })
    return
  }
  
  if (secondSelection.value?.id === question.id) {
    secondSelection.value = null
    emit('crosstab-selection', { first: firstSelection.value, second: null })
    return
  }

  // Select first, then second
  if (!firstSelection.value) {
    firstSelection.value = question
    // After selecting first DV, automatically go to IV replace mode
    // (Don't set replaceMode yet - wait until IV is selected)
    emit('crosstab-selection', { first: question, second: secondSelection.value })
  } else if (!secondSelection.value) {
    // Don't allow selecting the same question as both DV and IV
    if (firstSelection.value?.id === question.id) {
      return // Do nothing
    }
    secondSelection.value = question
    // Automatically enter replace mode for IV after initial selection
    replaceMode.value = 2
    emit('crosstab-selection', { first: firstSelection.value, second: question })
  } else {
    // Both already selected - ignore the click (UI shows Change buttons)
    return
  }
}

function getSelectionNumber(question) {
  if (props.selectionMode !== 'crosstab') return null
  
  if (firstSelection.value?.id === question.id) return '1'
  if (secondSelection.value?.id === question.id) return '2'
  return null
}

function getSelectionBadgeClasses(question) {
  if (firstSelection.value?.id === question.id) {
    return 'bg-blue-600'
  }
  if (secondSelection.value?.id === question.id) {
    return 'bg-green-600'
  }
  return ''
}

function getQuestionClasses(question) {
  const questionId = question.id || question
  
  // On statsig tab, make IVs (type 1) appear disabled
  if (props.activeTab === 'statsig' && question.variableType === 1) {
    return 'opacity-40 cursor-not-allowed border-transparent'
  }
  
  if (props.selectionMode === 'crosstab') {
    const isFirst = firstSelection.value?.id === questionId
    const isSecond = secondSelection.value?.id === questionId
    
    // If in replace mode, show appropriate highlighting
    if (replaceMode.value !== null) {
      if (isFirst && replaceMode.value === 1) {
        // Replacing DV - show current DV as highlighted for replacement
        return 'bg-blue-100 dark:bg-blue-900/40 border-blue-500 dark:border-blue-400 border-2'
      }
      if (isSecond && replaceMode.value === 2) {
        // Replacing IV - show current IV as highlighted for replacement
        return 'bg-green-100 dark:bg-green-900/40 border-green-500 dark:border-green-400 border-2'
      }
      if (isFirst && replaceMode.value === 2) {
        // Replacing IV but this is the DV - keep DV highlighted but subtle
        return 'bg-blue-50 dark:bg-blue-900/30 border-blue-400/60 dark:border-blue-500/60 border'
      }
      // Dim other questions slightly
      return 'hover:bg-blue-50 dark:hover:bg-blue-900/20 border-transparent opacity-80'
    }
    
    // Normal mode highlighting
    if (isFirst) {
      return 'bg-blue-50 dark:bg-blue-900/30 border-blue-400/60 dark:border-blue-500/60 border'
    }
    if (isSecond) {
      return 'bg-green-50 dark:bg-green-900/30 border-green-400/60 dark:border-green-500/60 border'
    }
    return 'hover:bg-gray-100 dark:hover:bg-gray-700 border-transparent'
  }

  // Single selection mode
  const isSelected = (props.selectedQuestionId && props.selectedQuestionId === questionId) || 
                     (singleSelection.value?.id === questionId)
  
  return isSelected
    ? 'bg-blue-50 dark:bg-blue-900/30 border-blue-400/60 dark:border-blue-500/60 border'
    : 'hover:bg-gray-100 dark:hover:bg-gray-700 border-transparent'
}

function toggleBlock(block) {
  block.isExpanded = !block.isExpanded
  
  // Emit expanded blocks for state management
  if (props.usePreprocessedEndpoint && block.blockId) {
    const expandedBlockIds = questions.value
      .filter(q => q.type === 'block' && q.isExpanded)
      .map(q => q.blockId)
    emit('expanded-blocks-changed', expandedBlockIds)
  }
}

function expandAll() {
  questions.value.forEach(item => {
    if (item.type === 'block') {
      item.isExpanded = true
    }
  })
  
  // Emit expanded blocks
  if (props.usePreprocessedEndpoint) {
    const expandedBlockIds = questions.value
      .filter(q => q.type === 'block')
      .map(q => q.blockId)
    emit('expanded-blocks-changed', expandedBlockIds)
  }
}

function collapseAll() {
  questions.value.forEach(item => {
    if (item.type === 'block') {
      item.isExpanded = false
    }
  })
  
  // Emit empty array
  emit('expanded-blocks-changed', [])
}

function clearSearch() {
  searchQuery.value = ''
}

function clearSelections() {
  firstSelection.value = null
  secondSelection.value = null
  replaceMode.value = null
  emit('selection-cleared')
  emit('crosstab-selection', { first: null, second: null })
}

function enterReplaceMode(variableNumber) {
  replaceMode.value = variableNumber
}

// Sync singleSelection with selectedQuestionId prop (for single selection mode)
watch(() => props.selectedQuestionId, (newId) => {
  if (props.selectionMode === 'single') {
    if (!newId) {
      singleSelection.value = null
    } else if (questions.value.length > 0) {
      // Find the question in our list and set it
      const findQuestionById = (items, id) => {
        for (const item of items) {
          if (item.type === 'block') {
            const found = item.questions.find(q => q.id === id)
            if (found) return found
          } else if (item.type === 'question') {
            const q = item.question || item
            if (q.id === id) return q
          }
        }
        return null
      }
      
      const question = findQuestionById(questions.value, newId)
      if (question) {
        singleSelection.value = question
      }
    }
  }
})

// Watch for changes to initialExpandedBlocks and update block expansion state
watch(() => props.initialExpandedBlocks, (newExpandedBlocks) => {
  if (newExpandedBlocks && newExpandedBlocks.length > 0 && questions.value.length > 0) {
    newExpandedBlocks.forEach(blockId => {
      const block = questions.value.find(q => q.type === 'block' && q.blockId === blockId)
      if (block && !block.isExpanded) {
        block.isExpanded = true
      }
    })
  }
}, { deep: true })

// Load questions on mount
watch(() => props.surveyId, () => {
  if (props.surveyId) {
    loadQuestions()
  }
}, { immediate: true })
</script>

<style scoped>
@keyframes pulse-twice {
  0%, 100% {
    opacity: 1;
    transform: scale(1);
  }
  25%, 75% {
    opacity: 0.5;
    transform: scale(0.98);
  }
  50% {
    opacity: 1;
    transform: scale(1);
  }
}

.animate-pulse-twice {
  animation: pulse-twice 1.5s ease-in-out;
}
</style>
