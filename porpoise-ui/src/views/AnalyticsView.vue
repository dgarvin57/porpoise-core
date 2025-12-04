<template>
  <div class="fixed inset-0 top-16 flex overflow-hidden bg-gray-50 dark:bg-gray-900">
    <!-- Sidebar Navigation -->
    <aside class="w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-y-auto">
      <div class="p-4 border-b border-gray-200 dark:border-gray-700">
        <p class="text-xs text-gray-500 dark:text-gray-400 mb-1">{{ projectName }}</p>
        <div 
          class="group relative"
          @mouseenter="isHoveringName = true"
          @mouseleave="isHoveringName = false"
        >
          <input
            v-model="editableSurveyName"
            @focus="isEditingName = true"
            @blur="saveSurveyName"
            @keyup.enter="$event.target.blur()"
            :class="[
              'w-full text-lg font-semibold bg-transparent outline-none transition-all',
              'text-gray-900 dark:text-white',
              isHoveringName || isEditingName 
                ? 'border-b-2 border-blue-500 dark:border-blue-400 cursor-text' 
                : 'border-b-2 border-transparent cursor-default'
            ]"
            :title="editableSurveyName"
          />
        </div>
        <div class="flex items-center space-x-4 mt-2 text-sm text-gray-600 dark:text-gray-400">
          <span>{{ questionCount }} questions</span>
          <span>•</span>
          <span>{{ totalCases }} cases</span>
        </div>
      </div>

      <!-- Navigation Menu -->
      <nav class="p-2">
        <div class="space-y-1">
          <!-- Results Section -->
          <button
            @click="activeSection = 'results'"
            :class="[
              'w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
              activeSection === 'results'
                ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
            ]"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
            <span>Results</span>
          </button>

          <!-- Crosstab Section -->
          <button
            @click="activeSection = 'crosstab'"
            :class="[
              'w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
              activeSection === 'crosstab'
                ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
            ]"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
            <span>Crosstab</span>
          </button>

          <!-- Questions Section -->
          <button
            @click="activeSection = 'questions'"
            :class="[
              'w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
              activeSection === 'questions'
                ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
            ]"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            <span>Questions</span>
          </button>

          <!-- Data View Section -->
          <button
            @click="activeSection = 'data'"
            :class="[
              'w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
              activeSection === 'data'
                ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
            ]"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
            <span>Data View</span>
          </button>

          <!-- Data Cleansing Section -->
          <button
            @click="activeSection = 'cleansing'"
            :class="[
              'w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
              activeSection === 'cleansing'
                ? 'bg-blue-50 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300'
                : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
            ]"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4h13M3 8h9m-9 4h6m4 0l4-4m0 0l4 4m-4-4v12" />
            </svg>
            <span>Data Cleansing</span>
          </button>
        </div>

        <!-- Divider -->
        <div class="my-4 border-t border-gray-200 dark:border-gray-700"></div>

        <!-- Back to Projects -->
        <button
          @click="backToProjects"
          class="w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
          </svg>
          <span>Back to Projects</span>
        </button>
      </nav>
    </aside>

    <!-- Main Content Area -->
    <main class="flex-1 overflow-hidden">
      <!-- Results View -->
      <ResultsView 
        v-if="activeSection === 'results'" 
        :surveyId="surveyId"
        :surveyNotes="surveyNotes"
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
        @survey-notes-updated="handleSurveyNotesUpdated"
      />

      <!-- Crosstab View -->
      <div v-else-if="activeSection === 'crosstab'" class="h-full flex items-center justify-center">
        <div class="text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Crosstab Analysis</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Coming soon...</p>
        </div>
      </div>

      <!-- Questions View -->
      <QuestionsView v-else-if="activeSection === 'questions'" :surveyId="surveyId" />

      <!-- Data View -->
      <div v-else-if="activeSection === 'data'" class="h-full flex items-center justify-center">
        <div class="text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M3 14h18m-9-4v8m-7 0h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Data View</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Coming soon...</p>
        </div>
      </div>

      <!-- Data Cleansing -->
      <div v-else-if="activeSection === 'cleansing'" class="h-full flex items-center justify-center">
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
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import ResultsView from '../components/Analytics/ResultsView.vue'
import QuestionsView from '../components/Analytics/QuestionsView.vue'

const route = useRoute()
const router = useRouter()

const surveyId = ref(route.params.id)
const surveyName = ref('Loading...')
const editableSurveyName = ref('')
const isHoveringName = ref(false)
const isEditingName = ref(false)
const projectName = ref('')
const surveyNotes = ref('')
const totalCases = ref(0)
const questionCount = ref(0)
const activeSection = ref('results')

// State management helpers
const selectedQuestionId = ref(null)
const expandedBlocks = ref([])
const columnMode = ref('totalN')
const infoExpanded = ref(false)
const infoTab = ref('question')

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
    timestamp: Date.now()
  }
  localStorage.setItem(getSurveyStateKey(), JSON.stringify(state))
  
  // Also update query params for URL state
  router.replace({
    query: {
      ...route.query,
      section: activeSection.value,
      question: selectedQuestionId.value || undefined
    }
  })
}

function loadSurveyState() {
  // First check URL query params (highest priority)
  if (route.query.section) {
    activeSection.value = route.query.section
  }
  if (route.query.question) {
    selectedQuestionId.value = route.query.question
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
        if (!route.query.question) {
          selectedQuestionId.value = state.selectedQuestionId || null
        }
        expandedBlocks.value = state.expandedBlocks || []
        columnMode.value = state.columnMode || 'totalN'
        infoExpanded.value = state.infoExpanded || false
        infoTab.value = state.infoTab || 'question'
      }
    } catch (error) {
      console.error('Error loading saved state:', error)
    }
  }
}

// Watch for changes and save state
watch(activeSection, () => {
  saveSurveyState()
})

watch(selectedQuestionId, () => {
  saveSurveyState()
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

function handleSurveyNotesUpdated(notes) {
  surveyNotes.value = notes
}

async function loadSurveyInfo() {
  try {
    const response = await axios.get(`http://localhost:5107/api/surveys/${surveyId.value}`)
    surveyName.value = response.data.surveyName || response.data.name
    editableSurveyName.value = surveyName.value
    surveyNotes.value = response.data.surveyNotes || ''
    
    // Load project info
    if (response.data.projectId) {
      try {
        const projectResponse = await axios.get(`http://localhost:5107/api/projects/${response.data.projectId}`)
        projectName.value = projectResponse.data.projectName || 'Project'
      } catch (error) {
        console.error('Error loading project info:', error)
      }
    }

    // Get question count and case count from stats
    try {
      const statsResponse = await axios.get(`http://localhost:5107/api/surveys/${surveyId.value}/stats`)
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
      await axios.patch(`http://localhost:5107/api/surveys/${surveyId.value}`, {
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

function backToProjects() {
  router.push('/')
}

onMounted(() => {
  loadSurveyState()
  loadSurveyInfo()
})

// Watch for route changes (when navigating to the same route with different params)
watch(() => route.params.id, (newId) => {
  if (newId) {
    surveyId.value = newId
    loadSurveyState()
    loadSurveyInfo()
  }
})
</script>
