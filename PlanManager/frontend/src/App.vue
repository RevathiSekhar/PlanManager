<template>
  <div id="app">
    <!-- Notifications -->
    <Teleport to="body">
      <TransitionGroup name="notif" tag="div" class="notifications">
        <div v-for="n in notifications" :key="n.id" class="notif" :class="n.type">
          <strong>{{ n.title }}</strong>
          <span>{{ n.message }}</span>
        </div>
      </TransitionGroup>
    </Teleport>

    <!-- Header -->
    <header class="app-header">
      <div class="header-inner">
        <div class="logo">
          <svg width="28" height="28" viewBox="0 0 28 28" fill="none">
            <rect x="2" y="2" width="24" height="24" rx="6" stroke="var(--accent)" stroke-width="1.5"/>
            <path d="M8 10h12M8 14h8M8 18h10" stroke="var(--accent)" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
          <span>PlanManager</span>
        </div>
        <div class="header-meta">
          <span class="conn-dot" :class="{ active: true }"></span>
          <span class="conn-label">GraphQL · WebSocket</span>
        </div>
      </div>
    </header>

    <!-- Main -->
    <main class="main">
      <div class="sidebar">
        <!-- Create Plan form -->
        <div class="create-panel">
          <h2>New Plan</h2>
          <form @submit.prevent="submitPlan">
            <label>Title</label>
            <input v-model="newPlan.title" placeholder="e.g. Launch v2.0" required />
            <label>Description</label>
            <textarea v-model="newPlan.description" placeholder="What is this plan about?" rows="3"></textarea>
            <button type="submit" class="btn-primary" :disabled="creating">
              <span v-if="creating" class="spinner white"></span>
              <span v-else>Create Plan</span>
            </button>
            <p v-if="createError" class="form-error">{{ createError }}</p>
          </form>
        </div>

        <!-- Stats -->
        <div class="stats">
          <div class="stat">
            <span class="stat-value">{{ plans.length }}</span>
            <span class="stat-label">Plans</span>
          </div>
          <div class="stat">
            <span class="stat-value">{{ totalSteps }}</span>
            <span class="stat-label">Steps</span>
          </div>
          <div class="stat">
            <span class="stat-value">{{ draftCount }}</span>
            <span class="stat-label">Draft</span>
          </div>
        </div>

        <!-- GraphQL hint -->
        <div class="gql-hint">
          <p class="hint-title">GraphQL Endpoint</p>
          <code>http://localhost:5000/graphql</code>
          <p class="hint-title" style="margin-top:12px">WebSocket</p>
          <code>ws://localhost:5000/graphql</code>
          <p class="hint-body">Use Banana Cake Pop or any GraphQL client to explore the schema.</p>
        </div>
      </div>

      <!-- Plans list -->
      <div class="plans-area">
        <div class="plans-header">
          <h1>Plans <span class="count-badge">{{ plans.length }}</span></h1>
          <button class="btn-icon" @click="refetch()" title="Refresh">
            <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
              <path d="M13.5 8A5.5 5.5 0 1 1 8 2.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
              <path d="M8 2.5L10.5 5M8 2.5L10.5 0" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
          </button>
        </div>

        <div v-if="loading && !plans.length" class="loading-state">
          <div class="loader"></div>
          <p>Loading plans…</p>
        </div>

        <div v-else-if="!plans.length" class="empty-state">
          <svg width="48" height="48" viewBox="0 0 48 48" fill="none">
            <rect x="8" y="8" width="32" height="32" rx="8" stroke="var(--muted)" stroke-width="1.5" stroke-dasharray="4 3"/>
            <path d="M16 24h16M16 30h10" stroke="var(--muted)" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
          <p>No plans yet. Create your first one →</p>
        </div>

        <TransitionGroup v-else name="plan-list" tag="div" class="plans-list">
          <PlanCard
            v-for="plan in plans"
            :key="plan.id"
            :plan="plan"
            :on-add-step="addStep"
          />
        </TransitionGroup>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import PlanCard from './components/PlanCard.vue'
import { usePlans } from './composables/usePlans'

const { plans, loading, creating, notifications, createPlan, addStep, refetch } = usePlans()

const newPlan = ref({ title: '', description: '' })
const createError = ref('')

