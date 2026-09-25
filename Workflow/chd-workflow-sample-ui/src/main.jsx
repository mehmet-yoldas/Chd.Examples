import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App.jsx'
import './App.css'

const ua = navigator.userAgent
if (/Chrome\//.test(ua) && !/Edg\//.test(ua)) {
  document.documentElement.classList.add('is-chrome')
}

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
