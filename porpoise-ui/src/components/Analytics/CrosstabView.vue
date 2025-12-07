<template>
  <div class="h-full flex">
    <!-- Question List Sidebar -->
    <aside v-if="!hideSidebar" class="w-96 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col">
      <QuestionListSelector
        :surveyId="surveyId"
        selectionMode="crosstab"
        :initialFirstSelection="firstQuestion"
        :initialSecondSelection="secondQuestion"
        @crosstab-selection="handleCrosstabSelection"
      />
    </aside>

    <!-- Crosstab Results -->
    <div class="flex-1 flex flex-col bg-gray-50 dark:bg-gray-900">
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
        <div class="sticky top-0 z-10 border-b border-gray-200 dark:border-gray-700">
          <div class="bg-white dark:bg-gray-800 py-3 px-6">
            <div class="relative flex items-center justify-between">
              <div class="flex items-center space-x-3 min-w-0">
                <h2 class="text-lg font-semibold text-gray-900 dark:text-white">
                  {{ crosstabData.firstQuestion.label }}&nbsp;&nbsp;<span class="text-gray-500 dark:text-gray-400 font-normal">by</span>&nbsp;&nbsp;{{ crosstabData.secondQuestion.label }}
                </h2>
              </div>
              <!-- Fixed position CROSSTAB title -->
              <div class="absolute left-[45%]">
                <h1 class="text-xl font-bold text-blue-600 dark:text-blue-400 text-left">
                  CROSSTAB
                </h1>
              </div>
              <div class="flex items-center space-x-3 flex-shrink-0">
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
        </div>

        <!-- Scrollable Content -->
        <div class="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900 p-6">
        <div class="max-w-5xl mx-auto space-y-6">
        <!-- Table -->
        <div class="bg-white dark:bg-gray-800 rounded-lg border border-gray-200 dark:border-gray-700 overflow-hidden">
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
                    {{ segment.value.toFixed(1) }}%
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Legend and Action Buttons -->
          <div class="mt-6 flex items-center justify-between">
            <div class="flex items-center gap-3">
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
                <span v-if="!aiAnalysis" class="absolute -top-1 -right-1 flex h-3 w-3">
                  <span class="animate-ping absolute inline-flex h-full w-full rounded-full bg-blue-400 opacity-75"></span>
                  <span class="relative inline-flex rounded-full h-3 w-3 bg-blue-500 shadow-lg shadow-blue-500/75"></span>
                </span>
              </Button>
              <Button
                @click="showExplanation = true"
                variant="ghost"
                size="md"
              >
                <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-8-3a1 1 0 00-.867.5 1 1 0 11-1.731-1A3 3 0 0113 8a3.001 3.001 0 01-2 2.83V11a1 1 0 11-2 0v-1a1 1 0 011-1 1 1 0 100-2zm0 8a1 1 0 100-2 1 1 0 000 2z" clip-rule="evenodd" />
                </svg>
                <span class="text-base font-medium">Understanding Crosstabs</span>
              </Button>
            </div>
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
        </div> <!-- Close max-w-6xl container -->
        </div> <!-- Close scrollable content -->
      </div> <!-- Close crosstab results -->
    </div>
    
    <!-- Explanation Modal (Understanding Crosstab) -->
    <div v-if="showExplanation" class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4" @click.self="showExplanation = false">
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl max-w-3xl w-full max-h-[85vh] overflow-y-auto">
        <div class="sticky top-0 bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 px-6 py-4">
          <div class="flex items-center justify-between">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white">Understanding Crosstabs</h3>
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
                <p class="text-sm text-gray-700 dark:text-gray-300">{{ quickTipText }}</p>
              </div>
            </div>
          </div>
          
          <!-- What is Crosstab Analysis -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">What is Crosstab Analysis?</h4>
            <p class="text-sm text-gray-700 dark:text-gray-300">Crosstab (cross-tabulation) analysis examines the relationship between two categorical variables by displaying their frequencies in a table format. This helps identify patterns and correlations between different survey questions.</p>
          </div>
          
          <!-- Statistical Measures -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Statistical Measures</h4>
            <ul class="list-disc list-inside space-y-2 ml-2 text-sm text-gray-700 dark:text-gray-300">
              <li><strong>Chi-Square (χ²):</strong> Tests whether a relationship exists (statistically significant vs random chance). If p&lt;0.05, the relationship is real, not due to chance.</li>
              <li><strong>Cramér's V:</strong> Measures the <em>strength</em> of the relationship (0=no association, 0.1=weak, 0.3=moderate, 0.5+=strong). A relationship can be statistically significant but weak.</li>
              <li><strong>Phi (φ):</strong> Similar to Cramér's V but specifically for 2×2 tables</li>
              <li><strong>Total N:</strong> Total number of valid responses analyzed</li>
            </ul>
            <div class="mt-3 p-3 bg-gray-50 dark:bg-gray-700/50 rounded border border-gray-200 dark:border-gray-600">
              <p class="text-sm text-gray-700 dark:text-gray-300"><strong>Understanding Both Together:</strong></p>
              <p class="text-sm text-gray-600 dark:text-gray-400 mt-1">Think of Chi-Square as asking "Is there smoke?" (Is the relationship real?) and Cramér's V as asking "How big is the fire?" (How strong is it?). A relationship can be real (significant Chi-Square) but weak (low Cramér's V), meaning other factors likely have stronger effects.</p>
            </div>
          </div>
          
          <!-- Graph Modes -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Graph Modes</h4>
            <ul class="list-disc list-inside space-y-1 ml-2 text-sm text-gray-700 dark:text-gray-300">
              <li><strong>Graph Index:</strong> Shows a single overall sentiment score (0-200 scale) for each group, where 100 is neutral, above 100 is positive, and below 100 is negative</li>
              <li><strong>Graph Pos/Neg Percent:</strong> Shows the actual percentage of people who responded positively (blue) vs. negatively (red) in each group. This makes it easy to see exactly how many people in each category were satisfied/dissatisfied</li>
            </ul>
          </div>
          
          <!-- Index Values Explained -->
          <div>
            <h4 class="font-semibold text-gray-900 dark:text-white mb-2">Index Values Explained</h4>
            <ul class="list-disc list-inside space-y-1 ml-2 text-sm text-gray-700 dark:text-gray-300">
              <li><strong>Positive Indexes:</strong> Percentage of favorable/desirable responses (e.g., "Strongly Agree", "Very Satisfied")</li>
              <li><strong>Negative Indexes:</strong> Percentage of unfavorable/undesirable responses (e.g., "Strongly Disagree", "Very Dissatisfied")</li>
              <li><strong>Overall Index:</strong> Combined measure where higher values indicate more positive sentiment</li>
            </ul>
          </div>
        </div>
        <div class="sticky bottom-0 bg-gray-50 dark:bg-gray-900 px-6 py-4 flex justify-end border-t border-gray-200 dark:border-gray-700">
          <Button @click="showExplanation = false">
            Close
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
                {{ crosstabData.firstQuestion.label }} <span class="font-normal">by</span> {{ crosstabData.secondQuestion.label }}
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
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z" />
                </svg>
              </div>
              <h4 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">Generate AI Analysis</h4>
              <p class="text-sm text-gray-600 dark:text-gray-400 max-w-md mx-auto">
                Our AI will analyze your crosstab data and provide insights including a summary, statistical interpretation, category comparisons, and actionable recommendations.
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
    
    <!-- Statistics Modal -->
    <CrosstabStatisticsModal :show="showStatistics" :data="crosstabData" @close="showStatistics = false" />
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
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
  },
  hideSidebar: {
    type: Boolean,
    default: false
  }
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
const showAIModal = ref(false)
const showStatistics = ref(false)
const aiAnalysis = ref('')
const loadingAnalysis = ref(false)

