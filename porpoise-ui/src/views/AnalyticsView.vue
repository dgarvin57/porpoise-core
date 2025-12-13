<template>
  <div class="fixed inset-0 top-12 flex overflow-hidden bg-gray-50 dark:bg-gray-900">
    <!-- Sidebar Navigation -->
    <aside 
      :class="[
        'bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-y-auto transition-all duration-300',
        sidebarCollapsed ? 'w-16' : 'w-64'
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
        <div class="space-y-1">
          <!-- Analytics Section -->
          <SidebarButton
            label="Analytics"
            icon-path="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"
            :is-active="activeSection === 'results' || activeSection === 'crosstab' || activeSection === 'statsig' || activeSection === 'fullblock' || activeSection === 'matchingblocks' || activeSection === 'index' || activeSection === 'indexplus' || activeSection === 'profile' || activeSection === 'oneresponse'"
            :collapsed="sidebarCollapsed"
            @click="activeSection = 'results'"
          />
        </div>

        <!-- Divider -->
        <div class="my-4 border-t border-gray-200 dark:border-gray-700"></div>

        <div class="space-y-1">
          <!-- Questions Section -->
          <SidebarButton
            label="Questions"
            icon-path="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            :is-active="activeSection === 'questions'"
            :collapsed="sidebarCollapsed"
            badge="in progress"
            @click="activeSection = 'questions'"
          />

          <!-- Data View Section -->
          <SidebarButton
            label="Data View"
            icon-path="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z"
            :is-active="activeSection === 'dataview'"
            :collapsed="sidebarCollapsed"
            badge="in progress"
            @click="activeSection = 'dataview'"
          />

          <!-- Data Cleansing Section -->
          <SidebarButton
            label="Data Cleansing"
            icon-path="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
            :is-active="activeSection === 'datacleansing'"
            :collapsed="sidebarCollapsed"
            badge="to do"
            @click="activeSection = 'datacleansing'"
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

    <!-- Permanent Question List with Resizer -->
    <aside 
      :style="{ width: questionListWidth + 'px' }"
      class="bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col relative"
    >
      <QuestionListSelector
        :surveyId="surveyId"
        :selectionMode="questionListMode"
        :showNotesIcons="true"
        :wrapStandaloneQuestions="true"
        :selectedQuestionId="questionListMode === 'single' ? selectedQuestionId : null"
        :initialExpandedBlocks="expandedBlocks"
        :initialFirstSelection="questionListMode === 'crosstab' ? crosstabFirstQuestion : null"
        :initialSecondSelection="questionListMode === 'crosstab' ? crosstabSecondQuestion : null"
        @question-selected="handleQuestionListSelection"
        @crosstab-selection="handleQuestionListCrosstabSelection"
        @expanded-blocks-changed="handleExpandedBlocksChanged"
      />
      
      <!-- Resize Handle -->
      <div
        @mousedown="startResizingQuestionList"
        class="absolute top-0 right-0 bottom-0 w-1 cursor-col-resize hover:bg-blue-500 dark:hover:bg-blue-400 transition-colors group"
        title="Drag to resize"
      >
        <div class="absolute inset-y-0 -right-1 w-3"></div>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="flex-1 flex flex-col overflow-hidden">
      <Splitter layout="vertical" class="flex-1">
        <!-- Top Panel: Context Area (Response Results + Question/Block Stem) -->
        <SplitterPanel :size="30" :minSize="15">
          <div v-if="selectedQuestionWithResponses && (activeSection === 'results' || activeSection === 'crosstab' || activeSection === 'statsig')" 
               class="h-full bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 overflow-auto">
            <ResultsTable 
              :key="selectedQuestionWithResponses.id"
              :question="selectedQuestionWithResponses"
              :columnMode="columnMode"
              :surveyId="surveyId"
              :activeSection="activeSection"
              @column-mode-changed="handleColumnModeChanged"
              @analyze-crosstab="handleAnalyzeCrosstab"
            />
          </div>
        </SplitterPanel>

        <!-- Bottom Panel: Tab Navigation + Analysis Area -->
        <SplitterPanel :size="70" :minSize="40">
          <div class="h-full flex flex-col">
            <!-- Tab Navigation -->
            <div class="flex-shrink-0 primevue-tabs-wrapper">
              <Tabs 
                :value="activeSection" 
                @update:value="changeTab"
              >
                <TabList>
                  <Tab v-for="tab in analysisTabs" :key="tab.id" :value="tab.id">
                    {{ tab.label }}
                    <span v-if="tab.badge" class="ml-1.5 text-[10px] opacity-60">
                      ({{ tab.badge }})
                    </span>
                  </Tab>
                </TabList>
              </Tabs>
            </div>

            <!-- Analysis Area -->
            <div class="flex-1 overflow-hidden bg-gray-50 dark:bg-gray-900">
              <!-- Split View: Results Panel + Crosstab side-by-side -->
              <div v-show="splitViewEnabled && activeSection === 'crosstab'" class="h-full flex p-4">
                <!-- Results View (Left) - Shows selected question results only -->
                <div :style="{ width: splitViewLeftWidth + '%' }" class="h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden flex flex-col">
                  <div class="bg-gray-100 dark:bg-gray-900 px-3 py-1.5 border-b border-gray-200 dark:border-gray-700">
                    <h3 class="text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wide">Results</h3>
                  </div>
                  <div class="flex-1 overflow-hidden">
                    <ResultsView 
                      :surveyId="surveyId"
                      :preselectedQuestionId="selectedQuestionForSplit?.id"
                      :hideSidebar="true"
                    />
                  </div>
                </div>
                
                <!-- Resizable Splitter with padding -->
                <div class="px-2 flex items-stretch">
                  <div 
                    class="w-0.5 bg-gray-300 dark:bg-gray-600 hover:bg-blue-500 dark:hover:bg-blue-400 hover:w-1 cursor-col-resize transition-all"
                    @mousedown="startResize"
                  ></div>
                </div>
                
                <!-- Crosstab (Right) -->
                <div :style="{ width: (100 - splitViewLeftWidth) + '%' }" class="h-full bg-white dark:bg-gray-800 rounded-lg shadow-lg overflow-hidden flex flex-col">
                  <div class="bg-gray-100 dark:bg-gray-900 px-3 py-1.5 border-b border-gray-200 dark:border-gray-700">
                    <h3 class="text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wide">Crosstab Analysis</h3>
                  </div>
                  <div class="flex-1 overflow-hidden">
                    <CrosstabView 
                      v-show="true"
                      :surveyId="surveyId"
                      :initialFirstQuestion="crosstabFirstQuestion"
                      :initialSecondQuestion="crosstabSecondQuestion"
                      @selections-changed="handleCrosstabSelectionsChanged"
                    />
                  </div>
                </div>
              </div>

              <!-- Regular Single View -->
              <div v-if="!splitViewEnabled || activeSection !== 'crosstab'" class="h-full overflow-hidden p-4">
                <!-- Results View (Chart only, table is in context area) -->
                <ResultsView 
                  v-show="activeSection === 'results'" 
                  :surveyId="surveyId"
                  :preselectedQuestionId="selectedQuestionId"
                  :hideSidebar="true"
                  :hideTable="true"
                  :initialQuestionId="selectedQuestionId"
                  :initialExpandedBlocks="expandedBlocks"
                  :initialColumnMode="columnMode"
                  :initialInfoExpanded="infoExpanded"
                  :initialInfoTab="infoTab"
                  @question-selected="handleQuestionSelected"
                  @expanded-blocks-changed="handleExpandedBlocksChanged"
                  @column-mode-changed="handleColumnModeChanged"
                  @info-expanded-changed="handleInfoExpandedChanged"
                  @info-tab-changed="handleInfoTabChanged"
                  @analyze-crosstab="handleAnalyzeCrosstab"
                />

                <!-- Crosstab View (Table is shown above) -->
                <CrosstabView 
                  v-show="activeSection === 'crosstab'" 
                  :surveyId="surveyId"
                  :hideSidebar="true"
                  :initialFirstQuestion="crosstabFirstQuestion"
                  :initialSecondQuestion="crosstabSecondQuestion"
                  @selections-changed="handleCrosstabSelectionsChanged"
                />

                <!-- Stat Sig View -->
                <StatSigView 
                  v-show="activeSection === 'statsig'" 
                  :surveyId="surveyId"
                  :selectedQuestion="selectedQuestionWithResponses"
                  @question-selected="handleStatSigQuestionSelected"
                />

                <!-- Index View -->
                <div v-show="activeSection === 'index'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 12l3-3 3 3 4-4M8 21l4-4 4 4M3 4h18M4 4h16v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4z" />
                    </svg>
                    <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Index</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Quickest way to select target groups</p>
                    <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★★ Priority - Coming soon...</p>
                  </div>
                </div>

                <!-- Index + View -->
                <div v-show="activeSection === 'indexplus'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                    </svg>
                    <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Index +</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Compare two questions within a block</p>
                    <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★ Priority - Coming soon...</p>
                  </div>
                </div>

                <!-- Profile View -->
                <div v-show="activeSection === 'profile'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                    </svg>
                    <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Profile</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Quick targeting and media strategy advice</p>
                    <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★★ Priority - Coming soon...</p>
                  </div>
                </div>

                <!-- One Response View -->
                <div v-show="activeSection === 'oneresponse'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="mx-auto h-12 w-12 text-blue-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                    </svg>
                    <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">One Response</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Compare specific responses across a block</p>
                    <p class="mt-1 text-xs text-gray-400 dark:text-gray-500">★★★ Priority - Coming soon...</p>
                  </div>
                </div>

                <!-- Full Block View -->
                <div v-show="activeSection === 'fullblock'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="mx-auto h-12 w-12 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 5a1 1 0 011-1h14a1 1 0 011 1v2a1 1 0 01-1 1H5a1 1 0 01-1-1V5zM4 13a1 1 0 011-1h6a1 1 0 011 1v6a1 1 0 01-1 1H5a1 1 0 01-1-1v-6zM16 13a1 1 0 011-1h2a1 1 0 011 1v6a1 1 0 01-1 1h-2a1 1 0 01-1-1v-6z" />
                    </svg>
                    <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Full Block</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">See patterns when graphically displayed</p>
                    <p class="mt-1 text-xs text-yellow-600 dark:text-yellow-400">★★★★★ Critical Priority - Coming soon...</p>
                  </div>
                </div>

                <!-- Matching Blocks View -->
                <div v-show="activeSection === 'matchingblocks'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="mx-auto h-12 w-12 text-yellow-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 17V7m0 10a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h2a2 2 0 012 2m0 10a2 2 0 002 2h2a2 2 0 002-2M9 7a2 2 0 012-2h2a2 2 0 012 2m0 10V7m0 10a2 2 0 002 2h2a2 2 0 002-2V7a2 2 0 00-2-2h-2a2 2 0 00-2 2" />
                    </svg>
                    <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Matching Blocks</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">The researcher's technological storyteller</p>
                    <p class="mt-1 text-xs text-yellow-600 dark:text-yellow-400">★★★★★ Critical Priority - Coming soon...</p>
                  </div>
                </div>

                <!-- Questions View -->
                <QuestionsView v-show="activeSection === 'questions'" :surveyId="surveyId" />

                <!-- Data View -->
                <DataView v-show="activeSection === 'dataview'" :surveyId="surveyId" />

                <!-- Data Cleansing -->
                <div v-show="activeSection === 'cleansing'" class="h-full flex items-center justify-center">
                  <div class="text-center">
                    <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4h13M3 8h9m-9 4h6m4 0l4-4m0 0l4 4m-4-4v12" />
                    </svg>
                    <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Data Cleansing</h3>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Coming soon...</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, computed, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Tabs from 'primevue/tabs'
