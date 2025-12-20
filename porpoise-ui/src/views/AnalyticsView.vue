<template>
  <div class="fixed inset-0 top-12 bg-gray-50 dark:bg-gray-900">
    <!-- Sidebar Navigation (overlays when expanded) -->
    <aside 
      :class="[
        'fixed left-0 top-12 bottom-0 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-y-auto overflow-x-hidden transition-all duration-300 z-20',
        sidebarCollapsed ? 'w-14' : 'w-72 shadow-2xl'
      ]"
    >
      <!-- Collapse/Expand Button -->
      <div :class="['flex items-center', sidebarCollapsed ? 'justify-center p-3' : 'justify-start p-4']">
        <button
          @click="toggleSidebar"
          class="p-2 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 rounded-full transition-all duration-200 focus:outline-none"
          :title="sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
        >
          <svg 
            class="w-5 h-5 transition-transform duration-200" 
            fill="none" 
            stroke="currentColor" 
            viewBox="0 0 24 24" 
            stroke-width="2"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
      </div>

      <!-- Navigation Menu -->
      <nav class="px-2 pb-2">
        <div>
          <!-- Analytics Section -->
          <SidebarButton
            label="Analytics"
            icon-path="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
            :is-active="activeSection === 'results' || activeSection === 'crosstab' || activeSection === 'statsig' || activeSection === 'fullblock' || activeSection === 'matchingblocks' || activeSection === 'index' || activeSection === 'indexplus' || activeSection === 'profile' || activeSection === 'oneresponse'"
            :collapsed="sidebarCollapsed"
            @click="() => { activeSection = 'results'; sidebarCollapsed = true; }"
          />
        </div>

        <!-- Divider -->
        <div class="my-4 border-t border-gray-200 dark:border-gray-700"></div>

        <div>
          <!-- Questions Section -->
          <SidebarButton
            label="Questions"
            icon-path="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            :is-active="activeSection === 'questions'"
            :collapsed="sidebarCollapsed"
            @click="() => { activeSection = 'questions'; sidebarCollapsed = true; }"
          />

          <!-- Data View Section -->
          <SidebarButton
            label="Data View"
            icon-path="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z"
            :is-active="activeSection === 'dataview'"
            :collapsed="sidebarCollapsed"
            @click="() => { activeSection = 'dataview'; sidebarCollapsed = true; }"
          />

          <!-- Data Cleansing Section -->
          <SidebarButton
            label="Data Cleansing"
            icon-path="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            :is-active="activeSection === 'datacleansing'"
            :collapsed="sidebarCollapsed"
            @click="() => { activeSection = 'datacleansing'; sidebarCollapsed = true; }"
          />
        </div>

        <!-- Divider -->
        <div class="my-4 border-t border-gray-200 dark:border-gray-700"></div>

        <!-- Back to Projects -->
        <SidebarButton
          label="Back to Projects"
          icon-path="M10 19l-7-7m0 0l7-7m-7 7h18"
          :is-active="false"
          :collapsed="sidebarCollapsed"
          @click="backToProjects"
        />
      </nav>
    </aside>

    <!-- Main content area with left margin for collapsed sidebar -->
    <div class="fixed top-12 bottom-0 right-0 left-14 overflow-hidden">
      <!-- Content Area with Question List (for Analytics sections) -->
      <template v-if="activeSection !== 'dataview' && activeSection !== 'datacleansing' && activeSection !== 'questions'">
        <!-- Horizontal Layout: Question List | Main Content -->
        <div class="flex h-full">
          <!-- Left: Question List -->
          <div 
            :style="{ width: questionListWidth + 'px' }"
            class="flex-shrink-0 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden"
          >
            <div class="h-full overflow-y-auto">
              <QuestionListSelector
                :surveyId="surveyId"
                :selectionMode="questionListMode"
                :activeTab="activeAnalysisTab"
                :showNotesIcons="true"
                :wrapStandaloneQuestions="true"
                :selectedQuestionId="questionListMode === 'single' ? selectedQuestionId : null"
                :initialExpandedBlocks="expandedBlocks"
                :initialFirstSelection="questionListMode === 'crosstab' ? crosstabFirstQuestion : null"
                :initialSecondSelection="questionListMode === 'crosstab' ? crosstabSecondQuestion : null"
                @question-selected="handleQuestionListSelection"
                @crosstab-selection="handleQuestionListCrosstabSelection"
                @expanded-blocks-changed="handleExpandedBlocksChanged"
                @questions-loaded="handleQuestionsLoaded"
              />
            </div>
          </div>

          <!-- Vertical Resize Handle for Question List -->
          <div
            class="w-1 bg-gray-200 dark:bg-gray-700 hover:bg-blue-500 dark:hover:bg-blue-600 cursor-col-resize flex-shrink-0 transition-colors"
            @mousedown="startResizeQuestionList"
          ></div>

          <!-- Right: Main Content Area (Context + Content) -->
          <div class="flex-1 flex flex-col min-w-0">
            <!-- Top: Context Area (Responses Table + Question/Block) -->
            <div 
              :style="{ height: contextHeight + 'px' }"
              class="flex-shrink-0 flex"
            >
              <!-- Left: Responses Table -->
              <div 
                :style="{ width: responsesTableWidth + 'px' }"
                class="flex-shrink-0 bg-white dark:bg-gray-800 overflow-hidden"
              >
                <div class="h-full">
                  <ResponsesTableOnly 
                    v-show="selectedQuestionWithResponses"
                    :question="selectedQuestionWithResponses || {}"
                    :columnMode="columnMode"
                    @column-mode-changed="handleColumnModeChanged"
                  />
                  
                  <!-- Show placeholder when no question is selected -->
                  <div v-show="!selectedQuestionWithResponses" class="h-full flex flex-col">
                    <!-- Skeleton header -->
                    <div class="flex-shrink-0 px-4 py-1 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900 flex items-center justify-between">
                      <div class="h-4 w-24 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                      <div class="h-6 w-28 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                    </div>
                    <!-- Skeleton table -->
                    <div class="flex-1 p-4 space-y-2">
                      <div class="h-6 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                      <div class="h-6 bg-gray-100 dark:bg-gray-800 rounded animate-pulse"></div>
                      <div class="h-6 bg-gray-100 dark:bg-gray-800 rounded animate-pulse"></div>
                      <div class="h-6 bg-gray-100 dark:bg-gray-800 rounded animate-pulse"></div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Vertical Resize Handle for Responses Table -->
              <div
                class="w-1 bg-gray-200 dark:bg-gray-700 hover:bg-blue-500 dark:hover:bg-blue-600 cursor-col-resize flex-shrink-0 transition-colors"
                @mousedown="startResizeResponsesTable"
              ></div>

              <!-- Right: Question/Block Text -->
              <div class="flex-1 bg-white dark:bg-gray-800 overflow-hidden min-w-0">
                <div class="h-full">
                  <QuestionBlockText 
                    v-show="selectedQuestionWithResponses"
                    :question="selectedQuestionWithResponses || {}"
                    :surveyId="surveyId"
                  />
                  
                  <!-- Show placeholder when no question is selected -->
                  <div v-show="!selectedQuestionWithResponses" class="h-full flex flex-col">
                    <!-- Skeleton tabs -->
                    <div class="flex-shrink-0 border-b border-gray-200 dark:border-gray-700">
                      <div class="flex gap-2 p-2">
                        <div class="h-8 w-24 bg-gray-200 dark:bg-gray-700 rounded animate-pulse"></div>
                        <div class="h-8 w-20 bg-gray-100 dark:bg-gray-800 rounded animate-pulse"></div>
                      </div>
                    </div>
                    <!-- Skeleton content -->
                    <div class="flex-1 p-4 space-y-2">
                      <div class="h-4 bg-gray-200 dark:bg-gray-700 rounded animate-pulse w-full"></div>
                      <div class="h-4 bg-gray-100 dark:bg-gray-800 rounded animate-pulse w-5/6"></div>
                      <div class="h-4 bg-gray-100 dark:bg-gray-800 rounded animate-pulse w-4/5"></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Horizontal Resize Handle for Context Area -->
            <div
              class="h-1 bg-gray-200 dark:bg-gray-700 hover:bg-blue-500 dark:hover:bg-blue-600 cursor-row-resize flex-shrink-0 transition-colors"
              @mousedown="startResizeContext"
            ></div>

            <!-- Bottom: Content Area (Tabs + Chart) -->
            <div class="flex-1 bg-gray-50 dark:bg-gray-900 overflow-hidden min-h-0 flex flex-col">
              <!-- Analysis Tabs -->
              <div class="flex-shrink-0 border-t border-t-blue-500 dark:border-t-black border-b border-gray-200 dark:border-gray-700 bg-blue-50 dark:bg-gray-800 shadow-sm dark:shadow-none">
                <div class="flex gap-1 px-4">
                  <!-- Visible Tabs: Results, Crosstab, Stat Sig, (+ Promoted Tab) -->
                  <button
                    v-for="tab in visibleTabs"
                    :key="tab.id"
                    @click="activeAnalysisTab = tab.id"
                    :class="[
                      'flex items-center gap-2 px-3 py-0.5 text-xs font-medium whitespace-nowrap transition-colors border-b-2',
                      activeAnalysisTab === tab.id
                        ? 'border-blue-500 text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-900/20'
                        : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
                    ]"
                  >
                    <svg
                      v-if="tab.iconType !== 'text'"
                      class="w-5 h-5"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="tab.icon" />
                    </svg>
                    <span v-else class="text-lg font-bold">{{ tab.icon }}</span>
                    <span>{{ tab.label }}</span>
                  </button>

                  <!-- More Tabs Dropdown -->
                  <div class="relative" v-click-outside="() => showMoreTabs = false">
                    <button
                      @click="showMoreTabs = !showMoreTabs"
                      :class="[
                        'flex items-center gap-1 px-3 pt-[9px] pb-[8px] text-xs font-medium whitespace-nowrap transition-colors border-b-2',
                        availableMoreTabs.some(t => t.id === activeAnalysisTab)
                          ? 'border-transparent text-blue-600 dark:text-blue-400'
                          : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
                      ]"
                    >
                      <span>More</span>
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
                      </svg>
                    </button>
                    
                    <!-- Dropdown Menu -->
                    <div
                      v-if="showMoreTabs"
                      class="absolute top-full left-0 mt-1 w-56 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-lg z-50"
                    >
                      <button
                        v-for="tab in availableMoreTabs"
                        :key="tab.id"
                        @click="() => { promotedTab = tab; activeAnalysisTab = tab.id; showMoreTabs = false }"
                        :class="[
                          'w-full flex items-center gap-3 px-3 py-0.5 text-xs transition-colors',
                          activeAnalysisTab === tab.id
                            ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400'
                            : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700'
                        ]"
                      >
                        <svg
                          v-if="tab.iconType !== 'text'"
                          class="w-5 h-5"
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="tab.icon" />
                        </svg>
                        <span v-else class="text-lg font-bold">{{ tab.icon }}</span>
                        <span>{{ tab.label }}</span>
                      </button>
                    </div>
                  </div>
                  
                  <!-- Right side buttons -->
                  <div class="flex items-center gap-2">
                    <!-- AI Analysis Button (appears on most tabs) -->
                    <button
                      @click="handleAIAnalysisClick"
                      :disabled="!canShowAIAnalysis"
                      :class="[
                        'flex items-center gap-2 px-3 py-1.5 text-sm font-medium rounded-lg transition-colors',
                        canShowAIAnalysis 
                          ? 'text-gray-700 dark:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700 cursor-pointer'
                          : 'text-gray-400 dark:text-gray-600 cursor-not-allowed opacity-50'
                      ]"
                      :title="getAIAnalysisTooltip"
                    >
                      <svg class="w-5 h-5 text-yellow-600 fill-yellow-600 dark:text-yellow-400 dark:fill-transparent" fill="currentColor" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
                      </svg>
                      AI Analysis
                    </button>
                    
                    <!-- Reset Tour Button (crosstab) -->
                    <button
                      v-if="activeAnalysisTab === 'crosstab' && hasCrosstabTourCompleted()"
                      @click="resetCrosstabTour"
                      class="flex items-center gap-1.5 px-3 py-1 text-xs text-gray-500 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition-colors rounded hover:bg-blue-50 dark:hover:bg-blue-900/20"
                      title="Restart the variable selection tour"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <span>Restart Tour</span>
                    </button>
                    
                    <!-- Reset Tour Button (results) -->
                    <button
                      v-if="activeAnalysisTab === 'results' && hasResultsTourCompleted()"
                      @click="resetResultsTour"
                      class="flex items-center gap-1.5 px-3 py-1 text-xs text-gray-500 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition-colors rounded hover:bg-blue-50 dark:hover:bg-blue-900/20"
                      title="Restart the results tour"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <span>Restart Tour</span>
                    </button>
                    
                    <!-- Reset Tour Button (statsig) -->
                    <button
                      v-if="activeAnalysisTab === 'statsig' && hasStatSigTourCompleted()"
                      @click="resetStatSigTour"
                      class="flex items-center gap-1.5 px-3 py-1 text-xs text-gray-500 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition-colors rounded hover:bg-blue-50 dark:hover:bg-blue-900/20"
                      title="Restart the statistical significance tour"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                      <span>Restart Tour</span>
                    </button>
                  </div>
                </div>
              </div>

              <!-- Tab Content -->
              <div class="flex-1 overflow-auto ">
                <!-- Results Tab -->
                <div v-show="activeAnalysisTab === 'results'" class="h-full">
                  <ResultsTabContent
                    :selectedQuestion="selectedQuestionWithResponses"
                    @analyze-crosstab="handleAnalyzeCrosstab"
                    @ai-analysis="handleAIAnalysis"
                    @show-info="handleShowInfo"
                  />
                </div>

                <!-- Crosstab Tab -->
                <div v-show="activeAnalysisTab === 'crosstab'" class="h-full">
                  <CrosstabView
                    :surveyId="surveyId"
                    :firstQuestion="crosstabFirstQuestion"
                    :secondQuestion="crosstabSecondQuestion"
                    :triggerAIModal="showCrosstabAIModal"
                    @ai-modal-shown="showCrosstabAIModal = false"
                  />
                </div>

                <!-- Statistical Significance Tab -->
                <div v-show="activeAnalysisTab === 'statsig'" class="h-full flex justify-center ">
                  <div v-if="selectedQuestion" class="w-full">
                    <StatSigView
                      :surveyId="surveyId"
                      :selectedQuestion="selectedQuestion"
                      :triggerAIModal="showStatSigAIModal"
                      @ai-modal-shown="showStatSigAIModal = false"
                    />
                  </div>
                  
                  <!-- Empty state for StatSig -->
                  <div v-else class="h-full flex items-center justify-center">
                    <div class="text-center">
                      <svg class="w-16 h-16 mx-auto mb-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 14h.01M12 14h.01M15 11h.01M12 11h.01M9 11h.01M7 21h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z" />
                      </svg>
                      <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">No Question Selected</h3>
                      <p class="text-sm text-gray-500 dark:text-gray-400">Select a question from the list to view statistical significance</p>
                    </div>
                  </div>
                </div>

                <!-- Coming Soon for other tabs -->
                <div v-show="activeAnalysisTab !== 'results' && activeAnalysisTab !== 'crosstab' && activeAnalysisTab !== 'statsig'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="w-16 h-16 mx-auto mb-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                    </svg>
                    <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-2">{{ analysisTabs.find(t => t.id === activeAnalysisTab)?.label }}</h3>
                    <p class="text-sm text-gray-500 dark:text-gray-400">Coming soon...</p>
                  </div>
                </div>
              </div>
      </div>
    </div>

    <!-- AI Analysis Modal -->
    <AIAnalysisModal
      :show="showAIModal"
      :questionLabel="selectedQuestionWithResponses?.label || selectedQuestionWithResponses?.qstLabel || ''"
      :analysis="aiAnalysis"
      :loading="loadingAnalysis"
      :context="activeAnalysisTab === 'results' ? 'Results' : activeAnalysisTab === 'statsig' ? 'Statistical Significance' : ''"
      @close="showAIModal = false"
      @generate="generateAIAnalysis"
      @regenerate="generateAIAnalysis"
    />

    <!-- Understanding Results Modal -->
    <UnderstandingResultsModal
      :show="showUnderstandingModal"
      @close="showUnderstandingModal = false"
    />
  </div>
