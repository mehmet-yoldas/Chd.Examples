import { existsSync } from 'node:fs'
import { fileURLToPath } from 'node:url'
import { dirname, resolve } from 'node:path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

const root = dirname(fileURLToPath(import.meta.url))
const workflowReact = resolve(root, '../../../library-core/chd-workflow-react')
const workflowReactEntry = resolve(workflowReact, 'src/index.ts')
const useLocalWorkflowReact = existsSync(workflowReactEntry)

if (useLocalWorkflowReact) {
  console.log('Using local qp-workflow-react:', workflowReactEntry)
} else {
  console.log('Local chd-workflow-react not found; using the npm package')
}

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: useLocalWorkflowReact
      ? { 'qp-workflow-react': workflowReactEntry }
      : {},
  },
  server: {
    port: 5174,
    strictPort: true,
    fs: { allow: useLocalWorkflowReact ? [root, workflowReact] : [root] },
    proxy: {
      '/api': {
        target: 'http://localhost:5088',
        changeOrigin: true,
      },
    },
  },
  optimizeDeps: useLocalWorkflowReact
    ? { exclude: ['qp-workflow-react'] }
    : {},
})
