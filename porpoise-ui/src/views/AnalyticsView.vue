<template>
  <div class="flex h-screen overflow-hidden bg-gray-50 dark:bg-gray-900 pt-16">
    <!-- Sidebar Navigation -->
    <aside class="w-64 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-y-auto">
      <div class="p-4 border-b border-gray-200 dark:border-gray-700">
        <p class="text-xs text-gray-500 dark:text-gray-400 mb-1">{{ projectName }}</p>
        <h2 class="text-lg font-semibold text-gray-900 dark:text-white truncate" :title="surveyName">
          {{ surveyName }}
        </h2>
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
                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
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
                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
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
                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
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
                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
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
                : 'text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700'
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
          class="w-full flex items-center space-x-3 px-3 py-2 rounded-lg text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
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
      <ResultsView v-if="activeSection === 'results'" :surveyId="surveyId" />

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
      <div v-else-if="activeSection === 'questions'" class="h-full flex items-center justify-center">
        <div class="text-center">
          <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">Question Editor</h3>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">Coming soon...</p>
        </div>
      </div>

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
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'
import ResultsView from '../components/Analytics/ResultsView.vue'

const route = useRoute()
const router = useRouter()

const surveyId = ref(route.params.id)
const surveyName = ref('Loading...')
const projectName = ref('')
const totalCases = ref(0)
const questionCount = ref(0)
const activeSection = ref('results')

async function loadSurveyInfo() {
  try {
    const response = await axios.get(`http://localhost:5107/api/surveys/${surveyId.value}`)
    surveyName.value = response.data.surveyName || response.data.name
    
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
  }
}

function backToProjects() {
  router.push('/')
}

onMounted(() => {
  loadSurveyInfo()
})
</script>
