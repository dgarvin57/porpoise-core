<template>
  <div v-if="isOpen" class="fixed inset-0 z-[200] overflow-y-auto" @click.self="closeModal">
    <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:p-0">
      <!-- Background overlay -->
      <div class="fixed inset-0 transition-opacity bg-gray-900/50 dark:bg-black/70 backdrop-blur-sm" @click="closeModal"></div>

      <!-- Modal panel -->
      <div class="relative inline-block w-full max-w-2xl my-8 overflow-hidden text-left align-middle transition-all transform bg-white dark:bg-gray-800 shadow-2xl rounded-2xl border border-gray-200 dark:border-gray-700">
        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200 dark:border-gray-700 bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-gray-800 dark:to-gray-800">
          <h3 class="text-xl font-semibold text-gray-900 dark:text-white">
            Project Settings
          </h3>
          <CloseButton @click="closeModal" />
        </div>

        <!-- Loading State -->
        <div v-if="loading" class="px-6 py-12 flex items-center justify-center">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
        </div>

        <!-- Tabs -->
        <div v-else>
          <div class="flex border-b border-gray-200 dark:border-gray-700 bg-gray-50/50 dark:bg-gray-800/50">
            <button
              type="button"
              @click="activeTab = 'settings'"
              :class="[
                'flex-1 px-6 py-3 text-sm font-medium transition-colors',
                activeTab === 'settings'
                  ? 'text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400 bg-white dark:bg-gray-800'
                  : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700/50'
              ]"
            >
              Settings
            </button>
            <button
              type="button"
              @click="activeTab = 'info'"
              :class="[
                'flex-1 px-6 py-3 text-sm font-medium transition-colors',
                activeTab === 'info'
                  ? 'text-blue-600 dark:text-blue-400 border-b-2 border-blue-600 dark:border-blue-400 bg-white dark:bg-gray-800'
                  : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-700/50'
              ]"
            >
              Info
            </button>
          </div>

          <!-- Settings Tab -->
          <form v-show="activeTab === 'settings'" @submit.prevent="saveChanges" class="px-5 py-3 space-y-2">
          <!-- Project Name -->
          <div>
            <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-0.5">
              Project Name
            </label>
            <input
              v-model="formData.projectName"
              type="text"
              required
              class="w-full px-2.5 py-1.5 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              placeholder="Enter project name"
            />
          </div>

          <!-- Client Name -->
          <div>
            <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-0.5">
              Client Name
            </label>
            <input
              v-model="formData.clientName"
              type="text"
              class="w-full px-2.5 py-1.5 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              placeholder="Enter client name"
            />
          </div>

          <!-- Client Logo -->
          <div>
            <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-0.5">
              Client Logo
            </label>
            <div class="flex items-start space-x-2">
              <div class="flex items-center space-x-2">
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
                  class="px-2.5 py-1.5 text-xs font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
                >
                  Choose File
                </button>
                <span v-if="logoFileName" class="text-xs text-gray-600 dark:text-gray-400 truncate">
                  {{ logoFileName }}
                </span>
                <span v-else-if="formData.clientLogoFilename" class="text-xs text-gray-600 dark:text-gray-400 truncate">
                  {{ formData.clientLogoFilename }}
                </span>
                <span v-else class="text-xs text-gray-400 dark:text-gray-500">
                  No file chosen
                </span>
              </div>
              <!-- Image Preview to the right -->
              <div v-if="logoPreviewUrl" class="flex-shrink-0">
                <img :src="logoPreviewUrl" alt="Logo preview" class="max-h-16 max-w-[100px] object-contain shadow-sm rounded" />
              </div>
            </div>
          </div>

          <!-- Description -->
          <div>
            <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-0.5">
              Description
            </label>
            <textarea
              v-model="formData.description"
              rows="2"
              class="w-full px-2.5 py-1.5 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none transition-shadow"
              placeholder="Enter project description"
            ></textarea>
          </div>

          <!-- Default Weighting Scheme -->
          <div>
            <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-0.5">
              Default Weighting Scheme
            </label>
            <input
              v-model="formData.defaultWeightingScheme"
              type="text"
              class="w-full px-2.5 py-1.5 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              placeholder="Enter default weighting scheme"
            />
          </div>

          <!-- Date Range -->
          <div class="grid grid-cols-2 gap-2">
            <div>
              <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-0.5">
                Start Date
              </label>
              <input
                v-model="formData.startDate"
                type="date"
                class="w-full px-2.5 py-1.5 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              />
            </div>
            <div>
              <label class="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-0.5">
                End Date
              </label>
              <input
                v-model="formData.endDate"
                type="date"
                class="w-full px-2.5 py-1.5 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-900 text-gray-900 dark:text-gray-100 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-shadow"
              />
            </div>
          </div>

          <!-- Action Buttons for Settings Tab -->
          <div v-show="activeTab === 'settings'" class="flex justify-end space-x-2 px-5 py-2.5 bg-gray-50/50 dark:bg-gray-800/50">
            <button
              type="button"
              @click="closeModal"
              class="px-3 py-1.5 text-xs font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              @click="saveChanges"
              :disabled="saving || !hasChanges"
              class="px-4 py-1.5 text-xs font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors shadow-sm"
            >
              {{ saving ? 'Saving...' : 'Save Changes' }}
            </button>
          </div>
        </form>

        <!-- Info Tab -->
        <div v-show="activeTab === 'info'" class="px-5 py-4">
          <div class="space-y-4">
            <div class="grid grid-cols-2 gap-4">
              <div class="space-y-0.5">
                <label class="block text-[10px] font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Project ID</label>
                <p class="text-xs text-gray-900 dark:text-gray-100 font-mono bg-gray-100 dark:bg-gray-900 px-2 py-1.5 rounded border border-gray-200 dark:border-gray-700">
                  {{ props.project?.id || 'N/A' }}
                </p>
              </div>
              <div class="space-y-0.5">
                <label class="block text-[10px] font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Status</label>
                <p class="text-xs text-gray-900 dark:text-gray-100 bg-gray-100 dark:bg-gray-900 px-2 py-1.5 rounded border border-gray-200 dark:border-gray-700">
                  {{ metadata.status || 'Active' }}
                </p>
              </div>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div class="space-y-0.5">
                <label class="block text-[10px] font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Created</label>
                <div class="bg-gray-100 dark:bg-gray-900 px-2 py-1.5 rounded border border-gray-200 dark:border-gray-700">
                  <p class="text-xs text-gray-900 dark:text-gray-100">{{ formatDateTime(metadata.createdDate) }}</p>
                  <p v-if="metadata.createdBy" class="text-[10px] text-gray-500 dark:text-gray-400 mt-0.5">
                    <span class="font-medium">by</span> {{ metadata.createdBy }}
                  </p>
                  <p v-else class="text-[10px] text-gray-400 dark:text-gray-500 mt-0.5 italic">Unknown user</p>
                </div>
              </div>
              <div class="space-y-0.5">
                <label class="block text-[10px] font-medium text-gray-500 dark:text-gray-400 uppercase tracking-wider">Last Modified</label>
                <div class="bg-gray-100 dark:bg-gray-900 px-2 py-1.5 rounded border border-gray-200 dark:border-gray-700">
                  <p class="text-xs text-gray-900 dark:text-gray-100">{{ formatDateTime(metadata.modifiedDate) }}</p>
                  <p v-if="metadata.modifiedBy" class="text-[10px] text-gray-500 dark:text-gray-400 mt-0.5">
                    <span class="font-medium">by</span> {{ metadata.modifiedBy }}
                  </p>
                  <p v-else class="text-[10px] text-gray-400 dark:text-gray-500 mt-0.5 italic">Unknown user</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Close Button for Info Tab -->
          <div class="flex justify-end mt-4 pt-4 border-t border-gray-200 dark:border-gray-700">
            <button
              type="button"
              @click="closeModal"
              class="px-3 py-1.5 text-xs font-medium text-gray-700 dark:text-gray-300 bg-white dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded-md hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
            >
              Close
            </button>
          </div>
        </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { API_BASE_URL } from '@/config/api'
