<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-900 flex justify-center">
    <div class="w-full max-w-7xl px-2 sm:px-4 lg:px-6 pt-2 pb-0 space-y-1">
      <!-- Window Width Indicator (Debug Tool) -->
      <div class="fixed bottom-4 right-4 bg-black/80 text-white px-4 py-2 rounded-lg font-mono text-sm z-50 shadow-lg">
        Window: {{ windowWidth }}px
      </div>

      <!-- Header -->
    <div class="flex items-center justify-between max-w-6xl mx-auto">
      <div class="flex items-baseline space-x-3">
        <h1 class="text-xl font-bold text-gray-900 dark:text-white" style="font-size: 1.25rem !important; line-height: 1.75rem !important;">
          Projects & Surveys
        </h1>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          ({{ filteredProjects.length }} {{ filteredProjects.length === 1 ? 'project' : 'projects' }}, {{ totalSurveyCount }} {{ totalSurveyCount === 1 ? 'survey' : 'surveys' }})
        </p>
      </div>
      
      <!-- Trash Button (only in header) -->
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

    <!-- Recent Surveys Section -->
    <div v-if="recentSurveys.length > 0" class="max-w-6xl mx-auto pb-2 mb-1 border-b border-gray-200 dark:border-gray-700">
      <button 
        @click="toggleRecentSurveys"
        class="flex items-center gap-2 mb-3 hover:opacity-70 transition-opacity"
      >
        <svg 
          class="w-4 h-4 text-blue-500 transition-transform"
          :class="showRecentSurveys ? '' : '-rotate-90'"
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
        </svg>
        <svg class="w-4 h-4 text-blue-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <h2 class="text-sm font-semibold text-gray-700 dark:text-gray-300">Recent Surveys</h2>
      </button>
      <div v-if="showRecentSurveys" class="flex gap-3 overflow-x-auto pb-2">
        <div
          v-for="survey in recentSurveys"
          :key="survey.id"
          :class="[
            'flex-shrink-0 w-56 p-3 rounded-lg transition-all group shadow-sm hover:shadow-md relative cursor-pointer focus:outline-none focus:ring-2 focus:ring-blue-500',
            focusedSurveyId === survey.id
              ? 'bg-blue-50 dark:bg-blue-900/30 border-2 border-blue-500 dark:border-blue-400'
              : 'bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 hover:border-blue-400 dark:hover:border-blue-500'
          ]"
          role="button"
          tabindex="0"
          @click="navigateToSurvey(survey.id, false)"
          @keydown.enter.prevent="navigateToSurvey(survey.id, false)"
          @keydown.space.prevent="navigateToSurvey(survey.id, false)"
        >
          <!-- Survey Icon and Title -->
          <div class="flex items-start gap-2 mb-2">
            <svg class="w-5 h-5 text-blue-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
            </svg>
            <div class="flex-1 min-w-0 text-left">
              <h3 class="text-sm font-semibold text-gray-900 dark:text-white truncate group-hover:text-blue-600 dark:group-hover:text-blue-400 leading-tight">
                {{ survey.name }}
              </h3>
            </div>
            <svg class="w-4 h-4 text-gray-400 group-hover:text-blue-500 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
            </svg>
          </div>
          
          <!-- Project with Gear Icon -->
          <div class="mb-2 text-left">
            <div class="flex items-center gap-1.5 text-xs text-gray-600 dark:text-gray-400 mb-0.5">
              <svg class="w-5 h-5 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
              </svg>
              <span class="truncate flex-1">{{ survey.projectName || 'No project' }}</span>
              <button
                v-if="survey.projectId"
                @click.stop="openProjectSettings(survey.projectId)"
                class="p-0.5 hover:bg-gray-100 dark:hover:bg-gray-700 rounded transition-colors"
                title="Project settings"
              >
                <svg class="w-3.5 h-3.5 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
              </button>
            </div>
            <div v-if="survey.clientName" class="flex items-center gap-1.5 text-xs text-gray-500 dark:text-gray-500">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
              </svg>
              <span class="truncate">{{ survey.clientName }}</span>
            </div>
          </div>
          
          <!-- Stats and Time on same line -->
          <div class="flex items-center justify-between text-xs text-gray-600 dark:text-gray-400">
            <div class="flex items-center gap-3">
              <span class="flex items-center gap-1">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
                {{ survey.caseCount }}
              </span>
              <span class="flex items-center gap-1">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                {{ survey.questionCount }}
              </span>
            </div>
            <div class="text-gray-500 dark:text-gray-400" :title="formatExactTime(survey.lastAccessedDate)">
              {{ formatRelativeTime(survey.lastAccessedDate) }}
            </div>
          </div>
        </div>
      </div>
    </div>
    <div v-else class="max-w-6xl mx-auto pb-2 mb-1 border-b border-gray-200 dark:border-gray-700">
      <div class="flex items-center gap-2 mb-3 text-gray-500 dark:text-gray-400">
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <h2 class="text-sm font-semibold">Recent Surveys</h2>
      </div>
      <div class="flex items-center justify-between p-3 rounded-lg border border-dashed border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-600 dark:text-gray-400">
        <div class="flex items-center gap-2">
          <svg class="w-5 h-5 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z" />
          </svg>
          <div>
            <p class="font-medium text-gray-700 dark:text-gray-200">No recent surveys yet</p>
            <p class="text-xs text-gray-500 dark:text-gray-400">Open a survey to have it show up here.</p>
          </div>
        </div>
        <button
          @click="$router.push('/projects')"
          class="px-3 py-1.5 text-xs font-medium text-blue-600 dark:text-blue-400 hover:text-blue-700 dark:hover:text-blue-300 bg-blue-50 dark:bg-blue-900/20 rounded-md border border-blue-200 dark:border-blue-800 transition-colors"
        >
          Browse projects
        </button>
      </div>
    </div>

    <!-- Search Bar and Controls Row -->
    <div class="flex items-center justify-between gap-4 max-w-6xl mx-auto pt-1">
      <div class="flex items-center gap-4">
        <div class="w-80">
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
              class="block w-full pl-10 pr-10 py-1.5 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-xs text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
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
        <label class="flex items-center gap-2 text-xs text-gray-700 dark:text-gray-300 whitespace-nowrap cursor-pointer">
          <input
            v-model="searchInQuestions"
            type="checkbox"
            class="w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 dark:focus:ring-blue-600 dark:ring-offset-gray-800 focus:ring-2 dark:bg-gray-700 dark:border-gray-600"
          />
          <span>Search in questions</span>
        </label>
      </div>
      
      <!-- Right-aligned controls group -->
      <div class="flex items-center gap-3">
        <!-- Filter Toggle -->
        <button
          @click="showFilters = !showFilters"
          :class="showFilters ? 'bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-300' : 'bg-transparent text-gray-600 dark:text-gray-400'"
          class="p-2 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          title="Toggle filters"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z" />
            </svg>
          </button>
          <button
            @click="viewMode = 'list'"
            :class="viewMode === 'list' ? 'bg-white dark:bg-gray-700 shadow-sm text-gray-900 dark:text-white' : 'bg-transparent text-gray-600 dark:text-gray-400'"
            class="px-3 py-1.5 rounded-md transition-all"
            title="List view"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
            </svg>
          </button>
        </div>

        <!-- Sort Select (PrimeVue) -->
        <div class="flex items-center gap-2">
          <label class="text-xs font-medium text-gray-700 dark:text-gray-300 whitespace-nowrap">
            Sort by:
          </label>
          <Select
            v-model="sortBy"
            :options="sortOptions"
            optionLabel="label"
            optionValue="value"
            appendTo="self"
            class="w-40"
            size="small"
            :pt="{
              root: { class: 'text-xs border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100' },
              trigger: { class: 'px-2.5 py-1.5' },
              label: { class: 'text-xs' },
              panel: { class: 'bg-white dark:bg-gray-700 border border-gray-200 dark:border-gray-600 rounded-md shadow-lg min-w-[10rem] py-1' },
              item: { class: 'text-xs text-gray-800 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-600 px-2.5 py-1' },
              itemLabel: { class: 'text-xs' },
              header: { class: 'hidden' }
            }"
          >
            <template #value="{ value, placeholder }">
              <div class="flex items-center gap-2">
                <i :class="sortOptions.find(o => o.value === value)?.icon" class="text-gray-500"></i>
                <span class="text-xs">{{ sortOptions.find(o => o.value === value)?.label || placeholder }}</span>
              </div>
            </template>
            <template #option="slotProps">
              <div class="flex items-center gap-2">
                <i :class="slotProps.option.icon" class="text-gray-500"></i>
                <span class="text-xs">{{ slotProps.option.label }}</span>
              </div>
            </template>
          </Select>
        </div>
      </div>
    </div>

    <!-- Filters (Collapsible) -->
    <div class="mt-4">
      <transition
        enter-active-class="transition-opacity duration-200 ease-out"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition-opacity duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <ProjectFilters
          v-show="showFilters"
          :clients="uniqueClients"
          @filter-change="handleFilterChange"
        />
      </transition>
    </div>

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
    <div v-else-if="viewMode === 'grid'" class="grid grid-cols-1 md: grid-cols-2 lg:grid-cols-3 gap-6 pt-1 max-w-6xl mx-auto">
      <ProjectCard
        v-for="project in filteredProjects"
        :key="project.id"
        :project="project"
        :is-expanded="expandedProjects.has(project.id)"
        :is-focused="focusedProjectId === project.id"
        :focused-survey-id="focusedSurveyId"
        @toggle-expand="handleToggleExpand(project.id)"
        @set-focus="handleSetFocus(project.id)"
        @survey-click="handleSurveyClickFromGrid"
        @clear-all="handleClearAll"
        @delete-project="deleteProject"
        @delete-survey="deleteSurvey"
        @project-updated="handleProjectUpdated"
      />
    </div>

    <!-- Project List (Folder Tree - Single Column, Centered) -->
    <div v-else class="max-w-6xl mx-auto overflow-x-auto">
      
      <!-- Column Headers -->
      <div class="grid grid-cols-[32px_minmax(200px,1fr)_140px_80px_80px_110px_90px_60px] gap-3 items-center px-3 py-1 mt-2 border-b-2 border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-800 sticky top-0 min-w-[860px]">
        <button
          class="flex items-center justify-center w-6 h-6 rounded hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
          :class="expandedProjects.size === 0 ? 'text-gray-400 dark:text-gray-600 cursor-not-allowed' : 'text-gray-600 dark:text-gray-300'"
          :disabled="expandedProjects.size === 0"
          @click="collapseAll()"
          title="Collapse all"
          aria-label="Collapse all"
        >
          <svg class="w-4 h-4" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 14l5-5 5 5" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 19l5-5 5 5" />
          </svg>
        </button>
        
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
          label="Recent Activity"
          class="text-[8px]"
          sort-key="activity"
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
              :class="[
                'grid grid-cols-[32px_minmax(200px,1fr)_24px_140px_80px_80px_110px_90px_60px] gap-3 items-center px-3 py-1 hover:bg-gray-100 dark:hover:bg-gray-800 group border-b border-gray-100 dark:border-gray-800 min-w-[830px]',
                focusedProjectId === project.id && expandedProjects.has(project.id)
                  ? 'bg-blue-50 dark:bg-blue-900/20'
                  : ''
              ]"
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
              <div class="flex items-center gap-1.5 min-w-0">
                <span class="text-xs font-medium text-gray-700 dark:text-gray-300 truncate group-hover:text-blue-600 dark:group-hover:text-blue-400">
                  {{ project.name }}
                </span>
                <button
                  @click.stop="openProjectSettings(project.id)"
                  class="opacity-50 hover:opacity-100 transition-opacity flex-shrink-0"
                  title="Project Settings"
                >
                  <svg class="w-3.5 h-3.5 text-gray-500 dark:text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                </button>
                <span class="text-xs text-gray-500 dark:text-gray-400 flex-shrink-0 italic">
                  {{ project.surveyCount }} {{ project.surveyCount === 1 ? 'survey' : 'surveys' }}
                </span>
              </div>
              <!-- Client Logo (if exists) -->
              <div class="flex items-center justify-center">
                <img v-if="projectLogos.get(project.id)" :src="projectLogos.get(project.id)" :alt="project.clientName" class="max-h-4 max-w-[24px] object-contain" />
              </div>
              <span class="text-xs text-gray-500 dark:text-gray-400 truncate">
                {{ project.clientName || '' }}
              </span>
              <span class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums italic">
                
              </span>
              <span class="text-xs text-gray-500 dark:text-gray-400 text-right"></span>
              <span
                class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums"
              >
                {{ formatDateShort(project.activityDate) }}
              </span>
              <div class="flex justify-center">
                <span v-if="project.status" :class="getStatusClass(project.status)" class="text-xs px-2 py-0.5 rounded-full whitespace-nowrap">
                  {{ project.status }}
                </span>
                <span v-else class="text-xs text-gray-400"></span>
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
              <div v-if="loadingSurveys.has(project.id)" class="flex items-center space-x-2 px-3 py-1 ml-8">
                <div class="animate-spin rounded-full h-3 w-3 border-b-2 border-blue-500"></div>
                <span class="text-xs text-gray-500 dark:text-gray-400">Loading...</span>
              </div>
              <template v-else-if="projectSurveys.get(project.id) && projectSurveys.get(project.id).length > 0">
                <div
                  v-for="survey in projectSurveys.get(project.id)"
                  :key="survey.id"
                  :ref="el => { if (focusedSurveyId === survey.id) surveyRowRef = el }"
                  :class="[
                    'grid grid-cols-[32px_minmax(200px,1fr)_24px_140px_80px_80px_110px_90px_60px] gap-3 items-center px-3 py-0.5 group border-b border-gray-100 dark:border-gray-800 min-w-[860px]',
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
                      @click="navigateToSurvey(survey.id, false); handleSurveyClick(survey.id)"
                      class="text-xs text-gray-600 dark:text-gray-400 truncate hover:text-blue-600 dark:hover:text-blue-400 cursor-pointer"
                    >
                      {{ survey.name }}
                    </span>
                  </div>
                  <span class="text-xs text-gray-500 dark:text-gray-400"></span>
                  <span class="text-xs text-gray-500 dark:text-gray-400"></span>
                  <span class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums">
                    {{ survey.caseCount || 0 }}
                  </span>
                  <span class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums">
                    {{ survey.questionCount || 0 }}
                  </span>
                  <span
                    class="text-xs text-gray-500 dark:text-gray-400 text-right tabular-nums"
                  >
                    {{ formatDateShort(survey.lastAccessedDate) }}
                  </span>
                  <span class="text-xs text-gray-500 dark:text-gray-400 text-center"></span>
                  
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

    <ProjectSettingsModal
      :is-open="showSettingsModal"
      :project="selectedProject"
      @close="showSettingsModal = false"
      @saved="handleProjectSettingsSaved"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onActivated, watch, nextTick } from 'vue'
