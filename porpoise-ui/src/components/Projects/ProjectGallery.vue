<template>
  <div class="space-y-6">
    <!-- Window Width Indicator (Debug Tool) -->
    <div class="fixed bottom-4 right-4 bg-black/80 text-white px-4 py-2 rounded-lg font-mono text-sm z-50 shadow-lg">
      Window: {{ windowWidth }}px
    </div>

    <!-- Header -->
    <div class="flex items-center justify-between">
      <div class="flex items-baseline space-x-3">
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
          Projects & Surveys
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          ({{ filteredProjects.length }} {{ filteredProjects.length === 1 ? 'project' : 'projects' }}, {{ totalSurveyCount }} {{ totalSurveyCount === 1 ? 'survey' : 'surveys' }})
        </p>
      </div>
      
      <!-- View Toggle and Sort -->
      <div class="flex items-center space-x-4">
        <!-- Filter Toggle -->
        <button
          @click="showFilters = !showFilters"
          :class="showFilters ? 'bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300' : 'bg-transparent text-gray-600 dark:text-gray-400'"
          class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          title="Toggle filters"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z" />
          </svg>
        </button>

        <!-- View Toggle -->
        <div class="flex items-center bg-gray-100 dark:bg-gray-800 rounded-lg p-1">
          <button
            @click="viewMode = 'grid'"
            :class="viewMode === 'grid' ? 'bg-white dark:bg-gray-700 shadow-sm text-gray-900 dark:text-white' : 'bg-transparent text-gray-600 dark:text-gray-400'"
            class="px-3 py-1.5 rounded-md transition-all"
            title="Grid view"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
            </svg>
          </button>
          <button
            @click="viewMode = 'list'"
            :class="viewMode === 'list' ? 'bg-white dark:bg-gray-700 shadow-sm text-gray-900 dark:text-white' : 'bg-transparent text-gray-600 dark:text-gray-400'"
            class="px-3 py-1.5 rounded-md transition-all"
            title="List view"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>

        <!-- Sort Dropdown -->
        <div class="flex items-center space-x-3">
          <label class="text-sm font-medium text-gray-700 dark:text-gray-300">
            Sort by:
          </label>
          <select
            v-model="sortBy"
            class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-gray-100 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          >
            <option value="modified">Most Recent</option>
            <option value="name">Name (A-Z)</option>
            <option value="created">Date Created</option>
            <option value="client">Client Name</option>
            <option value="status">Status</option>
          </select>
        </div>

        <!-- Trash Button -->
        <button 
          @click="$router.push('/trash')"
          class="px-3 py-2 text-sm text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 border border-gray-300 dark:border-gray-600 rounded-md flex items-center gap-2 transition-colors"
          title="View trash"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
          </svg>
          Trash
        </button>
      </div>
    </div>

    <!-- Search Bar -->
    <div class="flex items-center gap-4">
      <div class="flex-1 max-w-2xl">
        <div class="relative">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Search projects and surveys..."
            class="block w-full pl-10 pr-10 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <button
            v-if="searchQuery"
            @click="clearSearch"
            class="absolute right-3 top-1/2 -translate-y-1/2 p-0.5 rounded bg-transparent text-gray-500 hover:text-gray-900 hover:bg-gray-100 dark:text-gray-400 dark:hover:text-white dark:hover:bg-gray-700 transition-colors"
            title="Clear search"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>
      <label class="flex items-center gap-2 text-sm text-gray-700 dark:text-gray-300 whitespace-nowrap cursor-pointer">
        <input
          v-model="searchInQuestions"
          type="checkbox"
          class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600"
        />
        <span>Search in questions</span>
      </label>
    </div>

    <!-- Filters (Collapsible) -->
    <transition
      enter-active-class="transition-all duration-200 ease-out"
      enter-from-class="opacity-0 -translate-y-2"
      enter-to-class="opacity-100 translate-y-0"
      leave-active-class="transition-all duration-150 ease-in"
      leave-from-class="opacity-100 translate-y-0"
      leave-to-class="opacity-0 -translate-y-2"
    >
      <ProjectFilters
        v-if="showFilters"
        :clients="uniqueClients"
        @filter-change="handleFilterChange"
      />
    </transition>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
      <p class="text-red-800 dark:text-red-200">{{ error }}</p>
    </div>

    <!-- Empty State -->
    <div v-else-if="filteredProjects.length === 0" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">No projects found</h3>
      <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
        {{ activeFilters.client || activeFilters.status || activeFilters.dateRange || activeFilters.projectType
          ? 'Try adjusting your filters'
          : 'Get started by importing your first project' }}
      </p>
    </div>

    <!-- Project Grid -->
    <div v-else-if="viewMode === 'grid'" class="grid grid-cols-1 lg:grid-cols-2 3xl:grid-cols-3 4xl:grid-cols-4 5xl:grid-cols-5 gap-6">
      <ProjectCard
        v-for="project in filteredProjects"
        :key="project.id"
        :project="project"
        :is-expanded="expandedProjects.has(project.id)"
        :is-focused="focusedProjectId === project.id"
        :focused-survey-id="focusedSurveyId"
        @toggle-expand="handleToggleExpand(project.id)"
        @set-focus="handleSetFocus(project.id)"
        @survey-click="handleSurveyClick"
        @clear-all="handleClearAll"
        @delete-project="deleteProject"
        @delete-survey="deleteSurvey"
        @project-updated="handleProjectUpdated"
      />
    </div>

    <!-- Project List (Folder Tree - Single Column, Centered) -->
    <div v-else class="max-w-6xl mx-auto overflow-x-auto">
      <!-- Collapse All (Always visible, left-aligned) -->
      <div class="flex justify-start mb-1 px-3">
        <span
          @click="expandedProjects.size > 0 && collapseAll()"
          :class="expandedProjects.size === 0 ? 'text-gray-400 dark:text-gray-600 cursor-not-allowed' : 'text-blue-600 dark:text-blue-400 hover:text-blue-800 dark:hover:text-blue-300 hover:underline cursor-pointer'"
          class="text-xs transition-colors"
        >
          Collapse All
        </span>
      </div>
      
      <!-- Column Headers -->
      <div class="grid grid-cols-[32px_minmax(200px,1fr)_140px_80px_80px_110px_110px_90px_60px] gap-3 items-center px-3 py-2 border-b-2 border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-800 sticky top-0 min-w-[960px]">
        <span class="text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase"></span>
        
        <SortableColumnHeader
          label="Name"
          sort-key="name"
          :current-sort="sortBy"
          :direction="sortDirection"
          @sort="sortByColumn"
        />
        
        <SortableColumnHeader
          label="Client"
          sort-key="client"
          :current-sort="sortBy"
          :direction="sortDirection"
          @sort="sortByColumn"
        />
        
        <div class="flex items-center justify-end space-x-1 text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase">
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
          <span>Cases</span>
        </div>
        
        <div class="flex items-center justify-end space-x-1 text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase">
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <span>Qs</span>
        </div>
        
        <SortableColumnHeader
          label="Created"
          sort-key="created"
          :current-sort="sortBy"
          :direction="sortDirection"
          text-align="right"
          @sort="sortByColumn"
        />
        
        <SortableColumnHeader
          label="Modified"
          sort-key="modified"
          :current-sort="sortBy"
          :direction="sortDirection"
          text-align="right"
          @sort="sortByColumn"
        />
        
        <SortableColumnHeader
          label="Status"
          sort-key="status"
          :current-sort="sortBy"
          :direction="sortDirection"
          text-align="center"
          @sort="sortByColumn"
        />
        
        <span class="text-xs font-semibold text-gray-600 dark:text-gray-400 uppercase text-center"></span>
      </div>

      <!-- Project Rows -->
      <div class="space-y-0">
        <div v-for="project in filteredProjects" :key="project.id">
          <!-- Project Row (All Expandable) -->
          <div>
            <div
              class="grid grid-cols-[32px_minmax(200px,1fr)_140px_80px_80px_110px_110px_90px_60px] gap-3 items-center px-3 py-1.5 hover:bg-gray-100 dark:hover:bg-gray-800 group border-b border-gray-100 dark:border-gray-800 min-w-[960px]"
            >
              <div @click="toggleProjectExpand(project.id)" class="cursor-pointer contents">
              <div class="flex items-center justify-center space-x-0.5">
                <svg
                  :class="{ 'rotate-90': expandedProjects.has(project.id) }"
                  class="w-3 h-3 text-gray-400 transition-transform flex-shrink-0"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
                <svg class="w-4 h-4 text-amber-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
                </svg>
              </div>
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300 truncate group-hover:text-blue-600 dark:group-hover:text-blue-400">
                {{ project.name }}
              </span>
              <span class="text-xs text-gray-500 dark:text-gray-400 truncate">
                {{ project.clientName || '—' }}
              </span>
              <span class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums italic">
                {{ project.surveyCount }}
              </span>
              <span class="text-xs text-gray-500 dark:text-gray-400 text-right">—</span>
              <span
                class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums"
              >
                {{ formatDateShort(project.createdAt) }}
              </span>
              <span
                class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums"
              >
                {{ formatDateShort(project.lastModified) }}
              </span>
              <div class="flex justify-center">
                <span v-if="project.status" :class="getStatusClass(project.status)" class="text-xs px-2 py-0.5 rounded-full whitespace-nowrap">
                  {{ project.status }}
                </span>
                <span v-else class="text-xs text-gray-400">—</span>
              </div>
              </div>
              
              <div class="flex justify-center" @click.stop>
                <svg
                  @click="deleteProject(project.id, project.name)"
                  class="w-4 h-4 text-gray-400 hover:text-red-600 dark:hover:text-red-400 transition-colors cursor-pointer"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                  title="Delete project"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                </svg>
              </div>
            </div>

            <!-- Expanded Surveys -->
            <div v-if="expandedProjects.has(project.id)" class="bg-gray-50 dark:bg-gray-900/30">
              <div v-if="loadingSurveys.has(project.id)" class="flex items-center space-x-2 px-3 py-1.5 ml-8">
                <div class="animate-spin rounded-full h-3 w-3 border-b-2 border-blue-500"></div>
                <span class="text-xs text-gray-500 dark:text-gray-400">Loading...</span>
              </div>
              <template v-else-if="projectSurveys.get(project.id) && projectSurveys.get(project.id).length > 0">
                <div
                  v-for="survey in projectSurveys.get(project.id)"
                  :key="survey.id"
                  :class="[
                    'grid grid-cols-[32px_minmax(200px,1fr)_140px_80px_80px_110px_110px_90px_60px] gap-3 items-center px-3 py-1.5 group border-b border-gray-100 dark:border-gray-800 min-w-[960px]',
                    focusedSurveyId === survey.id
                      ? 'bg-blue-50 dark:bg-blue-900/30'
                      : 'hover:bg-gray-100 dark:hover:bg-gray-800'
                  ]"
                >
                  <div class="flex justify-center">
                  </div>
                  <div class="flex items-center space-x-2">
                    <div class="flex-shrink-0">
                      <svg class="w-3.5 h-3.5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                      </svg>
                    </div>
                    <span 
                      @click="navigateToSurvey(survey.id); handleSurveyClick(survey.id)"
                      class="text-sm text-gray-600 dark:text-gray-400 truncate hover:text-blue-600 dark:hover:text-blue-400 cursor-pointer"
                    >
                      {{ survey.name }}
                    </span>
                  </div>
                  <span class="text-xs text-gray-500 dark:text-gray-400">—</span>
                  <span class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums">
                    {{ survey.caseCount || 0 }}
                  </span>
                  <span class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums">
                    {{ survey.questionCount || 0 }}
                  </span>
                  <span
                    class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums"
                  >
                    {{ formatDateShort(survey.createdDate) }}
                  </span>
                  <span
                    class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums"
                  >
                    {{ formatDateShort(survey.modifiedDate) }}
                  </span>
                  <span class="text-xs text-gray-500 dark:text-gray-400 text-center">—</span>
                  
                  <div class="flex justify-center" @click.stop>
                    <svg
                      @click="deleteSurvey(survey.id, survey.name)"
                      class="w-4 h-4 text-gray-400 hover:text-red-600 dark:hover:text-red-400 transition-colors cursor-pointer"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                      title="Delete survey"
                    >
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </div>
                </div>
              </template>
              <div v-else class="px-3 py-2 ml-8 text-xs text-gray-500 dark:text-gray-400">
                No surveys found
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onActivated, watch } from 'vue'
import axios from 'axios'
import { useRouter, useRoute } from 'vue-router'
import ProjectCard from './ProjectCard.vue'
import ProjectFilters from './ProjectFilters.vue'
import SortableColumnHeader from '../UI/SortableColumnHeader.vue'

