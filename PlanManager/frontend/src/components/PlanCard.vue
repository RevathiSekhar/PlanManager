<template>
  <div class="plan-card" :class="[`status-${plan.status.toLowerCase()}`, { expanded }]">
    <div class="plan-header" @click="expanded = !expanded">
      <div class="plan-meta">
        <span class="status-dot"></span>
        <div>
          <h3 class="plan-title">{{ plan.title }}</h3>
          <p v-if="plan.description" class="plan-desc">{{ plan.description }}</p>
        </div>
      </div>
      <div class="plan-actions">
        <span class="step-count">{{ plan.steps.length }} step{{ plan.steps.length !== 1 ? 's' : '' }}</span>
        <span class="status-badge">{{ plan.status }}</span>
        <button class="chevron" :class="{ open: expanded }">
          <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
            <path d="M4 6L8 10L12 6" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
          </svg>
        </button>
      </div>
    </div>

    <Transition name="expand">
      <div v-if="expanded" class="plan-body">
        <!-- Steps list -->
        <div class="steps-section">
          <div v-if="plan.steps.length === 0" class="empty-steps">
            No steps yet. Add the first one ↓
          </div>
          <TransitionGroup name="step-list" tag="ul" class="steps-list">
            <li v-for="step in sortedSteps" :key="step.id" class="step-item">
              <span class="step-order">{{ step.order }}</span>
              <div class="step-content">
                <span class="step-title">{{ step.title }}</span>
                <span v-if="step.description" class="step-desc">{{ step.description }}</span>
              </div>
              <span class="step-status" :class="step.status.toLowerCase()">{{ step.status }}</span>
            </li>
          </TransitionGroup>
        </div>

        <!-- Live events feed -->
        <div v-if="events.length" class="events-feed">
          <p class="feed-label">Live events</p>
          <TransitionGroup name="event-list" tag="ul" class="events-list">
            <li v-for="ev in events" :key="ev.occurredAt" class="event-item">
              <span class="event-type">{{ ev.eventType }}</span>
              <span class="event-msg">{{ ev.message }}</span>
              <span class="event-time">{{ formatTime(ev.occurredAt) }}</span>
            </li>
          </TransitionGroup>
        </div>

        <!-- Add step form -->
        <form class="add-step-form" @submit.prevent="submitStep">
          <p class="form-label">Add a step</p>
          <div class="form-row">
            <input v-model="stepForm.title" placeholder="Step title" required />
            <input v-model="stepForm.description" placeholder="Description (optional)" />
            <input v-model.number="stepForm.order" type="number" placeholder="Order" min="1" required style="width:80px" />
            <button type="submit" class="btn-add" :disabled="submitting">
              <span v-if="submitting" class="spinner"></span>
              <span v-else>Add</span>
            </button>
          </div>
          <p v-if="stepError" class="form-error">{{ stepError }}</p>
        </form>
      </div>
    </Transition>

    <!-- Realtime pulse indicator -->
    <div v-if="hasNewEvent" class="pulse-ring"></div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { usePlanSubscription } from '../composables/usePlans'

const props = defineProps({
  plan: { type: Object, required: true },
  onAddStep: { type: Function, required: true }
})

const expanded = ref(false)
const submitting = ref(false)
const stepError = ref('')
const events = ref([])
const hasNewEvent = ref(false)

const stepForm = ref({ title: '', description: '', order: (props.plan.steps.length + 1) })

const sortedSteps = computed(() =>
  [...props.plan.steps].sort((a, b) => a.order - b.order)
)

// Subscribe to real-time plan updates
usePlanSubscription(props.plan.id, (ev) => {
  events.value.unshift(ev)
  if (events.value.length > 20) events.value.pop()
  hasNewEvent.value = true
  setTimeout(() => { hasNewEvent.value = false }, 3000)
})

watch(() => props.plan.steps.length, (n) => {
  stepForm.value.order = n + 1
})

async function submitStep() {
  if (!stepForm.value.title.trim()) return
  submitting.value = true
  stepError.value = ''
  try {
    await props.onAddStep(
      props.plan.id,
      stepForm.value.title,
      stepForm.value.description || null,
      stepForm.value.order
    )
    stepForm.value.title = ''
    stepForm.value.description = ''
  } catch (e) {
    stepError.value = e.message
  } finally {
    submitting.value = false
  }
}

function formatTime(iso) {
  return new Date(iso).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}
</script>
