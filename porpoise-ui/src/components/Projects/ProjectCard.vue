<template>
  <div class="relative h-full">
    <div :class="[
      'bg-white dark:bg-gray-800 shadow-sm overflow-hidden h-full flex flex-col',
      props.isExpanded 
        ? 'rounded-t-lg border-x-2 border-t-2 border-b-0 border-blue-600/70 dark:border-blue-500/70'
        : props.isFocused
          ? 'rounded-lg border-2 border-blue-500/50 dark:border-blue-400/50'
          : isActive
            ? 'rounded-lg border-2 border-blue-600/70 dark:border-blue-500/70'
            : 'rounded-lg border border-gray-200 dark:border-gray-700 hover:shadow-md'
    ]">
    <!-- Single Survey Project - Direct Click -->
    <div
      v-if="project.surveyCount === 1"
      @click="handleSingleSurveyClick"
      @mousedown="isActive = true"
      @mouseup="isActive = false"
      @mouseleave="isActive = false"
      class="p-6 cursor-pointer flex-1 flex flex-col"
    >
      <div class="flex items-start justify-between min-w-0">
        <div class="flex items-start space-x-4 flex-1 min-w-0">
          <!-- Icon -->
          <div class="flex-shrink-0">
            <svg class="w-10 h-10 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
          </div>
          
          <!-- Content -->
          <div class="flex-1 min-w-0">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white truncate">
              {{ project.name }}
            </h3>
            <p v-if="project.clientName" class="text-sm text-gray-600 dark:text-gray-400 mt-1">
              {{ project.clientName }}
            </p>
            <p v-if="project.description" class="text-sm text-gray-500 dark:text-gray-400 mt-2 line-clamp-2">
              {{ project.description }}
            </p>
            
            <!-- Metadata -->
            <div class="flex flex-wrap items-center gap-x-4 gap-y-1 mt-3 text-xs text-gray-500 dark:text-gray-400">
              <span class="flex items-center flex-shrink-0">
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                </svg>
                {{ project.caseCount || 0 }} cases
              </span>
              <span class="flex items-center flex-shrink-0">
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {{ project.questionCount || 0 }} questions
              </span>
              <span v-if="project.createdAt" class="flex items-center flex-shrink-0">
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span class="truncate">Created {{ formatDate(project.createdAt) }}</span>
              </span>
              <span v-if="project.lastModified" class="flex items-center flex-shrink-0">
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                <span class="truncate">Modified {{ formatDate(project.lastModified) }}</span>
              </span>
            </div>
          </div>
        </div>
        
        <!-- Status Badge -->
        <div v-if="project.status" class="ml-4 flex-shrink-0">
          <span
            :class="getStatusClass(project.status)"
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
          >
            {{ project.status }}
          </span>
        </div>
      </div>
    </div>

    <!-- Multi-Survey Project - Expandable -->
    <div v-else class="flex-1 flex flex-col">
      <!-- Project Header -->
      <div
        @click="toggleExpand"
        @mousedown="isActive = true"
        @mouseup="isActive = false"
        @mouseleave="isActive = false"
        class="p-6 cursor-pointer flex items-start justify-between min-w-0 flex-1"
      >
        <div class="flex items-start space-x-4 flex-1 min-w-0">
          <!-- Folder Icon -->
          <div class="flex-shrink-0">
            <svg class="w-10 h-10 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
            </svg>
          </div>
          
          <!-- Content -->
          <div class="flex-1 min-w-0">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white truncate">
              {{ project.name }}
            </h3>
            <span class="text-sm text-gray-500 dark:text-gray-400 mt-1 block 5xl:inline 5xl:ml-2 5xl:mt-0">
              ({{ project.surveyCount }} surveys)
            </span>
            <p v-if="project.clientName" class="text-sm text-gray-600 dark:text-gray-400 mt-1">
              {{ project.clientName }}
            </p>
            <p v-if="project.description" class="text-sm text-gray-500 dark:text-gray-400 mt-2 line-clamp-2">
              {{ project.description }}
            </p>
            
            <!-- Metadata -->
            <div class="flex flex-wrap items-center gap-x-4 gap-y-1 mt-3 text-xs text-gray-500 dark:text-gray-400">
              <span v-if="project.createdAt" class="flex items-center flex-shrink-0">
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <span class="truncate">Created {{ formatDate(project.createdAt) }}</span>
              </span>
              <span v-if="project.lastModified" class="flex items-center flex-shrink-0">
                <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                </svg>
                <span class="truncate">Modified {{ formatDate(project.lastModified) }}</span>
              </span>
            </div>
          </div>
        </div>
        
        <!-- Status & Expand Icon -->
        <div class="flex items-center space-x-3 ml-4 flex-shrink-0">
          <span
            v-if="project.status"
            :class="getStatusClass(project.status)"
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
          >
            {{ project.status }}
          </span>
          <svg
            :class="{ 'rotate-180': isExpanded }"
            class="w-5 h-5 text-gray-400 transition-transform"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
          </svg>
        </div>
      </div>

      <!-- Expanded Survey List -->
      <div
        v-if="props.isExpanded"
        class="absolute left-0 right-0 top-full z-50 bg-gray-50 dark:bg-gray-900/95 backdrop-blur-sm rounded-b-lg border-x-2 border-b-2 border-blue-600/70 dark:border-blue-500/70 shadow-2xl overflow-hidden"
      >
        <div class="px-6 py-4">
          <div v-if="loadingSurveys" class="flex items-center justify-center py-4">
            <div class="animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500"></div>
          </div>
          <SurveyList 
            v-else 
            :projectId="project.id" 
            :surveys="surveys" 
            :focused-survey-id="props.focusedSurveyId"
            @survey-click="(surveyId) => emit('survey-click', surveyId)"
          />
        </div>
      </div>
    </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import SurveyList from './SurveyList.vue'

