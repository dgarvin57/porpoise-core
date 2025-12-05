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
            <div class="flex items-center space-x-3">
              <button
                v-if="!hideSidebar"
                @click="analyzeCrosstab"
                class="px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors flex items-center gap-2"
                title="Analyze this question in crosstab with another variable"
              >
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
                </svg>
                Analyze in Crosstab
              </button>
              <div class="flex items-center space-x-4 text-sm text-gray-600 dark:text-gray-400">
                <span><span class="font-medium">Index:</span> {{ selectedQuestion.index || '128' }}</span>
                <span>•</span>
                <span><span class="font-medium">CI:</span> +/- {{ selectedQuestion.samplingError?.toFixed(1) || '0.0' }}</span>
                <span>•</span>
                <span><span class="font-medium">Total N:</span> {{ selectedQuestion.totalCases }}</span>
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

        <!-- Combined Results and Chart Content -->
        <div class="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900 p-6 [&::-webkit-scrollbar]:w-2 [&::-webkit-scrollbar-track]:bg-gray-100 dark:[&::-webkit-scrollbar-track]:bg-gray-800 [&::-webkit-scrollbar-thumb]:bg-gray-300 dark:[&::-webkit-scrollbar-thumb]:bg-gray-600 [&::-webkit-scrollbar-thumb]:rounded-full [&::-webkit-scrollbar-thumb:hover]:bg-gray-400 dark:[&::-webkit-scrollbar-thumb:hover]:bg-gray-500">
          <div class="space-y-6">
            <!-- Question Info Panel -->
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
              <button
                @click="infoExpanded = !infoExpanded"
                class="w-full px-6 py-3 flex items-center justify-between bg-gray-50 dark:bg-gray-900 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors focus:outline-none"
              >
                <div class="flex items-center space-x-3">
                  <svg 
                    class="w-5 h-5 text-gray-500 dark:text-gray-400 transition-transform"
                    :class="{ 'rotate-90': infoExpanded }"
                    fill="currentColor" 
                    viewBox="0 0 20 20"
                  >
                    <path fill-rule="evenodd" d="M7.293 14.707a1 1 0 010-1.414L10.586 10 7.293 6.707a1 1 0 011.414-1.414l4 4a1 1 0 010 1.414l-4 4a1 1 0 01-1.414 0z" clip-rule="evenodd" />
                  </svg>
                  <svg class="w-5 h-5 text-blue-500 dark:text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                    <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd" />
                  </svg>
                  <span class="text-sm font-medium text-gray-900 dark:text-white">Question Information</span>
                </div>
                <div v-if="!infoExpanded && selectedQuestion?.text" class="flex-1 mx-4 text-left">
                  <span class="text-sm text-gray-500 dark:text-gray-400 italic line-clamp-1">
                    {{ selectedQuestion.text }}
                  </span>
                </div>
              </button>
              
              <div v-show="infoExpanded" class="border-t border-gray-200 dark:border-gray-700">
                <!-- Tabs -->
                <div class="flex border-b border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800">
                  <button
                    v-for="tab in infoTabs"
                    :key="tab.id"
                    @click="activeInfoTab = tab.id"
                    :disabled="tab.id === 'block' && !blockStemForQuestion"
                    class="px-4 py-2 text-sm font-medium border-b-2 transition-colors focus:outline-none flex items-center space-x-2"
                    :class="[
                      activeInfoTab === tab.id
                        ? 'border-blue-500 text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20'
                        : 'border-transparent text-gray-600 dark:text-gray-400 bg-gray-50 dark:bg-gray-800 hover:text-gray-900 dark:hover:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 hover:border-gray-300 dark:hover:border-gray-600',
                      tab.id === 'block' && !blockStemForQuestion ? 'opacity-50 cursor-not-allowed' : ''
                    ]"
                  >
                    <!-- Tab Icons -->
                    <svg v-if="tab.id === 'question'" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    <svg v-else-if="tab.id === 'block'" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 12h8m-8 4h5" />
                    </svg>
                    <svg v-else-if="tab.id === 'questionNotes'" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    <svg v-else-if="tab.id === 'surveyNotes'" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
                    </svg>
                    
                    <span>{{ tab.label }}</span>
                    
                    <!-- Indicator dot for notes with content -->
                    <svg
                      v-if="(tab.id === 'questionNotes' && selectedQuestion?.questionNotes) || (tab.id === 'surveyNotes' && surveyNotes)"
                      class="w-4 h-4 text-blue-500 dark:text-blue-400"
                      fill="currentColor"
                      viewBox="0 0 20 20"
                      :title="tab.id === 'questionNotes' ? 'Has question notes' : 'Has survey notes'"
                    >
                      <circle cx="10" cy="10" r="4" />
                    </svg>
                  </button>
                </div>
                
                <!-- Tab Content -->
                <div class="p-6">
                  <!-- Question Stem (Read-only) -->
                  <div v-if="activeInfoTab === 'question'">
                    <p v-if="selectedQuestion?.text" class="text-sm text-gray-500 dark:text-gray-500 italic whitespace-pre-wrap leading-relaxed">{{ selectedQuestion.text }}</p>
                    <p v-else class="text-sm text-gray-400 dark:text-gray-600 italic">No question stem available</p>
                  </div>
                  
                  <!-- Block Stem (Read-only, from first question in block) -->
                  <div v-else-if="activeInfoTab === 'block'">
                    <p v-if="blockStemForQuestion" class="text-sm text-gray-500 dark:text-gray-500 italic whitespace-pre-wrap leading-relaxed">{{ blockStemForQuestion }}</p>
                    <p v-else class="text-sm text-gray-400 dark:text-gray-600 italic">No block stem available</p>
                  </div>
                  
                  <!-- Question Notes (Editable) -->
                  <div v-else-if="activeInfoTab === 'questionNotes'">
                    <div v-if="editingQuestionNotes">
                      <textarea
                        v-model="editedQuestionNotes"
                        class="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 resize-y min-h-[100px]"
                        placeholder="Enter question notes..."
                      ></textarea>
                      <div class="flex justify-end space-x-2 mt-2">
                        <button
                          @click="cancelQuestionNotesEdit"
                          class="px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                        >
                          Cancel
                        </button>
                        <button
                          @click="saveQuestionNotes"
                          class="px-3 py-1.5 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                        >
                          Save
                        </button>
                      </div>
                    </div>
                    <div v-else>
                      <p v-if="selectedQuestion?.questionNotes" class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">{{ selectedQuestion.questionNotes }}</p>
                      <p v-else class="text-sm text-gray-400 dark:text-gray-600 italic">No question notes</p>
                      <div class="mt-3 flex space-x-2">
                        <button
                          @click="startEditingQuestionNotes"
                          class="px-2.5 py-1 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 dark:bg-blue-600 dark:hover:bg-blue-500 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                        >
                          {{ selectedQuestion?.questionNotes ? 'Edit Notes' : 'Add Notes' }}
                        </button>
                        <button
                          v-if="selectedQuestion?.questionNotes"
                          @click="deleteQuestionNotes"
                          class="px-2.5 py-1 text-xs font-medium text-white bg-red-600 hover:bg-red-700 dark:bg-red-600 dark:hover:bg-red-500 rounded focus:outline-none focus:ring-2 focus:ring-red-500"
                        >
                          Delete Notes
                        </button>
                      </div>
                    </div>
                  </div>
                  
                  <!-- Survey Notes (Editable) -->
                  <div v-else-if="activeInfoTab === 'surveyNotes'">
                    <div v-if="editingSurveyNotes">
                      <textarea
                        v-model="editedSurveyNotes"
                        class="w-full px-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 resize-y min-h-[100px]"
                        placeholder="Enter survey notes..."
                      ></textarea>
                      <div class="flex justify-end space-x-2 mt-2">
                        <button
                          @click="cancelSurveyNotesEdit"
                          class="px-3 py-1.5 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                        >
                          Cancel
                        </button>
                        <button
                          @click="saveSurveyNotes"
                          class="px-3 py-1.5 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
                        >
                          Save
                        </button>
                      </div>
                    </div>
                    <div v-else>
                      <p v-if="surveyNotes" class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">{{ surveyNotes }}</p>
                      <p v-else class="text-sm text-gray-400 dark:text-gray-600 italic">No survey notes</p>
                      <div class="mt-3 flex space-x-2">
                        <button
                          @click="startEditingSurveyNotes"
                          class="px-2.5 py-1 text-xs font-medium text-white bg-blue-600 hover:bg-blue-700 dark:bg-blue-600 dark:hover:bg-blue-500 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                        >
                          {{ surveyNotes ? 'Edit Notes' : 'Add Notes' }}
                        </button>
                        <button
                          v-if="surveyNotes"
                          @click="deleteSurveyNotes"
                          class="px-2.5 py-1 text-xs font-medium text-white bg-red-600 hover:bg-red-700 dark:bg-red-600 dark:hover:bg-red-500 rounded focus:outline-none focus:ring-2 focus:ring-red-500"
                        >
                          Delete Notes
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Results Table -->
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
              <div class="px-6 py-3 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 flex items-center justify-between">
                <h3 class="text-base font-medium text-gray-900 dark:text-white">
                  Response Results
                </h3>
                <div class="relative">
                  <select
                    v-model="columnMode"
                    @change="emit('column-mode-changed', columnMode)"
                    class="text-sm border border-gray-300 dark:border-gray-600 rounded-md px-3 py-1.5 bg-white dark:bg-gray-700 text-gray-700 dark:text-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 cursor-pointer"
                  >
                    <option value="totalN">Total N</option>
                    <option value="cumulative">Show Cumulative</option>
                    <option value="inverseCumulative">Show Inverse Cumulative</option>
                    <option value="samplingError">Sampling Error</option>
                    <option value="blank">Leave Blank</option>
                  </select>
                </div>
              </div>
              <div class="overflow-x-auto">
                <table class="min-w-full">
                  <thead class="bg-gray-50 dark:bg-gray-700">
                    <tr>
                      <th class="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        #
                      </th>
                      <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Response
                      </th>
                      <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        %
                      </th>
                      <th class="px-6 py-3 text-center text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Index
                      </th>
                      <th v-if="columnModeConfig.showColumn" class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        {{ columnModeConfig.header }}
                      </th>
                    </tr>
                  </thead>
                  <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                    <tr v-for="(response, index) in computedResponses" :key="index" class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
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
                      <td v-if="columnMode === 'totalN'" class="px-6 py-2 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {{ response.count }}
                      </td>
                      <td v-else-if="columnMode === 'cumulative'" class="px-6 py-2 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {{ response.cumulative.toFixed(1) }}
                      </td>
                      <td v-else-if="columnMode === 'inverseCumulative'" class="px-6 py-2 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {{ response.inverseCumulative.toFixed(1) }}
                      </td>
                      <td v-else-if="columnMode === 'samplingError'" class="px-6 py-2 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {{ response.samplingError.toFixed(1) }}
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
import QuestionListSelector from './QuestionListSelector.vue'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  surveyNotes: {
    type: String,
    default: ''
  },
  hideSidebar: {
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

const emit = defineEmits(['question-selected', 'expanded-blocks-changed', 'column-mode-changed', 'info-expanded-changed', 'info-tab-changed', 'survey-notes-updated', 'analyze-crosstab'])

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
  { id: 'block', label: 'Block Stem' },
  { id: 'questionNotes', label: 'Question Notes' },
  { id: 'surveyNotes', label: 'Survey Notes' }
]