const router = useRouter()
const route = useRoute()
const projects = ref([])
const loading = ref(true)
const error = ref(null)
const sortBy = ref('modified')
const sortDirection = ref('desc') // 'asc' or 'desc'
const viewMode = ref(localStorage.getItem('projectViewMode') || 'grid') // 'grid' or 'list', persisted
const showFilters = ref(false)
// Load expansion state based on current view mode
const storageKey = viewMode.value === 'grid' ? 'expandedProjectsGrid' : 'expandedProjectsList'
const expandedProjects = ref(new Set(JSON.parse(localStorage.getItem(storageKey) || '[]')))
const windowWidth = ref(window.innerWidth)
const focusedProjectId = ref(localStorage.getItem('focusedProjectId') || null)
const focusedSurveyId = ref(localStorage.getItem('focusedSurveyId') || null)
const loadingSurveys = ref(new Set())
const projectSurveys = ref(new Map())
const activeFilters = ref({
  client: '',
  status: '',
  dateRange: '',
  projectType: ''
})
const searchQuery = ref('')
const searchInQuestions = ref(false)
const allSurveysForSearch = ref(new Map()) // Cache all surveys for search

// Fetch projects from API
async function fetchProjects() {
  loading.value = true
  error.value = null
  
  try {
    // Fetch projects with counts (logos loaded on-demand per card)
    const response = await axios.get('http://localhost:5107/api/projects/with-counts')
    
    // Map API response to component format
    projects.value = response.data.map(p => ({
      id: p.id,
      name: p.name,
      clientName: p.clientName,
      description: p.description,
      surveyCount: p.surveyCount,
      caseCount: p.caseCount,
      questionCount: p.questionCount,
      createdAt: p.createdAt,
      lastModified: p.modifiedDate || p.lastModified || p.createdAt,
      startDate: p.startDate,
      endDate: p.endDate,
      status: p.status || getStatusFromDates(p.startDate, p.endDate),
      // Logo will be loaded on-demand by ProjectCard
      clientLogoBase64: null,
      defaultWeightingScheme: p.defaultWeightingScheme
    }))
  } catch (err) {
    error.value = 'Failed to load projects. Please try again.'
    console.error('Error fetching projects:', err)
  } finally {
    loading.value = false
  }
}

