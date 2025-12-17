<template>
  <div class="h-full flex flex-col bg-gray-50 dark:bg-gray-900">
    <!-- Empty State - Only show when truly no question selected -->
    <div 
      v-if="!selectedQuestion"
      class="flex items-center justify-center h-full"
    >
      <div class="text-center">
        <!-- Phi symbol icon -->
        <div class="mx-auto h-16 w-16 flex items-center justify-center">
          <svg class="w-full h-full text-gray-400" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
            <circle cx="12" cy="12" r="5" />
            <line x1="12" y1="3" x2="12" y2="21" />
          </svg>
        </div>
        <h3 class="mt-2 text-lg font-medium text-gray-900 dark:text-white">No Question Selected</h3>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          Select a question from the list to view statistical significance
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
          @click="loadStatSigData"
          class="mt-4 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm"
        >
          Try Again
        </button>
      </div>
    </div>

    <!-- Independent Variable Warning -->
    <Transition name="fade" mode="out-in">
      <div 
        v-if="selectedQuestion && selectedQuestion.variableType === 1"
        key="iv-warning"
        class="flex items-center justify-center h-full"
      >
        <div class="text-center max-w-md p-6">
          <svg class="mx-auto h-12 w-12 text-yellow-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <h3 class="mt-4 text-lg font-medium text-gray-900 dark:text-white">Independent Variable Selected</h3>
          <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
            The question <span class="font-semibold">"{{ selectedQuestion.label || selectedQuestion.qstLabel }}"</span> that you selected is an independent variable type, which cannot be used when on the Statistical Significance tab.
          </p>
          <p class="mt-3 text-sm text-gray-600 dark:text-gray-400">
            Please choose another question that is a 
            <span class="inline-flex items-center gap-1">
              <svg class="w-4 h-4 text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
              </svg>
              <span class="font-semibold">dependent variable</span>
            </span> type.
          </p>
          <p class="mt-4 text-xs text-gray-500 dark:text-gray-500 italic">
            In the Statistical Significance tab, select a nominal DV by clicking a 
            <span class="inline-flex items-center gap-1">
              <svg class="w-3 h-3 text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
              </svg>
              blue
            </span> dependent variable question.
          </p>
        </div>
      </div>

      <!-- Skeleton Loader - Show when question selected but no data yet -->
      <div v-else-if="selectedQuestion && (!statSigData || statSigData.length === 0)" key="skeleton" class="h-full overflow-auto">
        <div class="pt-3 px-6 pb-6">
          <!-- Header Skeleton -->
          <div class="flex items-end justify-between mb-2 pb-2">
            <div class="space-y-2">
              <div class="h-5 bg-gray-200 dark:bg-gray-700 rounded w-48 animate-pulse"></div>
              <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-32 animate-pulse"></div>
            </div>
            <div class="flex items-center gap-3">
              <div class="h-8 w-8 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
              <div class="h-8 w-32 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
            </div>
          </div>

          <!-- Table Skeleton -->
          <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm">
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead class="bg-blue-50 dark:bg-gray-700">
                  <tr>
                    <th class="px-6 py-1.5 text-left">
                      <div class="h-3 bg-gray-300 dark:bg-gray-600 rounded w-12 animate-pulse"></div>
                    </th>
                    <th class="px-6 py-1.5 text-left">
                      <div class="h-3 bg-gray-300 dark:bg-gray-600 rounded w-24 animate-pulse"></div>
                    </th>
                    <th class="px-6 py-1.5 text-left">
                      <div class="h-3 bg-gray-300 dark:bg-gray-600 rounded w-20 animate-pulse"></div>
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  <tr v-for="i in 8" :key="i">
                    <td class="px-6 py-1">
                      <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-12 animate-pulse"></div>
                    </td>
                    <td class="px-6 py-1">
                      <div class="h-5 bg-gray-200 dark:bg-gray-700 rounded w-28 animate-pulse"></div>
                    </td>
                    <td class="px-6 py-1">
                      <div class="h-3 bg-gray-200 dark:bg-gray-700 rounded w-40 animate-pulse"></div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      <!-- Statistical Significance Results -->
      <div v-else-if="statSigData && statSigData.length > 0" key="results" class="h-full overflow-auto">
        <div class="pt-3 px-6 pb-6 flex justify-center">
          <div class="w-full max-w-[833px] mt-[10px]">
            <!-- Header with question label and buttons (matching Results tab) -->
            <div class="flex items-end justify-between mb-2 pb-2">
            <div>
              <h3 class="text-base font-semibold text-gray-900 dark:text-white">
                {{ selectedQuestion.label || selectedQuestion.qstLabel }}
              </h3>
              <div class="text-[10px] font-medium text-blue-600 dark:text-blue-400 uppercase tracking-wide">
                STATISTICAL SIGNIFICANCE
              </div>
            </div>
            <div class="flex items-center gap-3">
              <!-- Info Button -->
              <button 
                @click="showExplanation = true"
                class="inline-flex items-center gap-2 px-3 py-1 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors rounded-lg hover:bg-gray-100 dark:hover:bg-gray-700"
                title="Understanding Statistical Significance"
              >
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
                </svg>
              </button>
            </div>
          </div>

          <!-- Table Card -->
          <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden shadow-sm">
            <div class="overflow-x-auto">
              <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                <thead class="bg-blue-50 dark:bg-gray-700">
                  <tr>
                    <th 
                      scope="col"
                      class="px-6 py-1.5 text-left text-xs font-semibold text-gray-800 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-blue-100 dark:hover:bg-gray-600"
                      @click="sortBy('phi')"
                    >
                      <div class="flex items-center space-x-1">
                        <span>Phi</span>
                        <svg v-if="sortColumn === 'phi'" class="w-4 h-4" :class="sortDirection === 'asc' ? 'transform rotate-180' : ''" fill="currentColor" viewBox="0 0 20 20">
                          <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
                        </svg>
                      </div>
                    </th>
                    <th 
                      scope="col"
                      class="px-6 py-1.5 text-left text-xs font-semibold text-gray-800 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-blue-100 dark:hover:bg-gray-600"
                      @click="sortBy('significance')"
                    >
                      <div class="flex items-center space-x-1">
                        <span>Significance</span>
                        <svg v-if="sortColumn === 'significance'" class="w-4 h-4" :class="sortDirection === 'asc' ? 'transform rotate-180' : ''" fill="currentColor" viewBox="0 0 20 20">
                          <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
                        </svg>
                      </div>
                    </th>
                    <th 
                      scope="col"
                      class="px-6 py-1.5 text-left text-xs font-semibold text-gray-800 dark:text-gray-400 uppercase tracking-wider cursor-pointer hover:bg-blue-100 dark:hover:bg-gray-600"
                      @click="sortBy('question')"
                    >
                      <div class="flex items-center space-x-1">
                        <span>Question</span>
                        <svg v-if="sortColumn === 'question'" class="w-4 h-4" :class="sortDirection === 'asc' ? 'transform rotate-180' : ''" fill="currentColor" viewBox="0 0 20 20">
                          <path fill-rule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clip-rule="evenodd" />
                        </svg>
                      </div>
                    </th>
                  </tr>
                </thead>
                <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                  <tr 
                    v-for="(item, idx) in sortedStatSigData"
                    :key="item.id"
                    :class="[
                      'hover:bg-blue-50 dark:hover:bg-gray-700 cursor-pointer transition-colors',
                      activeQuestionId === item.id ? 'bg-blue-50 dark:bg-blue-900/30 border-l-4 border-blue-500' : (idx % 2 === 0 ? 'bg-white dark:bg-gray-800' : 'bg-gray-50 dark:bg-gray-700/30')
                    ]"
                    @click="navigateToQuestion(item.id)"
                  >
                    <td class="px-6 py-1 whitespace-nowrap text-xs font-medium text-gray-900 dark:text-white">
                      {{ item.phi.toFixed(3) }}
                    </td>
                    <td class="px-6 py-1 whitespace-nowrap text-xs">
                      <span 
                        :class="[
                          'inline-flex px-2 py-0 text-xs font-semibold rounded-full',
                          item.significance.includes('p<0.01') || item.significance.includes('p<.01')
                            ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
                            : item.significance.includes('p<0.05') || item.significance.includes('p<.05')
                            ? 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400'
                            : 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-400'
                        ]"
                      >
                        {{ item.significance }}
                      </span>
                    </td>
                    <td class="px-6 py-1 text-xs">
                      <div class="flex items-center gap-2">
                        <!-- Variable Type Icon -->
                        <svg 
                          class="w-3.5 h-3.5 flex-shrink-0 text-red-400"
                          fill="currentColor" 
                          viewBox="0 0 20 20"
                          title="Independent Variable"
                        >
                          <path fill-rule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clip-rule="evenodd" />
                        </svg>
                        <span class="text-blue-600 dark:text-blue-400 hover:underline">
                          {{ item.questionLabel }}
                        </span>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          </div>
        </div>
      </div>

      <!-- No Results State -->
      <div v-else key="no-results" class="flex items-center justify-center h-full">
        <div class="text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">No Significant Results</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            No statistically significant relationships found for this question.
          </p>
        </div>
      </div>
    </Transition>

    <!-- Understanding Modal -->
    <div v-if="showExplanation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showExplanation = false">
      <div class="bg-white dark:bg-gray-800 rounded-lg max-w-2xl w-full max-h-[80vh] overflow-y-auto shadow-xl">
        <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4 z-10">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Understanding Statistical Significance</h3>
            <CloseButton @click="showExplanation = false" />
          </div>
        </div>
        <div class="px-6 py-4 space-y-4">
          <div>
            <p class="text-sm text-gray-700 dark:text-gray-300"><strong>What is Statistical Significance?</strong></p>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
              Statistical significance testing helps determine whether the relationship between two survey questions is likely due to a real pattern in your data, or just random chance. This table shows all independent variables (demographics and classifiers) ranked by the strength of their relationship with your selected question.
            </p>
          </div>
          
          <div>
            <p class="text-sm text-gray-700 dark:text-gray-300"><strong>What is Phi (φ)?</strong></p>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
              <strong>Phi</strong> (pronounced "fie" or "fee") is a measure of association between two categorical variables. It ranges from 0 to 1:
            </p>
            <ul class="mt-2 ml-6 text-sm text-gray-600 dark:text-gray-400 list-disc space-y-1">
              <li><strong>0.0 to 0.1:</strong> Negligible relationship</li>
              <li><strong>0.1 to 0.3:</strong> Weak relationship</li>
              <li><strong>0.3 to 0.5:</strong> Moderate relationship</li>
              <li><strong>0.5 and above:</strong> Strong relationship</li>
            </ul>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
              Higher Phi values indicate stronger associations between variables.
            </p>
          </div>

          <div>
            <p class="text-sm text-gray-700 dark:text-gray-300"><strong>Significance Levels:</strong></p>
            <ul class="mt-2 ml-6 text-sm text-gray-600 dark:text-gray-400 list-disc space-y-1">
              <li><strong class="text-green-600 dark:text-green-400">Significant (p&lt;.01):</strong> Very strong evidence of a real relationship. Less than 1% chance this is random.</li>
              <li><strong class="text-blue-600 dark:text-blue-400">Significant (p&lt;.05):</strong> Strong evidence of a real relationship. Less than 5% chance this is random.</li>
              <li><strong class="text-gray-600 dark:text-gray-400">Not significant:</strong> Insufficient evidence to conclude a real relationship exists. The pattern might be due to chance.</li>
            </ul>
          </div>

          <div>
            <p class="text-sm text-gray-700 dark:text-gray-300"><strong>How to Use This Table:</strong></p>
            <ul class="mt-2 ml-6 text-sm text-gray-600 dark:text-gray-400 list-disc space-y-1">
              <li>Variables are sorted by Phi coefficient (strongest relationships at top)</li>
              <li>Click any row to view the detailed crosstab analysis</li>
              <li>Focus on variables with higher Phi values and significant p-values for meaningful insights</li>
              <li>Use this to identify which demographics or classifiers have the strongest relationship with your selected question</li>
            </ul>
          </div>

          <div>
            <p class="text-sm text-gray-700 dark:text-gray-300"><strong>Why Is This Important?</strong></p>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
              This analysis helps you quickly identify which demographic groups or segments show meaningfully different patterns in their responses. For example, if "Age" has a high Phi value and is significant, it means different age groups respond very differently to your question - making age an important variable to analyze further.
            </p>
          </div>
        </div>
        <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 border-t border-gray-200 dark:border-gray-700 flex justify-end">
          <Button @click="showExplanation = false">
            Close
          </Button>
        </div>
      </div>
    </div>

    <!-- AI Analysis Modal -->
    <AIAnalysisModal
      :show="showAIModal"
      :questionLabel="selectedQuestion?.label || selectedQuestion?.qstLabel || ''"
      :analysis="aiAnalysis"
      :loading="loadingAnalysis"
      context="Statistical Significance"
      @close="showAIModal = false"
      @generate="generateAIAnalysis"
      @regenerate="generateAIAnalysis"
    />
  </div>