// Edit state for notes
const editingQuestionNotes = ref(false)
const editedQuestionNotes = ref('')
const editingSurveyNotes = ref(false)
const editedSurveyNotes = ref('')

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
  selectedQuestion.value = question
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

// Question Notes editing
function startEditingQuestionNotes() {
  editedQuestionNotes.value = selectedQuestion.value?.questionNotes || ''
  editingQuestionNotes.value = true
}

function cancelQuestionNotesEdit() {
  editingQuestionNotes.value = false
  editedQuestionNotes.value = ''
}

async function saveQuestionNotes() {
  try {
    await axios.patch(`http://localhost:5107/api/surveys/${props.surveyId}/questions/${selectedQuestion.value.id}`, {
      questionNotes: editedQuestionNotes.value
    })
    
    // Update the question in the questions array (to maintain order)
    const questionIndex = questions.value.findIndex(q => q.id === selectedQuestion.value.id)
    if (questionIndex !== -1) {
      questions.value[questionIndex].questionNotes = editedQuestionNotes.value
    }
    
    // Also update selected question reference
    if (selectedQuestion.value) {
      selectedQuestion.value.questionNotes = editedQuestionNotes.value
    }
    
    editingQuestionNotes.value = false
  } catch (err) {
    console.error('Error saving question notes:', err)
    alert('Failed to save question notes. Please try again.')
  }
}

