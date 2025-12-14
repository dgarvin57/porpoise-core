import { API_BASE_URL } from '@/config/api'
import axios from 'axios'

const UI_VERSION = '1.2.0'

export async function checkVersions() {
  try {
    const response = await axios.get(`${API_BASE_URL}/version`)
    const apiVersion = response.data.version
    
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━')
    console.log('🐬 Porpoise Analytics')
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━')
    console.log(`UI Version:  ${UI_VERSION}`)
    console.log(`API Version: ${apiVersion}`)
    console.log(`Environment: ${response.data.environment}`)
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━')
    
    return { ui: UI_VERSION, api: apiVersion }
  } catch (error) {
    console.error('Failed to check API version:', error)
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━')
    console.log('🐬 Porpoise Analytics')
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━')
    console.log(`UI Version:  ${UI_VERSION}`)
    console.log(`API Version: Unable to connect`)
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━')
    
    return { ui: UI_VERSION, api: 'unknown' }
  }
}

export function getUIVersion() {
  return UI_VERSION
}