function getStatusFromDates(startDate, endDate) {
  if (!startDate || !endDate) return 'Active'
  const now = new Date()
  const start = new Date(startDate)
  const end = new Date(endDate)
  
  if (now < start) return 'Draft'
  if (now > end) return 'Completed'
  return 'Active'
}

// Get unique clients for filter dropdown
const uniqueClients = computed(() => {
  const clients = projects.value
    .map(p => p.clientName)
    .filter(c => c)
  return [...new Set(clients)].sort()
})

// Filter projects based on active filters and search query
const filteredProjects = computed(() => {
  let result = [...projects.value]
  
  // Search filter
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase().trim()
    result = result.filter(p => {
      // Search in project name
      if (p.name?.toLowerCase().includes(query)) return true
      
      // Search in project description
      if (p.description?.toLowerCase().includes(query)) return true
      
      // Search in survey names (check both loaded surveys and search cache)
      const surveysToSearch = projectSurveys.value.get(p.id) || allSurveysForSearch.value.get(p.id) || []
      if (surveysToSearch.some(s => s.name?.toLowerCase().includes(query))) return true
      
      // If "search in questions" is enabled, search in question labels/stems
      if (searchInQuestions.value) {
        // Search through surveys for question matches
        for (const survey of surveysToSearch) {
          if (survey.questions && survey.questions.length > 0) {
            // Search in question numbers
            if (survey.questions.some(q => q.number?.toLowerCase().includes(query))) return true
            
            // Search in question labels
            if (survey.questions.some(q => q.label?.toLowerCase().includes(query))) return true
            
            // Search in question stems (full question text)
            if (survey.questions.some(q => q.stem?.toLowerCase().includes(query))) return true
            
            // Search in block labels
            if (survey.questions.some(q => q.blockLabel?.toLowerCase().includes(query))) return true
            
            // Search in block stems
            if (survey.questions.some(q => q.blockStem?.toLowerCase().includes(query))) return true
          }
        }
      }
      
      return false
    })
  }
  
  // Client filter
  if (activeFilters.value.client) {
    result = result.filter(p => p.clientName === activeFilters.value.client)
  }
  
  // Status filter
  if (activeFilters.value.status) {
    result = result.filter(p => p.status === activeFilters.value.status)
  }
  
  // Date range filter
  if (activeFilters.value.dateRange) {
    const now = new Date()
    result = result.filter(p => {
      if (!p.lastModified) return false
      const projectDate = new Date(p.lastModified)
      const diffDays = Math.floor((now - projectDate) / (1000 * 60 * 60 * 24))
      
      switch (activeFilters.value.dateRange) {
        case 'today': return diffDays === 0
        case 'week': return diffDays <= 7
        case 'month': return diffDays <= 30
        case 'quarter': return diffDays <= 90
        case 'year': return diffDays <= 365
        default: return true
      }
    })
  }
  
  // Sort projects
  result.sort((a, b) => {
    let comparison = 0
    switch (sortBy.value) {
      case 'name':
        comparison = (a.name || '').localeCompare(b.name || '')
        break
      case 'created':
        comparison = new Date(a.createdAt || 0) - new Date(b.createdAt || 0)
        break
      case 'client':
        comparison = (a.clientName || '').localeCompare(b.clientName || '')
        break
      case 'status':
        const statusOrder = { 'Active': 1, 'Draft': 2, 'Completed': 3, 'Archived': 4 }
        const aOrder = statusOrder[a.status] || 999
        const bOrder = statusOrder[b.status] || 999
        comparison = aOrder - bOrder
        break
      case 'modified':
      default:
        comparison = new Date(a.lastModified || 0) - new Date(b.lastModified || 0)
        break
    }
    // Apply sort direction
    return sortDirection.value === 'asc' ? comparison : -comparison
  })
  
  return result
})