const totalSteps = computed(() => plans.value.reduce((s, p) => s + p.steps.length, 0))
const draftCount = computed(() => plans.value.filter(p => p.status === 'Draft').length)

async function submitPlan() {
  createError.value = ''
  try {
    await createPlan(newPlan.value.title, newPlan.value.description || null)
    newPlan.value = { title: '', description: '' }
  } catch (e) {
    createError.value = e.message
  }
}
</script>

<style>
/* ── Reset & tokens ─────────────────────────────────────────────────────────── */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0 }

:root {
  --bg:        #0d0f14;
  --surface:   #13161e;
  --surface2:  #1a1e28;
  --border:    #252936;
  --border2:   #2e3347;
  --text:      #e2e6f0;
  --muted:     #5a6080;
  --accent:    #6c8aff;
  --accent2:   #a78bfa;
  --success:   #34d399;
  --warning:   #fbbf24;
  --danger:    #f87171;
  --radius:    12px;
  --font:      'DM Mono', 'Fira Code', monospace;
}

html, body { height: 100%; background: var(--bg); color: var(--text); font-family: var(--font); font-size: 14px; line-height: 1.6 }

/* ── Layout ─────────────────────────────────────────────────────────────────── */
#app { min-height: 100vh; display: flex; flex-direction: column }

.app-header {
  border-bottom: 1px solid var(--border);
  padding: 0 32px;
  height: 56px;
  display: flex;
  align-items: center;
  background: var(--surface);
  position: sticky; top: 0; z-index: 100;
}
.header-inner { width: 100%; display: flex; align-items: center; justify-content: space-between }
.logo { display: flex; align-items: center; gap: 10px; font-size: 15px; font-weight: 600; letter-spacing: -0.3px }
.header-meta { display: flex; align-items: center; gap: 8px; font-size: 12px; color: var(--muted) }
.conn-dot { width: 7px; height: 7px; border-radius: 50%; background: var(--muted) }
.conn-dot.active { background: var(--success); box-shadow: 0 0 0 3px rgba(52,211,153,.2); animation: pulse-dot 2s infinite }

@keyframes pulse-dot { 0%,100%{box-shadow:0 0 0 3px rgba(52,211,153,.2)} 50%{box-shadow:0 0 0 6px rgba(52,211,153,.05)} }

.main { flex: 1; display: flex; gap: 0; max-width: 1400px; width: 100%; margin: 0 auto; padding: 32px 24px; gap: 32px }

/* ── Sidebar ────────────────────────────────────────────────────────────────── */
.sidebar { width: 280px; flex-shrink: 0; display: flex; flex-direction: column; gap: 16px }

.create-panel {
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 20px;
}
.create-panel h2 { font-size: 13px; font-weight: 600; color: var(--muted); text-transform: uppercase; letter-spacing: 1px; margin-bottom: 16px }
.create-panel form { display: flex; flex-direction: column; gap: 10px }
.create-panel label { font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .8px }

input, textarea {
  background: var(--surface2); border: 1px solid var(--border2); border-radius: 8px; color: var(--text);
  font-family: var(--font); font-size: 13px; padding: 8px 12px; width: 100%; outline: none;
  transition: border-color .15s;
}
input:focus, textarea:focus { border-color: var(--accent) }
textarea { resize: vertical }

.btn-primary {
  background: var(--accent); color: #fff; border: none; border-radius: 8px; padding: 9px 16px;
  font-family: var(--font); font-size: 13px; font-weight: 600; cursor: pointer; transition: opacity .15s, transform .1s;
  display: flex; align-items: center; justify-content: center; gap: 8px;
}
.btn-primary:hover:not(:disabled) { opacity: .88 }
.btn-primary:active:not(:disabled) { transform: scale(.98) }
.btn-primary:disabled { opacity: .5; cursor: not-allowed }

.form-error { font-size: 12px; color: var(--danger) }

.stats {
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 16px;
  display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px;
}
.stat { display: flex; flex-direction: column; align-items: center; gap: 4px }
.stat-value { font-size: 22px; font-weight: 700; color: var(--accent) }
.stat-label { font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .7px }