</template>

<script setup>
import { ref, watch, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import Button from '../common/Button.vue'
import CloseButton from '../common/CloseButton.vue'
import AIAnalysisModal from './AIAnalysisModal.vue'

const router = useRouter()
const route = useRoute()

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  selectedQuestion: {
    type: Object,
    default: null
  },
  triggerAIModal: {
    type: Boolean,
    default: false
  }
})

// State
const loading = ref(false)
const error = ref(null)
const statSigData = ref([])
const sortColumn = ref('phi')
const sortDirection = ref('desc')
const showExplanation = ref(false)
const showAIModal = ref(false)
const aiAnalysis = ref('')
const loadingAnalysis = ref(false)

// Watch for parent triggering AI modal
watch(() => props.triggerAIModal, (newVal) => {
  if (newVal) {
    showAIModal.value = true
    emit('ai-modal-shown')
  }
})

// Track last clicked IV question for active row highlighting
const lastClickedIV = ref(null)

// Track the last question ID we loaded data for (to prevent unnecessary reloads)
const lastLoadedQuestionId = ref(null)

// Track active question from route (when returning from crosstab)
const activeQuestionId = computed(() => {
  // When returning from crosstab, secondQuestion will be the IV we visited
  return route.query.fromStatSig === 'true' && route.query.secondQuestion 
    ? route.query.secondQuestion 
    : lastClickedIV.value
})

