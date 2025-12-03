<template>
  <div class="h-full flex">
    <!-- Question List Sidebar -->
    <aside class="w-80 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col">
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
          <div class="flex items-center space-x-1.5 ml-auto text-xs">
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
                <!-- Selected indicator -->
                <svg
                  v-if="item.questions.some(q => selectedQuestion?.id === q.id)"
                  class="w-4 h-4 text-blue-500 dark:text-blue-400 flex-shrink-0"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  title="Contains selected question"
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
                  @click="selectQuestion(question)"
                  :class="[
                    'px-2 py-0.5 rounded cursor-pointer transition-all flex items-center space-x-2',
                    selectedQuestion?.id === question.id
                      ? 'bg-blue-50 dark:bg-blue-900/30 border-l-2 border-blue-500 dark:border-blue-400'
                      : 'hover:bg-gray-100 dark:hover:bg-gray-700 border-l-2 border-transparent'
                  ]"
                >
                  <!-- Variable Type Icon: 1=IV (red), 2=DV (blue) -->
                  <svg 
                    class="w-3.5 h-3.5 flex-shrink-0" 
                    :class="question.variableType === 1 ? 'text-red-400' : question.variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
                    fill="currentColor" 
                    viewBox="0 0 20 20"
                  >
                    <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                  </svg>
                  <div class="flex-1 min-w-0 leading-none">
                    <span class="text-sm text-gray-700 dark:text-gray-300">
                      {{ question.label }}
                    </span>
                    <span class="text-xs text-gray-400 dark:text-gray-500 ml-2">
                      {{ question.qstNumber }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Standalone Question -->
            <div
              v-else
              @click="selectQuestion(item.question)"
              :class="[
                'px-2 py-0.5 rounded cursor-pointer transition-all flex items-center space-x-2',
                selectedQuestion?.id === item.question.id
                  ? 'bg-blue-50 dark:bg-blue-900/30 border-l-2 border-blue-500 dark:border-blue-400'
                  : 'hover:bg-gray-100 dark:hover:bg-gray-700 border-l-2 border-transparent'
              ]"
            >
              <!-- Variable Type Icon: 1=IV (red), 2=DV (blue) -->
              <svg 
                class="w-3.5 h-3.5 flex-shrink-0" 
                :class="item.question.variableType === 1 ? 'text-red-400' : item.question.variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
                fill="currentColor" 
                viewBox="0 0 20 20"
              >
                <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
              </svg>
              <div class="flex-1 min-w-0 leading-none">
                <span class="text-sm text-gray-700 dark:text-gray-300">
                  {{ item.question.label }}
                </span>
                <span class="text-xs text-gray-400 dark:text-gray-500 ml-2">
                  {{ item.question.qstNumber }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Total Cases removed - shown per question in header instead -->
    </aside>

    <!-- Main Content Area -->
    <div class="flex-1 overflow-hidden flex flex-col">
      <!-- No Question Selected State -->
      <div v-if="!selectedQuestion" class="flex-1 flex items-center justify-center">
        <div class="text-center">
          <svg class="mx-auto h-16 w-16 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
          </svg>
          <h3 class="mt-4 text-lg font-medium text-gray-900 dark:text-white">Select a Question</h3>
          <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
            Choose a question from the list to view its results and visualization
          </p>
        </div>
      </div>

      <!-- Question Results -->
      <template v-else>
        <!-- Question Header -->
        <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-3">
          <div class="flex items-center justify-between">
            <div class="flex items-center space-x-3">
              <!-- Variable Type Icon: 1=IV (red), 2=DV (blue) -->
              <svg 
                class="w-5 h-5" 
                :class="selectedQuestion.variableType === 1 ? 'text-red-400' : selectedQuestion.variableType === 2 ? 'text-blue-400' : 'text-gray-400'"
                fill="currentColor" 
                viewBox="0 0 20 20"
              >
                <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
              </svg>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white">
                {{ selectedQuestion.label }}
              </h2>
              <span class="text-sm text-gray-400 dark:text-gray-500">
                {{ selectedQuestion.qstNumber }}
              </span>
            </div>
            <div class="flex items-center space-x-4 text-sm text-gray-600 dark:text-gray-400">
              <span><span class="font-medium">Index:</span> {{ selectedQuestion.index || '128' }}</span>
              <span>•</span>
              <span><span class="font-medium">Total N:</span> {{ selectedQuestion.totalCases }}</span>
            </div>
          </div>
        </div>

        <!-- Combined Results and Chart Content -->
        <div class="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900 p-6 [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-gray-100 dark:[&::-webkit-scrollbar-track]:bg-gray-800 [&::-webkit-scrollbar-thumb]:bg-gray-300 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb:hover]:bg-gray-400 dark:[&::-webkit-scrollbar-thumb:hover]:bg-gray-500">
          <div class="space-y-6">
            <!-- Results Table -->
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
              <div class="px-6 py-3 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
                <h3 class="text-base font-medium text-gray-900 dark:text-white">
                  Response Results
                </h3>
              </div>
              <div class="overflow-x-auto">
                <table class="min-w-full">
                  <thead>
                    <tr class="border-b-2 border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-800">
                      <th class="px-6 py-3 text-center text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider">
                        #
                      </th>
                      <th class="px-6 py-3 text-left text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider">
                        Response
                      </th>
                      <th class="px-6 py-3 text-right text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider">
                        %
                      </th>
                      <th class="px-6 py-3 text-center text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider">
                        Index
                      </th>
                      <th class="px-6 py-3 text-right text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase tracking-wider">
                        Count
                      </th>
                    </tr>
                  </thead>
                  <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                    <tr v-for="(response, index) in selectedQuestion.responses" :key="index" class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                      <td class="px-6 py-2 whitespace-nowrap text-sm text-center text-gray-500 dark:text-gray-400">
                        {{ index + 1 }}
                      </td>
                      <td class="px-6 py-2 text-sm text-gray-900 dark:text-white">
                        {{ response.label }}
                      </td>
                      <td class="px-6 py-2 whitespace-nowrap text-sm text-right text-gray-900 dark:text-white font-medium">
                        {{ response.percentage.toFixed(1) }}
                      </td>
                      <td class="px-6 py-2 whitespace-nowrap text-sm text-center text-gray-500 dark:text-gray-400">
                        {{ response.indexSymbol || '' }}
                      </td>
                      <td class="px-6 py-2 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {{ response.count }}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Chart -->
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
              <div class="px-6 py-3 border-b border-gray-200 dark:border-gray-700">
                <h3 class="text-base font-medium text-gray-900 dark:text-white">
                  Frequency Distribution
                </h3>
              </div>
              <div class="p-6">
                <QuestionChart :question="selectedQuestion" />
              </div>
            </div>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import axios from 'axios'
import QuestionChart from './QuestionChart.vue'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  initialQuestionId: {
    type: String,
    default: null
  },
  initialExpandedBlocks: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['question-selected', 'expanded-blocks-changed'])