import axios from 'axios'
import { useRouter, useRoute } from 'vue-router'
import { API_BASE_URL } from '@/config/api'
import ProjectCard from './ProjectCard.vue'
import ProjectFilters from './ProjectFilters.vue'
import SortableColumnHeader from '../UI/SortableColumnHeader.vue'
import Select from 'primevue/select'
import ProjectSettingsModal from './ProjectSettingsModal.vue'

const surveyRowRef = ref(null)

const router = useRouter()
const route = useRoute()
const projects = ref([])
const recentSurveys = ref([])
const showRecentSurveys = ref(localStorage.getItem('showRecentSurveys') !== 'false')
const loading = ref(true)
const error = ref(null)
const sortBy = ref('activity')
const sortDirection = ref('desc') // 'asc' or 'desc'
const viewMode = ref(localStorage.getItem('projectViewMode') || 'list') // 'grid' or 'list', persisted
const showFilters = ref(false)
const showSettingsModal = ref(false)
const selectedProject = ref(null)
// Options for PrimeVue Select (Sort by)
const sortOptions = [
  { label: 'Recent Activity', value: 'activity', icon: 'pi pi-clock' },
  { label: 'Name (A-Z)', value: 'name', icon: 'pi pi-sort-alpha-up' },
  { label: 'Date Created', value: 'created', icon: 'pi pi-calendar' },
  { label: 'Client Name', value: 'client', icon: 'pi pi-user' },
  { label: 'Status', value: 'status', icon: 'pi pi-tag' }
]
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
const projectLogos = ref(new Map()) // Cache project logos for list view