import TabList from 'primevue/tablist'
import Tab from 'primevue/tab'
import Splitter from 'primevue/splitter'
import SplitterPanel from 'primevue/splitterpanel'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import SidebarButton from '../components/SidebarButton.vue'
import ResultsView from '../components/Analytics/ResultsView.vue'
import CrosstabView from '../components/Analytics/CrosstabView.vue'
import StatSigView from '../components/Analytics/StatSigView.vue'
import QuestionsView from '../components/Analytics/QuestionsView.vue'
import DataView from '../components/Analytics/DataView.vue'
import QuestionListSelector from '../components/Analytics/QuestionListSelector.vue'
import CondensedResultsTable from '../components/Analytics/CondensedResultsTable.vue'
import ResultsTable from '../components/Analytics/ResultsTable.vue'

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

// Sidebar collapse state
const sidebarCollapsed = ref(localStorage.getItem('sidebarCollapsed') === 'true')

// Question list panel width
const questionListWidth = ref(parseInt(localStorage.getItem('questionListWidth') || '320'))
const isResizingQuestionList = ref(false)

// Analysis tabs configuration
const analysisTabs = [
  { id: 'results', label: 'Results', badge: null },
  { id: 'crosstab', label: 'Crosstab', badge: null },
  { id: 'statsig', label: 'Stat Sig', badge: null },
  { id: 'fullblock', label: 'Full Block', badge: null },
  { id: 'matchingblocks', label: 'Matching Blocks', badge: null },
  { id: 'index', label: 'Index', badge: null },
  { id: 'indexplus', label: 'Index +', badge: null },
  { id: 'profile', label: 'Profile', badge: null },
  { id: 'oneresponse', label: 'One Response', badge: null },
]

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
const infoTab = ref('question')
const isInitialRouteLoad = ref(true)  // Track initial load to prevent route query interference