import { ref, watch } from 'vue'
import axios from 'axios'
import CloseButton from '../common/CloseButton.vue'

const props = defineProps({
  isOpen: Boolean,
  project: Object
})

const emit = defineEmits(['close', 'saved'])

const activeTab = ref('settings')
const formData = ref({
  projectName: '',
  clientName: '',
  description: '',
  clientLogoBase64: '',
  defaultWeightingScheme: '',
  startDate: '',
  endDate: ''
})

const saving = ref(false)
const loading = ref(false)
const logoFile = ref(null)
const logoFileName = ref('')
const logoPreviewUrl = ref('')
const hasChanges = ref(false)
const initialFormData = ref({})

const metadata = ref({
  status: '',
  createdDate: '',
  createdBy: '',
  modifiedDate: '',
  modifiedBy: ''
})

// Watch for modal open/close and fetch full project details
watch(() => props.isOpen, async (isOpen) => {
  if (isOpen && props.project) {
    activeTab.value = 'settings'
    loading.value = true
    try {
      // Fetch full project details including logo
      const response = await axios.get(`${API_BASE_URL}/api/projects/${props.project.id}`)
      const projectData = response.data
      
      formData.value = {
        projectName: projectData.projectName || '',
        clientName: projectData.clientName || '',
        description: projectData.description || '',
        clientLogoBase64: projectData.clientLogoBase64 || '',
        defaultWeightingScheme: projectData.defaultWeightingScheme || '',
        startDate: projectData.startDate ? projectData.startDate.split('T')[0] : '',
        endDate: projectData.endDate ? projectData.endDate.split('T')[0] : ''
      }

      // Load metadata
      metadata.value = {
        status: projectData.status || 'Active',
        createdDate: projectData.createdAt || projectData.createdDate || '',
        createdBy: projectData.createdBy || '',
        modifiedDate: projectData.lastModified || projectData.modifiedDate || '',
        modifiedBy: projectData.modifiedBy || ''
      }
      
      // Store initial values for comparison
      initialFormData.value = { ...formData.value }
      
      // Reset file upload state
      logoFile.value = null
      logoFileName.value = ''
      if (logoPreviewUrl.value && logoPreviewUrl.value.startsWith('blob:')) {
        URL.revokeObjectURL(logoPreviewUrl.value)
        logoPreviewUrl.value = ''
      }
      
      // Load existing logo from base64 if available
      if (projectData.clientLogoBase64) {
        logoPreviewUrl.value = `data:image/png;base64,${projectData.clientLogoBase64}`
      }
      
      hasChanges.value = false
    } catch (error) {
      console.error('Error fetching project details:', error)
    } finally {
      loading.value = false
    }
  }
})

