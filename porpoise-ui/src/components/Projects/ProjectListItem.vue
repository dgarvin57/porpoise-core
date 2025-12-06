<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 hover:shadow-md transition-shadow">
    <!-- Single Survey Project - Direct Click -->
    <div
      v-if="project.surveyCount === 1"
      @click="navigateToAnalytics"
      class="px-4 py-3 cursor-pointer"
    >
      <div class="flex items-center space-x-4">
        <!-- Icon -->
        <svg class="w-6 h-6 text-blue-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        
        <!-- Project Name & Client -->
        <div class="flex-1 min-w-0">
          <h3 class="text-sm font-semibold text-gray-900 dark:text-white truncate">
            {{ project.name }}
          </h3>
          <p v-if="project.clientName" class="text-xs text-gray-600 dark:text-gray-400 truncate">
            {{ project.clientName }}
          </p>
        </div>
        
        <!-- Stats -->
        <div class="flex items-center space-x-4 text-xs text-gray-500 dark:text-gray-400">
          <span class="flex items-center whitespace-nowrap">
            <svg class="w-3.5 h-3.5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            {{ project.caseCount || 0 }}
          </span>
          <span class="flex items-center whitespace-nowrap">
            <svg class="w-3.5 h-3.5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {{ project.questionCount || 0 }}
          </span>
          <span v-if="project.createdAt" class="whitespace-nowrap">
            {{ formatDate(project.createdAt) }}
          </span>
        </div>
        
        <!-- Status Badge -->
        <div v-if="project.status" class="flex-shrink-0">
          <span
            :class="getStatusClass(project.status)"
            class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
          >
            {{ project.status }}
          </span>
        </div>
      </div>
    </div>

    <!-- Multi-Survey Project - Expandable -->
    <div v-else>
      <!-- Project Header -->
      <div
        @click="toggleExpand"
        class="px-4 py-3 cursor-pointer"
      >
        <div class="flex items-center space-x-4">
          <!-- Folder Icon + Expand Arrow -->
          <div class="flex items-center space-x-2 flex-shrink-0">
            <svg
              :class="{ 'rotate-90': isExpanded }"
              class="w-4 h-4 text-gray-400 transition-transform"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
            <svg class="w-6 h-6 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
            </svg>
          </div>
          
          <!-- Project Name & Client -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center space-x-2">
              <h3 class="text-sm font-semibold text-gray-900 dark:text-white truncate">
                {{ project.name }}
              </h3>
              <span class="text-xs text-gray-500 dark:text-gray-400 whitespace-nowrap">
                ({{ project.surveyCount }} surveys)
              </span>
            </div>
            <p v-if="project.clientName" class="text-xs text-gray-600 dark:text-gray-400 truncate">
              {{ project.clientName }}
            </p>
          </div>
          
          <!-- Dates -->
          <div class="flex items-center space-x-4 text-xs text-gray-500 dark:text-gray-400">
            <span v-if="project.createdAt" class="whitespace-nowrap">
              Created {{ formatDate(project.createdAt) }}
            </span>
            <span v-if="project.lastModified" class="whitespace-nowrap">
              Modified {{ formatDate(project.lastModified) }}
            </span>
          </div>
          
          <!-- Status Badge -->
          <div v-if="project.status" class="flex-shrink-0">
            <span
              :class="getStatusClass(project.status)"
              class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
            >
              {{ project.status }}
            </span>
          </div>
        </div>
      </div>

      <!-- Expanded Survey List -->
      <div
        v-if="isExpanded"
        class="border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-900/50 px-4 py-2"
      >
        <div v-if="loadingSurveys" class="flex items-center justify-center py-2">
          <div class="animate-spin rounded-full h-5 w-5 border-b-2 border-blue-500"></div>
        </div>
        <div v-else-if="surveys.length > 0" class="space-y-1">
          <div
            v-for="survey in surveys"
            :key="survey.id"
            @click="navigateToSurvey(survey.id)"
            class="flex items-center space-x-3 px-3 py-2 rounded-md hover:bg-white dark:hover:bg-gray-800 cursor-pointer transition-colors"
          >
            <svg class="w-4 h-4 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <span class="text-xs font-medium text-gray-900 dark:text-white flex-1 truncate">
              {{ survey.name }}
            </span>
            <div class="flex items-center space-x-3 text-xs text-gray-500 dark:text-gray-400">
              <span class="flex items-center whitespace-nowrap">
                <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                {{ survey.caseCount || 0 }}
              </span>
              <span class="flex items-center whitespace-nowrap">
                <svg class="w-3 h-3 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {{ survey.questionCount || 0 }}
              </span>
            </div>
          </div>
        </div>
        <div v-else class="text-xs text-gray-500 dark:text-gray-400 italic py-2 text-center">
          No surveys found
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { API_BASE_URL } from '@/config/api'
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const props = defineProps({
  project: {
    type: Object,
    required: true
  }
})

const router = useRouter()
const isExpanded = ref(false)
const surveys = ref([])
const loadingSurveys = ref(false)

async function toggleExpand() {
  isExpanded.value = !isExpanded.value
  
  // Fetch surveys if expanding and not already loaded
  if (isExpanded.value && surveys.value.length === 0 && props.project.surveyCount > 1) {
    loadingSurveys.value = true
    try {
      const response = await axios.get(`${API_BASE_URL}/api/projects/${props.project.id}/surveys`)
      // Map API response to component format
      surveys.value = response.data.map(s => ({
        id: s.Id,
        name: s.SurveyName,
        status: s.Status,
        caseCount: s.CaseCount,
        questionCount: s.QuestionCount,
        createdDate: s.CreatedDate,
        modifiedDate: s.ModifiedDate
      }))
    } catch (error) {
      console.error('Error fetching surveys:', error)
    } finally {
      loadingSurveys.value = false
    }
  }
}

function navigateToAnalytics() {
  // For single-survey projects, navigate directly
  if (props.project.surveyCount === 1) {
    router.push(`/analytics/${props.project.id}`)
  }
}

function navigateToSurvey(surveyId) {
  router.push(`/analytics/${surveyId}`)
}

function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  const now = new Date()
  const diffTime = Math.abs(now - date)
  const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24))
  
  if (diffDays === 0) return 'Today'
  if (diffDays === 1) return 'Yesterday'
  if (diffDays < 7) return `${diffDays}d ago`
  if (diffDays < 30) return `${Math.floor(diffDays / 7)}w ago`
  
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
}

function getStatusClass(status) {
  const statusMap = {
    'Active': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300',
    'Completed': 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300',
    'Archived': 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300',
    'Draft': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300'
  }
  return statusMap[status] || 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
}
</script>