const questions = ref([])
const selectedQuestion = ref(null)
const searchQuery = ref('')
const loading = ref(true)
const error = ref(null)
const totalCases = ref(0)

// Track expanded state of blocks using a reactive Set
const expandedBlocks = ref(new Set())

// Organize questions into blocks
const organizedQuestions = computed(() => {
  const result = []
  const questionList = questions.value
  let currentBlock = null
  let blockIndex = 0
  
  for (const question of questionList) {
    if (question.blkQstStatus === 1) { // FirstQuestionInBlock
      // Start a new block
      currentBlock = {
        type: 'block',
        blockId: `block-${blockIndex++}`,
        label: question.blkLabel || 'Untitled Block',
        questions: [question],
        get isExpanded() {
          return expandedBlocks.value.has(this.blockId)
        },
        set isExpanded(value) {
          if (value) {
            expandedBlocks.value.add(this.blockId)
          } else {
            expandedBlocks.value.delete(this.blockId)
          }
        }
      }
      result.push(currentBlock)
    } else if (question.blkQstStatus === 2 && currentBlock) { // ContinuationQuestion
      // Add to current block
      currentBlock.questions.push(question)
    } else { // DiscreetQuestion or no block status
      // Add as standalone question
      result.push({
        type: 'question',
        question: question
      })
    }
  }
  
  return result
})