// Fetch projects from API
async function fetchProjects() {
  loading.value = true
  error.value = null
  
  try {
    // Fetch projects with counts (logos loaded on-demand per card)
    const response = await axios.get(`${API_BASE_URL}/api/projects/with-counts`)
    
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
      lastAccessedDate: p.lastAccessedDate,
      activityDate: p.recentActivityDate || p.lastAccessedDate || p.lastModified || p.modifiedDate || p.createdAt,
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

// Fetch recently accessed surveys
async function fetchRecentSurveys() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/recent?limit=4`)
    recentSurveys.value = response.data
  } catch (err) {
    console.error('Error fetching recent surveys:', err)
    recentSurveys.value = []
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
      case 'activity':
        comparison = new Date(a.activityDate || 0) - new Date(b.activityDate || 0)
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
      default:
        // Default to recent activity ordering
        comparison = new Date(a.activityDate || 0) - new Date(b.activityDate || 0)
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
    // New column, default to sensible direction
    sortBy.value = column
    sortDirection.value = (column === 'activity' || column === 'created') ? 'desc' : 'asc'
  }
}

// Watch sortBy from dropdown and reset to sensible default direction
watch(sortBy, (newVal) => {
  if (newVal === 'created' || newVal === 'activity') {
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
  
  // Load logos for list view
  if (newVal === 'list') {
    loadProjectLogos()
  }
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
            const response = await axios.get(`${API_BASE_URL}/api/projects/${project.id}/surveys`)
            allSurveysForSearch.value.set(project.id, response.data.map(s => ({
              id: s.id,
              name: s.name,
              status: s.status,
              caseCount: s.caseCount,
              questionCount: s.questionCount,
              createdDate: s.createdDate,
              modifiedDate: s.modifiedDate,
              lastAccessedDate: s.lastAccessedDate,
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
            const response = await axios.get(`${API_BASE_URL}/api/surveys/${surveyId}/questions-list`)
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

// When a survey is clicked from a grid ProjectCard's dropdown list,
// navigate without pre-scroll and record access so recents and ordering update.
async function handleSurveyClickFromGrid(surveyId) {
  handleSurveyClick(surveyId)
  await navigateToSurvey(surveyId, false)
}

function toggleRecentSurveys() {
  showRecentSurveys.value = !showRecentSurveys.value
  localStorage.setItem('showRecentSurveys', showRecentSurveys.value.toString())
}

function openProjectSettings(projectId) {
  const project = projects.value.find(p => p.id === projectId)
  if (!project) return
  selectedProject.value = project
  showSettingsModal.value = true
}

function formatDateShort(dateString) {
  if (!dateString) return '—'
  const date = new Date(dateString)
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: '2-digit' })
}

function formatExactTime(dateString) {
  if (!dateString) return 'Never'
  const date = new Date(dateString)
  return date.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true
  })
}

function formatRelativeTime(dateString) {
  if (!dateString) return 'Never'
  
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now - date
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)
  
  if (diffMins < 1) return 'Just now'
  if (diffMins < 60) return `${diffMins}m ago`
  if (diffHours < 24) return `${diffHours}h ago`
  if (diffDays === 1) return 'Yesterday'
  if (diffDays < 7) return `${diffDays}d ago`
  if (diffDays < 30) return `${Math.floor(diffDays / 7)}w ago`
  return formatDateShort(dateString)
}

function handleFilterChange(filters) {
  activeFilters.value = filters
}

async function toggleProjectExpand(projectId, shouldClearFocus = true) {
  if (expandedProjects.value.has(projectId)) {
    expandedProjects.value.delete(projectId)
  } else {
    // Allow multiple projects to stay expanded
    expandedProjects.value.add(projectId)
    
    // Fetch surveys if not already loaded
    if (!projectSurveys.value.has(projectId)) {
      loadingSurveys.value.add(projectId)
      try {
        const response = await axios.get(`${API_BASE_URL}/api/projects/${projectId}/surveys`)
        projectSurveys.value.set(projectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          lastAccessedDate: s.lastAccessedDate,
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
  // Only clear focused survey when explicitly toggling (not when auto-expanding)
  if (shouldClearFocus) {
    focusedSurveyId.value = null
    localStorage.removeItem('focusedSurveyId')
  }
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
    const response = await axios.get(`${API_BASE_URL}/api/projects/${project.id}/surveys`)
    if (response.data && response.data.length > 0) {
      navigateToSurvey(response.data[0].id, false)
    } else {
      navigateToProject(project)
    }
  } catch (error) {
    console.error('Error loading survey:', error)
    navigateToProject(project)
  }
}

async function navigateToSurvey(surveyId, autoExpandAndScroll = true) {
  // Set focused survey for highlighting when returning
  focusedSurveyId.value = surveyId
  localStorage.setItem('focusedSurveyId', surveyId)
  
  // Find which project contains this survey
  let parentProjectId = null
  for (const project of projects.value) {
    const surveys = projectSurveys.value.get(project.id) || await loadSurveysForProject(project.id)
    if (surveys && surveys.some(s => s.id === surveyId)) {
      parentProjectId = project.id
      break
    }
  }
  
  // If we found the parent project and auto-expand is enabled
  if (parentProjectId && autoExpandAndScroll && viewMode.value === 'list') {
    focusedProjectId.value = parentProjectId
    localStorage.setItem('focusedProjectId', parentProjectId)
    
    // Expand the project if not already expanded (don't clear focused survey)
    if (!expandedProjects.value.has(parentProjectId)) {
      await toggleProjectExpand(parentProjectId, false)
    }
    
    // Scroll to the survey row after the DOM updates
    await nextTick()
    if (surveyRowRef.value) {
      surveyRowRef.value.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
  }
  
  // Record survey access before navigating
  try {
    await axios.post(`${API_BASE_URL}/api/surveys/${surveyId}/access`)
    // Refresh recent surveys and projects after recording access
    await Promise.all([
      fetchRecentSurveys(),
      fetchProjects()
    ])
    // If we were on a different sort, switch to Recent Activity so grid/list reorder
    sortBy.value = 'activity'
  } catch (error) {
    console.error('Error recording survey access:', error)
    // Continue navigation even if recording fails
  }
  router.push(`/analytics/${surveyId}`)
}

async function loadSurveysForProject(projectId) {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/projects/${projectId}/surveys`)
    const surveys = response.data.map(s => ({
      id: s.id,
      name: s.name,
      status: s.status,
      caseCount: s.caseCount,
      questionCount: s.questionCount,
      createdDate: s.createdDate,
      modifiedDate: s.modifiedDate,
      lastAccessedDate: s.lastAccessedDate,
      createdBy: s.createdBy,
      modifiedBy: s.modifiedBy
    }))
    projectSurveys.value.set(projectId, surveys)
    return surveys
  } catch (error) {
    console.error('Error loading surveys:', error)
    return []
  }
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

