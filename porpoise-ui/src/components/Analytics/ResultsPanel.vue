<template>
  <div class="h-full flex flex-col bg-gray-50 dark:bg-gray-900">
    <!-- Debug Info -->
    <div class="p-2 bg-yellow-100 text-xs">
      <div>Component Loaded: YES</div>
      <div>SurveyId: {{ surveyId }}</div>
      <div>SelectedQuestion: {{ selectedQuestion?.label || 'null' }}</div>
      <div>Loading: {{ loading }}</div>
      <div>QuestionData: {{ questionData ? 'loaded' : 'null' }}</div>
      <div>QuestionData.label: {{ questionData?.label || 'N/A' }}</div>
      <div>QuestionData.results: {{ questionData?.results?.length || 0 }} items</div>
    </div>
    
    <!-- Loading State -->
    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="text-center">
        <svg class="animate-spin h-10 w-10 text-blue-600 dark:text-blue-400 mx-auto" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">Loading question data...</p>
      </div>
    </div>

    <!-- Results table and chart only (no question selector) -->
    <div v-else-if="questionData && questionData.results" class="flex-1 flex flex-col overflow-hidden p-4">
      <!-- Question Title -->
      <div class="mb-4">
        <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
          {{ questionData.label }}
        </h3>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          {{ questionData.qstNumber }} • {{ getVariableTypeName(questionData.variableType) }}
        </p>
      </div>

      <!-- Results Table -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden mb-4">
        <table class="min-w-full divide-y divide-gray-200 dark:divide-gray-700">
          <thead class="bg-gray-50 dark:bg-gray-900">
            <tr>
              <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                Response
              </th>
              <th scope="col" class="px-4 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                Count
              </th>
              <th scope="col" class="px-4 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-400 uppercase">
                Percent
              </th>
            </tr>
          </thead>
          <tbody class="bg-white dark:bg-gray-800 divide-y divide-gray-200 dark:divide-gray-700">
            <tr v-for="result in questionData.results" :key="result.respValue">
              <td class="px-4 py-3 text-sm text-gray-900 dark:text-white">
                {{ result.respLabel }}
              </td>
              <td class="px-4 py-3 text-sm text-right text-gray-700 dark:text-gray-300">
                {{ result.count || 0 }}
              </td>
              <td class="px-4 py-3 text-sm text-right text-gray-700 dark:text-gray-300">
                {{ result.percent }}%
              </td>
            </tr>
            <tr class="bg-gray-50 dark:bg-gray-900 font-semibold">
              <td class="px-4 py-3 text-sm text-gray-900 dark:text-white">
                Total
              </td>
              <td class="px-4 py-3 text-sm text-right text-gray-900 dark:text-white">
                {{ questionData.totalN || 0 }}
              </td>
              <td class="px-4 py-3 text-sm text-right text-gray-900 dark:text-white">
                100.0%
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Chart -->
      <div v-if="chartData" class="bg-white dark:bg-gray-800 rounded-lg shadow p-4 flex-1 min-h-0">
        <div ref="chartContainer" class="h-full"></div>
      </div>
    </div>

    <!-- No Question Selected -->
    <div v-else class="flex-1 flex items-center justify-center">
      <div class="text-center text-gray-500 dark:text-gray-400">
        <svg class="mx-auto h-12 w-12 mb-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
        </svg>
        <p class="text-sm">Select a question from the crosstab to view results</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, onBeforeUnmount } from 'vue'
import axios from 'axios'
import * as echarts from 'echarts'

const props = defineProps({
  surveyId: {
    type: String,
    required: true
  },
  selectedQuestion: {
    type: Object,
    default: null
  }
})

const chartContainer = ref(null)
const chartInstance = ref(null)
const chartData = ref(null)
const questionData = ref(null)
const loading = ref(false)

const getVariableTypeName = (type) => {
  const types = {
    1: 'Single Choice',
    2: 'Multiple Choice',
    3: 'Numeric',
    4: 'Open-ended'
  }
  return types[type] || 'Unknown'
}

const loadQuestionData = async () => {
  
  if (!props.selectedQuestion?.id) {
    questionData.value = null
    return
  }

  loading.value = true
  try {
    const response = await axios.get(`/api/surveys/${props.surveyId}/questions/${props.selectedQuestion.id}/results`)
    questionData.value = response.data
    
    // Wait for next tick to ensure DOM is updated
    await new Promise(resolve => setTimeout(resolve, 0))
    createChart()
  } catch (error) {
    console.error('ResultsPanel: Error loading question data:', error)
    console.error('ResultsPanel: Error details:', error.response?.data)
  } finally {
    loading.value = false
  }
}

const createChart = () => {
  if (!chartContainer.value || !questionData.value) return

  // Initialize chart if needed
  if (!chartInstance.value) {
    chartInstance.value = echarts.init(chartContainer.value)
  }

  const results = questionData.value.results || []
  const labels = results.map(r => r.respLabel)
  const values = results.map(r => r.percent || 0)

  const option = {
    tooltip: {
      trigger: 'axis',
      axisPointer: {
        type: 'shadow'
      },
      formatter: (params) => {
        const param = params[0]
        return `${param.name}<br/>${param.value.toFixed(1)}%`
      }
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      data: labels,
      axisLabel: {
        rotate: labels.some(l => l.length > 15) ? 45 : 0,
        interval: 0
      }
    },
    yAxis: {
      type: 'value',
      name: 'Percentage',
      axisLabel: {
        formatter: '{value}%'
      }
    },
    series: [{
      type: 'bar',
      data: values,
      itemStyle: {
        color: '#3b82f6'
      }
    }]
  }

  chartInstance.value.setOption(option)
  chartData.value = option
}

watch(() => props.selectedQuestion, (newQuestion) => {
  if (newQuestion && newQuestion.id) {
    loadQuestionData()
  }
}, { immediate: true, deep: true })

onMounted(() => {
  loadQuestionData()
  window.addEventListener('resize', () => {
    chartInstance.value?.resize()
  })
})

onBeforeUnmount(() => {
  chartInstance.value?.dispose()
})
</script>