</template>    <!-- Standalone Main Content (when question list is hidden) -->
    <main v-if="activeSection === 'dataview' || activeSection === 'datacleansing' || activeSection === 'questions'" class="h-full w-full overflow-hidden">
      <!-- Questions View -->
      <QuestionsView v-if="activeSection === 'questions'" :surveyId="surveyId" class="h-full w-full" />

      <!-- Data View -->
      <DataView v-if="activeSection === 'dataview'" :surveyId="surveyId" class="h-full w-full" />

      <!-- Data Cleansing -->
      <div v-if="activeSection === 'datacleansing'" class="h-full w-full flex items-center justify-center bg-gray-50 dark:bg-gray-900">
        <div class="text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4h13M3 8h9m-9 4h6m4 0l4-4m0 0l4 4m-4-4v12" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Data Cleansing</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Coming soon...</p>
        </div>
      </div>
    </main>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed, nextTick, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import SidebarButton from '../components/SidebarButton.vue'
import QuestionsView from '../components/Analytics/QuestionsView.vue'
import DataView from '../components/Analytics/DataView.vue'
import QuestionListSelector from '@/components/Analytics/QuestionListSelector.vue'
import ResponsesTableOnly from '@/components/Analytics/ResponsesTableOnly.vue'
import QuestionBlockText from '@/components/Analytics/QuestionBlockText.vue'
import ResultsChart from '@/components/Analytics/ResultsChart.vue'
import ResultsTabContent from '@/components/Analytics/ResultsTabContent.vue'
import AIAnalysisModal from '@/components/Analytics/AIAnalysisModal.vue'
import UnderstandingResultsModal from '@/components/Analytics/UnderstandingResultsModal.vue'
import CrosstabView from '@/components/Analytics/CrosstabView.vue'
import StatSigView from '@/components/Analytics/StatSigView.vue'
import { useCrosstabTour } from '@/composables/useCrosstabTour'
import { useResultsTour } from '@/composables/useResultsTour'
import { useStatSigTour } from '@/composables/useStatSigTour'
import { useTourManager } from '@/composables/useTourManager'
import '@/assets/shepherd-theme.css'

