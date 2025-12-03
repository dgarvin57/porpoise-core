<template>
  <div class="project-list-container">
    <header class="page-header">
      <h1>Projects & Surveys</h1>
      <p class="subtitle">Organize your surveys by project for pooling and trending</p>
    </header>

    <div v-if="loading" class="loading">
      <p>Loading projects...</p>
    </div>

    <div v-else-if="error" class="error">
      <p>{{ error }}</p>
      <button @click="loadProjects" class="retry-button">Retry</button>
    </div>

    <div v-else class="projects-grid">
      <!-- All projects shown uniformly -->
      <div v-if="allProjects.length > 0" class="projects-section">
        <div class="project-folders">
          <div
            v-for="project in allProjects"
            :key="project.Id"
            class="project-folder"
            :class="{ expanded: expandedProjects.has(project.Id) }"
          >
            <div class="folder-header" @click="toggleProject(project.Id)">
              <span class="folder-icon">{{ expandedProjects.has(project.Id) ? '📂' : '📁' }}</span>
              <div class="folder-content">
                <h3>{{ project.ProjectName }}</h3>
                <p class="client-name">{{ project.ClientName }}</p>
                <span class="survey-count">{{ project.SurveyCount }} {{ project.SurveyCount === 1 ? 'survey' : 'surveys' }}</span>
              </div>
              <span class="toggle-icon">{{ expandedProjects.has(project.Id) ? '▼' : '▶' }}</span>
            </div>

            <div v-if="expandedProjects.has(project.Id)" class="folder-surveys">
              <div v-if="loadingSurveys.has(project.Id)" class="loading-surveys">
                <p>Loading surveys...</p>
              </div>
              <div v-else-if="projectSurveys.get(project.Id) && projectSurveys.get(project.Id).length > 0" class="survey-list">
                <div
                  v-for="survey in projectSurveys.get(project.Id)"
                  :key="survey.Id"
                  class="survey-item"
                  @click.stop="viewSurvey(project, survey)"
                >
                  <span class="survey-icon">📊</span>
                  <div class="survey-info">
                    <span class="survey-name">{{ survey.SurveyName }}</span>
                    <div class="survey-meta">
                      <span class="survey-date">{{ formatDate(survey.CreatedDate) }}</span>
                      <span class="survey-stat">{{ survey.QuestionCount || 0 }} questions</span>
                      <span class="survey-stat">{{ survey.CaseCount || 0 }} cases</span>
                    </div>
                    <span class="survey-status" :class="`status-${survey.Status}`">
                      {{ getStatusLabel(survey.Status) }}
                    </span>
                  </div>
                </div>
              </div>
              <div v-else class="empty-surveys">
                <p>No surveys found in this project.</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="empty-state">
        <p>No projects or surveys found.</p>
        <p class="hint">Import a survey file (.porpz) to get started.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'

const API_URL = 'http://localhost:5107/api'
const TENANT_ID = 'demo-tenant'

const router = useRouter()
const loading = ref(true)
const error = ref(null)
const allProjects = ref([])
const expandedProjects = ref(new Set())
const loadingSurveys = ref(new Set())
const projectSurveys = ref(new Map())

// Computed: Standalone surveys (projects with 1 survey)
// Load all projects with survey counts
async function loadProjects() {
  try {
    loading.value = true
    error.value = null

    const response = await axios.get(`${API_URL}/projects/with-counts`, {
      headers: {
        'X-Tenant-Id': TENANT_ID
      }
    })

    allProjects.value = response.data
  } catch (err) {
    console.error('Error loading projects:', err)
    error.value = 'Failed to load projects. Please try again.'
  } finally {
    loading.value = false
  }
}

// Toggle project expansion and load surveys
async function toggleProject(projectId) {
  if (expandedProjects.value.has(projectId)) {
    expandedProjects.value.delete(projectId)
  } else {
    expandedProjects.value.add(projectId)

    // Load surveys if not already loaded
    if (!projectSurveys.value.has(projectId)) {
      await loadProjectSurveys(projectId)
    }
  }
  
  // Persist to localStorage
  localStorage.setItem('expandedProjectsList', JSON.stringify([...expandedProjects.value]))
}

// Load surveys for a specific project
async function loadProjectSurveys(projectId) {
  try {
    loadingSurveys.value.add(projectId)

    const response = await axios.get(`${API_URL}/projects/${projectId}/surveys`, {
      headers: {
        'X-Tenant-Id': TENANT_ID
      }
    })

    projectSurveys.value.set(projectId, response.data)
  } catch (err) {
    console.error(`Error loading surveys for project ${projectId}:`, err)
  } finally {
    loadingSurveys.value.delete(projectId)
  }
}