async function loadProjectLogos() {
  // Load logos for projects that don't have them yet
  const projectsNeedingLogos = filteredProjects.value.filter(p => 
    !projectLogos.value.has(p.id) && !p.clientLogoBase64
  )
  
  if (projectsNeedingLogos.length === 0) return
  
  // Load in batches of 10 to avoid overwhelming the server
  const batchSize = 10
  for (let i = 0; i < projectsNeedingLogos.length; i += batchSize) {
    const batch = projectsNeedingLogos.slice(i, i + batchSize)
    await Promise.all(batch.map(async (project) => {
      try {
        const response = await axios.get(`${API_BASE_URL}/api/projects/${project.id}`)
        if (response.data.clientLogoBase64) {
          const base64Data = response.data.clientLogoBase64
          let mimeType = 'image/png'
          if (base64Data.startsWith('/9j/')) mimeType = 'image/jpeg'
          else if (base64Data.startsWith('R0lGOD')) mimeType = 'image/gif'
          else if (base64Data.startsWith('UklGR')) mimeType = 'image/webp'
          
          const logoUrl = base64Data.startsWith('data:') ? base64Data : `data:${mimeType};base64,${base64Data}`
          projectLogos.value.set(project.id, logoUrl)
        }
      } catch (error) {
        console.debug('No logo for project:', project.id)
      }
    }))
  }
}