// Calculate total survey count from filtered projects
const totalSurveyCount = computed(() => {
  return filteredProjects.value.reduce((sum, project) => sum + (project.surveyCount || 0), 0)
})

function sortByColumn(column) {
  if (sortBy.value === column) {
    // Toggle direction if clicking the same column
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    // New column, default to ascending
    sortBy.value = column
    sortDirection.value = 'asc'
  }
}

// Watch sortBy from dropdown and reset to sensible default direction
watch(sortBy, (newVal) => {
  if (newVal === 'modified' || newVal === 'created') {
    sortDirection.value = 'desc' // Dates default to newest first
  } else {
    sortDirection.value = 'asc' // Names, clients, status default to A-Z
  }
})

// Persist view mode preference and switch expansion state when changing views
watch(viewMode, (newVal, oldVal) => {
  localStorage.setItem('projectViewMode', newVal)
  const oldStorageKey = oldVal === 'grid' ? 'expandedProjectsGrid' : 'expandedProjectsList'
  localStorage.setItem(oldStorageKey, JSON.stringify([...expandedProjects.value]))
  const newStorageKey = newVal === 'grid' ? 'expandedProjectsGrid' : 'expandedProjectsList'
  expandedProjects.value = new Set(JSON.parse(localStorage.getItem(newStorageKey) || '[]'))
})

