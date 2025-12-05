<template>
  <div class="h-full flex flex-col bg-gray-50 dark:bg-gray-900">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-6">
      <h2 class="text-2xl font-bold text-gray-900 dark:text-white mb-2">Question List</h2>
      <p class="text-sm text-gray-600 dark:text-gray-400">{{ questions.length }} questions total</p>
    </div>

    <!-- Question Tree -->
    <div class="flex-1 overflow-auto p-6">
      <div v-if="loading" class="flex items-center justify-center h-64">
        <div class="text-gray-500 dark:text-gray-400">Loading questions...</div>
      </div>

      <div v-else-if="error" class="text-center text-red-600 dark:text-red-400 p-4">
        {{ error }}
      </div>

      <div v-else class="space-y-2">
        <!-- Iterate through organized questions -->
        <template v-for="item in organizedQuestions" :key="item.question.id">
          <!-- Block Header (first question of block) -->
          <div v-if="item.isBlockStart" class="mt-4">
            <div
              @click="toggleBlock(item.blockId)"
              class="flex items-center px-4 py-3 bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg cursor-pointer hover:bg-blue-100 dark:hover:bg-blue-900/30 transition-colors"
            >
              <!-- Expand/Collapse Icon -->
              <svg
                class="w-5 h-5 text-blue-600 dark:text-blue-400 mr-3 transition-transform"
                :class="{ 'transform rotate-90': expandedBlocks.has(item.blockId) }"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>

              <!-- Block Icon -->
              <svg class="w-5 h-5 text-blue-600 dark:text-blue-400 mr-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10" />
              </svg>

              <div class="flex-1">
                <div class="font-semibold text-blue-900 dark:text-blue-100">
                  {{ item.question.blkLabel || 'Block' }}
                </div>
                <div v-if="item.question.blkStem" class="text-sm text-blue-700 dark:text-blue-300 mt-1">
                  {{ item.question.blkStem }}
                </div>
              </div>

              <span class="text-sm text-blue-600 dark:text-blue-400">
                {{ item.blockSize }} questions
              </span>
            </div>
          </div>

          <!-- Question Row -->
          <div
            v-show="!item.inCollapsedBlock"
            @click="selectQuestion(item.question)"
            :class="[
              'flex items-center px-4 py-3 rounded-lg border cursor-pointer transition-colors',
              item.indent ? 'ml-8' : '',
              selectedQuestion?.id === item.question.id
                ? 'bg-blue-50 dark:bg-blue-900/20 border-blue-300 dark:border-blue-700'
                : 'bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-700/50'
            ]"
          >
            <!-- Question Number -->
            <div class="flex-shrink-0 w-20 text-sm font-mono text-gray-600 dark:text-gray-400">
              {{ item.question.qstNumber }}
            </div>

            <!-- Question Label -->
            <div class="flex-1 min-w-0">
              <div class="font-medium text-gray-900 dark:text-white truncate">
                {{ item.question.qstLabel || 'Untitled Question' }}
              </div>
            </div>

            <!-- Variable Type Badge -->
            <div class="flex-shrink-0 ml-4">
              <span
                :class="[
                  'px-2 py-1 text-xs font-medium rounded',
                  item.question.variableType === 0
                    ? 'bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300'
                    : 'bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300'
                ]"
              >
                {{ item.question.variableType === 0 ? 'Dependent' : 'Independent' }}
              </span>
            </div>

            <!-- Chevron -->
            <svg class="w-5 h-5 text-gray-400 ml-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </div>
        </template>
      </div>
    </div>

    <!-- Selected Question Detail Panel -->
    <div
      v-if="selectedQuestion"
      class="border-t border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 p-6 max-h-64 overflow-y-auto"
    >
      <div class="flex items-start justify-between mb-4">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
          {{ selectedQuestion.qstLabel }}
        </h3>
        <button
          @click="selectedQuestion = null"
          class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
      </div>

      <div class="space-y-3 text-sm">
        <div v-if="selectedQuestion.qstStem">
          <span class="font-medium text-gray-700 dark:text-gray-300">Question Text:</span>
          <p class="text-gray-600 dark:text-gray-400 mt-1">{{ selectedQuestion.qstStem }}</p>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div>
            <span class="font-medium text-gray-700 dark:text-gray-300">Question Number:</span>
            <p class="text-gray-600 dark:text-gray-400">{{ selectedQuestion.qstNumber }}</p>
          </div>
          <div>
            <span class="font-medium text-gray-700 dark:text-gray-300">Data Column:</span>
            <p class="text-gray-600 dark:text-gray-400">{{ selectedQuestion.dataFileCol }}</p>
          </div>
          <div>
            <span class="font-medium text-gray-700 dark:text-gray-300">Variable Type:</span>
            <p class="text-gray-600 dark:text-gray-400">
              {{ selectedQuestion.variableType === 0 ? 'Dependent' : 'Independent' }}
            </p>
          </div>
          <div v-if="selectedQuestion.missValue1 || selectedQuestion.missValue2 || selectedQuestion.missValue3">
            <span class="font-medium text-gray-700 dark:text-gray-300">Missing Values:</span>
            <p class="text-gray-600 dark:text-gray-400">
              {{ [selectedQuestion.missValue1, selectedQuestion.missValue2, selectedQuestion.missValue3].filter(v => v).join(', ') }}
            </p>
          </div>
        </div>

        <div v-if="selectedQuestion.questionNotes">
          <span class="font-medium text-gray-700 dark:text-gray-300">Notes:</span>
          <p class="text-gray-600 dark:text-gray-400 mt-1">{{ selectedQuestion.questionNotes }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import axios from 'axios'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  }
})

