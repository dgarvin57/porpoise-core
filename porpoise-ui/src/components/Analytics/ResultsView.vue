<template>
  <div class="h-full flex">
    <!-- Question List Sidebar -->
    <aside v-if="!hideSidebar" class="w-80 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col">
      <QuestionListSelector
        :surveyId="surveyId"
        selectionMode="single"
        :showNotesIcons="true"
        :wrapStandaloneQuestions="true"
        :selectedQuestionId="selectedQuestion?.id"
        :initialExpandedBlocks="initialExpandedBlocks"
        @question-selected="selectQuestion"
        @expanded-blocks-changed="handleExpandedBlocksChanged"
      />
    </aside>

    <!-- Metric Definitions Modal -->
    <MetricDefinitionsModal :show="showMetricDefinitions" @close="showMetricDefinitions = false" />

    <!-- Main Content Area -->
    <div class="flex-1 flex flex-col" style="min-height: 0;">
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

      <!-- Question Results - CLEAN SIMPLE SCROLLING -->
      <template v-else>
        <div class="flex-1 overflow-y-scroll overflow-x-hidden bg-gray-50 dark:bg-gray-900 [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-gray-100 dark:[&::-webkit-scrollbar-track]:bg-gray-800 [&::-webkit-scrollbar-thumb]:bg-gray-300 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb:hover]:bg-gray-400 dark:[&::-webkit-scrollbar-thumb:hover]:bg-gray-500">
          <div class="p-6 pb-32">
            <!-- Chart Header -->
            <div class="flex items-center justify-between mb-6">
                <h3 class="text-lg font-medium text-gray-900 dark:text-white">
                  Frequency Distribution
                </h3>
                <button
                  @click="analyzeCrosstab"
                  class="px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors flex items-center gap-2 flex-shrink-0"
                  title="Analyze this question in crosstab with another variable"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                  </svg>
                  Analyze in Crosstab
                </button>
              </div>

              <!-- Question Chart -->
              <QuestionChart :question="selectedQuestion" />
              
              <!-- Action Buttons -->
              <div class="mt-6 pt-4 border-t border-gray-200 dark:border-gray-700 flex items-center gap-3">
                <Button
                  @click="showAIModal = true"
                  variant="ghost"
                  size="md"
                  class="relative"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                  </svg>
                  <span class="text-base font-medium glow-intense">AI Analysis</span>
                  <span v-if="aiAnalysis === ''" class="absolute -top-1 -right-1 flex h-3 w-3">
                    <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75"></span>
                    <span class="relative inline-flex rounded-full h-3 w-3 bg-blue-500 shadow-lg shadow-blue-500/75"></span>
                  </span>
                </Button>
                <Button
                  @click="showUnderstanding = true"
                  variant="ghost"
                  size="md"
                >
                  <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
                  </svg>
                  <span class="text-base font-medium">Understanding Results</span>
                </Button>
              </div>
            </div>
          </div>

        <!-- AI Analysis Modal -->
        <div v-if="showAIModal" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showAIModal = false">
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
              <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
                <div class="relative flex items-center justify-between">
                  <div class="flex items-center gap-2">
                    <svg class="w-5 h-5 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                    </svg>
                    <h3 class="text-lg font-semibold text-gray-900 dark:text-white">AI Analysis</h3>
                  </div>
                  <div class="absolute left-1/2 transform -translate-x-1/2">
                    <span class="text-base font-semibold text-blue-600 dark:text-blue-400 whitespace-nowrap">
                      {{ selectedQuestion?.label }}
                    </span>
                  </div>
                  <CloseButton @click="showAIModal = false" />
                </div>
              </div>
              <div class="px-6 py-4">
                <!-- Loading State -->
                <div v-if="loadingAnalysis" class="flex flex-col items-center justify-center py-8">
                  <svg class="animate-spin h-10 w-10 text-blue-600 dark:text-blue-400 mb-4" fill="none" viewBox="0 0 24 24">
                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  <p class="text-sm text-gray-600 dark:text-gray-400">Generating AI analysis...</p>
                </div>
                
                <!-- Analysis Content -->
                <div v-else-if="aiAnalysis" class="space-y-4">
                  <div v-for="(section, idx) in parseAIAnalysis(aiAnalysis)" :key="idx">
                    <h4 v-if="section.heading" class="font-semibold text-gray-900 dark:text-white mb-2 flex items-center gap-2">
                      <span class="w-1 h-5 bg-blue-500 rounded"></span>
                      {{ section.heading }}
                    </h4>
                    <p class="text-sm text-gray-700 dark:text-gray-300 leading-relaxed" :class="section.heading ? 'ml-3' : ''">
                      {{ section.content }}
                    </p>
                  </div>
                  
                  <!-- Regenerate Option -->
                  <div class="pt-4 border-t border-gray-200 dark:border-gray-700">
                    <Button
                      @click="generateAIAnalysis"
                      variant="ghost"
                      size="sm"
                      :loading="loadingAnalysis"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                      </svg>
                      Regenerate Analysis
                    </Button>
                  </div>
                </div>
                
                <!-- Generate Prompt -->
                <div v-else class="py-8">
                  <div class="text-center mb-6">
                    <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-blue-100 dark:bg-blue-900/30 mb-4">
                      <svg class="w-8 h-8 text-blue-600 dark:text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                      </svg>
                    </div>
                    <h4 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">Generate AI Analysis</h4>
                    <p class="text-sm text-gray-600 dark:text-gray-400 max-w-md mx-auto">
                      Our AI will analyze your question results and provide insights including an overview of the distribution, key findings, patterns, and actionable recommendations.
                    </p>
                  </div>
                  <div class="flex justify-center">
                    <Button
                      @click="generateAIAnalysis"
                      :loading="loadingAnalysis"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                      </svg>
                      Generate Analysis
                    </Button>
                  </div>
                </div>
              </div>
              <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 flex justify-end border-t border-gray-200 dark:border-gray-700">
                <Button @click="showAIModal = false">
                  Close
                </Button>
              </div>
            </div>
          </div>

          <!-- Understanding Results Modal -->
          <div v-if="showUnderstanding" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showUnderstanding = false">
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
              <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
                <div class="flex items-center justify-between">
                  <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Understanding Results</h3>
                  <CloseButton @click="showUnderstanding = false" />
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
                      <p class="text-sm text-gray-700 dark:text-gray-300">Click on any question in the sidebar to view its complete response distribution, statistics, and chart. Use the metrics at the top to quickly understand response patterns.</p>
                    </div>
                  </div>
                </div>
                
                <!-- What is Results View -->
                <div>
                  <h4 class="font-semibold text-gray-900 dark:text-white mb-2">What is the Results View?</h4>
                  <p class="text-sm text-gray-700 dark:text-gray-300">The Results View displays the complete distribution of responses for a single survey question. It shows how many respondents selected each answer choice, along with statistical measures that summarize the overall response pattern.</p>
                </div>
                
                <!-- Key Metrics -->
                <div>
                  <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Key Metrics</h4>
                  <ul class="list-disc list-inside space-y-1 ml-2 text-sm text-gray-700 dark:text-gray-300">
                    <li><strong>Total N:</strong> The total number of valid responses to this question</li>
                    <li><strong>Sampling Error:</strong> The margin of error for the results (±%)</li>
                    <li><strong>Cumulative %:</strong> Running total of percentages from top to bottom (optional column)</li>
                    <li><strong>Inverse Cumulative %:</strong> Running total of percentages from bottom to top (optional column)</li>
                  </ul>
                </div>
                
                <!-- Understanding the Chart -->
                <div>
                  <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Understanding the Chart</h4>
                  <ul class="list-disc list-inside space-y-1 ml-2 text-sm text-gray-700 dark:text-gray-300">
                    <li><strong>Frequency Distribution:</strong> Shows the count and percentage for each response option</li>
                    <li><strong>Visual Comparison:</strong> Bar heights make it easy to compare popularity of different responses</li>
                    <li><strong>Response Labels:</strong> Each bar is labeled with the response text and percentage</li>
                  </ul>
                </div>
                
                <!-- Available Actions -->
                <div>
                  <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Available Actions</h4>
                  <ul class="list-disc list-inside space-y-1 ml-2 text-sm text-gray-700 dark:text-gray-300">
                    <li><strong>AI Analysis:</strong> Get AI-powered insights about response patterns, notable findings, and recommendations</li>
                    <li><strong>Analyze in Crosstab:</strong> See how responses to this question vary across different demographic groups or other questions</li>
                    <li><strong>Metric Definitions:</strong> Click the ? icon next to metrics to see detailed explanations</li>
                  </ul>
                </div>
              </div>
              <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 flex justify-end border-t border-gray-200 dark:border-gray-700">
                <Button @click="showUnderstanding = false">
                  Close
                </Button>
              </div>
            </div>
          </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, nextTick } from 'vue'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import QuestionChart from './QuestionChart.vue'
