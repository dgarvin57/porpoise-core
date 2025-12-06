<template>
  <div class="container mx-auto px-10 py-8 max-w-6xl">
    <!-- Page Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">
        Import Survey Data
      </h1>
      <p class="text-gray-600 dark:text-gray-400">
        Upload survey files (.porps + .porpd for survey with data, optional .porp for project info, or .porpz archive)
      </p>
    </div>

    <!-- Upload Section -->
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-8">
      <!-- File Drop Zone -->
      <div
        @dragover.prevent="isDragging = true"
        @dragleave.prevent="isDragging = false"
        @drop.prevent="handleDrop"
        :class="[
          'border-2 border-dashed rounded-lg p-8 text-center transition-colors',
          isDragging
            ? 'border-blue-500 bg-blue-50 dark:bg-blue-900/10'
            : 'border-gray-300 dark:border-gray-600 hover:border-gray-400 dark:hover:border-gray-500'
        ]"
      >
        <div class="flex flex-col items-center">
          <!-- Upload Icon -->
          <svg class="w-16 h-16 text-gray-400 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
          </svg>

          <!-- Instructions -->
          <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-2">
            Drop your files here
          </h3>
          <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">
            or click to browse
          </p>

          <!-- File Input -->
          <input
            ref="fileInput"
            type="file"
            multiple
            accept=".porps,.porp,.porpd,.porpz"
            @change="handleFileSelect"
            class="hidden"
          />
          <button
            @click="$refs.fileInput.click()"
            class="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition-colors"
          >
            Select Files
          </button>

          <p class="text-xs text-gray-500 dark:text-gray-500 mt-4">
            Supported: .porps (survey), .porpd (data), .porp (project, optional), .porpz (archive) • Max 50MB
          </p>
        </div>
      </div>

      <!-- Selected Files List -->
      <div v-if="selectedFiles.length > 0" class="mt-6">
        <h4 class="text-sm font-semibold text-gray-900 dark:text-white mb-3">
          Selected Files ({{ selectedFiles.length }})
        </h4>
        <div class="space-y-2">
          <div
            v-for="(file, index) in selectedFiles"
            :key="index"
            class="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-900 rounded-lg"
          >
            <div class="flex items-center space-x-3 flex-1 min-w-0">
              <!-- File Icon -->
              <svg class="w-5 h-5 text-gray-400 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
              
              <!-- File Info -->
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900 dark:text-white truncate">
                  {{ file.name }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400">
                  {{ formatFileSize(file.size) }}
                </p>
              </div>

              <!-- Upload Progress -->
              <div v-if="file.uploading" class="flex items-center space-x-2">
                <div class="w-32 h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
                  <div
                    class="h-full bg-blue-600 transition-all duration-300"
                    :style="{ width: file.progress + '%' }"
                  ></div>
                </div>
                <span class="text-xs text-gray-600 dark:text-gray-400">{{ file.progress }}%</span>
              </div>

              <!-- Status Icon -->
              <div v-else-if="file.success" class="text-green-500">
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd" />
                </svg>
              </div>

              <div v-else-if="file.error" class="text-red-500">
                <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
                </svg>
              </div>
            </div>

            <!-- Remove Button -->
            <CloseButton
              v-if="!file.uploading"
              @click="removeFile(index)"
              size="sm"
              class="ml-3"
            />
          </div>
        </div>

        <!-- Error Messages -->
        <div v-if="errorMessages.length > 0" class="mt-4 space-y-2">
          <div
            v-for="(error, index) in errorMessages"
            :key="index"
            class="p-3 bg-red-50 dark:bg-red-900/10 border border-red-200 dark:border-red-800 rounded-lg"
          >
            <p class="text-sm text-red-800 dark:text-red-300">{{ error }}</p>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="flex items-center justify-between mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
          <Button
            @click="clearAll"
            :disabled="isUploading"
            variant="ghost"
          >
            Clear All
          </Button>
          
          <div class="flex space-x-3">
            <Button
              v-if="hasSuccessfulUploads"
              @click="showInspectModal = true"
              variant="secondary"
            >
              Inspect Data
            </Button>
            <Button
              @click="uploadFiles"
              :disabled="isUploading || selectedFiles.length === 0"
            >
              {{ isUploading ? 'Uploading...' : 'Upload Files' }}
            </Button>
          </div>
        </div>
      </div>
    </div>

    <!-- Recent Imports -->
    <div v-if="recentImports.length > 0" class="mt-8">
      <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-4">
        Recent Imports
      </h2>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full">
            <thead class="bg-gray-50 dark:bg-gray-900 border-b border-gray-200 dark:border-gray-700">
              <tr>
                <th class="px-6 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider">
                  Survey Name
                </th>
                <th class="px-6 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider">
                  Imported
                </th>
                <th class="px-6 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider">
                  Questions
                </th>
                <th class="px-6 py-3 text-left text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider">
                  Responses
                </th>
                <th class="px-6 py-3 text-right text-xs font-semibold text-gray-700 dark:text-gray-300 uppercase tracking-wider">
                  Actions
                </th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-200 dark:divide-gray-700">
              <tr
                v-for="survey in recentImports"
                :key="survey.id"
                class="hover:bg-gray-50 dark:hover:bg-gray-900/50"
              >
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm font-medium text-gray-900 dark:text-white">
                    {{ survey.name }}
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm text-gray-600 dark:text-gray-400">
                    {{ formatDate(survey.importedAt) }}
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm text-gray-900 dark:text-white">
                    {{ survey.questionCount }}
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap">
                  <div class="text-sm text-gray-900 dark:text-white">
                    {{ survey.responseCount }}
                  </div>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-right text-sm">
                  <Button
                    @click="viewSurvey(survey.id)"
                    variant="ghost"
                    size="sm"
                  >
                    View Analytics
                  </Button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Data Inspector Modal -->
    <div
      v-if="showInspectModal"
      class="fixed inset-0 z-50 overflow-y-auto"
      aria-labelledby="modal-title"
      role="dialog"
      aria-modal="true"
    >
      <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
        <!-- Background overlay -->
        <div
          @click="showInspectModal = false"
          class="fixed inset-0 bg-gray-500 bg-opacity-75 dark:bg-gray-900 dark:bg-opacity-75 transition-opacity"
        ></div>

        <!-- Modal panel -->
        <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-4xl sm:w-full">
          <div class="bg-white dark:bg-gray-800 px-6 pt-5 pb-4">
            <div class="flex items-start justify-between mb-4">
              <h3 class="text-lg font-semibold text-gray-900 dark:text-white">
                Raw Survey Data
              </h3>
              <CloseButton @click="showInspectModal = false" />
            </div>
            
            <div class="mt-4">
              <pre class="p-4 bg-gray-50 dark:bg-gray-900 rounded-lg text-xs overflow-auto max-h-96 text-gray-800 dark:text-gray-200">{{ inspectData }}</pre>
            </div>
          </div>
          <div class="bg-gray-50 dark:bg-gray-900 px-6 py-3 flex justify-end">
            <Button
              @click="showInspectModal = false"
            >
              Close
            </Button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { API_BASE_URL } from '@/config/api'
import { ref, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import Button from '../components/common/Button.vue'
import CloseButton from '../components/common/CloseButton.vue'

const router = useRouter()
const fileInput = ref(null)
const selectedFiles = ref([])
const isDragging = ref(false)
const isUploading = ref(false)
const errorMessages = ref([])
const recentImports = ref([])
const showInspectModal = ref(false)
const inspectData = ref('')

const hasSuccessfulUploads = computed(() => {
  return selectedFiles.value.some(f => f.success)
})

const handleDrop = (e) => {
  isDragging.value = false
  const files = Array.from(e.dataTransfer.files)
  addFiles(files)
}

const handleFileSelect = (e) => {
  const files = Array.from(e.target.files)
  addFiles(files)
}

const addFiles = (files) => {
  errorMessages.value = []
  
  for (const file of files) {
    // Validate file extension
    const ext = file.name.split('.').pop().toLowerCase()
    if (!['porps', 'porp', 'porpd', 'porpz'].includes(ext)) {
      errorMessages.value.push(`Invalid file type: ${file.name}. Only .porps, .porp, .porpd, and .porpz files are supported.`)
      continue
    }
    
    // Validate file size (50MB max)
    if (file.size > 50 * 1024 * 1024) {
      errorMessages.value.push(`File too large: ${file.name}. Maximum size is 50MB.`)
      continue
    }
    
    selectedFiles.value.push({
      file,
      name: file.name,
      size: file.size,
      uploading: false,
      progress: 0,
      success: false,
      error: false,
      surveyId: null
    })
  }
}

const removeFile = (index) => {
  selectedFiles.value.splice(index, 1)
}

const clearAll = () => {
  selectedFiles.value = []
  errorMessages.value = []
}

const uploadFiles = async () => {
  isUploading.value = true
  errorMessages.value = []
  
  for (const fileItem of selectedFiles.value) {
    if (fileItem.success || fileItem.error) continue
    
    fileItem.uploading = true
    fileItem.progress = 0
    
    try {
      const formData = new FormData()
      const ext = fileItem.name.split('.').pop().toLowerCase()
      
      if (ext === 'porpz') {
        // Upload porpz archive
        formData.append('porpzFile', fileItem.file)
        const response = await axios.post('${API_BASE_URL}/api/survey-import/porpz', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
          onUploadProgress: (progressEvent) => {
            fileItem.progress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
          }
        })
        
        fileItem.uploading = false
        fileItem.success = true
        fileItem.surveyId = response.data.surveyId
        
        recentImports.value.unshift({
          id: response.data.surveyId,
          name: response.data.surveyName || fileItem.name,
          importedAt: new Date(),
          questionCount: response.data.questionCount || 0,
          responseCount: response.data.responseCount || 0
        })
        
      } else if (ext === 'porps') {
        // Upload porps, look for matching porpd and porp files
        const baseName = fileItem.name.replace('.porps', '')
        const matchingPorpd = selectedFiles.value.find(f => 
          f.name === baseName + '.porpd' && !f.success && !f.error
        )
        const matchingPorp = selectedFiles.value.find(f => 
          f.name === baseName + '.porp' && !f.success && !f.error
        )
        
        formData.append('surveyFile', fileItem.file)
        if (matchingPorpd) {
          formData.append('dataFile', matchingPorpd.file)
          matchingPorpd.uploading = true
          matchingPorpd.progress = 0
        }
        if (matchingPorp) {
          formData.append('projectFile', matchingPorp.file)
          matchingPorp.uploading = true
          matchingPorp.progress = 0
        }
        
        const response = await axios.post('${API_BASE_URL}/api/survey-import/porps', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
          onUploadProgress: (progressEvent) => {
            const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
            fileItem.progress = progress
            if (matchingPorpd) matchingPorpd.progress = progress
            if (matchingPorp) matchingPorp.progress = progress
          }
        })
        
        fileItem.uploading = false
        fileItem.success = true
        fileItem.surveyId = response.data.surveyId
        fileItem.projectId = response.data.projectId
        
        if (matchingPorpd) {
          matchingPorpd.uploading = false
          matchingPorpd.success = true
          matchingPorpd.surveyId = response.data.surveyId
          matchingPorpd.projectId = response.data.projectId
        }
        if (matchingPorp) {
          matchingPorp.uploading = false
          matchingPorp.success = true
          matchingPorp.surveyId = response.data.surveyId
          matchingPorp.projectId = response.data.projectId
        }
        
        recentImports.value.unshift({
          id: response.data.surveyId,
          name: response.data.surveyName || fileItem.name,
          projectId: response.data.projectId,
          projectName: response.data.projectName,
          importedAt: new Date(),
          questionCount: response.data.questionCount || 0,
          responseCount: response.data.responseCount || 0
        })
        
      } else if (ext === 'porpd') {
        // Skip standalone porpd files - they should be paired with porps
        const baseName = fileItem.name.replace('.porpd', '')
        const matchingPorps = selectedFiles.value.find(f => f.name === baseName + '.porps')
        
        if (!matchingPorps) {
          fileItem.uploading = false
          fileItem.error = true
          errorMessages.value.push(`${fileItem.name}: Data file requires matching .porps survey file`)
        }
        // If matching porps exists, it will handle this file
        continue
      }
      
    } catch (error) {
      fileItem.uploading = false
      fileItem.error = true
      errorMessages.value.push(`Failed to upload ${fileItem.name}: ${error.response?.data?.message || error.message}`)
    }
  }
  
  isUploading.value = false
}

