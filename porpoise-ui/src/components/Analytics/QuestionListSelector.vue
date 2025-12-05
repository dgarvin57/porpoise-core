<template>
  <div class="h-full flex flex-col">
    <!-- Instructions -->
    <div v-if="selectionMode === 'crosstab'" class="p-4 bg-blue-50 dark:bg-blue-900/20 border-b border-blue-200 dark:border-blue-800" :class="{ 'animate-pulse-twice': pulseInstructions }">
      <p class="text-sm text-blue-900 dark:text-blue-100 font-medium mb-1">
        {{ instructionText }}
      </p>
      <div class="flex gap-2 text-xs overflow-hidden">
        <div class="flex items-center gap-1 min-w-0 flex-1">
          <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-blue-600 text-white font-semibold flex-shrink-0">1</span>
          <span class="text-blue-800 dark:text-blue-200 truncate" :title="firstSelection?.label || 'First Variable'">
            {{ firstSelection?.label || 'First Variable' }}
          </span>
        </div>
        <div class="flex items-center gap-1 min-w-0 flex-1">
          <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-green-600 text-white font-semibold flex-shrink-0">2</span>
          <span class="text-blue-800 dark:text-blue-200 truncate" :title="secondSelection?.label || 'Second Variable'">
            {{ secondSelection?.label || 'Second Variable' }}
          </span>
        </div>
      </div>
    </div>

    <!-- Search Bar -->
    <div class="p-3 border-b border-gray-200 dark:border-gray-700">
      <div class="relative">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Find question"
          autocomplete="off"
          data-1p-ignore
          data-lpignore="true"
          class="w-full px-3 py-1.5 pl-9 pr-9 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
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
      <div class="flex items-center justify-between text-xs mt-1">
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
          v-if="selectionMode === 'crosstab' && (firstSelection || secondSelection)"
          @click="clearSelections"
          class="text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 cursor-pointer hover:underline"
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
              class="w-full flex items-center space-x-2 px-2 py-1 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 rounded transition-colors text-left"
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
                <span class="text-sm font-medium text-gray-900 dark:text-white truncate">
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
            <div v-show="item.isExpanded" class="ml-6 mt-0.5 space-y-0">
              <div
                v-for="question in item.questions"
                :key="question.id"
                @click="handleQuestionClick(question)"
                :class="getQuestionClasses(question)"
                class="w-full flex items-center space-x-2 px-2 py-0.5 rounded cursor-pointer transition-all border-l"
              >
                <!-- Selection Badge or Variable Type Icon -->
                <span 
                  v-if="getSelectionNumber(question)"
                  :class="getSelectionBadgeClasses(question)"
                  class="inline-flex items-center justify-center w-5 h-5 rounded font-semibold text-white text-xs flex-shrink-0"
                >
                  {{ getSelectionNumber(question) }}
                </span>
                <svg 
                  v-else
                  class="w-3.5 h-3.5 flex-shrink-0" 
                  :class="question.variableType === 1 ? 'text-red-400' : question.variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
                  fill="currentColor" 
                  viewBox="0 0 20 20"
                >
                  <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                </svg>
                
                <div class="flex-1 min-w-0 leading-none">
                  <span class="text-sm text-gray-700 dark:text-gray-300">{{ question.label }}</span>
                  <svg v-if="showNotesIcons && question.questionNotes" class="inline-block w-3.5 h-3.5 text-blue-500 dark:text-blue-400 ml-1.5 -mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
                  </svg>
                  <span class="text-xs text-gray-400 dark:text-gray-500 ml-2">{{ question.qstNumber }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Standalone Question -->
          <div
            v-else
            @click="handleQuestionClick(item.question || item)"
            :class="getQuestionClasses(item.question || item)"
            class="w-full flex items-center space-x-2 px-2 py-0.5 rounded cursor-pointer transition-all border-l-2"
          >
            <!-- Selection Badge or Variable Type Icon -->
            <span 
              v-if="getSelectionNumber(item.question || item)"
              :class="getSelectionBadgeClasses(item.question || item)"
              class="inline-flex items-center justify-center w-5 h-5 rounded font-semibold text-white text-xs flex-shrink-0"
            >
              {{ getSelectionNumber(item.question || item) }}
            </span>
            <svg 
              v-else
              class="w-3.5 h-3.5 flex-shrink-0" 
              :class="(item.question || item).variableType === 1 ? 'text-red-400' : (item.question || item).variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
              fill="currentColor" 
              viewBox="0 0 20 20"
            >
              <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
            </svg>
            
            <div class="flex-1 min-w-0 leading-none">
              <span class="text-sm text-gray-700 dark:text-gray-300">{{ (item.question || item).label }}</span>
              <svg v-if="showNotesIcons && (item.question || item).questionNotes" class="inline-block w-3.5 h-3.5 text-blue-500 dark:text-blue-400 ml-1.5 -mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z" />
              </svg>
              <span class="text-xs text-gray-400 dark:text-gray-500 ml-2">{{ (item.question || item).qstNumber }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  
  <!-- Third Variable Selection Modal -->
  <div v-if="showThirdVariableModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50" @click.self="closeThirdVariableModal">
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-md w-full mx-4">
      <div class="p-6">
        <div class="flex items-start space-x-3 mb-4">
          <div class="flex-shrink-0">
            <svg class="w-6 h-6 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>
          <div class="flex-1">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
              Cannot Select Third Variable
            </h3>
            <p class="text-sm text-gray-600 dark:text-gray-300 mb-3">
              You already have two variables selected. To change a variable:
            </p>
            <ol class="text-sm text-gray-600 dark:text-gray-300 space-y-2 mb-4 ml-4 list-decimal">
              <li>Click on the variable you want to change (it will show a <span class="inline-flex items-center justify-center w-4 h-4 rounded bg-blue-500 text-white text-xs font-semibold mx-1">1</span> or <span class="inline-flex items-center justify-center w-4 h-4 rounded bg-green-600 text-white text-xs font-semibold mx-1">2</span> badge)</li>
              <li>Then select a new question to replace it</li>
            </ol>
            <p class="text-xs text-gray-500 dark:text-gray-400 italic">
              Or use "Clear Selection" to start over
            </p>
          </div>
        </div>
        
        <div class="mb-4 flex items-center">
          <input 
            id="dontShowAgain" 
            v-model="dontShowAgain" 
            type="checkbox" 
            class="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          />
          <label for="dontShowAgain" class="ml-2 text-sm text-gray-600 dark:text-gray-400 cursor-pointer">
            Don't show this message again
          </label>
        </div>
        
        <div class="flex justify-end">
          <button
            @click="closeThirdVariableModal"
            class="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors font-medium"
          >
            Got it
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import axios from 'axios'

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
  }
})