// Legacy watch for project prop changes (kept for backward compatibility)
watch(() => props.project, (newProject) => {
  // This now only runs if modal is already open and project changes
  // The main loading is handled by the isOpen watch above
  if (!props.isOpen && newProject) {
    formData.value = {
      projectName: newProject.name || '',
      clientName: newProject.clientName || '',
      description: newProject.description || '',
      clientLogoBase64: newProject.clientLogoBase64 || '',
      defaultWeightingScheme: newProject.defaultWeightingScheme || '',
      startDate: newProject.startDate ? newProject.startDate.split('T')[0] : '',
      endDate: newProject.endDate ? newProject.endDate.split('T')[0] : ''
    }
    // Store initial values for comparison
    initialFormData.value = { ...formData.value }
    
    // Reset file upload state
    logoFile.value = null
    logoFileName.value = ''
    if (logoPreviewUrl.value && logoPreviewUrl.value.startsWith('blob:')) {
      URL.revokeObjectURL(logoPreviewUrl.value)
      logoPreviewUrl.value = ''
    }
    
    // Load existing logo from base64 if available
    if (newProject.clientLogoBase64) {
      logoPreviewUrl.value = `data:image/png;base64,${newProject.clientLogoBase64}`
    }
    
    hasChanges.value = false
  }
}, { immediate: false })  // Changed to false since we fetch on modal open

// Watch for form changes
watch(formData, (newVal) => {
  if (initialFormData.value.projectName) {
    hasChanges.value = JSON.stringify(newVal) !== JSON.stringify(initialFormData.value) || logoFile.value !== null
  }
}, { deep: true })

function closeModal() {
  emit('close')
}

function handleFileUpload(event) {
  const file = event.target.files[0]
  if (file) {
    logoFile.value = file
    logoFileName.value = file.name
    
    // Convert to base64
    const reader = new FileReader()
    reader.onload = (e) => {
      // Get the full data URL (includes data:image/png;base64, prefix)
      const dataUrl = e.target.result
      formData.value.clientLogoBase64 = dataUrl
      
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

async function saveChanges() {
  if (!formData.value.projectName.trim()) {
    alert('Project name is required')
    return
  }

  saving.value = true
  try {
    // Extract just the base64 data without the data URL prefix
    let logoBase64 = formData.value.clientLogoBase64
    if (logoBase64 && logoBase64.startsWith('data:')) {
      // Remove the "data:image/...;base64," prefix
      logoBase64 = logoBase64.split(',')[1]
    }
    
    // Prepare the data to send
    const updateData = {
      projectName: formData.value.projectName.trim(),
      clientName: formData.value.clientName?.trim() || null,
      description: formData.value.description?.trim() || null,
      clientLogoBase64: logoBase64 || null,
      defaultWeightingScheme: formData.value.defaultWeightingScheme?.trim() || null,
      startDate: formData.value.startDate || null,
      endDate: formData.value.endDate || null
    }
    
    await axios.put(`${API_BASE_URL}/api/projects/${props.project.id}`, updateData)
    
    // Emit with proper field mapping for parent component
    emit('saved', {
      projectName: updateData.projectName,
      clientName: updateData.clientName,
      description: updateData.description,
      clientLogoBase64: updateData.clientLogoBase64,
      defaultWeightingScheme: updateData.defaultWeightingScheme,
      startDate: updateData.startDate,
      endDate: updateData.endDate
    })
    closeModal()
  } catch (error) {
    console.error('Error saving project:', error)
    alert('Failed to save project settings: ' + (error.response?.data?.message || error.message))
  } finally {
    saving.value = false
  }
}

function formatDateTime(dateString) {
  if (!dateString) return 'N/A'
  const date = new Date(dateString)
  return date.toLocaleString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true
  })
}
</script>