// View survey details
function viewSurvey(project, survey = null) {
  const surveyId = survey ? survey.Id : project.Id
  router.push(`/analytics/${surveyId}`)
}

// Format date helper
function formatDate(dateString) {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

// Get status label
function getStatusLabel(status) {
  const labels = {
    0: 'Initial',
    1: 'In Progress',
    2: 'Verified',
    3: 'Finalized'
  }
  return labels[status] || 'Unknown'
}

// Load projects on mount
onMounted(async () => {
  // Restore expanded state from localStorage
  const savedExpanded = localStorage.getItem('expandedProjectsList')
  if (savedExpanded) {
    try {
      const expandedArray = JSON.parse(savedExpanded)
      expandedProjects.value = new Set(expandedArray)
    } catch (e) {
      console.error('Error parsing expanded projects:', e)
    }
  }

  await loadProjects()
  
  // Reload surveys for any expanded projects
  for (const projectId of expandedProjects.value) {
    if (!projectSurveys.value.has(projectId)) {
      await loadProjectSurveys(projectId)
    }
  }
})
</script>

<style scoped>
.project-list-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.page-header {
  margin-bottom: 2rem;
}

.page-header h1 {
  font-size: 2rem;
  font-weight: 600;
  color: #1a202c;
  margin: 0 0 0.5rem 0;
}

.subtitle {
  color: #64748b;
  font-size: 0.95rem;
}

.loading,
.error {
  text-align: center;
  padding: 3rem;
  color: #64748b;
}

.error {
  color: #dc2626;
}

.retry-button {
  margin-top: 1rem;
  padding: 0.5rem 1.5rem;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 0.375rem;
  cursor: pointer;
  font-size: 0.95rem;
}

.retry-button:hover {
  background: #2563eb;
}

.projects-section {
  margin-bottom: 2rem;
}

.project-folders {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.project-folder {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 0.5rem;
  overflow: hidden;
}

.folder-header {
  display: flex;
  align-items: center;
  padding: 1.25rem;
  cursor: pointer;
  transition: background 0.2s;
}

.folder-header:hover {
  background: #f8fafc;
}

.folder-icon {
  font-size: 1.75rem;
  margin-right: 1rem;
}

.folder-content {
  flex: 1;
}

.folder-content h3 {
  font-size: 1.1rem;
  font-weight: 600;
  color: #1a202c;
  margin: 0 0 0.25rem 0;
}

.client-name {
  color: #64748b;
  font-size: 0.9rem;
  display: block;
}

.survey-count {
  display: inline-block;
  margin-top: 0.5rem;
  padding: 0.25rem 0.75rem;
  background: #eff6ff;
  color: #3b82f6;
  border-radius: 9999px;
  font-size: 0.813rem;
  font-weight: 500;
}

.toggle-icon {
  color: #94a3b8;
  font-size: 0.875rem;
  margin-left: 1rem;
}

.folder-surveys {
  border-top: 1px solid #e2e8f0;
  padding: 1rem;
  background: #f8fafc;
}

.loading-surveys {
  text-align: center;
  padding: 2rem;
  color: #64748b;
}

.empty-surveys {
  text-align: center;
  padding: 2rem;
  color: #94a3b8;
  font-style: italic;
  font-size: 0.9rem;
}

.survey-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.survey-item {
  display: flex;
  align-items: center;
  padding: 0.75rem 1rem;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 0.375rem;
  cursor: pointer;
  transition: all 0.2s;
}

.survey-item:hover {
  border-color: #3b82f6;
  background: #fafbff;
}

.survey-icon {
  font-size: 1.25rem;
  margin-right: 0.75rem;
}

.survey-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.survey-name {
  font-weight: 500;
  color: #1a202c;
  font-size: 0.938rem;
}

.survey-meta {
  display: flex;
  gap: 1rem;
  font-size: 0.813rem;
  color: #64748b;
}

.survey-date {
  color: #64748b;
}

.survey-stat {
  color: #64748b;
}

.survey-stat::before {
  content: '•';
  margin-right: 0.5rem;
  color: #cbd5e1;
}

.survey-status {
  align-self: flex-start;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.status-0 {
  background: #f1f5f9;
  color: #64748b;
}

.status-1 {
  background: #fef3c7;
  color: #d97706;
}

.status-2 {
  background: #d1fae5;
  color: #059669;
}

.status-3 {
  background: #dbeafe;
  color: #2563eb;
}

.empty-state {
  text-align: center;
  padding: 4rem 2rem;
  color: #64748b;
}

.empty-state p {
  margin: 0.5rem 0;
}

.hint {
  font-size: 0.9rem;
  color: #94a3b8;
}
</style>