import QuestionListSelector from './QuestionListSelector.vue'
import Button from '../common/Button.vue'
import CloseButton from '../common/CloseButton.vue'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  hideSidebar: {
    type: Boolean,
    default: false
  },
  hideTable: {
    type: Boolean,
    default: false
  },
  preselectedQuestionId: {
    type: String,
    default: null
  },
  initialQuestionId: {
    type: String,
    default: null
  },
  initialExpandedBlocks: {
    type: Array,
    default: () => []
  },
  initialColumnMode: {
    type: String,
    default: 'totalN'
  },
  initialInfoExpanded: {
    type: Boolean,
    default: false
  },
  initialInfoTab: {
    type: String,
    default: 'question'
  }
})

const emit = defineEmits(['question-selected', 'expanded-blocks-changed', 'column-mode-changed', 'info-expanded-changed', 'info-tab-changed', 'analyze-crosstab'])

const selectedQuestion = ref(null)
const columnMode = ref('totalN')
const questions = ref([]) // Still needed for loading initial question

// Metric definitions modal state
const showMetricDefinitions = ref(false)

// Info panel state
const infoExpanded = ref(false)
const activeInfoTab = ref('question')
const infoTabs = [
  { id: 'question', label: 'Question Stem' },
  { id: 'block', label: 'Block Stem' }
]