async function deleteQuestionNotes() {
  if (!confirm('Are you sure you want to delete these question notes?')) {
    return
  }
  
  try {
    await axios.patch(`http://localhost:5107/api/surveys/${props.surveyId}/questions/${selectedQuestion.value.id}`, {
      questionNotes: ''
    })
    
    // Update the question in the questions array
    const questionIndex = questions.value.findIndex(q => q.id === selectedQuestion.value.id)
    if (questionIndex !== -1) {
      questions.value[questionIndex].questionNotes = ''
    }
    
    // Also update selected question reference
    if (selectedQuestion.value) {
      selectedQuestion.value.questionNotes = ''
    }
  } catch (err) {
    console.error('Error deleting question notes:', err)
    alert('Failed to delete question notes. Please try again.')
  }
}

// Survey Notes editing
function startEditingSurveyNotes() {
  editedSurveyNotes.value = props.surveyNotes || ''
  editingSurveyNotes.value = true
}

function cancelSurveyNotesEdit() {
  editingSurveyNotes.value = false
  editedSurveyNotes.value = ''
}

async function saveSurveyNotes() {
  try {
    await axios.patch(`http://localhost:5107/api/surveys/${props.surveyId}`, {
      surveyNotes: editedSurveyNotes.value
    })
    
    // Emit event to parent to update survey notes
    emit('survey-notes-updated', editedSurveyNotes.value)
    
    editingSurveyNotes.value = false
  } catch (err) {
    console.error('Error saving survey notes:', err)
    alert('Failed to save survey notes. Please try again.')
  }
}

