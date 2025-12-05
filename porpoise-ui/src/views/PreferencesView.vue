<template>
  <div class="container mx-auto px-6 py-6">
    <div class="max-w-5xl mx-auto">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-6 mt-12">Preferences</h1>
      
      <div class="flex gap-6 justify-center">
        <!-- Left Sidebar Navigation -->
        <SidebarNav 
          v-model="activeSection"
          :items="navItems"
        />

        <!-- Vertical Divider -->
        <div class="w-px bg-gray-200 dark:bg-gray-700"></div>

        <!-- Right Content Area -->
        <div class="flex-1 min-w-0">
          <!-- Appearance Section -->
          <div v-if="activeSection === 'appearance'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Appearance</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Customize how the application looks</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 space-y-4">
              <!-- Theme Toggle -->
              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Theme</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">Switch between light and dark mode</div>
                </div>
                <button 
                  @click="toggleTheme"
                  class="flex items-center space-x-2 px-4 py-2 bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300 rounded-lg hover:bg-gray-200 dark:hover:bg-gray-600 transition-colors"
                >
                  <svg v-if="isDark" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
                  </svg>
                  <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
                  </svg>
                  <span>{{ isDark ? 'Light Mode' : 'Dark Mode' }}</span>
                </button>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <!-- Font Size -->
              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Font Size</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">Adjust text size throughout the app</div>
                </div>
                <select class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option>Small</option>
                  <option selected>Medium</option>
                  <option>Large</option>
                </select>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <!-- Compact Mode -->
              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Compact Mode</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">Show more content with reduced spacing</div>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" class="sr-only peer">
                  <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                </label>
              </div>
            </div>
          </div>

          <!-- Notifications Section -->
          <div v-else-if="activeSection === 'notifications'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Notifications</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Manage how you receive notifications</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 space-y-4">
              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Email Notifications</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">Receive updates via email</div>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" class="sr-only peer" checked>
                  <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                </label>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Browser Notifications</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">Show desktop notifications</div>
                </div>
                <label class="relative inline-flex items-center cursor-pointer">
                  <input type="checkbox" class="sr-only peer">
                  <div class="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-blue-300 dark:peer-focus:ring-blue-800 rounded-full peer dark:bg-gray-700 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all dark:border-gray-600 peer-checked:bg-blue-600"></div>
                </label>
              </div>
            </div>
          </div>

          <!-- Display Section -->
          <div v-else-if="activeSection === 'display'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Display</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Control how data is displayed</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 space-y-4">
              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Date Format</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">How dates are displayed</div>
                </div>
                <select class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option>MM/DD/YYYY</option>
                  <option>DD/MM/YYYY</option>
                  <option>YYYY-MM-DD</option>
                </select>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Number Format</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">Decimal separator style</div>
                </div>
                <select class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option>1,234.56 (US)</option>
                  <option>1.234,56 (EU)</option>
                  <option>1 234,56 (FR)</option>
                </select>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <div class="flex items-center justify-between">
                <div>
                  <div class="font-medium text-gray-900 dark:text-white">Decimal Places</div>
                  <div class="text-sm text-gray-500 dark:text-gray-400">Default precision for statistics</div>
                </div>
                <select class="px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option>1</option>
                  <option selected>2</option>
                  <option>3</option>
                  <option>4</option>
                </select>
              </div>
            </div>
          </div>

          <!-- About Section -->
          <div v-else-if="activeSection === 'about'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">About</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Application information and resources</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 space-y-4">
              <!-- Application Info -->
              <div class="space-y-3">
                <div class="flex items-center space-x-3">
                  <div class="w-12 h-12 bg-gradient-to-br from-blue-500 to-purple-600 rounded-lg flex items-center justify-center">
                    <span class="text-white font-bold text-2xl">P</span>
                  </div>
                  <div>
                    <div class="text-lg font-semibold text-gray-900 dark:text-white">Pulse Analytics</div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">Version 1.0.0</div>
                  </div>
                </div>

                <p class="text-sm text-gray-600 dark:text-gray-400">
                  Professional polling and survey analysis platform for political campaigns and market research.
                </p>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <!-- Links -->
              <div class="space-y-2">
                <a href="#" class="flex items-center justify-between px-3 py-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                  <span class="text-sm text-gray-700 dark:text-gray-300">Release Notes</span>
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </a>
                <a href="#" class="flex items-center justify-between px-3 py-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                  <span class="text-sm text-gray-700 dark:text-gray-300">Privacy Policy</span>
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </a>
                <a href="#" class="flex items-center justify-between px-3 py-2 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                  <span class="text-sm text-gray-700 dark:text-gray-300">Terms of Service</span>
                  <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                  </svg>
                </a>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <!-- Copyright -->
              <div class="text-center text-sm text-gray-500 dark:text-gray-400">
                © 2025 Pulse Analytics. All rights reserved.
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import SidebarNav from '../components/SidebarNav.vue'

const activeSection = ref('appearance')

const navItems = [
  { 
    label: 'Appearance', 
    value: 'appearance',
    icon: 'M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01'
  },
  { 
    label: 'Notifications', 
    value: 'notifications',
    icon: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9'
  },
  { 
    label: 'Display', 
    value: 'display',
    icon: 'M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z'
  },
  { 
    label: 'About', 
    value: 'about',
    icon: 'M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'
  }
]

// Read initial theme state
const isDark = ref(document.documentElement.classList.contains('dark'))

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
</script>