const questions = ref([])
const loading = ref(true)
const error = ref(null)
const selectedQuestion = ref(null)
const expandedBlocks = ref(new Set())

// Organize questions into blocks and standalone questions
const organizedQuestions = computed(() => {
  const result = []
  const blocks = new Map()

  // First pass: identify blocks and their questions
  for (const question of questions.value) {
    if (question.blkQstStatus === 1) {
      // First question of a block
      const blockId = question.qstNumber // Use question number as block identifier
      blocks.set(blockId, {
        firstQuestion: question,
        questions: [question]
      })
    } else if (question.blkQstStatus === 2) {
      // Member of a block - find the block by looking backwards
      let blockId = null
      for (let i = questions.value.indexOf(question) - 1; i >= 0; i--) {
        if (questions.value[i].blkQstStatus === 1) {
          blockId = questions.value[i].qstNumber
          break
        }
      }
      if (blockId && blocks.has(blockId)) {
        blocks.get(blockId).questions.push(question)
      }
    }
  }

  // Second pass: build the display list
  for (const question of questions.value) {
    if (question.blkQstStatus === 1) {
      // Block start
      const blockId = question.qstNumber
      const block = blocks.get(blockId)
      const isExpanded = expandedBlocks.value.has(blockId)

      result.push({
        question,
        isBlockStart: true,
        blockId,
        blockSize: block.questions.length,
        indent: false,
        inCollapsedBlock: false
      })

      // Add block questions
      for (const blockQuestion of block.questions) {
        result.push({
          question: blockQuestion,
          isBlockStart: false,
          blockId,
          indent: true,
          inCollapsedBlock: !isExpanded
        })
      }
    } else if (question.blkQstStatus !== 2) {
      // Standalone question (not part of a block)
      result.push({
        question,
        isBlockStart: false,
        blockId: null,
        indent: false,
        inCollapsedBlock: false
      })
    }
  }

  return result
})

const toggleBlock = (blockId) => {
  if (expandedBlocks.value.has(blockId)) {
    expandedBlocks.value.delete(blockId)
  } else {
    expandedBlocks.value.add(blockId)
  }
  // Force reactivity
  expandedBlocks.value = new Set(expandedBlocks.value)
}

const selectQuestion = (question) => {
  selectedQuestion.value = question
}

const loadQuestions = async () => {
  loading.value = true
  error.value = null
  try {
    const response = await axios.get(`http://localhost:5107/api/surveys/${props.surveyId}/questions-list`)
    questions.value = response.data || []
  } catch (err) {
    error.value = 'Failed to load questions: ' + (err.response?.data?.message || err.message)
    console.error('Error loading questions:', err)
  } finally {
    loading.value = false
  }
}

// Watch for surveyId changes (important when using keep-alive)
watch(() => props.surveyId, (newSurveyId, oldSurveyId) => {
  if (newSurveyId && newSurveyId !== oldSurveyId) {
    selectedQuestion.value = null
    loadQuestions()
  }
})

onMounted(() => {
  loadQuestions()
})
</script>