.gql-hint {
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius); padding: 16px;
  display: flex; flex-direction: column; gap: 6px;
}
.hint-title { font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .8px }
.gql-hint code {
  background: var(--surface2); border: 1px solid var(--border2); border-radius: 6px; padding: 6px 10px;
  font-size: 12px; color: var(--accent2); display: block; word-break: break-all;
}
.hint-body { font-size: 12px; color: var(--muted); line-height: 1.5 }

/* ── Plans area ─────────────────────────────────────────────────────────────── */
.plans-area { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 20px }

.plans-header { display: flex; align-items: center; justify-content: space-between }
.plans-header h1 { font-size: 20px; font-weight: 700; display: flex; align-items: center; gap: 10px }
.count-badge {
  background: var(--surface2); border: 1px solid var(--border2); border-radius: 99px;
  padding: 1px 10px; font-size: 13px; color: var(--muted);
}
.btn-icon {
  background: var(--surface); border: 1px solid var(--border); border-radius: 8px;
  color: var(--muted); cursor: pointer; padding: 8px; display: flex; transition: color .15s, border-color .15s;
}
.btn-icon:hover { color: var(--text); border-color: var(--border2) }

.loading-state, .empty-state {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 12px; padding: 60px 20px; color: var(--muted); text-align: center;
}
.loader {
  width: 32px; height: 32px; border-radius: 50%;
  border: 2px solid var(--border2); border-top-color: var(--accent);
  animation: spin .8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg) } }

.plans-list { display: flex; flex-direction: column; gap: 12px }

/* ── Plan Card ──────────────────────────────────────────────────────────────── */
.plan-card {
  background: var(--surface); border: 1px solid var(--border); border-radius: var(--radius);
  overflow: hidden; position: relative; transition: border-color .2s, box-shadow .2s;
}
.plan-card:hover { border-color: var(--border2) }
.plan-card.status-draft { border-left: 3px solid var(--muted) }
.plan-card.status-active { border-left: 3px solid var(--accent) }
.plan-card.status-completed { border-left: 3px solid var(--success) }

.plan-header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 16px 20px; cursor: pointer; user-select: none;
}
.plan-meta { display: flex; align-items: flex-start; gap: 12px }
.status-dot {
  width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0; margin-top: 6px;
  background: var(--muted);
}
.status-draft .status-dot { background: var(--muted) }
.status-active .status-dot { background: var(--accent); box-shadow: 0 0 0 3px rgba(108,138,255,.15) }
.status-completed .status-dot { background: var(--success) }

.plan-title { font-size: 15px; font-weight: 600; color: var(--text) }
.plan-desc { font-size: 12px; color: var(--muted); margin-top: 2px }

.plan-actions { display: flex; align-items: center; gap: 10px }
.step-count { font-size: 12px; color: var(--muted) }
.status-badge {
  background: var(--surface2); border: 1px solid var(--border2); border-radius: 99px;
  padding: 2px 10px; font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .7px;
}
.chevron { background: none; border: none; color: var(--muted); cursor: pointer; display: flex; transition: transform .2s }
.chevron.open { transform: rotate(180deg) }

.plan-body { padding: 0 20px 20px; display: flex; flex-direction: column; gap: 16px }

.steps-section {}
.empty-steps { font-size: 12px; color: var(--muted); padding: 12px 0 }
.steps-list { list-style: none; display: flex; flex-direction: column; gap: 6px }
.step-item {
  display: flex; align-items: flex-start; gap: 10px; padding: 10px 14px;
  background: var(--surface2); border: 1px solid var(--border2); border-radius: 8px;
}
.step-order {
  width: 22px; height: 22px; border-radius: 50%; background: var(--border2); display: flex;
  align-items: center; justify-content: center; font-size: 11px; color: var(--muted); flex-shrink: 0;
}
.step-content { flex: 1; min-width: 0 }
.step-title { font-size: 13px; font-weight: 500; display: block }
.step-desc { font-size: 12px; color: var(--muted); display: block; margin-top: 2px }
.step-status {
  font-size: 11px; padding: 2px 8px; border-radius: 99px; flex-shrink: 0;
  background: var(--border); color: var(--muted); text-transform: uppercase; letter-spacing: .5px;
}
.step-status.completed { background: rgba(52,211,153,.1); color: var(--success) }
.step-status.inprogress { background: rgba(108,138,255,.1); color: var(--accent) }

