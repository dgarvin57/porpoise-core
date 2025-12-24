<template>
  <nav class="fixed top-0 left-0 right-0 z-50 bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700 shadow-sm">
    <div class="px-10 py-2">
      <div class="flex items-center justify-between gap-8">
        <!-- Logo and Brand -->
        <div class="flex items-center space-x-4 flex-shrink-0">
          <router-link to="/" class="flex items-center space-x-3">
            <img
              :src="porpoiseLogo"
              alt="Porpoise Analytics logo"
              class="w-8 h-8 rounded-lg shadow-sm"
            />
            <div class="leading-tight">
              <span class="text-base font-semibold text-gray-900 dark:text-white">{{ PRODUCT_FULL_NAME }}</span>
              <span class="ml-2 text-xs text-gray-600 dark:text-gray-400 hidden sm:inline">{{ PRODUCT_TAGLINE }}</span>
            </div>
          </router-link>
          
          <!-- Home Icon (hidden on home page) -->
          <router-link 
            v-if="route.name !== 'home'"
            to="/" 
            class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white transition-colors"
            title="Home"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
            </svg>
          </router-link>
          
          <!-- Survey Context (shown when on analytics page) -->
          <div v-if="surveyContext" class="flex items-center space-x-2 pl-4 border-l border-gray-300 dark:border-gray-600">
            <span v-if="surveyContext.projectName" class="text-xs text-gray-500 dark:text-gray-400">{{ surveyContext.projectName }} /</span>
            <span class="text-sm font-medium text-gray-900 dark:text-white">{{ surveyContext.name }}</span>
            <template v-if="surveyContext.cases > 0 || surveyContext.questions > 0">
              <span class="text-xs text-gray-500 dark:text-gray-400">•</span>
              <span class="text-xs text-gray-600 dark:text-gray-400">{{ surveyContext.cases }} cases</span>
              <span class="text-xs text-gray-500 dark:text-gray-400">•</span>
              <span class="text-xs text-gray-600 dark:text-gray-400">{{ surveyContext.questions }} questions</span>
            </template>
          </div>
        </div>

      <!-- Right: Actions -->
      <div class="flex items-center space-x-4 flex-shrink-0">
        <!-- Import -->
        <router-link 
          to="/import" 
          class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
          title="Import Survey"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>
        </router-link>

        <!-- Notifications -->
        <button class="relative bg-transparent text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
          <span class="absolute top-0 right-0 block h-2 w-2 rounded-full bg-red-500 ring-2 ring-white dark:ring-gray-900"></span>
        </button>

        <!-- Settings (Preferences) -->
        <router-link 
          to="/preferences" 
          class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
          title="Preferences"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </router-link>

        <!-- User Avatar -->
        <div class="relative" v-click-outside="closeUserMenu">
          <button @click="toggleUserMenu" class="bg-transparent flex items-center space-x-2 focus:outline-none">
            <img
              src="https://ui-avatars.com/api/?name=User&background=3b82f6&color=fff"
              alt="User avatar"
              class="w-7 h-7 rounded-full ring-2 ring-gray-200 dark:ring-gray-700"
            />
          </button>
          
          <!-- User Dropdown Menu -->
          <div v-if="showUserMenu" class="absolute right-0 mt-2 w-48 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 py-1 z-50">
            <router-link to="/settings" @click="closeUserMenu" class="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              Account
            </router-link>
            <router-link to="/help" @click="closeUserMenu" class="flex items-center gap-2 px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 5.636l-3.536 3.536m0 5.656l3.536 3.536M9.172 9.172L5.636 5.636m3.536 9.192l-3.536 3.536M21 12a9 9 0 11-18 0 9 9 0 0118 0zm-5 0a4 4 0 11-8 0 4 4 0 018 0z" />
              </svg>
              Help & Support
            </router-link>
            <hr class="my-1 border-gray-200 dark:border-gray-700">
            <a href="#" class="flex items-center gap-2 px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-gray-100 dark:hover:bg-gray-700">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
              Sign Out
            </a>
          </div>
        </div>
      </div>
    </div>
    </div>
  </nav>
</template>

<script setup>
import { ref, onMounted, computed, watch, provide } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from 'axios'
import { API_BASE_URL } from '@/config/api'
import { PRODUCT_FULL_NAME, PRODUCT_TAGLINE } from '@/config/branding'
import porpoiseLogo from '@/assets/Porpoise_48.png'

const router = useRouter()
const route = useRoute()
const showUserMenu = ref(false)

// Survey context for analytics pages
const surveyContext = ref(null)

// Watch route changes to update survey context
watch(() => route.params.id, async (surveyId) => {
  if (surveyId && route.name === 'analytics') {
    try {
      // Get survey basic info
      const surveyResponse = await axios.get(`${API_BASE_URL}/api/surveys/${surveyId}`)
      const survey = surveyResponse.data
      
      // Get stats for question and case counts
      const statsResponse = await axios.get(`${API_BASE_URL}/api/surveys/${surveyId}/stats`)
      const stats = statsResponse.data
      
      // Get project name if available
      let projectName = null
      if (survey.projectId) {
        try {
          const projectResponse = await axios.get(`${API_BASE_URL}/api/projects/${survey.projectId}`)
          projectName = projectResponse.data.projectName
        } catch (error) {
          console.error('Error loading project:', error)
        }
      }
      
      surveyContext.value = {
        name: survey.surveyName || survey.name || 'Survey',
        cases: stats.responseCount || 0,
        questions: stats.questionCount || 0,
        projectName: projectName
      }
    } catch (error) {
      console.error('Error loading survey context:', error)
      surveyContext.value = null
    }
  } else {
    surveyContext.value = null
  }
}, { immediate: true })

// Read initial theme state (already set by index.html script)
const isDark = ref(document.documentElement.classList.contains('dark'))

// Click outside directive
const vClickOutside = {
  mounted(el, binding) {
    el.clickOutsideEvent = (event) => {
      if (!(el === event.target || el.contains(event.target))) {
        binding.value(event)
      }
    }
    document.addEventListener('click', el.clickOutsideEvent)
  },
  unmounted(el) {
    document.removeEventListener('click', el.clickOutsideEvent)
  }
}

const toggleTheme = () => {
  isDark.value = !isDark.value
  
  if (isDark.value) {
    document.documentElement.classList.add('dark')
    localStorage.setItem('theme', 'dark')
  } else {
    document.documentElement.classList.remove('dark')
    localStorage.setItem('theme', 'light')
  }
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
}

// Close menu when clicking outside
const closeUserMenu = () => {
  showUserMenu.value = false
}
</script>