// Define emits
const emit = defineEmits(['question-selected', 'ai-modal-shown'])

// Computed
const sortedStatSigData = computed(() => {
  if (!statSigData.value || statSigData.value.length === 0) return []
  
  const data = [...statSigData.value]
  
  data.sort((a, b) => {
    let aVal, bVal
    
    switch (sortColumn.value) {
      case 'phi':
        aVal = a.phi
        bVal = b.phi
        break
      case 'significance':
        aVal = a.significance
        bVal = b.significance
        break
      case 'question':
        aVal = a.questionLabel.toLowerCase()
        bVal = b.questionLabel.toLowerCase()
        break
      default:
        return 0
    }
    
    if (aVal < bVal) return sortDirection.value === 'asc' ? -1 : 1
    if (aVal > bVal) return sortDirection.value === 'asc' ? 1 : -1
    return 0
  })
  
  return data
})

// Methods
function sortBy(column) {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = column === 'phi' ? 'desc' : 'asc'
  }
}

function navigateToQuestion(ivQuestionId) {
  // TODO: Navigate to crosstab once the crosstab UX is improved
  // For now, just show a message
  const item = statSigData.value.find(d => d.id === ivQuestionId)
  if (item) {
    alert(`Crosstab view coming soon!\n\nYou clicked: ${item.questionLabel}\n\nThis will show a crosstab of "${props.selectedQuestion.label || props.selectedQuestion.qstLabel}" by "${item.questionLabel}".`)
  }
}