async function deleteProject(projectId, projectName) {
  if (!confirm(`Delete project "${projectName}"? It will be moved to trash.`)) return
  
  try {
    await axios.post(`${API_BASE_URL}/api/projects/${projectId}/soft-delete`)
    await fetchProjects() // Reload the list
  } catch (error) {
    console.error('Error deleting project:', error)
    alert('Failed to delete project: ' + error.message)
  }
}

async function deleteSurvey(surveyId, surveyName) {
  if (!confirm(`Delete survey "${surveyName}"? It will be moved to trash.`)) return
  
  try {
    await axios.post(`${API_BASE_URL}/api/surveys/${surveyId}/soft-delete`)
    
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
        const response = await axios.get(`${API_BASE_URL}/api/projects/${affectedProjectId}/surveys`)
        projectSurveys.value.set(affectedProjectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          lastAccessedDate: s.lastAccessedDate,
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

function handleProjectSettingsSaved(updatedData) {
  if (selectedProject.value) {
    handleProjectUpdated(selectedProject.value.id, updatedData)
  }
  showSettingsModal.value = false
  // Refresh recent surveys in case project/client name changed
  fetchRecentSurveys()
}

onMounted(async () => {
  await fetchProjects()
  await fetchRecentSurveys()
  
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
        const response = await axios.get(`${API_BASE_URL}/api/projects/${projectId}/surveys`)
        projectSurveys.value.set(projectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          lastAccessedDate: s.lastAccessedDate,
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
  
  // Load logos if starting in list view
  if (viewMode.value === 'list') {
    loadProjectLogos()
  }
  
  // Cleanup
  return () => window.removeEventListener('resize', handleResize)
})

// Refetch projects when navigating back to the gallery (e.g., from a survey)
// Only refetch if projects are empty or stale
onActivated(async () => {
  // Refresh recent surveys when returning to this page
  await fetchRecentSurveys()
  
  // Only refetch if we don't have projects loaded
  if (projects.value.length === 0) {
    await fetchProjects()
  }
  
  // Restore focus in list view: derive project from focused survey if needed
  if (focusedSurveyId.value && viewMode.value === 'list') {
    let ensureProjectId = focusedProjectId.value
    // If missing or stale, recompute the parent project for the focused survey
    if (!ensureProjectId || !projectSurveys.value.get(ensureProjectId)?.some(s => s.id === focusedSurveyId.value)) {
      for (const project of projects.value) {
        const surveys = projectSurveys.value.get(project.id) || await loadSurveysForProject(project.id)
        if (surveys && surveys.some(s => s.id === focusedSurveyId.value)) {
          ensureProjectId = project.id
          break
        }
      }
    }

    if (ensureProjectId) {
      focusedProjectId.value = ensureProjectId
      localStorage.setItem('focusedProjectId', ensureProjectId)
      if (!expandedProjects.value.has(ensureProjectId)) {
        await toggleProjectExpand(ensureProjectId, false)
      }
      await nextTick()
      if (surveyRowRef.value) {
        surveyRowRef.value.scrollIntoView({ behavior: 'smooth', block: 'center' })
      }
    }
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
        const response = await axios.get(`${API_BASE_URL}/api/projects/${projectId}/surveys`)
        projectSurveys.value.set(projectId, response.data.map(s => ({
          id: s.id,
          name: s.name,
          status: s.status,
          caseCount: s.caseCount,
          questionCount: s.questionCount,
          createdDate: s.createdDate,
          modifiedDate: s.modifiedDate,
          lastAccessedDate: s.lastAccessedDate,
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
