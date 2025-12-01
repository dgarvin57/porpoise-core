<template>
  <div v-if="surveys && surveys.length > 0" class="space-y-2">
    <div
      v-for="survey in surveys"
      :key="survey.id"
      @click="navigateToSurvey(survey.id)"
      :class="[
        'flex items-center justify-between p-3 rounded-md border transition-all cursor-pointer',
        props.focusedSurveyId === survey.id
          ? 'bg-blue-50 dark:bg-blue-900/30 border-2 border-blue-500/70 dark:border-blue-400/70 shadow-sm'
          : 'bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 hover:border-blue-300 dark:hover:border-blue-600 hover:shadow-sm'
      ]"
    >
      <div class="flex items-center space-x-3 flex-1 min-w-0">
        <!-- Survey Icon -->
        <svg class="w-5 h-5 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
        </svg>
        
        <!-- Survey Name -->
        <span class="text-sm font-medium text-gray-900 dark:text-white truncate">
          {{ survey.name }}
        </span>
        
        <!-- Stats -->
        <div class="flex items-center space-x-3 text-xs text-gray-500 dark:text-gray-400 flex-shrink-0">
          <span class="flex items-center">
            <svg class="w-3.5 h-3.5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            {{ survey.caseCount || 0 }}
          </span>
          <span class="flex items-center">
            <svg class="w-3.5 h-3.5 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
            {{ survey.questionCount || 0 }}
          </span>
        </div>
      </div>
      
      <!-- Arrow Icon -->
      <svg class="w-4 h-4 text-gray-400 flex-shrink-0 ml-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
      </svg>
    </div>
  </div>
  <div v-else class="text-sm text-gray-500 dark:text-gray-400 italic">
    No surveys found
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'

const props = defineProps({
  projectId: {
    type: String,
    required: true
  },
  surveys: {
    type: Array,
    default: () => []
  },
  focusedSurveyId: {
    type: String,
    default: null
  }
})

const emit = defineEmits(['survey-click'])

const router = useRouter()

function navigateToSurvey(surveyId) {
  emit('survey-click', surveyId)
  router.push(`/analytics/${surveyId}`)
}
</script>