// Click outside directive
const vClickOutside = {
  mounted(el, binding) {
    el.clickOutsideEvent = (event) => {
      if (!(el === event.target || el.contains(event.target))) {
        binding.value(event)
      }
    }
    document.addEventListener('click', el.clickOutsideEvent)
  },
  unmounted(el) {
    document.removeEventListener('click', el.clickOutsideEvent)
  }
}

const route = useRoute()
const router = useRouter()

const surveyId = ref(route.params.id)
const surveyName = ref('Loading...')
const editableSurveyName = ref('')
const isHoveringName = ref(false)
const isEditingName = ref(false)
const projectName = ref('')
const totalCases = ref(0)
const questionCount = ref(0)
const activeSection = ref('results')

// Initialize Tours
const { hasTourBeenCompleted: hasCrosstabTourCompleted, startTour: startCrosstabTour, resetTour: resetCrosstabTourFlag } = useCrosstabTour()
const { hasTourBeenCompleted: hasResultsTourCompleted, startTour: startResultsTour, resetTour: resetResultsTourFlag } = useResultsTour()
const { hasTourBeenCompleted: hasStatSigTourCompleted, startTour: startStatSigTour, resetTour: resetStatSigTourFlag } = useStatSigTour()
const { hasAnyTourBeenCompleted } = useTourManager()