async function loadStatSigData() {
  if (!props.selectedQuestion) return
  
  // If we already have data for this question, don't reload
  if (lastLoadedQuestionId.value === props.selectedQuestion.id) {
    return
  }
  
  loading.value = true
  error.value = null
  
  try {
    const response = await axios.get(
      `${API_BASE_URL}/api/survey-analysis/${props.surveyId}/statistical-significance/${props.selectedQuestion.id}`
    )
    
    statSigData.value = response.data
    lastLoadedQuestionId.value = props.selectedQuestion.id
  } catch (err) {
    console.error('Error loading statistical significance:', err)
    error.value = err.response?.data?.message || err.message || 'Failed to load statistical significance data'
  } finally {
    loading.value = false
  }
}

async function generateAIAnalysis() {
  if (!props.selectedQuestion) return
  
  loadingAnalysis.value = true
  
  try {
    // Prepare context for AI
    const context = {
      questionLabel: props.selectedQuestion.label || props.selectedQuestion.qstLabel,
      statSigData: statSigData.value.map(item => ({
        variable: item.questionLabel,
        phi: item.phi,
        significance: item.significance
      }))
    }
    
    const response = await axios.post(
      `${API_BASE_URL}/api/survey-analysis/${props.surveyId}/analyze-statsig`,
      context
    )
    
    aiAnalysis.value = response.data.analysis
  } catch (err) {
    console.error('Error generating AI analysis:', err)
    aiAnalysis.value = 'Unable to generate analysis at this time. The statistical significance results above show which variables have the strongest relationships with your selected question.'
  } finally {
    loadingAnalysis.value = false
  }
}

// Watch for question changes
watch(() => props.selectedQuestion, (newQuestion, oldQuestion) => {
  if (newQuestion) {
    // Only reload data if the question ID actually changed
    const questionChanged = !oldQuestion || (oldQuestion.id !== newQuestion.id)
    
    if (questionChanged) {
      lastClickedIV.value = null
      loadStatSigData()
      aiAnalysis.value = '' // Reset AI analysis when question changes
    }
    // If question is the same, keep existing data and don't reload
  }
})

onMounted(() => {
  if (props.selectedQuestion) {
    loadStatSigData()
  }
})
</script>

<style scoped>
/* Fade transition for content switching */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 300ms ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.fade-enter-to,
.fade-leave-from {
  opacity: 1;
}
</style>
