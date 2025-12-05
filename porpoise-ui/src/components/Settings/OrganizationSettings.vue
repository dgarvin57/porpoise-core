<template>
  <div class="max-w-4xl mx-auto p-6">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
        Organization Settings
      </h1>
      <p class="text-gray-600 dark:text-gray-400">
        Manage your organization's branding and information
      </p>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-500"></div>
    </div>

    <!-- Form -->
    <form v-else @submit.prevent="saveChanges" class="space-y-6">
      <!-- Organization Name -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Organization Name
        </label>
        <input
          v-model="formData.organizationName"
          type="text"
          class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
          placeholder="Enter your organization name"
        />
        <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
          This is your company or consultant organization name
        </p>
      </div>

      <!-- Organization Logo -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-3">
          Organization Logo
        </label>
        
        <div class="flex items-start space-x-6">
          <!-- Upload Button -->
          <div class="flex-1">
            <input
              ref="fileInput"
              type="file"
              accept="image/png,image/jpeg,image/jpg,image/gif,image/svg+xml,image/webp"
              @change="handleFileUpload"
              class="hidden"
            />
            <button
              type="button"
              @click="$refs.fileInput.click()"
              class="px-5 py-3 text-sm font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
            >
              Choose Logo File
            </button>
            <span v-if="logoFileName" class="ml-3 text-sm text-gray-600 dark:text-gray-400">
              {{ logoFileName }}
            </span>
            <span v-else-if="!logoPreviewUrl" class="ml-3 text-sm text-gray-400 dark:text-gray-500">
              No logo uploaded
            </span>
            
            <!-- Clear Logo Button -->
            <button
              v-if="logoPreviewUrl"
              type="button"
              @click="clearLogo"
              class="ml-3 px-4 py-3 text-sm font-medium text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 transition-colors"
            >
              Remove Logo
            </button>
          </div>

          <!-- Logo Preview -->
          <div v-if="logoPreviewUrl" class="flex-shrink-0">
            <div class="p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700">
              <img 
                :src="logoPreviewUrl" 
                alt="Organization logo preview" 
                class="max-h-24 max-w-48 object-contain"
              />
            </div>
          </div>
        </div>
        
        <p class="mt-3 text-sm text-gray-500 dark:text-gray-400">
          This logo represents your organization and may appear on reports and documents
        </p>
      </div>

      <!-- Organization Tagline -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-6">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          Organization Tagline
        </label>
        <input
          v-model="formData.organizationTagline"
          type="text"
          class="w-full px-4 py-3 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
          placeholder="Enter a tagline or subtitle"
        />
        <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
          A short tagline or subtitle for your organization (optional)
        </p>
      </div>

      <!-- Save Button -->
      <div class="flex justify-end pt-4">
        <button
          type="submit"
          :disabled="saving || !hasChanges"
          class="px-6 py-3 text-sm font-medium text-white bg-blue-600 rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors shadow-sm"
        >
          {{ saving ? 'Saving...' : 'Save Changes' }}
        </button>
      </div>

      <!-- Success Message -->
      <div v-if="showSuccess" class="bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg p-4">
        <div class="flex items-center">
          <svg class="w-5 h-5 text-green-600 dark:text-green-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
          <p class="text-sm font-medium text-green-800 dark:text-green-200">
            Organization settings saved successfully
          </p>
        </div>
      </div>

      <!-- Error Message -->
      <div v-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
        <div class="flex items-center">
          <svg class="w-5 h-5 text-red-600 dark:text-red-400 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
          <p class="text-sm font-medium text-red-800 dark:text-red-200">
            {{ error }}
          </p>
        </div>
      </div>
    </form>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import axios from 'axios'

const formData = ref({
  organizationName: '',
  organizationLogoBase64: '',
  organizationTagline: ''
})

const loading = ref(false)
const saving = ref(false)
const logoFileName = ref('')
const logoPreviewUrl = ref('')
const hasChanges = ref(false)
const initialFormData = ref({})
const showSuccess = ref(false)
const error = ref('')

// Watch for form changes
watch(formData, (newVal) => {
  if (initialFormData.value.organizationName !== undefined) {
    hasChanges.value = JSON.stringify(newVal) !== JSON.stringify(initialFormData.value)
  }
}, { deep: true })

async function loadOrganizationSettings() {
  loading.value = true
  error.value = ''
  
  try {
    const response = await axios.get('http://localhost:5107/api/tenants/current')
    const data = response.data
    
    formData.value = {
      organizationName: data.organizationName || '',
      organizationLogoBase64: data.organizationLogoBase64 || '',
      organizationTagline: data.organizationTagline || ''
    }
    
    // Store initial values for comparison
    initialFormData.value = { ...formData.value }
    
    // Load existing logo preview if available
    if (data.organizationLogoBase64) {
      logoPreviewUrl.value = `data:image/png;base64,${data.organizationLogoBase64}`
    }
    
    hasChanges.value = false
  } catch (err) {
    error.value = 'Failed to load organization settings'
    console.error('Error loading organization settings:', err)
  } finally {
    loading.value = false
  }
}

function handleFileUpload(event) {
  const file = event.target.files[0]
  if (file) {
    logoFileName.value = file.name
    
    // Convert to base64
    const reader = new FileReader()
    reader.onload = (e) => {
      const dataUrl = e.target.result
      formData.value.organizationLogoBase64 = dataUrl
      
      // Create preview URL
      if (logoPreviewUrl.value && logoPreviewUrl.value.startsWith('blob:')) {
        URL.revokeObjectURL(logoPreviewUrl.value)
      }
      logoPreviewUrl.value = dataUrl
      hasChanges.value = true
    }
    reader.readAsDataURL(file)
  }
}

function clearLogo() {
  formData.value.organizationLogoBase64 = ''
  logoFileName.value = ''
  if (logoPreviewUrl.value && logoPreviewUrl.value.startsWith('blob:')) {
    URL.revokeObjectURL(logoPreviewUrl.value)
  }
  logoPreviewUrl.value = ''
  hasChanges.value = true
}

async function saveChanges() {
  saving.value = true
  error.value = ''
  showSuccess.value = false
  
  try {
    const updateData = {
      organizationName: formData.value.organizationName?.trim() || null,
      organizationLogoBase64: formData.value.organizationLogoBase64 || null,
      organizationTagline: formData.value.organizationTagline?.trim() || null
    }
    
    await axios.put('http://localhost:5107/api/tenants/current/organization', updateData)
    
    // Update initial form data to reflect saved state
    initialFormData.value = { ...formData.value }
    hasChanges.value = false
    
    // Show success message
    showSuccess.value = true
    setTimeout(() => {
      showSuccess.value = false
    }, 5000)
    
  } catch (err) {
    error.value = 'Failed to save organization settings: ' + (err.response?.data?.message || err.message)
    console.error('Error saving organization settings:', err)
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadOrganizationSettings()
})
</script>
