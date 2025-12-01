<template>
  <nav class="fixed top-0 left-0 right-0 z-50 bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700 shadow-sm">
    <div class="px-10 py-1">
      <div class="flex items-center justify-between gap-8">
        <!-- Logo and Brand -->
        <router-link to="/" class="flex items-center space-x-3 flex-shrink-0">
          <div class="w-8 h-8 bg-gradient-to-br from-blue-500 to-purple-600 rounded-lg flex items-center justify-center">
            <span class="text-white font-bold text-lg">P</span>
          </div>
          <div>
            <span class="text-lg font-semibold text-gray-900 dark:text-white">Pulse</span>
            <span class="text-lg font-light text-gray-600 dark:text-gray-400 ml-1">Analytics</span>
          </div>
        </router-link>

      <!-- Center: Global Search -->
      <div class="flex-1 max-w-2xl">
        <div class="relative w-full">
          <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <svg class="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
          </div>
          <input
            v-model="searchQuery"
            @input="handleSearch"
            type="text"
            placeholder="Search projects, surveys, clients..."
            class="block w-full pl-10 pr-12 py-1.5 border border-gray-300 dark:border-gray-600 rounded-lg bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-gray-100 placeholder-gray-500 dark:placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <kbd class="absolute inset-y-0 right-3 flex items-center text-xs text-gray-400">⌘K</kbd>
        </div>
      </div>

      <!-- Right: Actions -->
      <div class="flex items-center space-x-4 flex-shrink-0">
        <!-- Notifications -->
        <button class="relative text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
          </svg>
          <span class="absolute top-0 right-0 block h-2 w-2 rounded-full bg-red-500 ring-2 ring-white dark:ring-gray-900"></span>
        </button>

        <!-- Settings -->
        <button @click="openSettings" class="text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </button>

        <!-- User Avatar -->
        <div class="relative">
          <button @click="toggleUserMenu" class="flex items-center space-x-2 focus:outline-none">
            <img
              src="https://ui-avatars.com/api/?name=User&background=3b82f6&color=fff"
              alt="User avatar"
              class="w-8 h-8 rounded-full ring-2 ring-gray-200 dark:ring-gray-700"
            />
          </button>
          
          <!-- User Dropdown Menu -->
          <div v-if="showUserMenu" class="absolute right-0 mt-2 w-48 bg-white dark:bg-gray-800 rounded-lg shadow-lg border border-gray-200 dark:border-gray-700 py-1 z-50">
            <a href="#" class="block px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700">
              Profile
            </a>
            <a href="#" class="block px-4 py-2 text-sm text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700">
              Account Settings
            </a>
            <hr class="my-1 border-gray-200 dark:border-gray-700">
            <a href="#" class="block px-4 py-2 text-sm text-red-600 dark:text-red-400 hover:bg-gray-100 dark:hover:bg-gray-700">
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
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const searchQuery = ref('')
const showUserMenu = ref(false)

const toggleSidebar = () => {
  // Will emit event for parent to handle
  console.log('Toggle sidebar')
}

const handleSearch = () => {
  // Will implement global search
  console.log('Search:', searchQuery.value)
}

const openSettings = () => {
  router.push('/settings')
}

const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
}

// Close menu when clicking outside
const closeUserMenu = () => {
  showUserMenu.value = false
}
</script>