const props = defineProps({
  project: {
    type: Object,
    required: true
  },
  isExpanded: {
    type: Boolean,
    default: false
  },
  isFocused: {
    type: Boolean,
    default: false
  },
  focusedSurveyId: {
    type: String,
    default: null
  }
})

const emit = defineEmits(['toggle-expand', 'set-focus', 'survey-click', 'clear-all'])

const router = useRouter()
const isActive = ref(false)
const surveys = ref([])
const loadingSurveys = ref(false)

async function toggleExpand() {
  emit('toggle-expand')
  emit('set-focus')
  
  // Fetch surveys if expanding and not already loaded
  if (!props.isExpanded && surveys.value.length === 0 && props.project.surveyCount > 1) {
    loadingSurveys.value = true
    try {
      const response = await axios.get(`http://localhost:5107/api/projects/${props.project.id}/surveys`)
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

function handleSingleSurveyClick() {
  // Clear all expanded projects and focused surveys
  emit('clear-all')
  emit('set-focus')
  // Navigate to analytics
  if (props.project.surveyCount === 1) {
    router.push(`/analytics/${props.project.id}`)
  }
}

function navigateToAnalytics() {
  // For single-survey projects, navigate directly
  if (props.project.surveyCount === 1) {
    router.push(`/analytics/${props.project.id}`)
  }
}

// Reload surveys if component mounts in expanded state (e.g., returning from navigation)
onMounted(async () => {
  if (props.isExpanded && surveys.value.length === 0 && props.project.surveyCount > 1) {
    loadingSurveys.value = true
    try {
      const response = await axios.get(`http://localhost:5107/api/projects/${props.project.id}/surveys`)
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
      console.error('Error fetching surveys on mount:', error)
    } finally {
      loadingSurveys.value = false
    }
  }
})

function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  const now = new Date()
  const diffTime = Math.abs(now - date)
  const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24))
  
  if (diffDays === 0) return 'Today'
  if (diffDays === 1) return 'Yesterday'
  if (diffDays < 7) return `${diffDays} days ago`
  if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`
  if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`
  
  return date.toLocaleDateString()
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
