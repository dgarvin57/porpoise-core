<template>
  <div
    v-if="isOpen"
    class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
    @click.self="closeModal"
  >
    <div class="bg-white dark:bg-gray-800 rounded-lg shadow-xl w-full max-w-md p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl font-semibold text-gray-900 dark:text-white">
          Edit Survey
        </h2>
        <CloseButton @click="closeModal" />
      </div>

      <!-- Form -->
      <form @submit.prevent="saveSurvey" class="space-y-4">
        <!-- Survey Name -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Survey Name
          </label>
          <input
            v-model="editedSurvey.name"
            type="text"
            required
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
            placeholder="Enter survey name"
          />
        </div>

        <!-- Survey Status -->
        <div>
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
            Status
          </label>
          <select
            v-model="editedSurvey.status"
            class="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 dark:bg-gray-700 dark:text-white"
          >
            <option :value="0">Initial</option>
            <option :value="1">Verified</option>
          </select>
        </div>

        <!-- Buttons -->
        <div class="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            @click="closeModal"
            class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-700 hover:bg-gray-200 dark:hover:bg-gray-600 rounded-md"
          >
            Cancel
          </button>
          <button
            type="submit"
            :disabled="saving"
            class="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {{ saving ? 'Saving...' : 'Save Changes' }}
          </button>
        </div>
      </form>

      <!-- Error Message -->
      <div v-if="errorMessage" class="mt-4 p-3 bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400 rounded-md text-sm">
        {{ errorMessage }}
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import axios from 'axios'
import CloseButton from '../common/CloseButton.vue'

const props = defineProps({
  isOpen: {
    type: Boolean,
    required: true
  },
  survey: {
    type: Object,
    default: null
  }
})

const emit = defineEmits(['close', 'survey-updated'])

const editedSurvey = ref({
  name: '',
  status: 0
})

const saving = ref(false)
const errorMessage = ref('')

// Watch for survey changes to populate the form
watch(() => props.survey, (newSurvey) => {
  if (newSurvey) {
    editedSurvey.value = {
      name: newSurvey.name,
      status: newSurvey.status ?? 0
    }
  }
}, { immediate: true })

function closeModal() {
  errorMessage.value = ''
  emit('close')
}

async function saveSurvey() {
  if (!props.survey) return
  
  saving.value = true
  errorMessage.value = ''
  
  try {
    await axios.patch(`http://localhost:5107/api/surveys/${props.survey.id}`, {
      surveyName: editedSurvey.value.name,
      status: editedSurvey.value.status
    })
    
    emit('survey-updated', props.survey.id)
    closeModal()
  } catch (error) {
    console.error('Error updating survey:', error)
    errorMessage.value = error.response?.data?.message || error.message || 'Failed to update survey'
  } finally {
    saving.value = false
  }
}
</script>