// Load question data when "search in questions" is enabled
// Use a combined watcher to handle both checkbox and search query changes
watch([searchInQuestions, searchQuery], async ([questionsEnabled, query]) => {
  if (questionsEnabled && query.trim()) {
    // First, ensure all surveys are loaded
    const projectsNeedingSurveys = projects.value.filter(p => 
      !projectSurveys.value.has(p.id) && !allSurveysForSearch.value.has(p.id)
    )
    
    if (projectsNeedingSurveys.length > 0) {
      const batchSize = 10
      for (let i = 0; i < projectsNeedingSurveys.length; i += batchSize) {
        const batch = projectsNeedingSurveys.slice(i, i + batchSize)
        await Promise.all(batch.map(async (project) => {
          try {
            const response = await axios.get(`http://localhost:5107/api/projects/${project.id}/surveys`)
            allSurveysForSearch.value.set(project.id, response.data.map(s => ({
              id: s.id,
              name: s.name,
              status: s.status,
              caseCount: s.caseCount,
              questionCount: s.questionCount,
              createdDate: s.createdDate,
              modifiedDate: s.modifiedDate,
              createdBy: s.createdBy,
              modifiedBy: s.modifiedBy
            })))
          } catch (error) {
            console.error('Error loading surveys for search:', error)
          }
        }))
      }
    }
    
    // Now load questions for all surveys that don't have them yet
    const surveysToLoad = []
    
    for (const project of projects.value) {
      const surveys = projectSurveys.value.get(project.id) || allSurveysForSearch.value.get(project.id) || []
      for (const survey of surveys) {
        if (!survey.questions) {
          surveysToLoad.push({ projectId: project.id, surveyId: survey.id, survey })
        }
      }
    }
    
    if (surveysToLoad.length > 0) {
      // Load questions in parallel (but limit to avoid overwhelming the server)
      const batchSize = 5
      for (let i = 0; i < surveysToLoad.length; i += batchSize) {
        const batch = surveysToLoad.slice(i, i + batchSize)
        await Promise.all(batch.map(async ({ projectId, surveyId, survey }) => {
          try {
            const response = await axios.get(`http://localhost:5107/api/surveys/${surveyId}/questions-list`)
            // Add questions to the survey object - map to correct field names from API
            survey.questions = response.data.map(q => ({
              id: q.id,
              number: q.qstNumber,        // Question number
              label: q.qstLabel,          // Question label
              stem: q.qstStem || q.text,  // Question stem (try qstStem first, fallback to text)
              blockLabel: q.blkLabel,     // Block label
              blockStem: q.blkStem        // Block stem
            }))
          } catch (error) {
            console.error('Error loading questions for search:', error)
          }
        }))
      }
    }
  }
})