const emit = defineEmits(['question-selected', 'crosstab-selection', 'selection-cleared', 'expanded-blocks-changed'])

// State
const loading = ref(false)
const error = ref(null)
const questions = ref([])
const searchQuery = ref('')

// Crosstab selections
const firstSelection = ref(props.initialFirstSelection)
const secondSelection = ref(props.initialSecondSelection)

// Watch for prop changes to update selections
watch(() => props.initialFirstSelection, (newVal) => {
  console.log('QuestionListSelector: initialFirstSelection prop changed', newVal)
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
  console.log('QuestionListSelector: initialSecondSelection prop changed', newVal)
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

// Modal state
const showThirdVariableModal = ref(false)
const dontShowAgain = ref(false)
const pulseInstructions = ref(false)

// Single selection state
const singleSelection = ref(null)

// Computed
const instructionText = computed(() => {
  if (!firstSelection.value) {
    return 'Click on a question to select the first variable'
  } else if (!secondSelection.value) {
    return 'Click on another question to select the second variable'
  } else {
    return 'To change a variable, click it to deselect, then select a new one or clear selection'
  }
})

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
    const response = await axios.get(`http://localhost:5107/api/surveys/${props.surveyId}/questions-list`)
    
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
            blkLabel: q.blkLabel,
            questionNotes: q.questionNotes
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
            blkLabel: q.blkLabel,
            questionNotes: q.questionNotes
          })
        }
      } else {
        // Standalone question
        const questionData = {
          id: q.id,
          label: q.qstLabel,
          qstNumber: q.qstNumber,
          variableType: q.variableType,
          questionNotes: q.questionNotes
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
  } catch (err) {
    console.error('Failed to load questions:', err)
    error.value = 'Failed to load questions. Please try again.'
  } finally {
    loading.value = false
  }
}

function handleQuestionClick(question) {
  if (props.selectionMode === 'single') {
    singleSelection.value = question
    emit('question-selected', question)
  } else if (props.selectionMode === 'crosstab') {
    handleCrosstabSelection(question)
  } else if (props.selectionMode === 'multiple') {
    emit('question-selected', question)
  }
}

function handleCrosstabSelection(question) {
  // If clicking already selected question, deselect it
  if (firstSelection.value?.id === question.id) {
    firstSelection.value = null
    // Keep second selection as-is, just clear the first
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
    emit('crosstab-selection', { first: question, second: secondSelection.value })
  } else if (!secondSelection.value) {
    secondSelection.value = question
    emit('crosstab-selection', { first: firstSelection.value, second: question })
  } else {
    // Both already selected - show helpful modal (unless user disabled it)
    const hideModal = localStorage.getItem('hideThirdVariableModal') === 'true'
    if (!hideModal) {
      showThirdVariableModal.value = true
    } else {
      // User dismissed modal, so pulse the instructions to remind them
      pulseInstructions.value = true
      setTimeout(() => {
        pulseInstructions.value = false
      }, 1500) // Remove after 1.5 seconds (matches animation duration)
    }
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
  
  if (props.selectionMode === 'crosstab') {
    const isFirst = firstSelection.value?.id === questionId
    const isSecond = secondSelection.value?.id === questionId
    
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
    ? 'bg-blue-50 dark:bg-blue-900/30 border-blue-500 dark:border-blue-400'
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
  emit('selection-cleared')
  emit('crosstab-selection', { first: null, second: null })
}

function closeThirdVariableModal() {
  if (dontShowAgain.value) {
    localStorage.setItem('hideThirdVariableModal', 'true')
  }
  showThirdVariableModal.value = false
  dontShowAgain.value = false
}

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