// Sidebar collapse state
const sidebarCollapsed = ref(true) // Default to collapsed (closed) when survey first opens

// Resize state
const questionListWidth = ref(300) // Default 300px (reduced from 320px for more compact layout)
const contextHeight = ref(140) // Default 140px for context area (reduced from 200px)
const responsesTableWidth = ref(400) // Default 400px for responses table
const isContextCollapsed = ref(false) // Context area collapse state

let isResizing = ref(false)
let resizeType = ref(null) // 'questionList', 'context', 'responsesTable'
let startX = ref(0)
let startY = ref(0)
let startWidth = ref(0)
let startHeight = ref(0)

// Resize handlers
function startResizeQuestionList(event) {
  isResizing.value = true
  resizeType.value = 'questionList'
  startX.value = event.clientX
  startWidth.value = questionListWidth.value
  document.body.style.cursor = 'col-resize'
  document.addEventListener('mousemove', handleMouseMove)
  document.addEventListener('mouseup', stopResize)
  event.preventDefault()
}

function startResizeContext(event) {
  isResizing.value = true
  resizeType.value = 'context'
  startY.value = event.clientY
  startHeight.value = contextHeight.value
  document.body.style.cursor = 'row-resize'
  document.addEventListener('mousemove', handleMouseMove)
  document.addEventListener('mouseup', stopResize)
  event.preventDefault()
}

function startResizeResponsesTable(event) {
  isResizing.value = true
  resizeType.value = 'responsesTable'
  startX.value = event.clientX
  startWidth.value = responsesTableWidth.value
  document.body.style.cursor = 'col-resize'
  document.addEventListener('mousemove', handleMouseMove)
  document.addEventListener('mouseup', stopResize)
  event.preventDefault()
}

function handleMouseMove(event) {
  if (!isResizing.value) return

  if (resizeType.value === 'questionList') {
    const deltaX = event.clientX - startX.value
    const newWidth = Math.max(200, Math.min(600, startWidth.value + deltaX))
    questionListWidth.value = newWidth
  } else if (resizeType.value === 'context') {
    const deltaY = event.clientY - startY.value
    const newHeight = Math.max(150, Math.min(600, startHeight.value + deltaY))
    contextHeight.value = newHeight
  } else if (resizeType.value === 'responsesTable') {
    const deltaX = event.clientX - startX.value
    const newWidth = Math.max(250, Math.min(800, startWidth.value + deltaX))
    responsesTableWidth.value = newWidth
  }
}

function stopResize() {
  isResizing.value = false
  resizeType.value = null
  document.body.style.cursor = ''
  document.removeEventListener('mousemove', handleMouseMove)
  document.removeEventListener('mouseup', stopResize)
}

// Cleanup on unmount
onUnmounted(() => {
  document.removeEventListener('mousemove', handleMouseMove)
  document.removeEventListener('mouseup', stopResize)
})