function handleToggleExpand(projectId) {
  if (expandedProjects.value.has(projectId)) {
    expandedProjects.value.delete(projectId)
  } else {
    // In grid view, only allow one project to be expanded at a time
    expandedProjects.value.clear()
    expandedProjects.value.add(projectId)
  }
  // Clear focused survey when toggling expansion
  focusedSurveyId.value = null
  localStorage.removeItem('focusedSurveyId')
  // Persist to localStorage using grid-specific key
  localStorage.setItem('expandedProjectsGrid', JSON.stringify([...expandedProjects.value]))
}

function handleClearAll() {
  // Clear all expanded projects and focused surveys
  expandedProjects.value.clear()
  focusedSurveyId.value = null
  focusedProjectId.value = null
  localStorage.removeItem('expandedProjectsGrid')
  localStorage.removeItem('expandedProjectsList')
  localStorage.removeItem('focusedSurveyId')
  localStorage.removeItem('focusedProjectId')
}

function collapseAll() {
  // Collapse all expanded projects in list view
  expandedProjects.value.clear()
  focusedSurveyId.value = null
  localStorage.removeItem('expandedProjectsList')
  localStorage.removeItem('focusedSurveyId')
  // Trigger reactivity
  expandedProjects.value = new Set(expandedProjects.value)
}