// Crosstab state
const crosstabFirstQuestion = ref(null)
const crosstabSecondQuestion = ref(null)
const splitViewEnabled = ref(false)

// For split view - track which question to show in results panel
const selectedQuestionForSplit = ref(null)
const splitViewLeftWidth = ref(38)  // ~570px on typical screens
const isResizing = ref(false)

// Computed property to determine question list mode based on active section
const questionListMode = computed(() => {
  if (activeSection.value === 'crosstab') {
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
function handleQuestionListCrosstabSelection({ first, second }) {
  crosstabFirstQuestion.value = first
  crosstabSecondQuestion.value = second
  
  // Update split panel to show the first question's results
  if (first) {
    selectedQuestionForSplit.value = first
    selectedQuestion.value = first
    selectedQuestionId.value = first.id
    
    // Load full question data for first variable
    loadQuestionData(first.id)
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
      // Use nextTick to avoid race condition with Vue's update cycle
      await nextTick()
      selectedQuestionWithResponses.value = fullQuestion
    }
  } catch (error) {
    console.error('Error loading question data:', error)
  }
}

const startResize = (e) => {
  isResizing.value = true
  document.addEventListener('mousemove', handleResize)
  document.addEventListener('mouseup', stopResize)
  e.preventDefault()
}

const handleResize = (e) => {
  if (!isResizing.value) return
  const splitView = e.target.closest('.flex.p-4')
  if (!splitView) return
  const rect = splitView.getBoundingClientRect()
  const offsetX = e.clientX - rect.left
  const newWidth = (offsetX / rect.width) * 100
  // Constrain between 20% and 60%
  splitViewLeftWidth.value = Math.max(20, Math.min(60, newWidth))
  e.preventDefault()
}

const stopResize = () => {
  isResizing.value = false
  saveSurveyState() // Save the new width when resizing stops
  document.removeEventListener('mousemove', handleResize)
  document.removeEventListener('mouseup', stopResize)
}

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
    splitViewLeftWidth: splitViewLeftWidth.value,
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
        splitViewLeftWidth.value = state.splitViewLeftWidth || 38  // Default ~570px on typical screens
      }
    } catch (error) {
      console.error('Error loading saved state:', error)
    }
  }
}