// Analysis tabs configuration
const mainTabs = [
  { id: 'results', label: 'Results', icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' },
  { id: 'crosstab', label: 'Crosstab', icon: 'M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z' },
  { id: 'statsig', label: 'Stat Sig', icon: 'Φ', iconType: 'text' },
]

const moreTabs = [
  { id: 'fullblock', label: 'Full Block', icon: 'M4 5a1 1 0 011-1h14a1 1 0 011 1v2a1 1 0 01-1 1H5a1 1 0 01-1-1V5zM4 13a1 1 0 011-1h6a1 1 0 011 1v6a1 1 0 01-1 1H5a1 1 0 01-1-1v-6zM16 13a1 1 0 011-1h2a1 1 0 011 1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-6z' },
  { id: 'matchingblocks', label: 'Matching Blocks', icon: 'M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 012 2m0 10V7m0 10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2h-2a2 2 0 00-2 2' },
  { id: 'index', label: 'Index', icon: 'M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z' },
  { id: 'indexplus', label: 'Index +', icon: 'M12 6v6m0 0v6m0-6h6m-6 0H6' },
  { id: 'profile', label: 'Profile', icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
  { id: 'oneresponse', label: 'One Response', icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' },
]

// Computed: visible tabs including promoted tab
const visibleTabs = computed(() => {
  const tabs = [...mainTabs]
  if (promotedTab.value) {
    tabs.push(promotedTab.value)
  }
  return tabs
})

// Computed: more dropdown tabs (including promoted, which shows as selected)
const availableMoreTabs = computed(() => {
  return moreTabs
})

const analysisTabs = [...mainTabs, ...moreTabs]

// Function to change tab
const changeTab = (tabId) => {
  activeSection.value = tabId
}

// State management helpers
const selectedQuestionId = ref(null)
const selectedQuestion = ref(null)
const selectedQuestionWithResponses = ref(null)
const expandedBlocks = ref([])
const columnMode = ref('totalN')
const infoExpanded = ref(false)
const activeAnalysisTab = ref('results')
const showMoreTabs = ref(false)
const promotedTab = ref(null) // Currently promoted tab from More dropdown
const showAIModal = ref(false)
const showUnderstandingModal = ref(false)
const aiAnalysis = ref('')
const loadingAnalysis = ref(false)
const currentQuestion = ref(null)
const infoTab = ref('question')
const isInitialRouteLoad = ref(true)  // Track initial load to prevent route query interference

// Crosstab state
const crosstabFirstQuestion = ref(null)
const crosstabSecondQuestion = ref(null)
const splitViewEnabled = ref(false)

// For split view - track which question to show in results panel
const selectedQuestionForSplit = ref(null)

// Computed property to determine question list mode based on active section
const questionListMode = computed(() => {
  if (activeAnalysisTab.value === 'crosstab') {
    return 'crosstab'
  }
  return 'single'
})

// Handle question selection from permanent question list
function handleQuestionListSelection(question) {
  selectedQuestion.value = question
  selectedQuestionId.value = question.id
  
  // Don't call loadQuestionData here - the watch on selectedQuestionId will handle it
  
  // If we're in crosstab mode and clicked, treat as first selection
  if (activeSection.value === 'crosstab') {
    crosstabFirstQuestion.value = question
  }
}

// Handle question selection from StatSigView (currently unused - kept for future use)
// Question selection in StatSig happens via the sidebar question list
function handleStatSigQuestionSelected(question) {
  // This would handle if StatSigView had its own question selector
  // Currently questions are selected via the permanent sidebar
}

// Handle crosstab selection from permanent question list
async function handleQuestionListCrosstabSelection({ first, second }) {
  crosstabFirstQuestion.value = first
  crosstabSecondQuestion.value = second
  
  // Update split panel to show the first question's results
  if (first) {
    selectedQuestionForSplit.value = first
    selectedQuestion.value = first
    selectedQuestionId.value = first.id
    
    // Load full question data for first variable
    await loadQuestionData(first.id)
    
    // Update crosstabFirstQuestion with full data including responses
    if (selectedQuestionWithResponses.value) {
      crosstabFirstQuestion.value = selectedQuestionWithResponses.value
    }
  }
  
  saveSurveyState()
}

// Load questions by ID for crosstab (from Stat Sig navigation)
async function loadQuestionsForCrosstab(firstQuestionId, secondQuestionId) {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/${surveyId.value}/questions`)
    const questions = response.data
    
    const firstQ = questions.find(q => q.id === firstQuestionId)
    const secondQ = questions.find(q => q.id === secondQuestionId)
    
    if (firstQ && secondQ) {
      crosstabFirstQuestion.value = firstQ
      crosstabSecondQuestion.value = secondQ
      selectedQuestionId.value = firstQuestionId
      
      // Load full question data for display
      await loadQuestionData(firstQuestionId)
    }
  } catch (error) {
    console.error('Error loading questions for crosstab:', error)
  }
}

// Load full question data with responses
async function loadQuestionData(questionId) {
  if (!questionId) {
    selectedQuestionWithResponses.value = null
    return
  }
  
  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/${surveyId.value}/questions`)
    const questions = response.data
    const fullQuestion = questions.find(q => q.id === questionId)
    if (fullQuestion) {
      selectedQuestionWithResponses.value = fullQuestion
    }
  } catch (error) {
    console.error('Error loading question data:', error)
  }
}

// Resize handlers removed - using PrimeVue Splitter component instead

function getSurveyStateKey() {
  return `survey-state-${surveyId.value}`
}

function saveSurveyState() {
  const state = {
    activeSection: activeSection.value,
    selectedQuestionId: selectedQuestionId.value,
    expandedBlocks: expandedBlocks.value,
    columnMode: columnMode.value,
    infoExpanded: infoExpanded.value,
    infoTab: infoTab.value,
    crosstabFirstQuestion: crosstabFirstQuestion.value,
    crosstabSecondQuestion: crosstabSecondQuestion.value,
    promotedTabId: promotedTab.value?.id || null,
    timestamp: Date.now()
  }
  localStorage.setItem(getSurveyStateKey(), JSON.stringify(state))
  
  // Also update query params for URL state (only if different to avoid loops)
  const currentSection = route.query.section
  const currentQuestion = route.query.question
  
  if (currentSection !== activeSection.value || currentQuestion !== selectedQuestionId.value) {
    router.replace({
      query: {
        ...route.query,
        section: activeSection.value,
        question: selectedQuestionId.value || undefined
      }
    }).catch(() => {
      // Ignore navigation errors (e.g., navigating to same location)
    })
  }
}

function loadSurveyState() {
  // First check URL query params (highest priority)
  if (route.query.section) {
    activeSection.value = route.query.section
    // Also update activeAnalysisTab for analysis sections
    if (route.query.section === 'crosstab' || route.query.section === 'results' || route.query.section === 'statsig') {
      activeAnalysisTab.value = route.query.section
    }
  }
  if (route.query.questionId) {
    selectedQuestionId.value = route.query.questionId
  }
  
  // Handle crosstab navigation from Stat Sig
  if (route.query.firstQuestion && route.query.secondQuestion) {
    // Load questions by ID for crosstab
    loadQuestionsForCrosstab(route.query.firstQuestion, route.query.secondQuestion)
  }
  
  // Then check localStorage for saved state if URL params not present
  const savedState = localStorage.getItem(getSurveyStateKey())
  if (savedState) {
    try {
      const state = JSON.parse(savedState)
      // Only restore if less than 24 hours old
      if (Date.now() - state.timestamp < 24 * 60 * 60 * 1000) {
        if (!route.query.section) {
          activeSection.value = state.activeSection || 'results'
        }
        if (!route.query.questionId) {
          selectedQuestionId.value = state.selectedQuestionId || null
        }
        expandedBlocks.value = state.expandedBlocks || []
        columnMode.value = state.columnMode || 'totalN'
        infoExpanded.value = state.infoExpanded || false
        infoTab.value = state.infoTab || 'question'
        crosstabFirstQuestion.value = state.crosstabFirstQuestion || null
        crosstabSecondQuestion.value = state.crosstabSecondQuestion || null
        // Restore promoted tab by finding it in moreTabs
        if (state.promotedTabId) {
          promotedTab.value = moreTabs.find(t => t.id === state.promotedTabId) || null
        }
      }
    } catch (error) {
      console.error('Error loading saved state:', error)
    }
  }
}

// Watch for changes and save state
watch(activeSection, (newSection, oldSection) => {
  // When switching TO crosstab, ensure crosstab first question matches current selection
  if (newSection === 'crosstab' && selectedQuestionWithResponses.value) {
    // Only update if they differ
    if (!crosstabFirstQuestion.value || crosstabFirstQuestion.value.id !== selectedQuestionWithResponses.value.id) {
      crosstabFirstQuestion.value = selectedQuestionWithResponses.value
    }
  }
  
  // When switching FROM crosstab to results or statsig, sync the selected question
  if ((newSection === 'results' || newSection === 'statsig') && oldSection === 'crosstab' && crosstabFirstQuestion.value) {
    if (!selectedQuestionId.value || selectedQuestionId.value !== crosstabFirstQuestion.value.id) {
      selectedQuestionId.value = crosstabFirstQuestion.value.id
      selectedQuestion.value = crosstabFirstQuestion.value
      loadQuestionData(crosstabFirstQuestion.value.id)
    }
  }
  
  saveSurveyState()
})

// Watch activeAnalysisTab changes separately for tab-specific logic
watch(activeAnalysisTab, (newTab, oldTab) => {
  // When switching FROM crosstab to results or statsig, sync the selected question
  if ((newTab === 'results' || newTab === 'statsig') && oldTab === 'crosstab' && crosstabFirstQuestion.value) {
    if (!selectedQuestionId.value || selectedQuestionId.value !== crosstabFirstQuestion.value.id) {
      selectedQuestionId.value = crosstabFirstQuestion.value.id
      selectedQuestion.value = crosstabFirstQuestion.value
      loadQuestionData(crosstabFirstQuestion.value.id)
    }
  }
  
  // Update URL query params to reflect the current tab
  if (newTab && newTab !== route.query.section) {
    router.replace({
      query: {
        ...route.query,
        section: newTab
      }
    })
  }
  
  saveSurveyState()
})

watch(selectedQuestionId, () => {
  saveSurveyState()
})

// Watch for when ResultsView changes the selected question and sync back
watch(() => selectedQuestionId.value, (newId) => {
  if (newId && activeSection.value === 'results') {
    // When switching back to results view, ensure question list reflects the selection
    // This handles the case where crosstab changed the selection
  }
})

watch(expandedBlocks, () => {
  saveSurveyState()
}, { deep: true })

watch(columnMode, () => {
  saveSurveyState()
})

watch(infoExpanded, () => {
  saveSurveyState()
})

watch(infoTab, () => {
  saveSurveyState()
})

watch(crosstabFirstQuestion, (newVal, oldVal) => {
  // When crosstab first question changes and we're in results/statsig, sync the selection
  if (newVal && (activeAnalysisTab.value === 'results' || activeAnalysisTab.value === 'statsig')) {
    if (!selectedQuestionId.value || selectedQuestionId.value !== newVal.id) {
      selectedQuestionId.value = newVal.id
      selectedQuestion.value = newVal
      loadQuestionData(newVal.id)
    }
  }
  saveSurveyState()
}, { deep: true })

watch(crosstabSecondQuestion, (newVal, oldVal) => {
  saveSurveyState()
}, { deep: true })

watch(promotedTab, () => {
  saveSurveyState()
})

// Watch selectedQuestionId and load full question data
watch(selectedQuestionId, (newId) => {
  if (newId) {
    loadQuestionData(newId)
  } else {
    selectedQuestionWithResponses.value = null
  }
})

// Watch selectedQuestionWithResponses to sync with crosstab first question (results -> crosstab)
// This ensures when you change the dependent variable on Results, it's reflected in Crosstab
watch(selectedQuestionWithResponses, (newQuestion) => {
  // Only sync if we have a question and it's different from current crosstab selection
  if (newQuestion && (!crosstabFirstQuestion.value || crosstabFirstQuestion.value.id !== newQuestion.id)) {
    // Update crosstab first question to match the Results selection
    crosstabFirstQuestion.value = newQuestion
  }
})

// Handle question selection from ResultsView
function handleQuestionSelected(questionId) {
  selectedQuestionId.value = questionId
}

// Handle expanded blocks changes from ResultsView
function handleExpandedBlocksChanged(blocks) {
  expandedBlocks.value = blocks
}

// Handle questions loaded event - select first question if none selected
async function handleQuestionsLoaded(questions) {
  // Only auto-select if no question is currently selected
  if (!selectedQuestionId.value && questions && questions.length > 0) {
    // Find the first question (could be standalone or in a block)
    let firstQuestion = null
    let blockToExpand = null
    
    for (const item of questions) {
      if (item.type === 'block' && item.questions && item.questions.length > 0) {
        firstQuestion = item.questions[0]
        blockToExpand = item.blockId
        break
      } else if (item.type === 'question') {
        firstQuestion = item
        break
      }
    }
    
    if (firstQuestion) {
      // Expand block if needed - must happen before selecting question
      if (blockToExpand && !expandedBlocks.value.includes(blockToExpand)) {
        expandedBlocks.value = [...expandedBlocks.value, blockToExpand]
        // Give time for block to expand in DOM
        await nextTick()
      }
      
      // Select the first question
      selectedQuestionId.value = firstQuestion.id
      selectedQuestion.value = firstQuestion
    }
  }
}

// Handle column mode changes from ResultsView
function handleColumnModeChanged(mode) {
  columnMode.value = mode
}

// Handle info panel changes from ResultsView
function handleInfoExpandedChanged(expanded) {
  infoExpanded.value = expanded
}

function handleInfoTabChanged(tab) {
  infoTab.value = tab
}

// Handle crosstab selection changes
function handleCrosstabSelectionsChanged({ firstQuestion, secondQuestion }) {
  crosstabFirstQuestion.value = firstQuestion
  crosstabSecondQuestion.value = secondQuestion
  
  // Update split panel to show the first question's results
  if (firstQuestion) {
    selectedQuestionForSplit.value = firstQuestion
  }
  
  saveSurveyState()
}

// Watch split view toggle to sync the selected question
watch(splitViewEnabled, (enabled) => {
  if (enabled && crosstabFirstQuestion.value) {
    selectedQuestionForSplit.value = crosstabFirstQuestion.value
  }
})

// Watch crosstab first question to keep split panel in sync
watch(crosstabFirstQuestion, (newQuestion, oldQuestion) => {
  
  // Sync to split view panel
  if (splitViewEnabled.value && newQuestion) {
    selectedQuestionForSplit.value = newQuestion
  }
  
  // Sync to Results view selectedQuestionId (crosstab -> results)
  // Only update if the IDs don't match to prevent circular updates
  if (newQuestion && newQuestion !== oldQuestion && newQuestion.id !== selectedQuestionId.value) {
    selectedQuestionId.value = newQuestion.id
    
    // If the question has a blockId, ensure that block is expanded
    if (newQuestion.blockId && !expandedBlocks.value.includes(newQuestion.blockId)) {
      expandedBlocks.value = [...expandedBlocks.value, newQuestion.blockId]
    }
  }
})

// Watch selectedQuestionForSplit to see changes
watch(selectedQuestionForSplit, (newValue) => {
})

// Handle analyze in crosstab from Results view
function handleAnalyzeCrosstab(question) {
  // Use the passed question or fall back to selectedQuestionWithResponses
  const questionToUse = question || selectedQuestionWithResponses.value
  
  if (!questionToUse) return
  
  // Set the question as first variable in crosstab (force update even if same)
  crosstabFirstQuestion.value = questionToUse
  crosstabSecondQuestion.value = null // Reset second selection
  
  // Switch to crosstab view - must happen AFTER setting crosstabFirstQuestion
  // Use nextTick to ensure the watch has time to process
  nextTick(() => {
    activeAnalysisTab.value = 'crosstab'
    saveSurveyState()
  })
}

function handleAIAnalysis(question) {
  showAIModal.value = true
  aiAnalysis.value = '' // Reset analysis
}

async function generateAIAnalysis() {
  if (!selectedQuestionWithResponses.value) return
  
  loadingAnalysis.value = true
  try {
    const context = {
      questionLabel: selectedQuestionWithResponses.value.label || selectedQuestionWithResponses.value.qstLabel,
      totalN: selectedQuestionWithResponses.value.responses?.reduce((sum, r) => sum + r.count, 0) || 0,
      responses: selectedQuestionWithResponses.value.responses?.map(r => ({
        label: r.label,
        frequency: r.count || 0,
        percent: r.percentage || 0
      })) || []
    }
    
    const response = await axios.post(
      `${API_BASE_URL}/api/survey-analysis/${surveyId.value}/analyze-question`,
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

function handleShowInfo(question) {
  currentQuestion.value = question
  showUnderstandingModal.value = true
}

async function loadSurveyInfo() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/${surveyId.value}`)
    surveyName.value = response.data.surveyName || response.data.name
    editableSurveyName.value = surveyName.value
    
    // Load project info
    if (response.data.projectId) {
      try {
        const projectResponse = await axios.get(`${API_BASE_URL}/api/projects/${response.data.projectId}`)
        projectName.value = projectResponse.data.projectName || 'Project'
      } catch (error) {
        console.error('Error loading project info:', error)
      }
    }

    // Get question count and case count from stats
    try {
      const statsResponse = await axios.get(`${API_BASE_URL}/api/surveys/${surveyId.value}/stats`)
      questionCount.value = statsResponse.data.questionCount || 0
      totalCases.value = statsResponse.data.responseCount || 0
    } catch (error) {
      console.error('Error loading survey stats:', error)
    }
  } catch (error) {
    console.error('Error loading survey info:', error)
    surveyName.value = 'Survey Analytics'
    editableSurveyName.value = surveyName.value
  }
}

async function saveSurveyName() {
  isEditingName.value = false
  
  // Only save if the name has actually changed
  if (editableSurveyName.value && editableSurveyName.value.trim() !== surveyName.value) {
    try {
      await axios.patch(`${API_BASE_URL}/api/surveys/${surveyId.value}`, {
        surveyName: editableSurveyName.value.trim()
      })
      surveyName.value = editableSurveyName.value.trim()
    } catch (error) {
      console.error('Error updating survey name:', error)
      // Revert to original name on error
      editableSurveyName.value = surveyName.value
    }
  } else {
    // Revert to original name if empty or unchanged
    editableSurveyName.value = surveyName.value
  }
}

function toggleSidebar() {
  sidebarCollapsed.value = !sidebarCollapsed.value
  localStorage.setItem('sidebarCollapsed', sidebarCollapsed.value)
}

// Track if any onboarding tooltips have been dismissed (reactive to localStorage changes)
const hasAnyTooltipBeenDismissed = ref(false)

// Watch for section activation and start appropriate tour if not completed
watch(activeAnalysisTab, async (newTab, oldTab) => {
  // Results tab tour (primary onboarding)
  if (newTab === 'results' && !hasResultsTourCompleted()) {
    await nextTick()
    setTimeout(() => {
      startResultsTour()
    }, 800)
  }
  // Crosstab tab tour
  else if (newTab === 'crosstab' && !hasCrosstabTourCompleted()) {
    await nextTick()
    setTimeout(() => {
      startCrosstabTour()
    }, 800)
  }
  // StatSig tab tour
  else if (newTab === 'statsig' && !hasStatSigTourCompleted()) {
    await nextTick()
    setTimeout(() => {
      startStatSigTour()
    }, 800)
  }
})

function resetCrosstabTour() {
  resetCrosstabTourFlag()
  setTimeout(() => {
    startCrosstabTour()
  }, 300)
}

function resetResultsTour() {
  resetResultsTourFlag()
  setTimeout(() => {
    startResultsTour()
  }, 300)
}

function resetStatSigTour() {
  resetStatSigTourFlag()
  setTimeout(() => {
    startStatSigTour()
  }, 300)
}

// AI Analysis - computed to determine when to show the button
const canShowAIAnalysis = computed(() => {
  // Show on Results tab if a question is selected and has responses
  if (activeAnalysisTab.value === 'results') {
    return selectedQuestionWithResponses.value && 
           selectedQuestionWithResponses.value.responses && 
           selectedQuestionWithResponses.value.responses.length > 0
  }
  
  // Show on Crosstab tab if both questions are selected AND crosstab data exists
  if (activeAnalysisTab.value === 'crosstab') {
    return crosstabFirstQuestion.value && crosstabSecondQuestion.value
  }
  
  // Show on Stat Sig tab if a question is selected (dependent variable)
  if (activeAnalysisTab.value === 'statsig') {
    return selectedQuestion.value !== null
  }
  
  return false
})

// AI Analysis tooltip text
const getAIAnalysisTooltip = computed(() => {
  if (activeAnalysisTab.value === 'results') {
    if (!selectedQuestionWithResponses.value || !selectedQuestionWithResponses.value.responses || selectedQuestionWithResponses.value.responses.length === 0) {
      return 'Select a question to enable AI analysis'
    }
    return 'Get AI-powered insights about this question\'s response patterns'
  } else if (activeAnalysisTab.value === 'crosstab') {
    if (!crosstabFirstQuestion.value || !crosstabSecondQuestion.value) {
      return 'Select two questions to enable AI analysis of their relationship'
    }
    return 'Get AI analysis of the relationship between these two questions'
  } else if (activeAnalysisTab.value === 'statsig') {
    if (!selectedQuestion.value) {
      return 'Select a dependent variable to enable AI analysis'
    }
    return 'Get AI insights about statistical significance for this question'
  }
  return 'Get AI-powered analysis'
})

// AI Analysis click handler - delegates to appropriate child component
function handleAIAnalysisClick() {
  // Emit events that child components are listening for
  // The actual modals are managed by the child components (ResultsChart, CrosstabView, StatSigView)
  if (activeAnalysisTab.value === 'results') {
    handleAIAnalysis()
  } else if (activeAnalysisTab.value === 'statsig') {
    showStatSigAIModal.value = true
  } else if (activeAnalysisTab.value === 'crosstab') {
    // For crosstab, we need to trigger the AI modal in CrosstabView
    // This will be handled by setting a ref that CrosstabView watches
    showCrosstabAIModal.value = true
  }
}

// Refs to control AI modals from parent
const showCrosstabAIModal = ref(false)
const showStatSigAIModal = ref(false)

function backToProjects() {
  router.push('/')
}

onMounted(() => {
  loadSurveyState()
  loadSurveyInfo()
  
  // Load question data if there's already a selected question
  if (selectedQuestionId.value) {
    loadQuestionData(selectedQuestionId.value)
  }
  
  // Initialize split panel with first question if in split view
  if (splitViewEnabled.value && crosstabFirstQuestion.value) {
    selectedQuestionForSplit.value = crosstabFirstQuestion.value
  }
})

// Watch for route changes (when navigating to the same route with different params)
watch(() => route.params.id, (newId) => {
  if (newId) {
    const previousSurveyId = surveyId.value
    surveyId.value = newId
    
    // Clear state when switching to a different survey
    if (previousSurveyId && previousSurveyId !== newId) {
      // Clear localStorage for the old survey (using old ID)
      localStorage.removeItem(`survey-state-${previousSurveyId}`)
      
      // Clear in-memory state
      crosstabFirstQuestion.value = null
      crosstabSecondQuestion.value = null
      selectedQuestionForSplit.value = null
      selectedQuestionId.value = null
      selectedQuestion.value = null
      selectedQuestionWithResponses.value = null
      activeSection.value = 'results'
      activeAnalysisTab.value = 'results'
      expandedBlocks.value = []
      columnMode.value = 'totalN'
      promotedTab.value = null // Clear promoted tab when switching surveys
      
      // Don't load state for new survey - start fresh
      loadSurveyInfo()
    } else {
      // Only load saved state if we're staying on the same survey
      loadSurveyState()
      loadSurveyInfo()
    }
  }
})

// Watch section query param separately to handle back button navigation
watch(() => route.query.section, (newSection, oldSection) => {
  if (newSection && (newSection === 'crosstab' || newSection === 'results' || newSection === 'statsig')) {
    if (newSection !== activeAnalysisTab.value) {
      activeAnalysisTab.value = newSection
    }
  }
})

// Watch for query parameter changes (when navigating from Stat Sig to Crosstab)
watch([() => route.query.section, () => route.query.firstQuestion, () => route.query.secondQuestion], 
  ([newSection, newFirstQuestion, newSecondQuestion], [oldSection, oldFirstQuestion, oldSecondQuestion]) => {
  
  // Update section if it changed
  if (newSection && newSection !== activeSection.value) {
    activeSection.value = newSection
  }
  
  // Always update activeAnalysisTab for analysis sections
  if (newSection && (newSection === 'crosstab' || newSection === 'results' || newSection === 'statsig')) {
    if (newSection !== activeAnalysisTab.value) {
      activeAnalysisTab.value = newSection
    }
  }
  
  // Handle crosstab navigation parameters
  if (newFirstQuestion && newSecondQuestion) {
    loadQuestionsForCrosstab(newFirstQuestion, newSecondQuestion)
  }
}, { deep: true, flush: 'post' })

</script>

<style scoped>
.primevue-tabs-wrapper :deep(button[role="tab"]) {
  font-weight: 300 !important;
  font-size: 0.75rem !important;
  padding-top: 0.75rem !important;
  padding-bottom: 0.5rem !important;
}

.primevue-tabs-wrapper :deep(button[role="tab"][data-p-active="true"]) {
  font-weight: 600 !important;
  font-size: 0.875rem !important;
}

/* Light mode: dark text */
:root .primevue-tabs-wrapper :deep(button[role="tab"][data-p-active="true"]) {
  color: rgb(24 24 27) !important; /* zinc-900 */
}

/* Dark mode: white text */
.dark .primevue-tabs-wrapper :deep(button[role="tab"][data-p-active="true"]) {
  color: rgb(255 255 255) !important;
}

/* Splitter gutter hover effect */
:deep(.p-splitter-gutter) {
  transition: background-color 0.2s ease;
}

:deep(.p-splitter-gutter:hover) {
  background-color: rgb(59 130 246) !important; /* blue-500 */
}

:deep(.dark .p-splitter-gutter:hover) {
  background-color: rgb(96 165 250) !important; /* blue-400 */
}
</style>
