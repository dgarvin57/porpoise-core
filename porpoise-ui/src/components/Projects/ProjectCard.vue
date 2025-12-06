<template>
  <div class="relative h-full">
    <div :class="[
      'bg-white dark:bg-gray-800 shadow-sm h-full flex flex-col border-2 min-h-[200px]',
      props.isExpanded 
        ? 'rounded-t-lg border-b-0 border-blue-600/70 dark:border-blue-500/70'
        : props.isFocused
          ? 'rounded-lg border-blue-500/50 dark:border-blue-400/50 overflow-hidden'
          : isActive
            ? 'rounded-lg border-blue-600/70 dark:border-blue-500/70 overflow-hidden'
            : 'rounded-lg border-gray-200 dark:border-gray-700 hover:shadow-md overflow-hidden'
    ]">
    <!-- All Projects - Expandable -->
    <div class="flex-1 flex flex-col relative">
      <!-- Project Header -->
      <div
        @click="toggleExpand"
        @mousedown="isActive = true"
        @mouseup="isActive = false"
        @mouseleave="isActive = false"
        class="p-6 cursor-pointer flex-1 flex flex-col justify-between min-w-0"
      >
        <!-- Top row: folder + content + icons (unchanged) -->
        <div class="flex items-start justify-between mb-4">
          <div class="flex items-start space-x-4 flex-1 min-w-0 pr-8">
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
                ({{ project.surveyCount }} {{ project.surveyCount === 1 ? 'survey' : 'surveys' }})
              </span>
              <p v-if="project.clientName" class="text-sm text-gray-600 dark:text-gray-400 mt-1">
                {{ project.clientName }}
              </p>
              <p v-if="project.description" class="text-sm text-gray-500 dark:text-gray-400 mt-2 line-clamp-2">
                {{ project.description }}
              </p>
            </div>
          </div>
          
          <!-- Gear, Status, Delete Icon -->
          <div class="flex items-center space-x-3 ml-4 flex-shrink-0">
            <svg
              @click.stop="handleGearClick"
              class="w-4 h-4 text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 transition-colors cursor-pointer"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              title="Project settings"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            <span
              v-if="project.status"
              :class="getStatusClass(project.status)"
              class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
            >
              {{ project.status }}
            </span>
            <svg
              @click.stop="deleteProject"
              class="w-4 h-4 text-gray-400 hover:text-red-600 dark:hover:text-red-400 transition-colors cursor-pointer"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
              title="Delete project"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </div>
          
          <!-- Expand Icon -->
          <div class="flex items-center ml-4 flex-shrink-0">
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
        
        <!-- Metadata - at the bottom, left-aligned with content -->
        <div class="flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-gray-500 dark:text-gray-400 pl-14">
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
      
      <!-- Client Logo - positioned absolutely on right side, vertically centered, aligned with dropdown -->
      <img
        v-if="clientLogo"
        :src="clientLogo"
        alt="Client Logo"
        class="absolute right-12 top-1/2 -translate-y-1/2 max-h-20 max-w-32 object-contain pointer-events-none"
      />

      <!-- Expanded Survey List -->
      <div
        v-if="props.isExpanded"
        class="absolute left-[-2px] right-[-2px] top-full z-[60] bg-gray-50 dark:bg-gray-900/95 backdrop-blur-sm rounded-b-lg border-x-2 border-b-2 border-blue-600/70 dark:border-blue-500/70 shadow-2xl overflow-hidden"
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
            @delete-survey="(surveyId, surveyName) => emit('delete-survey', surveyId, surveyName)"
          />
        </div>
      </div>
      </div>
    </div>
    
    <!-- Project Settings Modal -->
    <ProjectSettingsModal
      :is-open="showSettingsModal"
      :project="project"
      @close="showSettingsModal = false"
      @saved="handleProjectSaved"
    />
  </div>
</template><script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import SurveyList from './SurveyList.vue'
import ProjectSettingsModal from './ProjectSettingsModal.vue'

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

const emit = defineEmits(['toggle-expand', 'set-focus', 'survey-click', 'clear-all', 'delete-project', 'delete-survey', 'project-updated'])

const router = useRouter()
const isActive = ref(false)
const showSettingsModal = ref(false)
const surveys = ref([])
const loadingSurveys = ref(false)
const logoBase64 = ref(null)

// Load logo lazily on mount
onMounted(async () => {
  if (!logoBase64.value && !props.project.clientLogoBase64) {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/projects/${props.project.id}`)
      if (response.data.clientLogoBase64) {
        logoBase64.value = response.data.clientLogoBase64
      }
    } catch (error) {
      // Silently fail - logo is optional
      console.debug('No logo for project:', props.project.id)
    }
  }
})

// Computed property for client logo - ensures reactivity when project updates
const clientLogo = computed(() => {
  // Use locally loaded logo if available, otherwise fall back to prop
  const base64Data = logoBase64.value || props.project.clientLogoBase64
  if (!base64Data) return null
  
  // If it already has the data URL prefix, return as-is
  if (base64Data.startsWith('data:')) {
    return base64Data
  }
  
  // Detect image type from base64 signature or default to PNG
  let mimeType = 'image/png'
  if (base64Data.startsWith('/9j/')) {
    mimeType = 'image/jpeg'
  } else if (base64Data.startsWith('iVBORw0KGgo')) {
    mimeType = 'image/png'
  } else if (base64Data.startsWith('R0lGOD')) {
    mimeType = 'image/gif'
  } else if (base64Data.startsWith('UklGR')) {
    mimeType = 'image/webp'
  }
  
  return `data:${mimeType};base64,${base64Data}`
})

async function deleteProject() {
  emit('delete-project', props.project.id, props.project.name)
}

function handleProjectSaved(updatedData) {
  // Update locally cached logo if it changed
  if (updatedData.clientLogoBase64) {
    logoBase64.value = updatedData.clientLogoBase64
  }
  // Emit event to parent with the updated data (no need to refetch from API)
  emit('project-updated', props.project.id, updatedData)
}

function handleGearClick() {
  emit('clear-all')  // Collapse all other cards
  emit('set-focus')  // Set this card as focused
  showSettingsModal.value = true
}

async function loadSurveys() {
  loadingSurveys.value = true
  try {
    const response = await axios.get(`${API_BASE_URL}/api/projects/${props.project.id}/surveys`)
    surveys.value = response.data.map(s => ({
      id: s.id,
      name: s.name,
      status: s.status,
      caseCount: s.caseCount,
      questionCount: s.questionCount,
      createdDate: s.createdDate,
      modifiedDate: s.modifiedDate
    }))
  } catch (error) {
    console.error('Error fetching surveys:', error)
  } finally {
    loadingSurveys.value = false
  }
}

async function toggleExpand() {
  emit('toggle-expand')
  emit('set-focus')
  
  // Fetch surveys if expanding and not already loaded
  if (!props.isExpanded && surveys.value.length === 0) {
    await loadSurveys()
  }
}

// Reload surveys if component mounts in expanded state (e.g., returning from navigation)
onMounted(async () => {
  if (props.isExpanded && surveys.value.length === 0) {
    await loadSurveys()
  }
})

// Watch for project changes and reload surveys if expanded
watch(() => props.project.surveyCount, async (newCount, oldCount) => {
  if (props.isExpanded && newCount !== oldCount && surveys.value.length > 0) {
    await loadSurveys()
  }
})

function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  const now = new Date()
  
  // Set time to midnight for both dates to compare just the date part
  const dateOnly = new Date(date.getFullYear(), date.getMonth(), date.getDate())
  const nowOnly = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  
  const diffTime = nowOnly - dateOnly
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
