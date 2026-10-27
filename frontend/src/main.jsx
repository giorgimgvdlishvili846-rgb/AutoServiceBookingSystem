import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles.css'
import AutoServiceApp from './AutoServiceApp.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <AutoServiceApp />
  </StrictMode>,
)
