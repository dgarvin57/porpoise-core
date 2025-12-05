<template>
  <div class="h-full flex flex-col">
    <!-- Instructions -->
    <div v-if="selectionMode === 'crosstab'" class="p-4 bg-blue-50 dark:bg-blue-900/20 border-b border-blue-200 dark:border-blue-800">
      <p class="text-sm text-blue-900 dark:text-blue-100 font-medium mb-1">
        {{ instructionText }}
      </p>
      <div class="flex gap-3 text-xs">
        <div class="flex items-center gap-1">
          <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-blue-600 text-white font-semibold">1</span>
          <span class="text-blue-800 dark:text-blue-200">First Variable</span>
        </div>
        <div class="flex items-center gap-1">
          <span class="inline-flex items-center justify-center w-5 h-5 rounded bg-green-600 text-white font-semibold">2</span>
          <span class="text-blue-800 dark:text-blue-200">Second Variable</span>
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
          class="w-full px-3 py-1.5 pl-9 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <svg class="absolute left-3 top-2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
        </svg>
      </div>
      <div class="flex items-center justify-between mt-1">
        <button
          @click="clearSearch"
          v-if="searchQuery"
          class="text-xs text-blue-600 dark:text-blue-400 hover:underline"
        >
          Clear
        </button>
        <button
          v-if="selectionMode === 'crosstab' && (firstSelection || secondSelection)"
          @click="clearSelections"
          class="text-xs text-red-600 dark:text-red-400 hover:underline ml-auto"
        >
          Clear Selections
        </button>
        <div v-else class="flex items-center space-x-1.5 ml-auto text-xs">
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
      </div>
    </div>

    <!-- Question List -->
    <div class="flex-1 overflow-y-auto [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-gray-100 dark:[&::-webkit-scrollbar-track]:bg-gray-800 [&::-webkit-scrollbar-thumb]:bg-gray-300 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-thumb]:rounded-full">
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
              class="w-full flex items-center space-x-2 px-2 py-1 bg-gray-50 dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 border border-gray-200 dark:border-gray-600 rounded transition-colors text-left"
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
              <span class="text-xs text-gray-400 dark:text-gray-500 flex-shrink-0">
                {{ item.questions.length }}
              </span>
            </button>
            
            <!-- Block Questions -->
            <div v-show="item.isExpanded" class="ml-6 mt-0.5 space-y-0">
              <button
                v-for="question in item.questions"
                :key="question.id"
                @click="handleQuestionClick(question)"
                :class="getQuestionClasses(question)"
                class="w-full flex items-center gap-2 px-2 py-1.5 rounded text-sm text-left transition-all"
              >
                <!-- Selection Badge -->
                <span 
                  v-if="getSelectionNumber(question)"
                  :class="getSelectionBadgeClasses(question)"
                  class="inline-flex items-center justify-center w-5 h-5 rounded font-semibold text-white text-xs flex-shrink-0"
                >
                  {{ getSelectionNumber(question) }}
                </span>
                
                <span class="flex-1 truncate">{{ question.label }}</span>
              </button>
            </div>
          </div>

          <!-- Standalone Question -->
          <button
            v-else
            @click="handleQuestionClick(item)"
            :class="getQuestionClasses(item)"
            class="w-full flex items-center gap-2 px-2 py-1.5 rounded text-sm text-left transition-all"
          >
            <!-- Selection Badge -->
            <span 
              v-if="getSelectionNumber(item)"
              :class="getSelectionBadgeClasses(item)"
              class="inline-flex items-center justify-center w-5 h-5 rounded font-semibold text-white text-xs flex-shrink-0"
            >
              {{ getSelectionNumber(item) }}
            </span>
            
            <span class="flex-1 truncate">{{ item.label }}</span>
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
  }
})

const emit = defineEmits(['question-selected', 'crosstab-selection', 'selection-cleared'])

// State
const loading = ref(false)
const error = ref(null)
const questions = ref([])
const searchQuery = ref('')
const expandedBlocks = ref(new Set())

// Crosstab selections
const firstSelection = ref(null)
const secondSelection = ref(null)

// Computed
const instructionText = computed(() => {
  if (!firstSelection.value) {
    return 'Click on a question to select the first variable'
  } else if (!secondSelection.value) {
    return 'Click on another question to select the second variable'
  } else {
    return 'Both variables selected. Click "Generate Crosstab" or select different variables'
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
      const matchingQuestions = item.questions.filter(q =>
        q.label.toLowerCase().includes(query)
      )
      if (matchingQuestions.length > 0) {
        filtered.push({
          ...item,
          questions: matchingQuestions,
          isExpanded: true
        })
      }
    } else if (item.label.toLowerCase().includes(query)) {
      filtered.push(item)
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
        // This is a block header - create or get the block
        if (!blockMap.has(q.blkLabel)) {
          const block = {
            type: 'block',
            label: q.blkLabel,
            stem: q.blkStem,
            questions: [],
            isExpanded: false
          }
          blockMap.set(q.blkLabel, block)
          processed.push(block)
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
        processed.push({
          type: 'question',
          id: q.id,
          label: q.qstLabel,
          qstNumber: q.qstNumber,
          variableType: q.variableType
        })
      }
    }

    questions.value = processed
  } catch (err) {
    console.error('Failed to load questions:', err)
    error.value = 'Failed to load questions. Please try again.'
  } finally {
    loading.value = false
  }
}

function handleQuestionClick(question) {
  if (props.selectionMode === 'single') {
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
    emit('crosstab-selection', { first: question, second: null })
  } else if (!secondSelection.value) {
    secondSelection.value = question
    emit('crosstab-selection', { first: firstSelection.value, second: question })
  } else {
    // Both already selected, replace first and shift
    firstSelection.value = secondSelection.value
    secondSelection.value = question
    emit('crosstab-selection', { first: firstSelection.value, second: question })
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
  const isFirst = firstSelection.value?.id === question.id
  const isSecond = secondSelection.value?.id === question.id
  const isSelected = isFirst || isSecond

  if (props.selectionMode === 'crosstab') {
    if (isFirst) {
      return 'bg-blue-50 dark:bg-blue-900/30 border-l-2 border-blue-600 text-gray-900 dark:text-white font-medium'
    }
    if (isSecond) {
      return 'bg-green-50 dark:bg-green-900/30 border-l-2 border-green-600 text-gray-900 dark:text-white font-medium'
    }
    return 'hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300'
  }

  return isSelected
    ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-900 dark:text-blue-100 font-medium'
    : 'hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300'
}

function toggleBlock(block) {
  block.isExpanded = !block.isExpanded
}

function expandAll() {
  questions.value.forEach(item => {
    if (item.type === 'block') {
      item.isExpanded = true
    }
  })
}

function collapseAll() {
  questions.value.forEach(item => {
    if (item.type === 'block') {
      item.isExpanded = false
    }
  })
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

// Load questions on mount
watch(() => props.surveyId, () => {
  if (props.surveyId) {
    loadQuestions()
  }
}, { immediate: true })
</script>