const filteredQuestions = computed(() => {
  if (!searchQuery.value) return organizedQuestions.value
  const query = searchQuery.value.toLowerCase()
  
  // Filter both blocks and standalone questions
  const filtered = []
  for (const item of organizedQuestions.value) {
    if (item.type === 'block') {
      // Check if block label matches or any question in block matches
      const blockMatches = item.label.toLowerCase().includes(query)
      const matchingQuestions = item.questions.filter(q =>
        q.label.toLowerCase().includes(query)
      )
      
      if (blockMatches || matchingQuestions.length > 0) {
        // Auto-expand matching blocks when searching
        if (!expandedBlocks.value.has(item.blockId)) {
          expandedBlocks.value.add(item.blockId)
        }
        filtered.push({
          ...item,
          questions: blockMatches ? item.questions : matchingQuestions
        })
      }
    } else {
      // Standalone question
      if (item.question.label.toLowerCase().includes(query)) {
        filtered.push(item)
      }
    }
  }
  
  return filtered
})

function clearSearch() {
  searchQuery.value = ''
}

function toggleBlock(block) {
  if (expandedBlocks.value.has(block.blockId)) {
    expandedBlocks.value.delete(block.blockId)
  } else {
    expandedBlocks.value.add(block.blockId)
  }
  // Emit the current expanded blocks as an array
  emit('expanded-blocks-changed', Array.from(expandedBlocks.value))
}

function expandAll() {
  // Add all block IDs to expandedBlocks
  organizedQuestions.value.forEach(item => {
    if (item.type === 'block') {
      expandedBlocks.value.add(item.blockId)
    }
  })
  emit('expanded-blocks-changed', Array.from(expandedBlocks.value))
}

function collapseAll() {
  // Clear all expanded blocks
  expandedBlocks.value.clear()
  emit('expanded-blocks-changed', Array.from(expandedBlocks.value))
}

function selectQuestion(question) {
  selectedQuestion.value = question
  // Emit the question ID to parent for state management
  emit('question-selected', question.id)
}

async function loadQuestions() {
  loading.value = true
  error.value = null

  try {
    const response = await axios.get(`http://localhost:5107/api/surveys/${props.surveyId}/questions`)
    questions.value = response.data
    totalCases.value = response.data[0]?.totalCases || 0
    
    // Restore expanded blocks from prop
    if (props.initialExpandedBlocks && props.initialExpandedBlocks.length > 0) {
      expandedBlocks.value = new Set(props.initialExpandedBlocks)
    }
    
    // Restore selected question if initialQuestionId provided
    if (props.initialQuestionId && questions.value.length > 0) {
      const savedQuestion = questions.value.find(q => q.id === props.initialQuestionId)
      if (savedQuestion) {
        selectedQuestion.value = savedQuestion
        // Emit to ensure parent state is in sync
        emit('question-selected', savedQuestion.id)
      } else {
        // If saved question not found, select first
        selectedQuestion.value = questions.value[0]
        emit('question-selected', questions.value[0].id)
      }
    } else if (questions.value.length > 0) {
      // Default to first question if none selected
      selectedQuestion.value = questions.value[0]
      emit('question-selected', questions.value[0].id)
    }
  } catch (err) {
    console.error('Error loading questions:', err)
    error.value = 'Failed to load questions. Please try again.'
  } finally {
    loading.value = false
  }
}

// Watch for initialQuestionId changes (e.g., browser back/forward)
watch(() => props.initialQuestionId, (newQuestionId) => {
  if (newQuestionId && questions.value.length > 0) {
    const question = questions.value.find(q => q.id === newQuestionId)
    if (question) {
      selectedQuestion.value = question
    }
  }
})

onMounted(() => {
  loadQuestions()
})
</script>