function handleSetFocus(projectId) {
  focusedProjectId.value = projectId
  localStorage.setItem('focusedProjectId', projectId)
}

function handleSurveyClick(surveyId) {
  focusedSurveyId.value = surveyId
  localStorage.setItem('focusedSurveyId', surveyId)
}

function formatDateShort(dateString) {
  if (!dateString) return '—'
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: '2-digit' })
}

function handleFilterChange(filters) {
  activeFilters.value = filters
}

async function toggleProjectExpand(projectId) {
  if (expandedProjects.value.has(projectId)) {
    expandedProjects.value.delete(projectId)
  } else {
    // Allow multiple projects to stay expanded
    expandedProjects.value.add(projectId)
    
    // Fetch surveys if not already loaded
    if (!projectSurveys.value.has(projectId)) {
      loadingSurveys.value.add(projectId)
      try {
        const response = await axios.get(`http://localhost:5107/api/projects/${projectId}/surveys`)
        projectSurveys.value.set(projectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          createdBy: s.createdBy,
          modifiedBy: s.modifiedBy
        })))
      } catch (error) {
        console.error('Error fetching surveys:', error)
      } finally {
        loadingSurveys.value.delete(projectId)
      }
    }
  }
  // Clear focused survey when toggling expansion
  focusedSurveyId.value = null
  localStorage.removeItem('focusedSurveyId')
  // Persist to localStorage using list-specific key
  localStorage.setItem('expandedProjectsList', JSON.stringify([...expandedProjects.value]))
  // Trigger reactivity
  expandedProjects.value = new Set(expandedProjects.value)
  loadingSurveys.value = new Set(loadingSurveys.value)
}

function navigateToProject(project) {
  router.push(`/analytics/${project.id}`)
}

async function handleSingleProjectClick(project) {
  // Clear expanded projects and focused surveys
  expandedProjects.value.clear()
  focusedSurveyId.value = null
  localStorage.removeItem('expandedProjects')
  localStorage.removeItem('focusedSurveyId')
  // Set focused project for highlighting when returning
  focusedProjectId.value = project.id
  localStorage.setItem('focusedProjectId', project.id)
  
  // For single survey projects, fetch the survey and navigate to it
  try {
    const response = await axios.get(`http://localhost:5107/api/projects/${project.id}/surveys`)
    if (response.data && response.data.length > 0) {
      navigateToSurvey(response.data[0].id)
    } else {
      navigateToProject(project)
    }
  } catch (error) {
    console.error('Error loading survey:', error)
    navigateToProject(project)
  }
}