// Watch for changes and save state
watch(activeSection, (newSection) => {
  // When switching TO crosstab, ensure crosstab first question matches current selection
  if (newSection === 'crosstab' && selectedQuestionWithResponses.value) {
    // Only update if they differ
    if (!crosstabFirstQuestion.value || crosstabFirstQuestion.value.id !== selectedQuestionWithResponses.value.id) {
      crosstabFirstQuestion.value = selectedQuestionWithResponses.value
    }
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
  saveSurveyState()
}, { deep: true })

watch(crosstabSecondQuestion, (newVal, oldVal) => {
  saveSurveyState()
}, { deep: true })

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
    activeSection.value = 'crosstab'
    saveSurveyState()
  })
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

function startResizingQuestionList(e) {
  isResizingQuestionList.value = true
  const startX = e.clientX
  const startWidth = questionListWidth.value

  const onMouseMove = (e) => {
    if (!isResizingQuestionList.value) return
    const delta = e.clientX - startX
    const newWidth = Math.max(200, Math.min(600, startWidth + delta))
    questionListWidth.value = newWidth
  }

  const onMouseUp = () => {
    isResizingQuestionList.value = false
    localStorage.setItem('questionListWidth', questionListWidth.value.toString())
    document.removeEventListener('mousemove', onMouseMove)
    document.removeEventListener('mouseup', onMouseUp)
  }

  document.addEventListener('mousemove', onMouseMove)
  document.addEventListener('mouseup', onMouseUp)
}

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
      // Clear localStorage for the old survey
      localStorage.removeItem(getSurveyStateKey())
      
      // Clear in-memory state
      crosstabFirstQuestion.value = null
      crosstabSecondQuestion.value = null
      selectedQuestionForSplit.value = null
      selectedQuestionId.value = null
      activeSection.value = 'results'
    }
    
    loadSurveyState()
    loadSurveyInfo()
  }
})

// Watch for query parameter changes (when navigating from Stat Sig to Crosstab)
watch(() => route.query, (newQuery, oldQuery) => {
  // Update section if it changed
  if (newQuery.section && newQuery.section !== activeSection.value) {
    activeSection.value = newQuery.section
  }
  
  // Only apply questionId from URL on initial load or if it explicitly changed
  // Don't override manual selections from sidebar
  if (newQuery.questionId && isInitialRouteLoad.value) {
    selectedQuestionId.value = newQuery.questionId
    loadQuestionData(newQuery.questionId)
    isInitialRouteLoad.value = false
  }
  
  // Handle crosstab navigation parameters
  if (newQuery.firstQuestion && newQuery.secondQuestion) {
    loadQuestionsForCrosstab(newQuery.firstQuestion, newQuery.secondQuestion)
  }
}, { deep: true })

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
</style>
