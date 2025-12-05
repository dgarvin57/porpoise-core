<template>
  <div class="container mx-auto px-6 py-6">
    <div class="max-w-5xl mx-auto">
      <h1 class="text-2xl font-bold text-gray-900 dark:text-white mb-6">Settings</h1>
      
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
          <!-- Profile Form -->
          <div v-if="activeSection === 'profile'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Profile</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Update your personal information</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Full Name
                </label>
                <input
                  type="text"
                  placeholder="Enter your full name"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Email
                </label>
                <input
                  type="email"
                  placeholder="your@email.com"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Title
                </label>
                <input
                  type="text"
                  placeholder="Your job title"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              
              <div class="flex justify-end pt-2">
                <button class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500">
                  Save Changes
                </button>
              </div>
            </div>
          </div>

          <!-- Account Form -->
          <div v-else-if="activeSection === 'account'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Account</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Manage your account security</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 space-y-4">
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Current Password
                </label>
                <input
                  type="password"
                  placeholder="Enter current password"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  New Password
                </label>
                <input
                  type="password"
                  placeholder="Enter new password"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              
              <div>
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Confirm New Password
                </label>
                <input
                  type="password"
                  placeholder="Confirm new password"
                  class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
              
              <div class="flex justify-end pt-2">
                <button class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500">
                  Change Password
                </button>
              </div>
            </div>
          </div>

          <!-- Organization Form -->
          <div v-else-if="activeSection === 'organization'">
            <OrganizationSettings />
          </div>

          <!-- Subscription Section -->
          <div v-else-if="activeSection === 'subscription'" class="space-y-4">
            <div>
              <h2 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Subscription</h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">Manage your billing and subscription</p>
            </div>
            
            <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 space-y-4">
              <!-- Current Plan -->
              <div>
                <div class="flex items-center justify-between mb-3">
                  <div>
                    <div class="font-semibold text-gray-900 dark:text-white text-lg">Professional Plan</div>
                    <div class="text-sm text-gray-500 dark:text-gray-400">Unlimited surveys and responses</div>
                  </div>
                  <span class="px-3 py-1 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400 text-sm font-medium rounded-full">
                    Active
                  </span>
                </div>
                
                <div class="text-2xl font-bold text-gray-900 dark:text-white">
                  $99.00 <span class="text-base font-normal text-gray-500 dark:text-gray-400">/ month</span>
                </div>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <!-- Billing Info -->
              <div class="space-y-2">
                <div class="flex justify-between text-sm">
                  <span class="text-gray-600 dark:text-gray-400">Next billing date</span>
                  <span class="font-medium text-gray-900 dark:text-white">January 4, 2026</span>
                </div>
                <div class="flex justify-between text-sm">
                  <span class="text-gray-600 dark:text-gray-400">Payment method</span>
                  <span class="font-medium text-gray-900 dark:text-white">•••• 4242</span>
                </div>
              </div>

              <hr class="border-gray-200 dark:border-gray-700">

              <!-- Actions -->
              <div class="flex flex-col space-y-2">
                <button class="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500">
                  Upgrade Plan
                </button>
                <button class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600">
                  Update Payment Method
                </button>
                <button class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600">
                  View Billing History
                </button>
                <button class="px-4 py-2 text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-lg">
                  Cancel Subscription
                </button>
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
import OrganizationSettings from '../components/Settings/OrganizationSettings.vue'

const activeSection = ref('profile')

const navItems = [
  { 
    label: 'Profile', 
    value: 'profile',
    icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z'
  },
  { 
    label: 'Account', 
    value: 'account',
    icon: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z'
  },
  { 
    label: 'Organization', 
    value: 'organization',
    icon: 'M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4'
  },
  { 
    label: 'Subscription', 
    value: 'subscription',
    icon: 'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z'
  }
]
</script>