function navigateToSurvey(surveyId) {
  router.push(`/analytics/${surveyId}`)
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

function clearSearch() {
  searchQuery.value = ''
  searchInQuestions.value = false
}

async function deleteProject(projectId, projectName) {
  if (!confirm(`Delete project "${projectName}"? It will be moved to trash.`)) return
  
  try {
    await axios.post(`http://localhost:5107/api/projects/${projectId}/soft-delete`)
    await fetchProjects() // Reload the list
  } catch (error) {
    console.error('Error deleting project:', error)
    alert('Failed to delete project: ' + error.message)
  }
}

async function deleteSurvey(surveyId, surveyName) {
  if (!confirm(`Delete survey "${surveyName}"? It will be moved to trash.`)) return
  
  try {
    await axios.post(`http://localhost:5107/api/surveys/${surveyId}/soft-delete`)
    
    // Reload projects to update counts
    await fetchProjects()
    
    // For list view: Find which project and reload its surveys
    let affectedProjectId = null
    for (const [projectId, surveys] of projectSurveys.value.entries()) {
      if (surveys.some(s => s.id === surveyId)) {
        affectedProjectId = projectId
        break
      }
    }
    
    if (affectedProjectId && expandedProjects.value.has(affectedProjectId)) {
      loadingSurveys.value.add(affectedProjectId)
      try {
        const response = await axios.get(`http://localhost:5107/api/projects/${affectedProjectId}/surveys`)
        projectSurveys.value.set(affectedProjectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          createdBy: s.createdBy,
          modifiedBy: s.modifiedBy
        })))
      } catch (error) {
        console.error('Error reloading surveys:', error)
      } finally {
        loadingSurveys.value.delete(affectedProjectId)
      }
    }
    
    // For grid view: surveys will be reloaded automatically on next expand due to reactive props
  } catch (error) {
    console.error('Error deleting survey:', error)
    alert('Failed to delete survey: ' + error.message)
  }
}

async function handleProjectUpdated(projectId, updatedData) {
  // Update the project in place without resorting - keeps card position stable for better UX
  const project = projects.value.find(p => p.id === projectId)
  if (project) {
    // Update properties individually to maintain Vue reactivity
    // Note: We intentionally do NOT update lastModified here to prevent re-sorting
    // The database has the correct modified date, which will be shown on next page load
    project.name = updatedData.projectName
    project.clientName = updatedData.clientName
    project.description = updatedData.description
    project.clientLogoBase64 = updatedData.clientLogoBase64
    project.defaultWeightingScheme = updatedData.defaultWeightingScheme
    project.startDate = updatedData.startDate
    project.endDate = updatedData.endDate
  }
}

onMounted(async () => {
  await fetchProjects()
  
  // Pre-load surveys for any expanded projects (from localStorage)
  const surveysToLoad = []
  for (const projectId of expandedProjects.value) {
    const project = projects.value.find(p => p.id === projectId)
    if (project && !projectSurveys.value.has(projectId)) {
      surveysToLoad.push(projectId)
    }
  }
  
  // Load all surveys in parallel
  if (surveysToLoad.length > 0) {
    await Promise.all(surveysToLoad.map(async (projectId) => {
      try {
        const response = await axios.get(`http://localhost:5107/api/projects/${projectId}/surveys`)
        projectSurveys.value.set(projectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          createdBy: s.createdBy,
          modifiedBy: s.modifiedBy
        })))
      } catch (err) {
        console.error('Error loading surveys:', err)
      }
    }))
  }
  
  // Update window width on resize
  const handleResize = () => {
    windowWidth.value = window.innerWidth
  }
  window.addEventListener('resize', handleResize)
  
  // Cleanup
  return () => window.removeEventListener('resize', handleResize)
})

// Refetch projects when navigating back to the gallery (e.g., from a survey)
// Only refetch if projects are empty or stale
onActivated(async () => {
  // Only refetch if we don't have projects loaded
  if (projects.value.length === 0) {
    await fetchProjects()
  }
  
  // Re-load surveys for expanded projects if needed
  const surveysToLoad = []
  for (const projectId of expandedProjects.value) {
    if (!projectSurveys.value.has(projectId)) {
      surveysToLoad.push(projectId)
    }
  }
  
  if (surveysToLoad.length > 0) {
    await Promise.all(surveysToLoad.map(async (projectId) => {
      try {
        const response = await axios.get(`http://localhost:5107/api/projects/${projectId}/surveys`)
        projectSurveys.value.set(projectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          createdBy: s.createdBy,
          modifiedBy: s.modifiedBy
        })))
      } catch (err) {
        console.error('Error loading surveys:', err)
      }
    }))
  }
})

// Watch route changes - only refetch if needed
watch(() => route.path, async (newPath) => {
  if (newPath === '/projects' || newPath === '/') {
    // Always refetch when returning to project gallery to pick up new imports
    await fetchProjects()
  }
})
</script>