const formatFileSize = (bytes) => {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
}

const formatDate = (date) => {
  return new Intl.DateTimeFormat('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(new Date(date))
}

const viewSurvey = (surveyId) => {
  router.push(`/analytics/${surveyId}`)
}

// Load recent imports on mount
const loadRecentImports = async () => {
  try {
    // This would call an API endpoint to get recent imports
    // For now, leaving empty
  } catch (error) {
    console.error('Failed to load recent imports:', error)
  }
}

// Fetch inspect data for the most recent successful upload
const loadInspectData = async () => {
  const successfulFile = selectedFiles.value.find(f => f.success)
  if (!successfulFile || !successfulFile.surveyId) return
  
  try {
    const response = await axios.get(`${API_BASE_URL}/api/surveys/${successfulFile.surveyId}/data`)
    const data = response.data
    
    // Format the data for display
    let output = `Survey ID: ${successfulFile.surveyId}\n`
    output += `Data File: ${data.dataFilePath}\n`
    output += `Total Rows: ${data.totalRows} (${data.dataRows} data rows + 1 header)\n`
    output += `Columns: ${data.headerRow.length}\n\n`
    output += `Header Row:\n${data.headerRow.join(', ')}\n\n`
    output += `First 10 Data Rows:\n`
    
    const previewRows = data.dataList.slice(1, Math.min(11, data.dataList.length))
    previewRows.forEach((row, idx) => {
      output += `Row ${idx + 1}: ${row.join(', ')}\n`
    })
    
    inspectData.value = output
  } catch (error) {
    console.error('Error loading survey data:', error)
    inspectData.value = `Survey ID: ${successfulFile.surveyId}\n\nFailed to load data: ${error.response?.data || error.message}`
  }
}

// Watch for inspect modal opening
watch(showInspectModal, (newValue) => {
  if (newValue) {
    loadInspectData()
  }
})
</script>