// Watch for prop changes and update internal state
watch(() => props.initialFirstQuestion, (newVal, oldVal) => {
  firstQuestion.value = newVal
}, { deep: true, immediate: true })

watch(() => props.initialSecondQuestion, (newVal, oldVal) => {
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

const quickTipText = computed(() => {
  if (!crosstabData.value) return ''
  
  const firstVar = firstQuestion.value?.label || 'first variable'
  const secondVar = secondQuestion.value?.label || 'second variable'
  
  if (graphMode.value === 'index') {
    return `The Graph Index shows a single overall sentiment score (0-200 scale) for each category of "${secondVar}". A score of 100 is neutral, above 100 is positive, and below 100 is negative. This helps you quickly compare how different groups feel about "${firstVar}".`
  } else {
    return `The Positive/Negative graph shows the percentage of positive (blue) vs. negative (red) responses for each category of "${secondVar}". Use this to quickly identify which groups are most positive or negative about "${firstVar}".`
  }
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
    return
  }

  loading.value = true
  error.value = null
  aiAnalysis.value = '' // Reset AI analysis when generating new crosstab

  try {
    const response = await axios.post(
      `${API_BASE_URL}/api/survey-analysis/${props.surveyId}/crosstab`,
      {
        firstQuestionId: firstQuestion.value.id,
        secondQuestionId: secondQuestion.value.id
      }
    )
    crosstabData.value = response.data
    
    // Reset AI analysis when new crosstab is generated
    aiAnalysis.value = ''
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
    // In crosstab "X by Y": X is dependent (measured outcome), Y is independent (grouping variable)
    const context = {
      dependentVariable: firstQuestion.value.label,  // Column variable - what we're measuring
      independentVariable: secondQuestion.value.label,  // Row variable - how we group/segment
      totalN: crosstabData.value.totalN,
      chiSquare: crosstabData.value.chiSquare,
      chiSquareSignificant: crosstabData.value.chiSquareSignificant,
      phi: crosstabData.value.phi,
      cramersV: crosstabData.value.cramersV,
      tableData: crosstabData.value.tableData,
      indexes: crosstabData.value.ivIndexes
    }
    
    const response = await axios.post(
      `${API_BASE_URL}/api/survey-analysis/${props.surveyId}/analyze-crosstab`,
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

function parseAIAnalysis(text) {
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

<style scoped>
.glow-subtle {
  @apply drop-shadow-[0_0_20px_rgba(255,255,255,0.6)] 
         drop-shadow-[0_0_50px_rgba(147,197,253,0.4)];
}

.glow-intense {
  @apply drop-shadow-[0_0_30px_rgba(255,255,255,0.8)] 
         drop-shadow-[0_0_70px_rgba(147,197,253,0.6)];
}
</style>
