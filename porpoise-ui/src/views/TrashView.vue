<template>
  <div class="trash-view p-6">
    <div class="flex justify-between items-center mb-6">
      <h1 class="text-2xl font-bold dark:text-white">Trash</h1>
      <button 
        @click="$router.push('/')"
        class="px-4 py-2 text-sm bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 rounded"
      >
        Back to Projects
      </button>
    </div>

    <div v-if="loading" class="text-center py-8">
      <p class="text-gray-600 dark:text-gray-400">Loading deleted items...</p>
    </div>

    <div v-else-if="error" class="bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400 p-4 rounded">
      {{ error }}
    </div>

    <div v-else-if="deletedProjects.length === 0 && deletedSurveys.length === 0" 
         class="text-center py-12">
      <p class="text-gray-600 dark:text-gray-400 text-lg">Trash is empty</p>
    </div>

    <div v-else>
      <!-- Deleted Projects Section -->
      <div v-if="deletedProjects.length > 0" class="mb-8">
        <h2 class="text-xl font-semibold mb-4 dark:text-white">Deleted Projects</h2>
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
          <table class="w-full">
            <thead class="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Project Name</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Client</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Surveys</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Deleted</th>
                <th class="px-4 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
              <tr v-for="project in deletedProjects" :key="project.Id" 
                  class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                <td class="px-4 py-3 dark:text-gray-200">{{ project.ProjectName }}</td>
                <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ project.ClientName || '-' }}</td>
                <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ project.SurveyCount }}</td>
                <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ formatDate(project.DeletedDate) }}</td>
                <td class="px-4 py-3 text-right space-x-2">
                  <button 
                    @click="restoreProject(project.Id)"
                    class="px-3 py-1.5 text-xs bg-blue-600 hover:bg-blue-700 text-white rounded"
                  >
                    Restore
                  </button>
                  <button 
                    @click="permanentlyDeleteProject(project.Id, project.ProjectName)"
                    class="px-3 py-1.5 text-xs bg-red-600 hover:bg-red-700 text-white rounded"
                  >
                    Delete Forever
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Deleted Surveys Section -->
      <div v-if="deletedSurveys.length > 0">
        <h2 class="text-xl font-semibold mb-4 dark:text-white">Deleted Surveys</h2>
        <div class="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
          <table class="w-full">
            <thead class="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Survey Name</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Project</th>
                <th class="px-4 py-3 text-left text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Deleted</th>
                <th class="px-4 py-3 text-right text-xs font-medium text-gray-500 dark:text-gray-300 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
              <tr v-for="survey in deletedSurveys" :key="survey.Id" 
                  class="hover:bg-gray-50 dark:hover:bg-gray-700/50">
                <td class="px-4 py-3 dark:text-gray-200">{{ survey.SurveyName }}</td>
                <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ survey.ProjectName || '-' }}</td>
                <td class="px-4 py-3 text-gray-600 dark:text-gray-400">{{ formatDate(survey.DeletedDate) }}</td>
                <td class="px-4 py-3 text-right space-x-2">
                  <button 
                    @click="restoreSurvey(survey.Id)"
                    class="px-3 py-1.5 text-xs bg-blue-600 hover:bg-blue-700 text-white rounded"
                  >
                    Restore
                  </button>
                  <button 
                    @click="permanentlyDeleteSurvey(survey.Id, survey.SurveyName)"
                    class="px-3 py-1.5 text-xs bg-red-600 hover:bg-red-700 text-white rounded"
                  >
                    Delete Forever
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5010'

const loading = ref(true)
const error = ref(null)
const deletedProjects = ref([])
const deletedSurveys = ref([])

const loadDeletedItems = async () => {
  loading.value = true
  error.value = null
  
  try {
    const [projectsRes, surveysRes] = await Promise.all([
      axios.get(`${API_BASE_URL}/api/projects/trash`),
      axios.get(`${API_BASE_URL}/api/surveys/trash`)
    ])
    
    deletedProjects.value = projectsRes.data
    deletedSurveys.value = surveysRes.data
  } catch (err) {
    error.value = 'Failed to load deleted items: ' + err.message
    console.error('Error loading trash:', err)
  } finally {
    loading.value = false
  }
}

const restoreProject = async (projectId) => {
  if (!confirm('Restore this project and all its surveys?')) return
  
  try {
    await axios.post(`${API_BASE_URL}/api/projects/${projectId}/restore`)
    await loadDeletedItems()
  } catch (err) {
    alert('Failed to restore project: ' + err.message)
  }
}

const restoreSurvey = async (surveyId) => {
  if (!confirm('Restore this survey?')) return
  
  try {
    await axios.post(`${API_BASE_URL}/api/surveys/${surveyId}/restore`)
    await loadDeletedItems()
  } catch (err) {
    alert('Failed to restore survey: ' + err.message)
  }
}

const permanentlyDeleteProject = async (projectId, projectName) => {
  if (!confirm(`PERMANENTLY DELETE "${projectName}" and all its data? This cannot be undone!`)) return
  
  try {
    await axios.delete(`${API_BASE_URL}/api/projects/${projectId}/permanent`)
    await loadDeletedItems()
  } catch (err) {
    alert('Failed to permanently delete project: ' + err.message)
  }
}

const permanentlyDeleteSurvey = async (surveyId, surveyName) => {
  if (!confirm(`PERMANENTLY DELETE "${surveyName}"? This cannot be undone!`)) return
  
  try {
    await axios.delete(`${API_BASE_URL}/api/surveys/${surveyId}/permanent`)
    await loadDeletedItems()
  } catch (err) {
    alert('Failed to permanently delete survey: ' + err.message)
  }
}

const formatDate = (dateString) => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleString()
}

onMounted(() => {
  loadDeletedItems()
})
</script>
