<template>
  <div class="h-full flex">
    <!-- Question List Sidebar -->
    <aside class="w-80 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 overflow-hidden flex flex-col">
      <!-- Search Bar -->
      <div class="p-4 border-b border-gray-200 dark:border-gray-700">
        <div class="relative">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Find question"
            autocomplete="off"
            data-1p-ignore
            data-lpignore="true"
            class="w-full px-3 py-2 pl-9 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
          <svg class="absolute left-3 top-2.5 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <button
          @click="clearSearch"
          v-if="searchQuery"
          class="mt-2 text-xs text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300"
        >
          Clear Selection
        </button>
      </div>

      <!-- Question List -->
      <div class="flex-1 overflow-y-auto">
        <div v-if="loading" class="flex items-center justify-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
        </div>
        
        <div v-else-if="error" class="p-4 text-sm text-red-600 dark:text-red-400">
          {{ error }}
        </div>

        <div v-else class="p-2">
          <div
            v-for="question in filteredQuestions"
            :key="question.id"
            @click="selectQuestion(question)"
            :class="[
              'p-3 mb-2 rounded-lg cursor-pointer transition-all',
              selectedQuestion?.id === question.id
                ? 'bg-blue-50 dark:bg-blue-900/30 border-2 border-blue-500 dark:border-blue-400'
                : 'bg-gray-50 dark:bg-gray-700 border border-gray-200 dark:border-gray-600 hover:border-blue-300 dark:hover:border-blue-600'
            ]"
          >
            <div class="flex items-start space-x-2">
              <span class="text-xs font-semibold text-gray-500 dark:text-gray-400 mt-0.5">
                {{ question.label }}
              </span>
              <p class="text-sm text-gray-900 dark:text-white flex-1">
                {{ question.text }}
              </p>
            </div>
            <div class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              {{ question.responses?.length || 0 }} responses
            </div>
          </div>
        </div>
      </div>

      <!-- Total Cases -->
      <div class="p-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900">
        <div class="text-sm text-gray-600 dark:text-gray-400">
          <span class="font-medium">Total N:</span> {{ totalCases }}
        </div>
      </div>
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
        <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-6">
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <h2 class="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                {{ selectedQuestion.label }} - {{ selectedQuestion.text }}
              </h2>
              <div class="flex items-center space-x-4 text-sm text-gray-600 dark:text-gray-400">
                <span>Index: {{ selectedQuestion.index || '128' }}</span>
                <span>•</span>
                <span>Total N: {{ totalCases }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Tabs -->
        <div class="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
          <div class="flex space-x-4 px-6">
            <button
              @click="activeTab = 'results'"
              :class="[
                'px-4 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === 'results'
                  ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                  : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
              ]"
            >
              Results
            </button>
            <button
              @click="activeTab = 'chart'"
              :class="[
                'px-4 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === 'chart'
                  ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                  : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
              ]"
            >
              Chart
            </button>
            <button
              @click="activeTab = 'statistics'"
              :class="[
                'px-4 py-3 text-sm font-medium border-b-2 transition-colors',
                activeTab === 'statistics'
                  ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                  : 'border-transparent text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white'
              ]"
            >
              Statistics
            </button>
          </div>
        </div>

        <!-- Content Area -->
        <div class="flex-1 overflow-y-auto bg-gray-50 dark:bg-gray-900">
          <!-- Results Tab -->
          <div v-if="activeTab === 'results'" class="p-6">
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
              <div class="overflow-x-auto">
                <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
                  <thead class="bg-gray-50 dark:bg-gray-900">
                    <tr>
                      <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        #
                      </th>
                      <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Response
                      </th>
                      <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        %
                      </th>
                      <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Index
                      </th>
                      <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">
                        Count
                      </th>
                    </tr>
                  </thead>
                  <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
                    <tr v-for="(response, index) in selectedQuestion.responses" :key="index">
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500 dark:text-gray-400">
                        {{ index + 1 }}
                      </td>
                      <td class="px-6 py-4 text-sm text-gray-900 dark:text-white">
                        {{ response.label }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-900 dark:text-white font-medium">
                        {{ response.percentage.toFixed(1) }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {{ response.index || '—' }}
                      </td>
                      <td class="px-6 py-4 whitespace-nowrap text-sm text-right text-gray-500 dark:text-gray-400">
                        {{ response.count }}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <!-- Chart Tab -->
          <div v-else-if="activeTab === 'chart'" class="p-6">
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-6">
                Frequency Distribution: {{ selectedQuestion.text }}
              </h3>
              <QuestionChart :question="selectedQuestion" />
            </div>
          </div>

          <!-- Statistics Tab -->
          <div v-else-if="activeTab === 'statistics'" class="p-6">
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
              <h3 class="text-lg font-medium text-gray-900 dark:text-white mb-4">Statistics</h3>
              <p class="text-sm text-gray-500 dark:text-gray-400">Statistical analysis coming soon...</p>
            </div>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import axios from 'axios'
import QuestionChart from './QuestionChart.vue'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  }
})

const questions = ref([])
const selectedQuestion = ref(null)
const searchQuery = ref('')
const loading = ref(true)
const error = ref(null)
const totalCases = ref(0)
const activeTab = ref('chart') // Default to chart view

const filteredQuestions = computed(() => {
  if (!searchQuery.value) return questions.value
  const query = searchQuery.value.toLowerCase()
  return questions.value.filter(q =>
    q.text.toLowerCase().includes(query) ||
    q.label.toLowerCase().includes(query)
  )
})

function clearSearch() {
  searchQuery.value = ''
}

function selectQuestion(question) {
  selectedQuestion.value = question
  activeTab.value = 'chart' // Show chart by default when selecting a question
}

async function loadQuestions() {
  loading.value = true
  error.value = null

  try {
    const response = await axios.get(`http://localhost:5107/api/surveys/${props.surveyId}/questions`)
    questions.value = response.data
    totalCases.value = response.data[0]?.totalCases || 0
  } catch (err) {
    console.error('Error loading questions:', err)
    error.value = 'Failed to load questions. Please try again.'
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  loadQuestions()
})
</script>
