<template>
  <div class="container mx-auto px-6 py-8">
    <div class="max-w-6xl mx-auto">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-8">Settings</h1>
      
      <!-- Tabs -->
      <div class="border-b border-gray-200 dark:border-gray-700 mb-8">
        <nav class="-mb-px flex space-x-8">
          <button
            @click="activeTab = 'organization'"
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'organization'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
            ]"
          >
            Organization
          </button>
          <button
            @click="activeTab = 'appearance'"
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'appearance'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
            ]"
          >
            Appearance
          </button>
          <button
            @click="activeTab = 'account'"
            :class="[
              'py-4 px-1 border-b-2 font-medium text-sm transition-colors',
              activeTab === 'account'
                ? 'border-blue-500 text-blue-600 dark:text-blue-400'
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300 hover:border-gray-300 dark:hover:border-gray-600'
            ]"
          >
            Account
          </button>
        </nav>
      </div>

      <!-- Tab Content -->
      <div>
        <!-- Organization Tab -->
        <div v-if="activeTab === 'organization'">
          <OrganizationSettings />
        </div>

        <!-- Appearance Tab -->
        <div v-else-if="activeTab === 'appearance'">
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 space-y-6">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Appearance</h2>
              <div class="space-y-4">
                <div class="flex items-center justify-between">
                  <span class="text-gray-700 dark:text-gray-300">Dark Mode</span>
                  <button class="px-4 py-2 bg-gray-200 dark:bg-gray-700 rounded-lg text-sm">
                    Coming Soon
                  </button>
                </div>
                <div class="flex items-center justify-between">
                  <span class="text-gray-700 dark:text-gray-300">Font Size</span>
                  <button class="px-4 py-2 bg-gray-200 dark:bg-gray-700 rounded-lg text-sm">
                    Coming Soon
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Account Tab -->
        <div v-else-if="activeTab === 'account'">
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6 space-y-6">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Account</h2>
              <div class="space-y-4">
                <div class="flex items-center justify-between">
                  <span class="text-gray-700 dark:text-gray-300">Profile</span>
                  <button class="px-4 py-2 bg-gray-200 dark:bg-gray-700 rounded-lg text-sm">
                    Edit
                  </button>
                </div>
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
import OrganizationSettings from '../components/Settings/OrganizationSettings.vue'

const activeTab = ref('organization')
</script>