// AI Analysis and Understanding modals
const showAIModal = ref(false)
const showUnderstanding = ref(false)
const aiAnalysis = ref('')
const loadingAnalysis = ref(false)

// Get block stem from first question in the block
const blockStemForQuestion = computed(() => {
  if (!selectedQuestion.value) return ''
  
  // Convert blkQstStatus to number for comparison (may come as string from API)
  const status = Number(selectedQuestion.value.blkQstStatus)
  
  // If this is the first question in a block, return its blkStem
  if (status === 1) {
    return selectedQuestion.value.blkStem || ''
  }
  
  // If this is a continuation question, find the first question in this block
  if (status === 2 && selectedQuestion.value.blkLabel) {
    const firstQuestionInBlock = questions.value.find(q => 
      q.blkLabel === selectedQuestion.value.blkLabel && Number(q.blkQstStatus) === 1
    )
    
    if (firstQuestionInBlock) {
      return firstQuestionInBlock.blkStem || ''
    }
  }
  
  return ''
})

// Column mode configuration
const columnModeConfig = computed(() => {
  const modes = {
    totalN: { label: 'Total N', header: 'N', showColumn: true },
    cumulative: { label: 'Show Cumulative', header: 'Cum %', showColumn: true },
    inverseCumulative: { label: 'Show Inverse Cumulative', header: 'Inv Cum %', showColumn: true },
    samplingError: { label: 'Sampling Error', header: '+/- %', showColumn: true },
    blank: { label: 'Leave Blank', header: '', showColumn: false }
  }
  return modes[columnMode.value] || modes.totalN
})