async function deleteSurveyNotes() {
  if (!confirm('Are you sure you want to delete these survey notes?')) {
    return
  }
  
  try {
    await axios.patch(`http://localhost:5107/api/surveys/${props.surveyId}`, {
      surveyNotes: ''
    })
    
    // Emit event to parent to update survey notes
    emit('survey-notes-updated', '')
  } catch (err) {
    console.error('Error deleting survey notes:', err)
    alert('Failed to delete survey notes. Please try again.')
  }
}

// Simplified - just load questions for block stem lookup and initializing selected question
async function loadQuestions() {
  try {
    const response = await axios.get(`http://localhost:5107/api/surveys/${props.surveyId}/questions`)
    questions.value = response.data
    
    // Restore column mode from prop
    if (props.initialColumnMode) {
      columnMode.value = props.initialColumnMode
    }
    
    // Restore info panel state from props
    infoExpanded.value = props.initialInfoExpanded
    activeInfoTab.value = props.initialInfoTab
    
    // Restore selected question if initialQuestionId provided
    if (props.initialQuestionId && questions.value.length > 0) {
      const savedQuestion = questions.value.find(q => q.id === props.initialQuestionId)
      if (savedQuestion) {
        selectedQuestion.value = savedQuestion
      } else {
        // If saved question not found, select first
        selectedQuestion.value = questions.value[0]
      }
    } else if (questions.value.length > 0 && !selectedQuestion.value) {
      // Default to first question if none selected
      selectedQuestion.value = questions.value[0]
    }
  } catch (err) {
    console.error('Error loading questions:', err)
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

// Watch for preselectedQuestionId changes (for split view)
watch(() => props.preselectedQuestionId, (newQuestionId) => {
  console.log('ResultsView: preselectedQuestionId changed to:', newQuestionId)
  if (newQuestionId && questions.value.length > 0) {
    const question = questions.value.find(q => q.id === newQuestionId)
    console.log('ResultsView: Found question:', question)
    if (question) {
      selectedQuestion.value = question
      
      // If question has a blockId, ensure that block is expanded
      if (question.blockId && expandedBlocks.value) {
        if (!expandedBlocks.value.includes(question.blockId)) {
          expandedBlocks.value = [...expandedBlocks.value, question.blockId]
          console.log('ResultsView: Expanded block:', question.blockId)
        }
      }
    }
  }
}, { immediate: true, deep: true })

// Watch for questions being loaded (to select preselected question)
watch(questions, (newQuestions) => {
  if (newQuestions.length > 0 && props.preselectedQuestionId && !selectedQuestion.value) {
    const question = newQuestions.find(q => q.id === props.preselectedQuestionId)
    console.log('ResultsView: Questions loaded, selecting preselected question:', question)
    if (question) {
      selectedQuestion.value = question
      
      // If question has a blockId, ensure that block is expanded
      if (question.blockId && expandedBlocks.value) {
        if (!expandedBlocks.value.includes(question.blockId)) {
          expandedBlocks.value = [...expandedBlocks.value, question.blockId]
          console.log('ResultsView: Expanded block on load:', question.blockId)
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
