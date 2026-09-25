import { useEffect, useMemo, useState } from 'react'
import {
  WorkflowClient,
  TreeDesigner,
  WorkflowRunner,
  WorkflowInbox,
} from '@quality-patterns/chd-workflow-react'
import './App.css'

const API = import.meta.env.VITE_API_URL || '/api/workflow'

const USERS = [
  { id: 'employee', label: { en: 'Employee', tr: 'Çalışan' }, roles: ['Employee'] },
  { id: 'manager', label: { en: 'Manager / Operations', tr: 'Amir / Operasyon' }, roles: ['Manager', 'Amir', 'Operasyon'] },
  { id: 'admin', label: { en: 'Admin / Finance', tr: 'Admin / Finans' }, roles: ['Admin', 'Finans', 'Yonetici'] },
]

const SCENARIOS = [
  {
    id: 'leave-request',
    name: { en: 'Leave request', tr: 'İzin talebi' },
    blurb: {
      en: 'Employee submits days and reason. Manager approves or rejects.',
      tr: 'Çalışan gün ve gerekçe girer. Yönetici onaylar veya reddeder.',
    },
  },
  {
    id: 'satin-alma-talebi',
    name: { en: 'Purchase request', tr: 'Satın alma talebi' },
    blurb: {
      en: 'Under 10,000 goes to manager. 10,000+ goes to finance.',
      tr: '10.000 altı amire, üzeri finansa gider.',
    },
  },
  {
    id: 'destek-sikayet',
    name: { en: 'Support ticket', tr: 'Destek kaydı' },
    blurb: {
      en: 'High priority goes to a supervisor. Others go to operations.',
      tr: 'Yüksek öncelik yöneticiye, diğerleri operasyona gider.',
    },
  },
]

export default function App() {
  const [mode, setMode] = useState('runner')
  const [locale, setLocale] = useState(() => sessionStorage.getItem('wf-locale') || 'en')
  const [userId, setUserId] = useState('employee')
  const [instanceId, setInstanceId] = useState('')
  const [inboxKey, setInboxKey] = useState(0)
  const [definitions, setDefinitions] = useState([])
  const [definitionId, setDefinitionId] = useState('satin-alma-talebi')
  const [message, setMessage] = useState('')
  const tr = locale === 'tr'
  const user = USERS.find((u) => u.id === userId) || USERS[0]
  const headers = useMemo(
    () => ({
      'X-Workflow-User': user.id,
      'X-Workflow-Roles': user.roles.join(','),
    }),
    [user],
  )
  const client = useMemo(() => new WorkflowClient({ apiUrl: API, headers }), [headers])

  async function reloadDefinitions() {
    try {
      const defs = (await client.getDefinitions()).map((d) => ({
        ...d,
        id: d.id || d.Id,
        name: d.name || d.Name || d.id || d.Id,
      }))
      const featured = SCENARIOS.map((s) => defs.find((d) => d.id === s.id)).filter(Boolean)
      const rest = defs.filter((d) => !SCENARIOS.some((s) => s.id === d.id))
      setDefinitions([...featured, ...rest])
      setDefinitionId((current) => {
        if (current && defs.some((d) => d.id === current)) return current
        return featured[0]?.id || defs[0]?.id || ''
      })
    } catch (err) {
      setMessage(err instanceof Error ? err.message : String(err))
    }
  }

  useEffect(() => {
    void reloadDefinitions()
  }, [client])

  async function start() {
    if (!definitionId) {
      setMessage(tr ? 'Önce bir workflow seçin.' : 'Select a workflow first.')
      return
    }
    try {
      setMessage('')
      const instance = await client.createInstance(definitionId, { requesterId: user.id })
      const state = await client.getState(instance.id)
      const first = state.availableActions.find((a) => a.action === 'start') ?? state.availableActions[0]
      if (first) await client.transition(instance.id, first.action, {})
      setInstanceId(instance.id)
      setInboxKey((k) => k + 1)
    } catch (err) {
      setMessage(err instanceof Error ? err.message : String(err))
    }
  }

  return (
    <div className="app">
      <header className="app-header">
        <h1>Chd.Workflow sample</h1>
        <select
          value={locale}
          onChange={(e) => {
            setLocale(e.target.value)
            sessionStorage.setItem('wf-locale', e.target.value)
          }}
          title={tr ? 'Dil' : 'Language'}
        >
          <option value="en">EN</option>
          <option value="tr">TR</option>
        </select>
        <select value={userId} onChange={(e) => { setUserId(e.target.value); setInboxKey((k) => k + 1) }}>
          {USERS.map((u) => <option key={u.id} value={u.id}>{u.label[locale] || u.label.en}</option>)}
        </select>
        <button type="button" className={mode === 'designer' ? 'active' : ''} onClick={() => setMode('designer')}>
          {tr ? 'Tasarımcı' : 'Designer'}
        </button>
        <button type="button" className={mode === 'runner' ? 'active' : ''} onClick={() => setMode('runner')}>
          {tr ? 'Çalıştır' : 'Runner'}
        </button>
      </header>

      {mode === 'designer' ? (
        <div className="app-main">
          <TreeDesigner
            key={locale}
            apiUrl={API}
            headers={headers}
            initialDefinitionId={definitionId || undefined}
            locale={locale}
            onSaved={(saved) => {
              setDefinitionId(saved.id)
              void reloadDefinitions()
            }}
          />
        </div>
      ) : (
        <div className="app-runner">
          <div className="scenario-grid">
            {SCENARIOS.map((scenario) => (
              <button
                key={scenario.id}
                type="button"
                className={definitionId === scenario.id ? 'scenario-card active' : 'scenario-card'}
                onClick={() => setDefinitionId(scenario.id)}
              >
                <strong>{scenario.name[locale]}</strong>
                <span>{scenario.blurb[locale]}</span>
              </button>
            ))}
          </div>

          <div className="panel">
            <label>
              {tr ? 'Kayıtlı workflow' : 'Saved workflows'}
              <select value={definitionId} onChange={(e) => setDefinitionId(e.target.value)}>
                {definitions.length === 0 && <option value="">{tr ? 'Kayıtlı workflow yok' : 'No saved workflows'}</option>}
                {definitions.map((def) => (
                  <option key={def.id} value={def.id}>{def.name || def.id}</option>
                ))}
              </select>
            </label>
            <button type="button" className="primary" onClick={start} disabled={!definitionId}>
              {tr ? 'Seçilen workflow’u başlat' : 'Start selected workflow'}
            </button>
            {message && <div className="error">{message}</div>}
          </div>

          <WorkflowInbox
            apiUrl={API}
            headers={headers}
            locale={locale}
            theme="dark"
            selectedId={instanceId}
            refreshToken={inboxKey}
            onSelect={(row) => setInstanceId(row.id)}
          />
          {instanceId && (
            <WorkflowRunner
              key={`${instanceId}-${user.id}-${locale}`}
              apiUrl={API}
              headers={headers}
              instanceId={instanceId}
              locale={locale}
              theme="dark"
              onCompleted={() => setInboxKey((k) => k + 1)}
              onTransition={() => setInboxKey((k) => k + 1)}
            />
          )}
        </div>
      )}
    </div>
  )
}