// Computed responses with additional calculated columns
const computedResponses = computed(() => {
  if (!selectedQuestion.value?.responses) return []
  
  const responses = selectedQuestion.value.responses
  let cumulativePercent = 0
  let inverseCumulativePercent = 100
  
  // Use unweighted total for sampling error (more accurate statistically)
  const unweightedTotal = selectedQuestion.value.totalCasesUnweighted || selectedQuestion.value.totalCases
  
  return responses.map((response, index) => {
    cumulativePercent += response.percentage
    const currentInverse = inverseCumulativePercent
    inverseCumulativePercent -= response.percentage
    
    // Calculate sampling error using UNWEIGHTED data: sqrt((p * q) / n) * 1.96
    // Where p is percentage, q is (100 - p), and 1.96 is Z-score for 95% confidence
    // Use unweighted count for more accurate sampling error calculation
    const unweightedCount = response.countUnweighted || response.count
    const unweightedPercent = unweightedTotal > 0 ? (unweightedCount / unweightedTotal) * 100 : 0
    const pVar = unweightedPercent
    const qVar = 100 - pVar
    const samplingError = unweightedTotal > 0
      ? Math.sqrt((pVar * qVar) / unweightedTotal) * 1.96
      : 0
    
    return {
      ...response,
      cumulative: cumulativePercent,
      inverseCumulative: currentInverse,
      samplingError: samplingError
    }
  })
})

function selectQuestion(question) {
  
  // Question from QuestionListSelector only has metadata, not responses
  // Find the full question data from questions.value which has responses loaded
  const fullQuestion = questions.value.find(q => q.id === question.id)
  
  if (fullQuestion) {
    selectedQuestion.value = fullQuestion
  } else {
    // Fallback to partial data if not found
    selectedQuestion.value = question
  }
  
  // Reset AI analysis when question changes
  aiAnalysis.value = ''
  
  // Emit the question ID to parent for state management
  emit('question-selected', question.id)
}

function analyzeCrosstab() {
  if (selectedQuestion.value) {
    emit('analyze-crosstab', selectedQuestion.value)
  }
}

function handleExpandedBlocksChanged(expandedBlockIds) {
  // Forward the event to parent
  emit('expanded-blocks-changed', expandedBlockIds)
}