/* ── Events feed ────────────────────────────────────────────────────────────── */
.events-feed {
  background: var(--bg); border: 1px solid var(--border); border-radius: 8px; padding: 12px 14px;
}
.feed-label { font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .8px; margin-bottom: 8px }
.events-list { list-style: none; display: flex; flex-direction: column; gap: 5px; max-height: 120px; overflow-y: auto }
.event-item { display: flex; align-items: baseline; gap: 8px; font-size: 12px }
.event-type { color: var(--accent2); font-weight: 600; flex-shrink: 0 }
.event-msg { color: var(--text); flex: 1 }
.event-time { color: var(--muted); flex-shrink: 0 }

/* ── Add step form ──────────────────────────────────────────────────────────── */
.add-step-form {
  background: var(--bg); border: 1px solid var(--border); border-radius: 8px; padding: 14px;
}
.form-label { font-size: 11px; color: var(--muted); text-transform: uppercase; letter-spacing: .8px; margin-bottom: 10px }
.form-row { display: flex; gap: 8px; flex-wrap: wrap }
.form-row input { flex: 1; min-width: 120px }
.btn-add {
  background: var(--accent); color: #fff; border: none; border-radius: 8px; padding: 8px 16px;
  font-family: var(--font); font-size: 13px; font-weight: 600; cursor: pointer; white-space: nowrap;
  display: flex; align-items: center; gap: 6px; transition: opacity .15s;
}
.btn-add:hover:not(:disabled) { opacity: .88 }
.btn-add:disabled { opacity: .5; cursor: not-allowed }

/* ── Pulse ring ─────────────────────────────────────────────────────────────── */
.pulse-ring {
  position: absolute; top: 12px; right: 12px; width: 10px; height: 10px;
  border-radius: 50%; background: var(--accent);
}
.pulse-ring::after {
  content: ''; position: absolute; inset: -4px; border-radius: 50%;
  border: 2px solid var(--accent); animation: pulse-ring .8s ease-out infinite;
}
@keyframes pulse-ring { to { opacity: 0; transform: scale(2) } }

/* ── Spinner ────────────────────────────────────────────────────────────────── */
.spinner {
  width: 14px; height: 14px; border-radius: 50%;
  border: 2px solid rgba(255,255,255,.3); border-top-color: #fff;
  animation: spin .6s linear infinite; display: inline-block;
}

/* ── Notifications ──────────────────────────────────────────────────────────── */
.notifications {
  position: fixed; bottom: 24px; right: 24px; z-index: 9999;
  display: flex; flex-direction: column; gap: 8px; pointer-events: none;
}
.notif {
  background: var(--surface); border: 1px solid var(--border2); border-radius: 10px;
  padding: 12px 16px; display: flex; flex-direction: column; gap: 3px;
  box-shadow: 0 8px 32px rgba(0,0,0,.4); max-width: 280px;
}
.notif.success { border-left: 3px solid var(--success) }
.notif strong { font-size: 13px; font-weight: 600; color: var(--text) }
.notif span { font-size: 12px; color: var(--muted) }

/* ── Transitions ────────────────────────────────────────────────────────────── */
.expand-enter-active, .expand-leave-active { transition: all .25s ease }
.expand-enter-from, .expand-leave-to { opacity: 0; transform: translateY(-6px) }

.plan-list-enter-active, .step-list-enter-active, .event-list-enter-active { transition: all .3s ease }
.plan-list-enter-from, .step-list-enter-from { opacity: 0; transform: translateX(-12px) }
.event-list-enter-from { opacity: 0; transform: translateY(-6px) }
.plan-list-leave-active, .step-list-leave-active { transition: all .2s ease }
.plan-list-leave-to, .step-list-leave-to { opacity: 0; transform: translateX(12px) }

.notif-enter-active { transition: all .3s ease }
.notif-enter-from { opacity: 0; transform: translateX(20px) }
.notif-leave-active { transition: all .25s ease }
.notif-leave-to { opacity: 0; transform: translateX(20px) }

/* ── Scrollbar ──────────────────────────────────────────────────────────────── */
::-webkit-scrollbar { width: 6px }
::-webkit-scrollbar-track { background: transparent }
::-webkit-scrollbar-thumb { background: var(--border2); border-radius: 3px }
</style>