// Simplified - just load questions for block stem lookup and initializing selected question
async function loadQuestions() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/${props.surveyId}/questions`)
    questions.value = response.data
    
    // Restore column mode from prop
    if (props.initialColumnMode) {
      columnMode.value = props.initialColumnMode
    }
    
    // Restore info panel state from props
    infoExpanded.value = props.initialInfoExpanded
    activeInfoTab.value = props.initialInfoTab
    
    // Restore selected question only if initialQuestionId provided
    if (props.initialQuestionId && questions.value.length > 0) {
      const savedQuestion = questions.value.find(q => q.id === props.initialQuestionId)
      if (savedQuestion) {
        selectedQuestion.value = savedQuestion
      }
    }
  } catch (err) {
    console.error('Error loading questions:', err)
  }
}

// Watch for initialQuestionId changes (e.g., browser back/forward, or synced from crosstab)
watch(() => props.initialQuestionId, (newQuestionId, oldQuestionId) => {
  // If null/undefined, clear the selection (happens when switching surveys)
  if (!newQuestionId) {
    selectedQuestion.value = null
    return
  }
  
  // Always clear and reset to ensure clean state
  if (questions.value.length > 0) {
    const question = questions.value.find(q => q.id === newQuestionId)
    if (question) {
      // Always update, even if same ID, to ensure clean state
      selectedQuestion.value = null  // Clear first
      nextTick(() => {
        selectedQuestion.value = question  // Then set new selection
      })
    }
  }
})

// Watch for preselectedQuestionId changes (for split view)
watch(() => props.preselectedQuestionId, (newQuestionId) => {
  if (newQuestionId && questions.value.length > 0) {
    const question = questions.value.find(q => q.id === newQuestionId)
    if (question) {
      selectedQuestion.value = question
      
      // If question has a blockId, ensure that block is expanded
      if (question.blockId && expandedBlocks.value) {
        if (!expandedBlocks.value.includes(question.blockId)) {
          expandedBlocks.value = [...expandedBlocks.value, question.blockId]
        }
      }
    }
  }
}, { immediate: true, deep: true })

// Watch for questions being loaded (to select preselected question)
watch(questions, (newQuestions) => {
  if (newQuestions.length > 0 && props.preselectedQuestionId && !selectedQuestion.value) {
    const question = newQuestions.find(q => q.id === props.preselectedQuestionId)
    if (question) {
      selectedQuestion.value = question
      
      // If question has a blockId, ensure that block is expanded
      if (question.blockId && expandedBlocks.value) {
        if (!expandedBlocks.value.includes(question.blockId)) {
          expandedBlocks.value = [...expandedBlocks.value, question.blockId]
        }
      }
    }
  }
})

// Watch info panel state changes
watch(infoExpanded, (newValue) => {
  emit('info-expanded-changed', newValue)
})

watch(activeInfoTab, (newValue) => {
  emit('info-tab-changed', newValue)
})

// Parse AI Analysis
const parseAIAnalysis = (text) => {
  if (!text) return []
  
  const sections = []
  const lines = text.split('\n')
  let currentSection = null
  
  for (const line of lines) {
    const trimmedLine = line.trim()
    
    // Check if line is a heading (starts with ##)
    if (trimmedLine.startsWith('## ')) {
      // Save previous section if it exists
      if (currentSection) {
        sections.push(currentSection)
      }
      // Start new section
      currentSection = {
        heading: trimmedLine.substring(3).trim(),
        content: ''
      }
    } else if (trimmedLine && currentSection) {
      // Add content to current section
      if (currentSection.content) {
        currentSection.content += ' '
      }
      currentSection.content += trimmedLine
    } else if (trimmedLine && !currentSection) {
      // Content without a heading (shouldn't happen with new format, but handle gracefully)
      sections.push({
        heading: null,
        content: trimmedLine
      })
    }
  }
  
  // Don't forget the last section
  if (currentSection) {
    sections.push(currentSection)
  }
  
  return sections
}

// Generate AI Analysis
const generateAIAnalysis = async () => {
  if (!selectedQuestion.value) return
  
  loadingAnalysis.value = true
  try {
    // Debug: Log the question object to see what properties it has
    console.log('selectedQuestion.value:', selectedQuestion.value)
    console.log('Label:', selectedQuestion.value.Label)
    console.log('label:', selectedQuestion.value.label)
    console.log('qstLabel:', selectedQuestion.value.qstLabel)
    
    // Prepare context for AI - send data from frontend like CrosstabView does
    const context = {
      questionLabel: selectedQuestion.value.Label || selectedQuestion.value.label || selectedQuestion.value.qstLabel,
      totalN: selectedQuestion.value.totalCases,
      responses: selectedQuestion.value.responses?.map(r => ({
        label: r.label,
        frequency: r.count || 0,
        percent: r.percentage || 0
      })) || []
    }
    
    console.log('Context being sent:', context)
    
    const response = await axios.post(
      `${API_BASE_URL}/api/survey-analysis/${props.surveyId}/analyze-question`,
      context
    )
    
    aiAnalysis.value = response.data.analysis
  } catch (error) {
    console.error('Error generating AI analysis:', error)
    aiAnalysis.value = 'Unable to generate analysis at this time. The results above show the key findings from your question.'
  } finally {
    loadingAnalysis.value = false
  }
}

// Watch for surveyId changes (important when using keep-alive)
watch(() => props.surveyId, (newSurveyId, oldSurveyId) => {
  if (newSurveyId && newSurveyId !== oldSurveyId) {
    loadQuestions()
  }
})

onMounted(() => {
  loadQuestions()
})
</script>

<script>
import MetricDefinitionsModal from '../MetricDefinitionsModal.vue'

export default {
  components: {
    MetricDefinitionsModal
  }
}
</script>

<style scoped>
.glow-subtle {
  filter: drop-shadow(0 0 20px rgba(255, 255, 255, 0.6)) drop-shadow(0 0 50px rgba(147, 197, 253, 0.4));
}

.glow-intense {
  filter: drop-shadow(0 0 30px rgba(255, 255, 255, 0.8)) drop-shadow(0 0 60px rgba(147, 197, 253, 0.6));
}
</style>
